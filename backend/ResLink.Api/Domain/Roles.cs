namespace ResLink.Api.Domain;

public static class Roles
{
    public const string Student = "Student";
    public const string Admin = "Admin";
    public const string Security = "Security";
    public const string Maintenance = "Maintenance";

    public static readonly string[] All = [Student, Admin, Security, Maintenance];
    public static readonly string[] Staff = [Admin, Security, Maintenance];
}
