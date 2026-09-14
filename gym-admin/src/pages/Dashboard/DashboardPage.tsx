import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";

type Member = { id: number; name: string; createdAt: string };
type Payment = { id: number; amount: number; paidAt: string; status: string; member?: { name?: string } };
type Expiring = { membershipId: number; memberName: string; planName: string; expiryDate: string; daysRemaining: number; status: string };
type Enquiry = { id: number; name: string; message: string; status: string; createdAt: string };
type RangeKey = "today" | "yesterday" | "7days" | "lastweek" | "lastmonth" | "custom";

function dateKey(date: Date) {
  return date.toISOString().slice(0, 10);
}

function getRange(key: RangeKey): [string, string] {
  const now = new Date();
  const end = new Date(now);
  const start = new Date(now);
  if (key === "yesterday") { start.setDate(start.getDate() - 1); end.setDate(end.getDate() - 1); }
  if (key === "7days") start.setDate(start.getDate() - 6);
  if (key === "lastweek") {
    const day = now.getDay() || 7;
    start.setDate(now.getDate() - day - 6);
    end.setDate(now.getDate() - day);
  }
  if (key === "lastmonth") {
    start.setMonth(now.getMonth() - 1, 1);
    end.setDate(0);
  }
  return [dateKey(start), dateKey(end)];
}

function inRange(value: string, from: string, to: string) {
  const date = dateKey(new Date(value));
  return date >= from && date <= to;
}

