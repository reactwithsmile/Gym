import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { Permission } from "../types/auth";
import { NotificationsBell } from "../components/NotificationsBell";

const navItems = [
  { to: "/", label: "Dashboard", permission: Permission.DashboardView },
  { to: "/hero", label: "Hero", permission: Permission.HeroView },
  { to: "/about", label: "About", permission: Permission.AboutManage },
  { to: "/services", label: "Services", permission: Permission.ServicesManage },
  { to: "/trainers", label: "Trainers", permission: Permission.TrainersManage },
  { to: "/membership-plans", label: "Membership Plans", permission: Permission.MembershipManage },
  { to: "/gallery", label: "Gallery", permission: Permission.GalleryManage },
  { to: "/testimonials", label: "Testimonials", permission: Permission.TestimonialsManage },
  { to: "/contact", label: "Contact", permission: Permission.ContactManage },
  { to: "/settings", label: "Settings", permission: Permission.SettingsManage },
  { to: "/users", label: "Users", permission: Permission.UsersView },
  { to: "/fees", label: "Fees & Payments", permission: Permission.FeesView },
  { to: "/enquiries", label: "Enquiries", permission: Permission.EnquiriesView },
  { to: "/roles", label: "Roles & Permissions", permission: Permission.RolesManage },
];

export function AdminLayout() {
  const { hasPermission, logout, user } = useAuth();
  const navigate = useNavigate();
  const items = navItems.filter((item) => hasPermission(item.permission));

  function onLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <div className="admin-shell">
      <aside className="admin-nav">
        <p className="brand">Gym Admin</p>
        <nav>
          {items.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.to === "/"}>
              {item.label}
            </NavLink>
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
