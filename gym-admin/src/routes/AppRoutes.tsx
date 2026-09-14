import { Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "../auth/ProtectedRoute";
import { AdminLayout } from "../layouts/AdminLayout";
import { AboutPage } from "../pages/About/AboutPage";
import { ContactPage } from "../pages/Contact/ContactPage";
import { DashboardPage } from "../pages/Dashboard/DashboardPage";
import { GalleryPage } from "../pages/Gallery/GalleryPage";
import { HeroPage } from "../pages/Hero/HeroPage";
import { LoginPage } from "../pages/Login/LoginPage";
import { MembershipPlansPage } from "../pages/MembershipPlans/MembershipPlansPage";
import { RolesPage } from "../pages/Roles/RolesPage";
import { ServicesPage } from "../pages/Services/ServicesPage";
import { SettingsPage } from "../pages/Settings/SettingsPage";
import { TestimonialsPage } from "../pages/Testimonials/TestimonialsPage";
import { TrainersPage } from "../pages/Trainers/TrainersPage";
import { UsersPage } from "../pages/Users/UsersPage";
import { FeesPage } from "../pages/Fees/FeesPage";
import { EnquiriesPage } from "../pages/Enquiries/EnquiriesPage";
import { ProductsPage } from "../pages/Products/ProductsPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AdminLayout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/hero" element={<HeroPage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/services" element={<ServicesPage />} />
          <Route path="/trainers" element={<TrainersPage />} />
          <Route path="/membership-plans" element={<MembershipPlansPage />} />
          <Route path="/gallery" element={<GalleryPage />} />
          <Route path="/testimonials" element={<TestimonialsPage />} />
          <Route path="/contact" element={<ContactPage />} />
          <Route path="/settings" element={<SettingsPage />} />
          <Route path="/roles" element={<RolesPage />} />
          <Route path="/users" element={<UsersPage />} />
          <Route path="/fees" element={<FeesPage />} />
          <Route path="/members" element={<FeesPage />} />
          <Route path="/enquiries" element={<EnquiriesPage />} />
          <Route path="/products" element={<ProductsPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
