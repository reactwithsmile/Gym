import { useEffect, useState } from "react";

type Trainer = { id: number; name: string; role: string; bio: string; imageUrl: string; specialization: string; experienceYears: number; instagramUrl: string; facebookUrl: string; displayOrder: number };
const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5182";

export function TrainersPage() {
  const [trainers, setTrainers] = useState<Trainer[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => {
    let current = true;
    fetch(`${apiUrl}/api/trainers`).then(async (response) => {
      if (!response.ok) throw new Error();
      return await response.json() as Trainer[];
    }).then((data) => { if (current) setTrainers(data); }).catch(() => { if (current) setTrainers([]); }).finally(() => { if (current) setLoading(false); });
    return () => { current = false; };
  }, []);
  return <main className="services-page"><section className="trainers-section trainers-route-section"><div className="trainers-heading"><div><span className="eyebrow">Meet the team</span><h1>Train with people<br />who know the work.</h1></div><p>Expert coaching, practical guidance, and the accountability to keep you moving forward.</p></div>{loading ? <p className="trainers-empty">Loading trainers…</p> : trainers.length ? <div className="trainers-grid">{trainers.map((trainer) => <article className="trainer-card" key={trainer.id}>{trainer.imageUrl ? <div className="trainer-image"><img src={trainer.imageUrl} alt={trainer.name} /></div> : <div className="trainer-image trainer-image-empty">G</div>}<div className="trainer-card-content"><span className="trainer-role">{trainer.role}</span><h2>{trainer.name}</h2>{trainer.specialization ? <p>{trainer.specialization}</p> : null}{trainer.experienceYears > 0 ? <small>{trainer.experienceYears}+ years experience</small> : null}{trainer.bio ? <small>{trainer.bio}</small> : null}</div></article>)}</div> : <p className="trainers-empty">Our coaching team is coming soon.</p>}</section></main>;
}
