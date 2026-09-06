import { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";

const navItems = [
  { to: "/", label: "Home" },
  { to: "/about", label: "About" },
  { to: "/services", label: "Services" },
  { to: "/trainers", label: "Trainers" },
  { to: "/membership", label: "Membership" },
  { to: "/gallery", label: "Gallery" },
  { to: "/testimonials", label: "Testimonials" },
  { to: "/contact", label: "Contact" },
];

export function PublicLayout() {
  const [isScrolled, setIsScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setIsScrolled(window.scrollY > 24);
    onScroll();
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <div className="site-shell">
      <header className={`site-header${isScrolled ? " site-header-scrolled" : ""}`}>
        <NavLink className="brand" to="/" onClick={() => setMenuOpen(false)}>
          <span className="brand-mark">G</span>
          <span>Gym<span className="brand-accent">.</span></span>
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
    </div>
  );
}
