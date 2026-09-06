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
  HeroView: "hero.view",
  HeroCreate: "hero.create",
  HeroEdit: "hero.edit",
  HeroDelete: "hero.delete",
  HeroManage: "hero.view",
  AboutManage: "about.view",
  AboutView: "about.view",
  AboutCreate: "about.create",
  AboutEdit: "about.edit",
  AboutDelete: "about.delete",
  ServicesManage: "services.view",
  ServicesView: "services.view",
  ServicesCreate: "services.create",
  ServicesEdit: "services.edit",
  ServicesDelete: "services.delete",
  TrainersManage: "trainers.view",
  MembershipManage: "membership.view",
  GalleryManage: "gallery.view",
  TestimonialsManage: "testimonials.view",
  ContactManage: "contact.view",
  SettingsManage: "settings.view",
  RolesManage: "roles.view",
} as const;
