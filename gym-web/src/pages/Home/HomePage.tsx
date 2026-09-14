import { useEffect, useState, type CSSProperties } from "react";
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
type TrainerContent = {
  id: number;
  name: string;
  role: string;
  bio: string;
  imageUrl: string;
  specialization: string;
  experienceYears: number;
  instagramUrl: string;
  facebookUrl: string;
  displayOrder: number;
};
type MembershipPlan = {
  id: number; name: string; shortDescription: string; description: string;
  price: number; durationMonths: number; features: string; displayOrder: number; isPopular: boolean;
};
type TestimonialContent = {
  id: number; customerName: string; roleOrDescription: string; review: string;
  rating: number; imageUrl: string; displayOrder: number;
};
type GalleryItem = {
  id: number; title: string; description: string; imageUrl: string; category: string; displayOrder: number;
};
type ContactContent = {
  gymName: string; address: string; phone: string; email: string; openingHours: string;
  googleMapsUrl: string; instagramUrl: string; facebookUrl: string; whatsAppNumber: string; description: string;
};
type GymSettings = { currency: string };

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
  const [trainers, setTrainers] = useState<TrainerContent[]>([]);
  const [plans, setPlans] = useState<MembershipPlan[]>([]);
  const [testimonials, setTestimonials] = useState<TestimonialContent[]>([]);
  const [gallery, setGallery] = useState<GalleryItem[]>([]);
  const [contact, setContact] = useState<ContactContent | null>(null);
  const [currency, setCurrency] = useState("INR");
  const [heroPointer, setHeroPointer] = useState({ x: 0, y: 0 });

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
    fetch(`${apiUrl}/api/gym-settings`)
      .then(async (response) => { if (!response.ok) throw new Error("Gym settings unavailable"); return await response.json() as GymSettings; })
      .then((settings) => setCurrency(settings.currency || "INR"))
      .catch(() => undefined);
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/contact`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Contact unavailable");
        return (await response.json()) as ContactContent;
      })
      .then(setContact)
      .catch(() => setContact(null));
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/testimonials`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Testimonials unavailable");
        return (await response.json()) as TestimonialContent[];
      })
      .then(setTestimonials)
      .catch(() => setTestimonials([]));
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/gallery`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Gallery unavailable");
        return (await response.json()) as GalleryItem[];
      })
      .then(setGallery)
      .catch(() => setGallery([]));
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/membership-plans`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Membership plans unavailable");
        return (await response.json()) as MembershipPlan[];
      })
      .then(setPlans)
      .catch(() => setPlans([]));
  }, []);

  useEffect(() => {
    fetch(`${apiUrl}/api/trainers`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Trainers unavailable");
        return (await response.json()) as TrainerContent[];
      })
      .then(setTrainers)
      .catch(() => setTrainers([]));
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
        <TrainersSection trainers={trainers} />
        <MembershipSection plans={plans} currency={currency} />
        <TestimonialsSection testimonials={testimonials} />
        <GallerySection items={gallery} />
        <ContactSection contact={contact} />
      </main>
    );
  }

  return (
    <main className="home-page">
      <section className="hero-section" aria-labelledby="hero-heading" style={{ "--hero-x": `${heroPointer.x}px`, "--hero-y": `${heroPointer.y}px` } as CSSProperties} onMouseMove={(event) => { const rect = event.currentTarget.getBoundingClientRect(); setHeroPointer({ x: (event.clientX - rect.left - rect.width / 2) * .018, y: (event.clientY - rect.top - rect.height / 2) * .012 }); }} onMouseLeave={() => setHeroPointer({ x: 0, y: 0 })}>
        <div className="hero-background-layer" aria-hidden="true" />
        <div className="hero-backdrop" />
        <div className="hero-grid" />
        <div className="hero-atmosphere" aria-hidden="true" />
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
      <AboutSection about={about} />
      <ServicesSection services={services} />
      <TrainersSection trainers={trainers} />
      <MembershipSection plans={plans} currency={currency} />
      <TestimonialsSection testimonials={testimonials} />
      <GallerySection items={gallery} />
      <ContactSection contact={contact} />
    </main>
  );
}

