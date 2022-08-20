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
    }
}
