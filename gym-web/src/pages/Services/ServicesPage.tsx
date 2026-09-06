import { useEffect, useState } from "react";

type ServiceContent = {
  id: number;
  name: string;
  shortDescription: string;
  description: string;
  imageUrl: string;
  icon: string;
  displayOrder: number;
};

const apiUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5182";

export function ServicesPage() {
  const [services, setServices] = useState<ServiceContent[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let isCurrent = true;
    fetch(`${apiUrl}/api/services`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Services unavailable");
        return (await response.json()) as ServiceContent[];
      })
      .then((content) => {
        if (isCurrent) setServices(content);
      })
      .catch(() => {
        if (isCurrent) setServices([]);
      })
      .finally(() => {
        if (isCurrent) setLoading(false);
      });

    return () => {
      isCurrent = false;
    };
  }, []);

  return (
    <main className="services-page">
      <section className="services-section services-route-section" aria-labelledby="services-page-heading">
        <div className="services-heading">
          <div>
            <span className="eyebrow">What we offer</span>
            <h1 id="services-page-heading">Train with purpose.</h1>
          </div>
          <p>Focused programs designed to make you stronger, fitter, and more confident.</p>
        </div>
        {loading ? <p className="services-empty">Loading services…</p> : services.length ? (
          <div className="services-grid">
            {services.map((service) => (
              <article className="service-card" key={service.id}>
                {service.imageUrl ? (
                  <div className="service-image"><img src={service.imageUrl} alt={service.name} /></div>
                ) : (
                  <div className="service-image service-image-empty">{service.icon || "＋"}</div>
                )}
                <div className="service-card-content">
                  <span className="service-index">{String(service.displayOrder).padStart(2, "0")}</span>
                  <h2>{service.name}</h2>
                  <p>{service.shortDescription}</p>
                  {service.description ? <small>{service.description}</small> : null}
                </div>
              </article>
            ))}
          </div>
        ) : <p className="services-empty">Our training programs are coming soon.</p>}
      </section>
    </main>
  );
}
