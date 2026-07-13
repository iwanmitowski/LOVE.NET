namespace LoveNet.Profile.Api.Data;

public class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string NameAscii { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int CountryId { get; set; }
}
