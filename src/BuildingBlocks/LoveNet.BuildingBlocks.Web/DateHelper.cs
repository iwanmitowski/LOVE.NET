namespace LoveNet.BuildingBlocks.Web;

public static class DateHelper
{
    public static int CalculateAge(DateTime birthdate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthdate.Year;

        if (birthdate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
