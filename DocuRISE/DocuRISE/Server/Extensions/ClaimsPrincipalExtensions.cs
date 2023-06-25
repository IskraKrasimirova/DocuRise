using System.Security.Claims;
using static DocuRISE.Common.GlobalConstants;

namespace DocuRISE.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        public static bool IsFacilityManager(this ClaimsPrincipal user)
        {
            return user.IsInRole(FacilityManagerRoleName);
        }

        public static bool IsCompanyManager(this ClaimsPrincipal user)
        {
            return user.IsInRole(CompanyManagerRoleName);
        }

        public static bool IsFacilityAccountant(this ClaimsPrincipal user)
        {
            return user.IsInRole(FacilityAccountantRoleName);
        }

        public static bool IsCompanyStaff(this ClaimsPrincipal user)
        {
            return user.IsInRole(CompanyStaffRoleName);
        }
    }
}
