import { useEffect, useState } from "react";
import { apiFetch } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { Permission } from "../../types/auth";
import * as XLSX from "xlsx";

type Membership = {
  id: number;
  membershipPlanId?: number | null;
  startDate: string;
  endDate: string;
  status: string;
};
type Member = { id: number; name: string; email: string; phone: string; isActive: boolean; memberships?: Membership[] };
type Plan = { id: number; name: string; price: number; durationMonths: number; isActive: boolean };
type Payment = { id: number; memberId: number; amount: number; paidAt: string; method: string; status: string; notes: string; createdByUser?: { name?: string; email?: string } | null; updatedAt: string };
type Expiry = { membershipId: number; memberId: number; memberName: string; planName: string; expiryDate: string; daysRemaining: number; status: string };
type Form = { memberId: number; membershipId: number; amount: number; paymentDate: string; paymentMethod: string; status: string; notes: string };
type MembershipForm = { planId: number; startDate: string; endDate: string };
type PaymentRange = "all" | "30days" | "custom";
type ImportRow = { name: string; email: string; phone: string; membershipPlan: string; startDate: string; endDate: string; amount: number; paymentDate: string; paymentMethod: string };

const emptyForm: Form = { memberId: 0, membershipId: 0, amount: 0, paymentDate: new Date().toISOString().slice(0, 10), paymentMethod: "Cash", status: "Paid", notes: "" };
const today = () => new Date().toISOString().slice(0, 10);
const emptyMembershipForm: MembershipForm = { planId: 0, startDate: today(), endDate: "" };

