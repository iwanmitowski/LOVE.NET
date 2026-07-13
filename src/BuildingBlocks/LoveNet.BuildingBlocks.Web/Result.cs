namespace LoveNet.BuildingBlocks.Web;

/// <summary>Lightweight success/error result carried up to controllers.</summary>
public class Result
{
    public bool Succeeded { get; private set; }

    public bool Failure => !Succeeded;

    public string[] Errors { get; private set; } = Array.Empty<string>();

    public static implicit operator Result(bool succeeded)
        => new() { Succeeded = succeeded };

    public static implicit operator Result(string error)
        => new() { Succeeded = false, Errors = new[] { error } };
}
