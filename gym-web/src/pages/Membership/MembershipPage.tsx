import { useEffect, useState } from "react";

type Plan = { id: number; name: string; shortDescription: string; price: number; durationMonths: number; features: string; displayOrder: number; isPopular: boolean };
type GymSettings = { currency: string };
const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5182";

export function MembershipPage() {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loading, setLoading] = useState(true);
  const [currency, setCurrency] = useState("INR");
  useEffect(() => {
    fetch(`${apiUrl}/api/membership-plans`).then(async (response) => { if (!response.ok) throw new Error(); return await response.json() as Plan[]; }).then(setPlans).catch(() => setPlans([])).finally(() => setLoading(false));
    fetch(`${apiUrl}/api/gym-settings`).then(async (response) => { if (!response.ok) throw new Error(); return await response.json() as GymSettings; }).then((settings) => setCurrency(settings.currency || "INR")).catch(() => undefined);
  }, []);
  return <main className="membership-page"><section className="membership-section membership-route-section"><div className="membership-heading"><div><span className="eyebrow">Membership</span><h1>Choose your<br />commitment.</h1></div><p>Simple plans designed to keep you consistent and moving forward.</p></div>{loading ? <p className="membership-empty">Loading plans…</p> : plans.length ? <div className="membership-grid">{plans.map((plan) => <article className={`membership-card${plan.isPopular ? " popular" : ""}`} key={plan.id}>{plan.isPopular ? <span className="popular-badge">Most popular</span> : null}<span className="membership-index">{String(plan.displayOrder).padStart(2, "0")}</span><h2>{plan.name}</h2><p>{plan.shortDescription}</p><strong className="membership-price">{currency} {plan.price.toLocaleString("en-IN")}<small> / {plan.durationMonths} {plan.durationMonths === 1 ? "month" : "months"}</small></strong><ul>{plan.features.split(/\r?\n/).map((feature) => feature.trim()).filter(Boolean).map((feature) => <li key={feature}>{feature}</li>)}</ul><a className="membership-cta" href="/contact">Get started <span>↗</span></a></article>)}</div> : <p className="membership-empty">Membership plans are coming soon.</p>}</section></main>;
}
