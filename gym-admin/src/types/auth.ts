export type AuthUser = {
  id: number;
  email: string;
  role: string;
  permissions: string[];
};

export type LoginResponse = {
  token: string;
  user: AuthUser;
};

export const Permission = {
  DashboardView: "dashboard.view",
  HeroManage: "hero.manage",
  AboutManage: "about.manage",
  ServicesManage: "services.manage",
  TrainersManage: "trainers.manage",
  MembershipManage: "membership.manage",
  GalleryManage: "gallery.manage",
  TestimonialsManage: "testimonials.manage",
  ContactManage: "contact.manage",
  SettingsManage: "settings.manage",
  RolesManage: "roles.manage",
} as const;
