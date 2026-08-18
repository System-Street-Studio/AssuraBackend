using Assura.Domain.Enums;

namespace Assura.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Procurement = "Procurement";
    public const string Maintenance = "Maintenance";
    public const string Superintendent = "Superintendent";
    public const string Storekeeper = "Storekeeper";
    public const string HR = "HR";
    public const string Employee = "Employee";
    public const string DivisionHead = "DivisionHead";
    public const string Accountant = "Accountant";
    public const string Auditor = "Auditor";
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Operational roles an HR user is permitted to assign to another user. Deliberately excludes
    /// Admin and SystemAdmin — those are privileged roles that must never be grantable through the
    /// HR role-assignment workflow, even though they are otherwise valid <see cref="UserRole"/> values.
    /// </summary>
    public static readonly IReadOnlySet<UserRole> HrAssignableRoles = new HashSet<UserRole>
    {
        UserRole.Procurement,
        UserRole.Maintenance,
        UserRole.Superintendent,
        UserRole.Storekeeper,
        UserRole.HR,
        UserRole.Employee,
        UserRole.DivisionHead,
        UserRole.Accountant,
        UserRole.Auditor
    };
}
