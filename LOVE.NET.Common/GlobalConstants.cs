namespace LOVE.NET.Common
{
    using System.IO;

    public static class GlobalConstants
    {
        public const string SystemName = "LOVE.NET";

        public const string AdministratorRoleName = "Administrator";

        public const string Data = "Data";

        public const string JWT = "JWT";

        public const string AuthSettingsKey = "AuthSettings:Key";

        public const string AuthSettingsIssuer = "AuthSettings:Issuer";

        public const string AuthSettingsAudience = "AuthSettings:Audience";

        public class JWTSecurityScheme
        {
            public const string JWTScheme = "bearer";
            public const string JWTName = "JWT Authentication";
            public const string JWTDescription = "Put **_ONLY_** your JWT Bearer token on textbox below!";
        }

        public class FilePaths
        {
            public const string OneDirectoryUp = "..";

            public const string SystemNameData = $"{SystemName}.{Data}";

            public const string Files = "Files";

            public const string CitiesWithCountriesFileName = "CitiesWithCountries.csv";

            public const string CountriesFileName = "Countries.csv";
        }

        public class ControllerRoutesConstants
        {
            public const string Api = "api/";

            public const string IdentityControllerName = Api + "identity";
            public const string RegisterRoute = "register";
        }

        public class ControllerResponseMessages
        {
            public const string EmailAlreadyInUse = "Email already in use";
            public const string PasswordsDontMatch = "Password and confirm password must match";
            public const string LogoutSuccessfully = "Logout successfully";
            public const string FillAllTheInformation = "Fill all the information";
            public const string UserNotFound = "User not found";
            public const string WrongPassword = "Wrong password";
            public const string WrongOldPassword = "Old password is invalid";
            public const string ChangePasswordsDontMatch = "New password and confirm password must match";
            public const string PasswordChangedSuccessfully = "Password changed successfully";
        }
    }
}
