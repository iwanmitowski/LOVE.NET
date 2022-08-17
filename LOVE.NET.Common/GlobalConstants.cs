namespace LOVE.NET.Common
{
    using System.IO;

    public static class GlobalConstants
    {
        public const string SystemName = "LOVE.NET";

        public const string AdministratorRoleName = "Administrator";

        public const string Data = "Data";

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
