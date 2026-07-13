namespace LoveNet.Profile.Api.Data;

public class Country
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();
}