export function DashboardPage() {
  const { hasPermission } = useAuth();
  const canViewMembers = hasPermission(Permission.MembersView);
  const canViewFees = hasPermission(Permission.FeesView);
  const canViewEnquiries = hasPermission(Permission.EnquiriesView);
  const [members, setMembers] = useState<Member[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [expiring, setExpiring] = useState<Expiring[]>([]);
  const [enquiries, setEnquiries] = useState<Enquiry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [rangeKey, setRangeKey] = useState<RangeKey>("today");
  const [range, setRange] = useState<[string, string]>(() => getRange("today"));

  useEffect(() => {
    let active = true;
    async function load() {
      setLoading(true);
      const requests: Promise<unknown>[] = [];
      if (canViewMembers) requests.push(apiFetch<Member[]>("/api/members"));
      if (canViewFees) {
        requests.push(apiFetch<Payment[]>("/api/payments"));
        requests.push(apiFetch<Expiring[]>("/api/expiring?days=7"));
      }
      if (canViewEnquiries) requests.push(apiFetch<Enquiry[]>("/api/enquiries"));
      const results = await Promise.allSettled(requests);
      if (!active) return;
      let index = 0;
      let failed = false;
      if (canViewMembers) {
        const result = results[index++];
        if (result.status === "fulfilled") setMembers(result.value as Member[]);
        else failed = true;
      }
      if (canViewFees) {
        const paymentResult = results[index++];
        const expiringResult = results[index++];
        if (paymentResult.status === "fulfilled") setPayments(paymentResult.value as Payment[]);
        else failed = true;
        if (expiringResult.status === "fulfilled") setExpiring(expiringResult.value as Expiring[]);
        else failed = true;
      }
      if (canViewEnquiries) {
        const result = results[index++];
        if (result.status === "fulfilled") setEnquiries(result.value as Enquiry[]);
        else failed = true;
      }
      if (failed) setError("Some dashboard data could not be loaded.");
      setLoading(false);
    }
    void load();
    return () => { active = false; };
  }, [canViewMembers, canViewFees, canViewEnquiries]);

  const filteredMembers = members.filter((member) => inRange(member.createdAt, range[0], range[1]));
  const filteredPayments = payments.filter((payment) => payment.status.toLowerCase() === "paid" && inRange(payment.paidAt, range[0], range[1]));
  const rangeMembers = filteredMembers.length;
  const rangeIncome = filteredPayments.reduce((total, payment) => total + payment.amount, 0);

  return <main className="page dashboard-page">
    <div className="dashboard-hero"><div><p className="eyebrow">OVERVIEW</p><h1>Good day, {new Date().toLocaleDateString(undefined, { weekday: "long" })}</h1><p className="muted">Review gym activity for any reporting period.</p></div><div className="dashboard-date">{new Date().toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" })}</div></div>
    <div className="dashboard-filters"><select value={rangeKey} onChange={(event) => { const value = event.target.value as RangeKey; setRangeKey(value); if (value !== "custom") setRange(getRange(value)); }}><option value="today">Today</option><option value="yesterday">Yesterday</option><option value="7days">Last 7 days</option><option value="lastweek">Last week</option><option value="lastmonth">Last month</option><option value="custom">Custom range</option></select>{rangeKey === "custom" ? <><input type="date" value={range[0]} onChange={(event) => setRange([event.target.value, range[1]])} /><span>to</span><input type="date" value={range[1]} onChange={(event) => setRange([range[0], event.target.value])} /></> : null}<span className="filter-caption">{range[0]} to {range[1]}</span></div>
    {error ? <p className="info error">{error}</p> : null}
    {loading ? <p>Loading dashboard…</p> : <><div className="dashboard-stats">
      <section className="dashboard-stat stat-orange"><div className="stat-icon">₹</div><div><p>Income in period</p><h2>{canViewFees ? rangeIncome.toLocaleString(undefined, { style: "currency", currency: "INR" }) : "—"}</h2><small>{filteredPayments.length} paid payments</small></div></section>
      <section className="dashboard-stat stat-blue"><div className="stat-icon">+</div><div><p>New members</p><h2>{canViewMembers ? rangeMembers : "—"}</h2><small>Added in period</small></div></section>
      <section className="dashboard-stat stat-red"><div className="stat-icon">!</div><div><p>Expiry follow-ups</p><h2>{canViewFees ? expiring.length : "—"}</h2><small>Expiring or recently expired</small></div></section>
      <section className="dashboard-stat stat-green"><div className="stat-icon">✓</div><div><p>Payment activity</p><h2>{canViewFees ? filteredPayments.length : "—"}</h2><small>Successful in period</small></div></section>
      <section className="dashboard-stat stat-orange"><div className="stat-icon">?</div><div><p>New enquiries</p><h2>{canViewEnquiries ? enquiries.filter((item) => item.status === "New").length : "—"}</h2><small>Need follow-up</small></div></section>
    </div><div className="dashboard-grid"><section className="card dashboard-panel"><div className="panel-heading"><div><p className="eyebrow">ATTENTION NEEDED</p><h2>Membership expiry follow-up</h2></div><span className="panel-count">{expiring.length}</span></div>{canViewFees && expiring.length ? <div className="expiry-grid compact"><div className="expiry-grid-head"><span>Member</span><span>Plan</span><span>Expiry</span><span>Days</span><span>Status</span></div>{expiring.slice(0, 5).map((item) => <div className="expiry-grid-row" key={item.membershipId}><strong>{item.memberName}</strong><span>{item.planName}</span><span>{new Date(item.expiryDate).toLocaleDateString()}</span><span>{item.status === "Expired" ? "Expired" : `${item.daysRemaining} days`}</span><span><span className={`expiry-status ${item.status === "Expired" ? "expired" : "soon"}`}>{item.status}</span></span></div>)}</div> : <p className="empty-state">{canViewFees ? "No memberships are expiring or recently expired." : "You do not have permission to view fee data."}</p>}</section><section className="card dashboard-panel"><div className="panel-heading"><div><p className="eyebrow">LEAD FOLLOW-UP</p><h2>Latest enquiries</h2></div><span className="panel-count">{enquiries.filter((item) => item.status === "New").length}</span></div>{canViewEnquiries && enquiries.length ? enquiries.slice(0, 5).map((item) => <div className="summary-row" key={item.id}><span><strong>{item.name}</strong><small>{new Date(item.createdAt).toLocaleDateString()}</small></span><span className={`enquiry-status ${item.status.toLowerCase()}`}>{item.status}</span></div>) : <p className="empty-state">{canViewEnquiries ? "No enquiries yet." : "You do not have permission to view enquiries."}</p>}</section><section className="card dashboard-panel dashboard-summary"><p className="eyebrow">PERIOD SNAPSHOT</p><h2>Keep your gym moving</h2><div className="summary-row"><span>Collected revenue</span><strong>{rangeIncome.toLocaleString(undefined, { style: "currency", currency: "INR" })}</strong></div><div className="summary-row"><span>New members</span><strong>{rangeMembers}</strong></div><div className="summary-row"><span>Renewals to follow up</span><strong>{expiring.length}</strong></div></section></div></>}
  </main>;
}
