namespace LOVE.NET.Web.Common.Helpers
{
    using System;

    public static class DateHelper
    {
        public static int AgeCalculator(DateTime birthdate)
        {
            var today = DateTime.UtcNow;

            var age = today.Year - birthdate.Year;

            // Leap years calculation
            if (birthdate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
