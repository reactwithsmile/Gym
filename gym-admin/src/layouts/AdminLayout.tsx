import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { Permission } from "../types/auth";

const navItems = [
  { to: "/", label: "Dashboard", permission: Permission.DashboardView },
  { to: "/hero", label: "Hero", permission: Permission.HeroManage },
  { to: "/about", label: "About", permission: Permission.AboutManage },
  { to: "/services", label: "Services", permission: Permission.ServicesManage },
  { to: "/trainers", label: "Trainers", permission: Permission.TrainersManage },
  { to: "/membership-plans", label: "Membership Plans", permission: Permission.MembershipManage },
  { to: "/gallery", label: "Gallery", permission: Permission.GalleryManage },
  { to: "/testimonials", label: "Testimonials", permission: Permission.TestimonialsManage },
  { to: "/contact", label: "Contact", permission: Permission.ContactManage },
  { to: "/settings", label: "Settings", permission: Permission.SettingsManage },
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
        <div className="admin-user">
          <p>{user?.email}</p>
          <button className="btn-link" type="button" onClick={onLogout}>
            Log out
          </button>
        </div>
      </aside>
      <div className="admin-content">
        <Outlet />
      </div>
    </div>
  );
}
