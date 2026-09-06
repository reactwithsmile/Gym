import { useEffect, useState } from "react";
import heroFallbackImage from "../../assets/hero.png";

type HeroContent = {
  heading: string;
  description: string;
  primaryButtonText: string;
  primaryButtonLink: string;
  secondaryButtonText: string;
  secondaryButtonLink: string;
  imageUrl: string;
  isActive: boolean;
};
type AboutContent = {
  subtitle: string;
  title: string;
  description: string;
  imageUrl: string;
  experienceYears: number;
  membersCount: number;
  trainersCount: number;
  isActive: boolean;
};
type ServiceContent = {
  id: number;
  name: string;
  shortDescription: string;
  description: string;
  imageUrl: string;
  icon: string;
  displayOrder: number;
};

const fallbackHero: HeroContent = {
  heading: "Train harder. Live stronger.",
  description: "Build strength, confidence, and consistency with a training experience designed to move you forward.",
  primaryButtonText: "Start training",
  primaryButtonLink: "/membership",
  secondaryButtonText: "Explore the gym",
  secondaryButtonLink: "/about",
  imageUrl: heroFallbackImage,
  isActive: true,
};

const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5182";

export function HomePage() {
  const [hero, setHero] = useState<HeroContent | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [about, setAbout] = useState<AboutContent | null>(null);
  const [services, setServices] = useState<ServiceContent[]>([]);

  useEffect(() => {
    let isCurrent = true;

    fetch(`${apiUrl}/api/hero`)
      .then(async (response) => {
        if (!response.ok) {
          throw new Error("Hero content unavailable");
        }
        return (await response.json()) as HeroContent;
      })
      .then((content) => {
        if (isCurrent && content.isActive) {
          setHero(content);
        } else if (isCurrent) {
          setHero(fallbackHero);
        }
      })
      .catch(() => {
        if (isCurrent) {
          setHero(fallbackHero);
        }
      })
      .finally(() => {
        if (isCurrent) {
          setIsLoading(false);
        }
      });

    return () => {
      isCurrent = false;
    };
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/services`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Services unavailable");
        return (await response.json()) as ServiceContent[];
      })
      .then(setServices)
      .catch(() => setServices([]));
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/about`)
      .then(async (response) => {
        if (!response.ok) throw new Error("About content unavailable");
        return (await response.json()) as AboutContent;
      })
      .then((content) => { setAbout(content); })
      .catch(() => setAbout(null));
  }, []);

  if (isLoading || !hero) {
    return (
      <main className="home-page">
        <section className="hero-section hero-loading" aria-label="Loading hero content">
          <span className="eyebrow">Loading experience</span>
        </section>
        <section className="about-section" aria-labelledby="about-heading">
          {about ? (
            <>
              <div className="about-visual">
                {about.imageUrl ? <img src={about.imageUrl} alt={about.title} /> : <div className="about-image-placeholder" />}
                <span className="about-line" />
              </div>
              <div className="about-copy">
                <span className="eyebrow">{about.subtitle}</span>
                <h2 id="about-heading">{about.title}</h2>
                <p>{about.description}</p>
                <div className="about-stats">
                  <div><strong>{about.experienceYears}<small>+</small></strong><span>Years Experience</span></div>
                  <div><strong>{about.membersCount}<small>+</small></strong><span>Happy Members</span></div>
                  <div><strong>{about.trainersCount}<small>+</small></strong><span>Expert Trainers</span></div>
                </div>
              </div>
            </>
          ) : <p className="about-empty">Our story is coming soon.</p>}
        </section>
        <ServicesSection services={services} />
      </main>
    );
  }

  return (
    <main className="home-page">
      <section className="hero-section" aria-labelledby="hero-heading" style={{ backgroundImage: `url("${hero.imageUrl || heroFallbackImage}")` }}>
        <div className="hero-backdrop" />
        <div className="hero-grid" />
        <div className="hero-content">
          <span className="eyebrow">Train <i>•</i> Build <i>•</i> Transform</span>
          <h1 id="hero-heading">{hero.heading}</h1>
          <p className="hero-description">{hero.description}</p>
          <div className="hero-actions">
            <a className="hero-button hero-button-primary" href={hero.primaryButtonLink}>
              {hero.primaryButtonText} <span>↗</span>
            </a>
            <a className="hero-button hero-button-secondary" href={hero.secondaryButtonLink}>
              <span className="play-icon">▶</span> {hero.secondaryButtonText}
            </a>
          </div>
          <div className="hero-meta">
            <span>01</span>
            <span>Strength / Performance / Community</span>
          </div>
        </div>
        <div className="hero-visual">
          <div className="hero-image-frame">
            <img src={hero.imageUrl || heroFallbackImage} alt={hero.heading} />
          </div>
          <span className="hero-vertical-label">EST. 2024 — YOUR BEST SELF</span>
          <span className="hero-cross hero-cross-one">×</span>
          <span className="hero-cross hero-cross-two">×</span>
        </div>
        <div className="hero-stats" aria-label="Gym statistics">
          <div><strong>10K<span>+</span></strong><small>Active Members</small></div>
          <div><strong>50<span>+</span></strong><small>Expert Trainers</small></div>
          <div><strong>98<span>%</span></strong><small>Success Rate</small></div>
        </div>
        <div className="hero-scroll"><span>Scroll</span><i /></div>
      </section>
      <ServicesSection services={services} />
    </main>
  );
}

function ServicesSection({ services }: { services: ServiceContent[] }) {
  return (
    <section className="services-section" aria-labelledby="services-heading">
      <div className="services-heading">
        <div><span className="eyebrow">What we offer</span><h2 id="services-heading">Train with purpose.</h2></div>
        <p>Focused programs designed to make you stronger, fitter, and more confident.</p>
      </div>
      {services.length ? <div className="services-grid">{services.map((service) => (
        <article className="service-card" key={service.id}>
          {service.imageUrl ? <div className="service-image"><img src={service.imageUrl} alt={service.name} /></div> : <div className="service-image service-image-empty">{service.icon || "＋"}</div>}
          <div className="service-card-content"><span className="service-index">{String(service.displayOrder).padStart(2, "0")}</span><h3>{service.name}</h3><p>{service.shortDescription}</p>{service.description ? <small>{service.description}</small> : null}</div>
        </article>
      ))}</div> : <p className="services-empty">Our training programs are coming soon.</p>}
    </section>
  );
}