function AboutSection({ about }: { about: AboutContent | null }) {
  return <section className="about-section" aria-labelledby="about-heading">
    {about ? <><div className="about-visual">{about.imageUrl ? <img src={about.imageUrl} alt={about.title} loading="lazy" /> : <div className="about-image-placeholder" />}<span className="about-line" /></div><div className="about-copy"><span className="eyebrow">{about.subtitle}</span><h2 id="about-heading">{about.title}</h2><p>{about.description}</p><div className="about-stats"><div><strong>{about.experienceYears}<small>+</small></strong><span>Years Experience</span></div><div><strong>{about.membersCount}<small>+</small></strong><span>Happy Members</span></div><div><strong>{about.trainersCount}<small>+</small></strong><span>Expert Trainers</span></div></div></div></> : <p className="about-empty">Our story is coming soon.</p>}
  </section>;
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

function TrainersSection({ trainers }: { trainers: TrainerContent[] }) {
  return (
    <section className="trainers-section" aria-labelledby="trainers-heading">
      <div className="trainers-heading">
        <div><span className="eyebrow">Meet the team</span><h2 id="trainers-heading">Train with people<br />who know the work.</h2></div>
        <p>Expert coaching, practical guidance, and the accountability to keep you moving forward.</p>
      </div>
      {trainers.length ? <div className="trainers-grid">{trainers.map((trainer) => (
        <article className="trainer-card" key={trainer.id}>
          {trainer.imageUrl ? <div className="trainer-image"><img src={trainer.imageUrl} alt={trainer.name} /></div> : <div className="trainer-image trainer-image-empty">G</div>}
          <div className="trainer-card-content">
            <span className="trainer-role">{trainer.role}</span>
            <h3>{trainer.name}</h3>
            {trainer.specialization ? <p>{trainer.specialization}</p> : null}
            {trainer.experienceYears > 0 ? <small>{trainer.experienceYears}+ years experience</small> : null}
            {trainer.bio ? <small>{trainer.bio}</small> : null}
            {(trainer.instagramUrl || trainer.facebookUrl) ? <div className="trainer-socials">
              {trainer.instagramUrl ? <a href={trainer.instagramUrl} target="_blank" rel="noreferrer" aria-label={`${trainer.name} Instagram`}>IG</a> : null}
              {trainer.facebookUrl ? <a href={trainer.facebookUrl} target="_blank" rel="noreferrer" aria-label={`${trainer.name} Facebook`}>FB</a> : null}
            </div> : null}
          </div>
        </article>
      ))}</div> : <p className="trainers-empty">Our coaching team is coming soon.</p>}
    </section>
  );
}

function MembershipSection({ plans, currency }: { plans: MembershipPlan[]; currency: string }) {
  return <section className="membership-section" aria-labelledby="membership-heading">
    <div className="membership-heading"><div><span className="eyebrow">Membership</span><h2 id="membership-heading">Choose your<br />commitment.</h2></div><p>Simple plans designed to keep you consistent and moving forward.</p></div>
    {plans.length ? <div className="membership-grid">{plans.map((plan) => <article className={`membership-card${plan.isPopular ? " popular" : ""}`} key={plan.id}>{plan.isPopular ? <span className="popular-badge">Most popular</span> : null}<span className="membership-index">{String(plan.displayOrder).padStart(2, "0")}</span><h3>{plan.name}</h3><p>{plan.shortDescription}</p><strong className="membership-price">{currency} {plan.price.toLocaleString("en-IN")}<small> / {plan.durationMonths} {plan.durationMonths === 1 ? "month" : "months"}</small></strong><ul>{plan.features.split(/\r?\n/).map((feature) => feature.trim()).filter(Boolean).map((feature) => <li key={feature}>{feature}</li>)}</ul><a className="membership-cta" href="/membership">Get started <span>↗</span></a></article>)}</div> : <p className="membership-empty">Membership plans are coming soon.</p>}
  </section>;
}

function TestimonialsSection({ testimonials }: { testimonials: TestimonialContent[] }) {
  return <section className="testimonials-section" aria-labelledby="testimonials-heading">
    <div className="testimonials-heading"><div><span className="eyebrow">Real results</span><h2 id="testimonials-heading">What our members say.</h2></div><p>Consistency feels better when you have the right people beside you.</p></div>
    {testimonials.length ? <div className="testimonials-grid">{testimonials.map((item) => <article className="testimonial-card" key={item.id}>
      {item.imageUrl ? <img className="testimonial-avatar" src={item.imageUrl} alt={item.customerName} /> : <div className="testimonial-avatar testimonial-avatar-empty">{item.customerName.charAt(0).toUpperCase()}</div>}
      <div className="testimonial-stars" aria-label={`${item.rating} out of 5 stars`}>{"★".repeat(item.rating)}<span>{"★".repeat(5 - item.rating)}</span></div>
      <blockquote>“{item.review}”</blockquote>
      <footer><strong>{item.customerName}</strong>{item.roleOrDescription ? <small>{item.roleOrDescription}</small> : null}</footer>
    </article>)}</div> : <p className="testimonials-empty">Member stories are coming soon.</p>}
  </section>;
}

function GallerySection({ items }: { items: GalleryItem[] }) {
  return <section className="gallery-section" id="gallery" aria-labelledby="gallery-heading">
    <div className="gallery-heading">
      <div><span className="eyebrow">Inside the gym</span><h2 id="gallery-heading">Built for<br />the work.</h2></div>
      <p>A closer look at the spaces, sessions, and details that make every training day count.</p>
    </div>
    {items.length ? <div className="gallery-grid">{items.map((item, index) => <article className={`gallery-card gallery-card-${index % 5}`} key={item.id}>
      <img src={item.imageUrl} alt={item.title} />
      <div className="gallery-card-overlay"><span>{item.category || "Gym"}</span><h3>{item.title}</h3>{item.description ? <p>{item.description}</p> : null}</div>
    </article>)}</div> : <p className="gallery-empty">Our gallery is coming soon.</p>}
  </section>;
}

function ContactSection({ contact }: { contact: ContactContent | null }) {
  return <section className="contact-section" id="contact" aria-labelledby="contact-heading">
    <div className="contact-heading"><span className="eyebrow">Get in touch</span><h2 id="contact-heading">Let's get<br />you moving.</h2>{contact?.description ? <p>{contact.description}</p> : null}</div>
    {contact ? <div className="contact-layout"><div className="contact-details">
      {contact.gymName ? <div><small>Gym</small><strong>{contact.gymName}</strong></div> : null}
      {contact.address ? <div><small>Address</small><strong>{contact.address}</strong></div> : null}
      {contact.phone ? <div><small>Phone</small><a href={`tel:${contact.phone}`}>{contact.phone}</a></div> : null}
      {contact.email ? <div><small>Email</small><a href={`mailto:${contact.email}`}>{contact.email}</a></div> : null}
      {contact.openingHours ? <div><small>Opening Hours</small><strong>{contact.openingHours}</strong></div> : null}
    </div><div className="contact-actions">
      {contact.googleMapsUrl ? <a className="contact-button contact-button-primary" href={contact.googleMapsUrl} target="_blank" rel="noreferrer">Get directions <span>↗</span></a> : null}
      {contact.whatsAppNumber ? <a className="contact-button" href={`https://wa.me/${contact.whatsAppNumber.replace(/\D/g, "")}`}>WhatsApp <span>↗</span></a> : null}
      {(contact.instagramUrl || contact.facebookUrl) ? <div className="contact-socials">{contact.instagramUrl ? <a href={contact.instagramUrl} target="_blank" rel="noreferrer">Instagram</a> : null}{contact.facebookUrl ? <a href={contact.facebookUrl} target="_blank" rel="noreferrer">Facebook</a> : null}</div> : null}
    </div></div> : <p className="contact-empty">Contact information is coming soon.</p>}
  </section>;
}
