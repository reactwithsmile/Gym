import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { Permission } from "../types/auth";
import { NotificationsBell } from "../components/NotificationsBell";

const navItems = [
  { to: "/", label: "Dashboard", group: "Overview", permission: Permission.DashboardView },
  { to: "/members", label: "Members", group: "Gym operations", permission: Permission.MembersView },
  { to: "/fees", label: "Payments & Fees", group: "Gym operations", permission: Permission.FeesView },
  { to: "/enquiries", label: "Enquiries", group: "Gym operations", permission: Permission.EnquiriesView },
  { to: "/membership-plans", label: "Membership Plans", group: "Gym operations", permission: Permission.MembershipManage },
  { to: "/products", label: "Products", group: "Gym operations", permission: Permission.ProductsView },
  { to: "/hero", label: "Hero", group: "Website content", permission: Permission.HeroView },
  { to: "/about", label: "About", group: "Website content", permission: Permission.AboutManage },
  { to: "/services", label: "Services", group: "Website content", permission: Permission.ServicesManage },
  { to: "/trainers", label: "Trainers", group: "Website content", permission: Permission.TrainersManage },
  { to: "/gallery", label: "Gallery", group: "Website content", permission: Permission.GalleryManage },
  { to: "/testimonials", label: "Testimonials", group: "Website content", permission: Permission.TestimonialsManage },
  { to: "/contact", label: "Contact", group: "Website content", permission: Permission.ContactManage },
  { to: "/settings", label: "Settings", group: "Administration", permission: Permission.SettingsManage },
  { to: "/users", label: "Users", group: "Administration", permission: Permission.UsersView },
  { to: "/roles", label: "Roles & Permissions", group: "Administration", permission: Permission.RolesManage },
];

export function AdminLayout() {
  const { hasPermission, logout, user } = useAuth();
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const items = navItems.filter((item) => hasPermission(item.permission));

  function onLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <div className={`admin-shell${collapsed ? " sidebar-collapsed" : ""}`}>
      <aside className="admin-nav">
        <div className="brand-row"><p className="brand">Gym Admin</p><button className="sidebar-toggle" type="button" onClick={() => setCollapsed((value) => !value)} aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}>{collapsed ? "»" : "«"}</button></div>
        <nav>
          {items.map((item, index) => (
            <div key={item.to}>
              {(index === 0 || item.group !== items[index - 1].group) ? <p className="nav-group-label">{item.group}</p> : null}
              <NavLink to={item.to} end={item.to === "/"} data-short={item.label.charAt(0)}>{item.label}</NavLink>
            </div>
          ))}
        </nav>
      </aside>
      <div className="admin-content">
        <header className="admin-topbar">
          <div>
            <span className="topbar-kicker">GYM MANAGEMENT</span>
            <strong>Admin workspace</strong>
          </div>
          <div className="topbar-actions">
            <NotificationsBell />
            <span className="topbar-user">{user?.email}</span>
            <button className="btn-link" type="button" onClick={onLogout}>Log out</button>
          </div>
        </header>
        <Outlet />
      </div>
    </div>
  );
}