export function FeesPage() {
  const { hasPermission } = useAuth();
  const [members, setMembers] = useState<Member[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [payments, setPayments] = useState<Payment[]>([]);
  const [expiring, setExpiring] = useState<Expiry[]>([]);
  const [form, setForm] = useState<Form>(emptyForm);
  const [membershipForm, setMembershipForm] = useState<MembershipForm>(emptyMembershipForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [memberSearch, setMemberSearch] = useState("");
  const [paymentRange, setPaymentRange] = useState<PaymentRange>("all");
  const [paymentFrom, setPaymentFrom] = useState("");
  const [paymentTo, setPaymentTo] = useState("");
  const [importRows, setImportRows] = useState<ImportRow[]>([]);
  const [importing, setImporting] = useState(false);
  const [memberForm, setMemberForm] = useState({ name: "", email: "", phone: "", isActive: true });
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const canView = hasPermission(Permission.FeesView);
  const canCreate = hasPermission(Permission.FeesCreate);
  const canCreateMember = hasPermission(Permission.MembersCreate);

  async function load() {
    setLoading(true);
    try {
      const [memberData, expiryData, planData] = await Promise.all([
        apiFetch<Member[]>("/api/members"),
        apiFetch<Expiry[]>("/api/expiring"),
        apiFetch<Plan[]>("/api/membership-plans")
      ]);
      setMembers(memberData);
      setExpiring(expiryData);
      setPlans(planData);
    } catch (error) {
      setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to load members." });
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { if (canView) void load(); }, [canView]);

  function getMemberships(memberId: number) {
    return members.find((member) => member.id === memberId)?.memberships ?? [];
  }

  async function selectMember(memberId: number) {
    const memberships = getMemberships(memberId);
    const activeMembership = memberships.find((membership) => membership.status === "Active");
    setForm((current) => ({ ...current, memberId, membershipId: activeMembership?.id ?? 0 }));
    setMembershipForm(emptyMembershipForm);
    if (memberId) setPayments(await apiFetch<Payment[]>(`/api/payments/member/${memberId}`));
    else setPayments([]);
  }

  async function createMembership() {
    if (!form.memberId || !membershipForm.startDate || !membershipForm.endDate) {
      setMessage({ type: "error", text: "Select a member and provide membership dates." });
      return;
    }
    if (membershipForm.startDate < today()) {
      setMessage({ type: "error", text: "Membership start date cannot be in the past." });
      return;
    }
    if (membershipForm.endDate <= membershipForm.startDate) {
      setMessage({ type: "error", text: "Membership end date must be after the start date." });
      return;
    }
    setSaving(true);
    try {
      const membership = await apiFetch<Membership>("/api/members/memberships", {
        method: "POST",
        body: JSON.stringify({
          memberId: form.memberId,
          membershipPlanId: membershipForm.planId || null,
          startDate: membershipForm.startDate,
          endDate: membershipForm.endDate
        })
      });
      setMembers((current) => current.map((member) => member.id === form.memberId
        ? { ...member, memberships: [...(member.memberships ?? []), membership] }
        : member));
      setForm((current) => ({ ...current, membershipId: membership.id }));
      setMembershipForm(emptyMembershipForm);
      setMessage({ type: "success", text: "Membership added successfully." });
    } catch (error) {
      setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to add membership." });
    } finally {
      setSaving(false);
    }
  }

  async function save() {
    if (!form.memberId || !form.amount || !form.paymentMethod) {
      setMessage({ type: "error", text: "Member and amount are required." });
      return;
    }
    if (form.paymentDate > today()) {
      setMessage({ type: "error", text: "Payment date cannot be in the future. Past dates are allowed for historical payments." });
      return;
    }
    setSaving(true);
    try {
      await apiFetch("/api/payments", {
        method: "POST",
        body: JSON.stringify({ memberId: form.memberId, membershipId: form.membershipId || null, amount: form.amount, paidAt: form.paymentDate, method: form.paymentMethod, status: form.status, notes: form.notes })
      });
      setMessage({ type: "success", text: "Payment saved successfully." });
      await selectMember(form.memberId);
    } catch (error) {
      setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to save payment." });
    } finally {
      setSaving(false);
    }
  }

  async function createMember() {
    if (!memberForm.name.trim() || !memberForm.email.trim()) {
      setMessage({ type: "error", text: "Member name and email are required." });
      return;
    }
    setSaving(true);
    try {
      const member = await apiFetch<Member>("/api/members", { method: "POST", body: JSON.stringify(memberForm) });
      setMembers((current) => [...current, member].sort((a, b) => a.name.localeCompare(b.name)));
      setMemberForm({ name: "", email: "", phone: "", isActive: true });
      setMessage({ type: "success", text: "Member added successfully." });
    } catch (error) {
      setMessage({ type: "error", text: error instanceof Error ? error.message : "Unable to add member." });
    } finally {
      setSaving(false);
    }
  }

  function excelDate(value: unknown) {
      if (typeof value === "number") return XLSX.SSF.format("yyyy-mm-dd", value);
      const parsed = new Date(String(value));
      return Number.isNaN(parsed.getTime()) ? "" : parsed.toISOString().slice(0, 10);
    }

  async function readImport(event: React.ChangeEvent<HTMLInputElement>) {
      const file = event.target.files?.[0];
      if (!file) return;
      if (!/\.(xlsx|xls|csv)$/i.test(file.name)) { setMessage({ type: "error", text: "Upload an .xlsx, .xls, or .csv file." }); return; }
      try {
        const workbook = XLSX.read(await file.arrayBuffer(), { type: "array" });
        const sheet = workbook.Sheets[workbook.SheetNames[0]];
        const rows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, { defval: "" });
        const mapped = rows.map((row) => ({
          name: String(row.Name ?? row.name ?? "").trim(),
          email: String(row.Email ?? row.email ?? "").trim(),
          phone: String(row.Phone ?? row.phone ?? "").trim(),
          membershipPlan: String(row.MembershipPlan ?? row["Membership Plan"] ?? row.plan ?? "").trim(),
          startDate: excelDate(row.StartDate ?? row["Start Date"]),
          endDate: excelDate(row.EndDate ?? row["End Date"]),
          amount: Number(row.Amount ?? row.amount ?? 0),
          paymentDate: excelDate(row.PaymentDate ?? row["Payment Date"]),
          paymentMethod: String(row.PaymentMethod ?? row["Payment Method"] ?? "Cash").trim()
        })).filter((row) => row.name || row.email);
        if (!mapped.length) { setMessage({ type: "error", text: "No member rows found. Use columns Name, Email, Phone, StartDate, EndDate, Amount, PaymentDate and PaymentMethod." }); return; }
        setImportRows(mapped);
        setMessage({ type: "success", text: `${mapped.length} rows ready to preview.` });
      } catch { setMessage({ type: "error", text: "Unable to read the spreadsheet." }); }
    }

  async function importMembers() {
      const invalid = importRows.find((row) => !row.name || !row.email || (row.startDate && row.startDate < today()) || (row.endDate && row.startDate && row.endDate <= row.startDate));
      if (invalid) { setMessage({ type: "error", text: "Import contains invalid name, email, or membership dates." }); return; }
      setImporting(true);
      let imported = 0;
      try {
        for (const row of importRows) {
          const member = await apiFetch<Member>("/api/members", { method: "POST", body: JSON.stringify({ name: row.name, email: row.email, phone: row.phone, isActive: true }) });
          if (row.startDate && row.endDate) {
            const plan = plans.find((item) => item.name.toLowerCase() === row.membershipPlan.toLowerCase());
            await apiFetch("/api/members/memberships", { method: "POST", body: JSON.stringify({ memberId: member.id, membershipPlanId: plan?.id ?? null, startDate: row.startDate, endDate: row.endDate }) });
          }
          if (row.amount > 0) await apiFetch("/api/payments", { method: "POST", body: JSON.stringify({ memberId: member.id, amount: row.amount, paidAt: row.paymentDate || today(), method: row.paymentMethod || "Cash", status: "Paid", notes: "Imported from spreadsheet" }) });
          imported++;
        }
        setImportRows([]);
        await load();
        setMessage({ type: "success", text: `${imported} members imported successfully.` });
      } catch (error) { setMessage({ type: "error", text: error instanceof Error ? `Import stopped after ${imported} rows: ${error.message}` : "Import failed." }); }
      finally { setImporting(false); }
  }

  if (!canView) return <main className="page"><h1>Fees & Payments</h1><p>You do not have permission to view this page.</p></main>;

  const selectedMemberships = getMemberships(form.memberId);
  const selectedMember = members.find((member) => member.id === form.memberId);
  const filteredMembers = members.filter((member) => `${member.name} ${member.email} ${member.phone}`.toLowerCase().includes(memberSearch.toLowerCase()));
  const activeMembership = selectedMemberships.find((membership) => membership.status === "Active");
  const memberPaidTotal = payments.filter((payment) => payment.status.toLowerCase() === "paid").reduce((total, payment) => total + payment.amount, 0);
  const visiblePayments = payments.filter((payment) => {
    const date = payment.paidAt.slice(0, 10);
    if (paymentRange === "all") return true;
    if (paymentRange === "30days") {
      const from = new Date(); from.setDate(from.getDate() - 30);
      return date >= from.toISOString().slice(0, 10);
    }
    return (!paymentFrom || date >= paymentFrom) && (!paymentTo || date <= paymentTo);
  });
  return <main className="page cms-page">
    <div className="page-header"><div><h1>Fees & Payments</h1><p className="muted">Add members, assign memberships, record payments and monitor expirations.</p></div></div>
    {message ? <p className={`info ${message.type}`}>{message.text}</p> : null}
    {canCreateMember ? <section className="card cms-form"><h2>Add Member</h2><div className="form-grid">
      <div className="form-field"><label>Name<input value={memberForm.name} onChange={(event) => setMemberForm({ ...memberForm, name: event.target.value })} /></label></div>
      <div className="form-field"><label>Email<input type="email" value={memberForm.email} onChange={(event) => setMemberForm({ ...memberForm, email: event.target.value })} /></label></div>
      <div className="form-field"><label>Phone<input value={memberForm.phone} onChange={(event) => setMemberForm({ ...memberForm, phone: event.target.value })} /></label></div>
    </div><div className="cms-form-footer cms-form-footer-end"><button className="btn" disabled={saving} onClick={() => void createMember()}>{saving ? "Saving…" : "Add Member"}</button></div></section> : null}
    {canCreateMember ? <section className="card cms-form import-card"><div className="import-heading"><div><p className="eyebrow">BULK ONBOARDING</p><h2>Import members</h2><p className="muted">Upload an Excel or CSV file to add members, memberships and payments.</p></div><label className="btn upload-button"><input type="file" accept=".xlsx,.xls,.csv" onChange={(event) => void readImport(event)} />Choose spreadsheet</label></div><p className="field-hint">Columns: Name, Email, Phone, StartDate, EndDate, Amount, PaymentDate, PaymentMethod.</p>{importRows.length ? <><div className="import-preview"><strong>{importRows.length} rows ready</strong>{importRows.slice(0, 3).map((row, index) => <span key={`${row.email}-${index}`}>{row.name} · {row.email}</span>)}</div><button className="btn" disabled={importing} onClick={() => void importMembers()}>{importing ? "Importing…" : "Import valid rows"}</button></> : null}</section> : null}
    <section className="member-directory card"><div className="member-directory-header"><div><p className="eyebrow">MEMBERS</p><h2>Member directory</h2><p className="muted">Select a member to view their profile, membership and payment history.</p></div><input className="member-search" placeholder="Search members..." value={memberSearch} onChange={(event) => setMemberSearch(event.target.value)} /></div><div className="member-list">{filteredMembers.map((member) => <button type="button" className={`member-row${member.id === form.memberId ? " selected" : ""}`} key={member.id} onClick={() => void selectMember(member.id)}><span className="member-avatar">{member.name.charAt(0).toUpperCase()}</span><span className="member-row-info"><strong>{member.name}</strong><small>{member.email}</small></span><span className={`member-status ${member.isActive ? "active" : "inactive"}`}>{member.isActive ? "Active" : "Inactive"}</span><span className="member-row-arrow">›</span></button>)}{!filteredMembers.length ? <p className="empty-state">No members found.</p> : null}</div></section>
    {selectedMember ? <section className="member-profile card"><div className="member-profile-main"><span className="profile-avatar">{selectedMember.name.charAt(0).toUpperCase()}</span><div><p className="eyebrow">MEMBER PROFILE</p><h2>{selectedMember.name}</h2><p className="muted">{selectedMember.email} {selectedMember.phone ? `• ${selectedMember.phone}` : ""}</p></div></div><div className="member-profile-stats"><div><span>Membership</span><strong>{activeMembership ? plans.find((plan) => plan.id === activeMembership.membershipPlanId)?.name ?? "Active plan" : "Not assigned"}</strong></div><div><span>Valid until</span><strong>{activeMembership ? new Date(activeMembership.endDate).toLocaleDateString() : "—"}</strong></div><div><span>Total paid</span><strong>₹{memberPaidTotal.toLocaleString()}</strong></div><div><span>Payments</span><strong>{payments.length}</strong></div></div></section> : null}
    <section className="card cms-form"><h2>Record Payment</h2><div className="form-grid">
      <div className="form-field"><label>Member<select value={form.memberId} onChange={(event) => void selectMember(Number(event.target.value))}><option value={0}>Select member</option>{members.map((member) => <option key={member.id} value={member.id}>{member.name} — {member.email}</option>)}</select></label></div>
      <div className="form-field"><label>Membership<select value={form.membershipId} disabled={!form.memberId || !selectedMemberships.length} onChange={(event) => setForm({ ...form, membershipId: Number(event.target.value) })}><option value={0}>{selectedMemberships.length ? "Select membership" : "No membership assigned"}</option>{selectedMemberships.map((membership) => <option key={membership.id} value={membership.id}>{plans.find((plan) => plan.id === membership.membershipPlanId)?.name ?? "Membership"} — {new Date(membership.endDate).toLocaleDateString()} ({membership.status})</option>)}</select></label></div>
      <div className="form-field"><label>Amount<input type="number" min="0" value={form.amount} onChange={(event) => setForm({ ...form, amount: Number(event.target.value) })} /></label></div>
      <div className="form-field"><label>Payment date<input type="date" max={today()} required value={form.paymentDate} onChange={(event) => setForm({ ...form, paymentDate: event.target.value })} /><small className="field-hint">Past dates are allowed for historical payments.</small></label></div>
      <div className="form-field"><label>Method<select value={form.paymentMethod} onChange={(event) => setForm({ ...form, paymentMethod: event.target.value })}><option>Cash</option><option>Card</option><option>UPI</option><option>Bank Transfer</option></select></label></div>
      <div className="form-field"><label>Status<select value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value })}><option>Paid</option><option>Pending</option><option>Failed</option><option>Refunded</option></select></label></div>
      <div className="form-field"><label>Notes<input value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} /></label></div>
    </div><div className="cms-form-footer cms-form-footer-end"><button className="btn" disabled={!canCreate || saving} onClick={() => void save()}>{saving ? "Saving…" : "Save Payment"}</button></div></section>
    {form.memberId && canCreateMember ? <section className="card cms-form"><h2>Add Membership</h2><p className="muted">Assign a plan and dates to the selected member.</p><div className="form-grid">
      <div className="form-field"><label>Plan<select value={membershipForm.planId} onChange={(event) => setMembershipForm({ ...membershipForm, planId: Number(event.target.value) })}><option value={0}>Custom membership</option>{plans.filter((plan) => plan.isActive).map((plan) => <option key={plan.id} value={plan.id}>{plan.name} — {plan.price}</option>)}</select></label></div>
      <div className="form-field"><label>Start date<input type="date" min={today()} required value={membershipForm.startDate} onChange={(event) => setMembershipForm({ ...membershipForm, startDate: event.target.value })} /></label></div>
      <div className="form-field"><label>End date<input type="date" min={membershipForm.startDate || today()} required value={membershipForm.endDate} onChange={(event) => setMembershipForm({ ...membershipForm, endDate: event.target.value })} /></label></div>
    </div><div className="cms-form-footer cms-form-footer-end"><button className="btn" disabled={saving} onClick={() => void createMembership()}>{saving ? "Saving…" : "Add Membership"}</button></div></section> : null}
    <section className="card expiry-card"><div className="payment-history-heading"><div><p className="eyebrow">MEMBERSHIP MONITORING</p><h2>Expiry follow-up</h2></div><span className="panel-count">{expiring.length}</span></div>{expiring.length ? <div className="expiry-grid"><div className="expiry-grid-head"><span>Member</span><span>Plan</span><span>Expiry date</span><span>Days left</span><span>Status</span></div>{expiring.map((item) => <div className="expiry-grid-row" key={item.membershipId}><strong>{item.memberName}</strong><span>{item.planName}</span><span>{new Date(item.expiryDate).toLocaleDateString()}</span><span>{item.status === "Expired" ? "Expired" : `${item.daysRemaining} days`}</span><span><span className={`expiry-status ${item.status === "Expired" ? "expired" : "soon"}`}>{item.status}</span></span></div>)}</div> : <p className="empty-state">No memberships are expiring or recently expired.</p>}</section>
    {form.memberId ? <section className="card payment-history-card"><div className="payment-history-heading"><div><p className="eyebrow">TRANSACTIONS</p><h2>Payment history</h2></div><span className="panel-count">{visiblePayments.length}</span></div><div className="history-filters"><select value={paymentRange} onChange={(event) => setPaymentRange(event.target.value as PaymentRange)}><option value="all">All time</option><option value="30days">Last 30 days</option><option value="custom">Custom range</option></select>{paymentRange === "custom" ? <><input type="date" value={paymentFrom} onChange={(event) => setPaymentFrom(event.target.value)} /><span>to</span><input type="date" value={paymentTo} onChange={(event) => setPaymentTo(event.target.value)} /></> : null}</div>{loading ? <p>Loading…</p> : visiblePayments.length ? <div className="payment-grid"><div className="payment-grid-head"><span>Date</span><span>Amount</span><span>Method</span><span>Status</span><span>Recorded by</span></div>{visiblePayments.map((payment) => <div className="payment-grid-row" key={payment.id}><span>{new Date(payment.paidAt).toLocaleDateString()}</span><strong>₹{payment.amount.toLocaleString()}</strong><span className="method-badge">{payment.method}</span><span><span className={`payment-status ${payment.status.toLowerCase()}`}>{payment.status}</span></span><span className="payment-user">{payment.createdByUser?.name || payment.createdByUser?.email || "—"}</span></div>)}</div> : <p className="empty-state">No payments found for this period.</p>}</section> : null}
  </main>;
}
