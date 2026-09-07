import { useEffect, useState } from "react";
type Testimonial = { id: number; customerName: string; roleOrDescription: string; review: string; rating: number; imageUrl: string };
const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5182";
export function TestimonialsPage() {
  const [items, setItems] = useState<Testimonial[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { fetch(`${apiUrl}/api/testimonials`).then(async (response) => { if (!response.ok) throw new Error(); return await response.json() as Testimonial[]; }).then(setItems).catch(() => setItems([])).finally(() => setLoading(false)); }, []);
  return <main className="testimonials-page"><section className="testimonials-section testimonials-route-section"><div className="testimonials-heading"><div><span className="eyebrow">Real results</span><h1>What our members say.</h1></div><p>Consistency feels better when you have the right people beside you.</p></div>{loading ? <p className="testimonials-empty">Loading testimonials…</p> : items.length ? <div className="testimonials-grid">{items.map((item) => <article className="testimonial-card" key={item.id}>{item.imageUrl ? <img className="testimonial-avatar" src={item.imageUrl} alt={item.customerName} /> : <div className="testimonial-avatar testimonial-avatar-empty">{item.customerName.charAt(0).toUpperCase()}</div>}<div className="testimonial-stars">{"★".repeat(item.rating)}<span>{"★".repeat(5 - item.rating)}</span></div><blockquote>“{item.review}”</blockquote><footer><strong>{item.customerName}</strong>{item.roleOrDescription ? <small>{item.roleOrDescription}</small> : null}</footer></article>)}</div> : <p className="testimonials-empty">Member stories are coming soon.</p>}</section></main>;
}
