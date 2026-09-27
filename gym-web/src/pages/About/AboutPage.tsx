import { useEffect, useState } from "react";
import { apiUrl } from "../../api/client";

type AboutContent = {
  title: string;
  subtitle: string;
  description: string;
  imageUrl: string;
  experienceYears: number;
  membersCount: number;
  trainersCount: number;
  isActive: boolean;
};

export function AboutPage() {
  const [about, setAbout] = useState<AboutContent | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(apiUrl("/api/about"))
      .then(async (response) => {
        if (!response.ok) {
          throw new Error("About content unavailable");
        }
        return (await response.json()) as AboutContent;
      })
      .then((content) => {
        if (content.isActive) {
          setAbout(content);
        }
      })
      .catch(() => setAbout(null))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <main className="page"><p>Loading About content…</p></main>;
  }

  if (!about) {
    return (
      <main className="page about-route-empty">
        <h1>About</h1>
        <p>Our story is coming soon.</p>
      </main>
    );
  }

  return (
    <main className="about-section about-route-section">
      <div className="about-visual">
        {about.imageUrl ? <ImageFrame src={about.imageUrl} alt={about.title} className="about-image" /> : <div className="about-image-placeholder" />}
        <span className="about-line" />
      </div>
      <div className="about-copy">
        <span className="eyebrow">{about.subtitle}</span>
        <h1 id="about-heading">{about.title}</h1>
        <p>{about.description}</p>
        <div className="about-stats">
          <div><strong>{about.experienceYears}<small>+</small></strong><span>Years Experience</span></div>
          <div><strong>{about.membersCount}<small>+</small></strong><span>Happy Members</span></div>
          <div><strong>{about.trainersCount}<small>+</small></strong><span>Expert Trainers</span></div>
        </div>
      </div>
    </main>
  );
}
import { ImageFrame } from "../../components/ImageFrame";
