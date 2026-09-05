namespace GymApi.Auth;

public static class PermissionCodes
{
    public const string DashboardView = "dashboard.view";
    public const string HeroManage = "hero.manage";
    public const string AboutManage = "about.manage";
    public const string ServicesManage = "services.manage";
    public const string TrainersManage = "trainers.manage";
    public const string MembershipManage = "membership.manage";
    public const string GalleryManage = "gallery.manage";
    public const string TestimonialsManage = "testimonials.manage";
    public const string ContactManage = "contact.manage";
    public const string SettingsManage = "settings.manage";
    public const string RolesManage = "roles.manage";

    public static readonly string[] All =
    [
        DashboardView,
        HeroManage,
        AboutManage,
        ServicesManage,
        TrainersManage,
        MembershipManage,
        GalleryManage,
        TestimonialsManage,
        ContactManage,
        SettingsManage,
        RolesManage
    ];
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Trainer = "Trainer";
}
