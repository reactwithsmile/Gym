import { Route, Routes } from "react-router-dom";
import { PublicLayout } from "../layouts/PublicLayout";
import { AboutPage } from "../pages/About/AboutPage";
import { ContactPage } from "../pages/Contact/ContactPage";
import { GalleryPage } from "../pages/Gallery/GalleryPage";
import { HomePage } from "../pages/Home/HomePage";
import { MembershipPage } from "../pages/Membership/MembershipPage";
import { ServicesPage } from "../pages/Services/ServicesPage";
import { TestimonialsPage } from "../pages/Testimonials/TestimonialsPage";
import { TrainersPage } from "../pages/Trainers/TrainersPage";
import { ProductsPage } from "../pages/Products/ProductsPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<PublicLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/services" element={<ServicesPage />} />
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/trainers" element={<TrainersPage />} />
        <Route path="/membership" element={<MembershipPage />} />
        <Route path="/gallery" element={<GalleryPage />} />
        <Route path="/testimonials" element={<TestimonialsPage />} />
        <Route path="/contact" element={<ContactPage />} />
      </Route>
    </Routes>
  );
}
