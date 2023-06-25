namespace DocuRISE.Common
{
    public class GlobalConstants
    {
        // Roles
        public const string FacilityManagerRoleName = "FacilityManager";

        public const string CompanyManagerRoleName = "CompanyManager";

        public const string FacilityAccountantRoleName = "FacilityAccountant";

        public const string CompanyStaffRoleName = "CompanyStaff";

        public const string BothManagerRoles = $"{FacilityManagerRoleName}, {CompanyManagerRoleName}";
        public const string FacilityManagerAndAccountantRoles = $"{FacilityManagerRoleName}, {FacilityAccountantRoleName}";

        // Facility Manager
        public const string FacilityManagerFirstName = "Nicoleta";
        public const string FacilityManagerLastName = "Valcheva";
        public const string FacilityManagerCompanyName = "Nemetschek Bulgaria";
        public const string FacilityManagerEmail = "FacilityManager@mail.bg";
        public const string FacilityManagerPassword = "Admin123#";
    }
}
