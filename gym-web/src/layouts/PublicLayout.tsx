import { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";

type GymSettings = { gymName: string; logoUrl: string; tagline: string; websiteUrl: string; currency: string };

const navItems = [
  { to: "/", label: "Home" },
  { to: "/about", label: "About" },
  { to: "/services", label: "Services" },
  { to: "/products", label: "Products" },
  { to: "/trainers", label: "Trainers" },
  { to: "/membership", label: "Membership" },
  { to: "/gallery", label: "Gallery" },
  { to: "/testimonials", label: "Testimonials" },
  { to: "/contact", label: "Contact" },
];

export function PublicLayout() {
  const [isScrolled, setIsScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [settings, setSettings] = useState<GymSettings>({ gymName: "Gym", logoUrl: "", tagline: "", websiteUrl: "", currency: "INR" });

  useEffect(() => {
    const onScroll = () => setIsScrolled(window.scrollY > 24);
    onScroll();
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  useEffect(() => {
    fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5182"}/api/gym-settings`)
      .then(async (response) => {
        if (!response.ok) throw new Error("Gym settings unavailable");
        return await response.json() as GymSettings;
      })
      .then((data) => setSettings({ gymName: data.gymName || "Gym", logoUrl: data.logoUrl || "", tagline: data.tagline || "", websiteUrl: data.websiteUrl || "", currency: data.currency || "INR" }))
      .catch(() => undefined);
  }, []);

  return (
    <div className="site-shell">
      <header className={`site-header${isScrolled ? " site-header-scrolled" : ""}`}>
        <NavLink className="brand" to="/" onClick={() => setMenuOpen(false)}>
          {settings.logoUrl ? <img className="brand-logo" src={settings.logoUrl} alt="" /> : <span className="brand-mark">{settings.gymName.charAt(0).toUpperCase()}</span>}
          <span>{settings.gymName}<span className="brand-accent">.</span></span>
        </NavLink>
        <button
          className="menu-toggle"
          type="button"
          aria-label={menuOpen ? "Close navigation" : "Open navigation"}
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((open) => !open)}
        >
          <span />
          <span />
        </button>
        <nav className={menuOpen ? "site-nav-open" : ""}>
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.to === "/"} onClick={() => setMenuOpen(false)}>
              {item.label}
            </NavLink>
          ))}
          <NavLink className="header-join" to="/membership" onClick={() => setMenuOpen(false)}>
            Join now <span>↗</span>
          </NavLink>
        </nav>
      </header>
      <Outlet />
      <footer className="site-footer">
        <div>
          <strong>{settings.gymName}</strong>
          {settings.tagline ? <p>{settings.tagline}</p> : null}
        </div>
        {settings.websiteUrl ? <a href={settings.websiteUrl} target="_blank" rel="noreferrer">Visit website <span>↗</span></a> : null}
      </footer>
    </div>
  );
}
