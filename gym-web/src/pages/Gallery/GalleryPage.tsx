import { useEffect, useState } from "react";
import { apiUrl } from "../../api/client";

export function GalleryPage() {
  const [items, setItems] = useState<Array<{ id: number; title: string; description: string; imageUrl: string; category: string }>>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { fetch(apiUrl("/api/gallery")).then(async (response) => { if (!response.ok) throw new Error(); return await response.json(); }).then(setItems).catch(() => setItems([])).finally(() => setLoading(false)); }, []);
  return <main className="gallery-page"><section className="gallery-section"><div className="gallery-heading"><div><span className="eyebrow">Inside the gym</span><h1>Built for<br />the work.</h1></div><p>A closer look at the spaces, sessions, and details that make every training day count.</p></div>{loading ? <p className="gallery-empty">Loading gallery…</p> : items.length ? <div className="gallery-grid">{items.map((item, index) => <article className={`gallery-card gallery-card-${index % 5}`} key={item.id}><img src={item.imageUrl} alt={item.title} /><div className="gallery-card-overlay"><span>{item.category || "Gym"}</span><h3>{item.title}</h3>{item.description ? <p>{item.description}</p> : null}</div></article>)}</div> : <p className="gallery-empty">Our gallery is coming soon.</p>}</section></main>;
}
