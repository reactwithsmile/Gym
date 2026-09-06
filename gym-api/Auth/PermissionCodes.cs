namespace GymApi.Auth;

public static class PermissionCodes
{
    public const string DashboardView = "dashboard.view";

    public const string HeroView = "hero.view";
    public const string HeroCreate = "hero.create";
    public const string HeroEdit = "hero.edit";
    public const string HeroDelete = "hero.delete";

    public const string AboutView = "about.view";
    public const string AboutCreate = "about.create";
    public const string AboutEdit = "about.edit";
    public const string AboutDelete = "about.delete";

    public const string ServicesView = "services.view";
    public const string ServicesCreate = "services.create";
    public const string ServicesEdit = "services.edit";
    public const string ServicesDelete = "services.delete";

    public const string TrainersView = "trainers.view";
    public const string TrainersCreate = "trainers.create";
    public const string TrainersEdit = "trainers.edit";
    public const string TrainersDelete = "trainers.delete";

    public const string MembershipView = "membership.view";
    public const string MembershipCreate = "membership.create";
    public const string MembershipEdit = "membership.edit";
    public const string MembershipDelete = "membership.delete";

    public const string GalleryView = "gallery.view";
    public const string GalleryCreate = "gallery.create";
    public const string GalleryEdit = "gallery.edit";
    public const string GalleryDelete = "gallery.delete";

    public const string TestimonialsView = "testimonials.view";
    public const string TestimonialsCreate = "testimonials.create";
    public const string TestimonialsEdit = "testimonials.edit";
    public const string TestimonialsDelete = "testimonials.delete";

    public const string ContactView = "contact.view";
    public const string ContactEdit = "contact.edit";

    public const string SettingsView = "settings.view";
    public const string SettingsEdit = "settings.edit";

    public const string RolesView = "roles.view";
    public const string RolesEdit = "roles.edit";

    public static readonly string[] All =
    [
        DashboardView,
        HeroView, HeroCreate, HeroEdit, HeroDelete,
        AboutView, AboutCreate, AboutEdit, AboutDelete,
        ServicesView, ServicesCreate, ServicesEdit, ServicesDelete,
        TrainersView, TrainersCreate, TrainersEdit, TrainersDelete,
        MembershipView, MembershipCreate, MembershipEdit, MembershipDelete,
        GalleryView, GalleryCreate, GalleryEdit, GalleryDelete,
        TestimonialsView, TestimonialsCreate, TestimonialsEdit, TestimonialsDelete,
        ContactView, ContactEdit,
        SettingsView, SettingsEdit,
        RolesView, RolesEdit
    ];
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Trainer = "Trainer";
}
