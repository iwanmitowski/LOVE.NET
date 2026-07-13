namespace LoveNet.Notifications.Api.Services;

public interface IEmailTemplateProvider
{
    Task<string> GetTemplateAsync(string fileName, CancellationToken cancellationToken = default);
}

/// <summary>Reads templates from the Templates folder next to the binaries.</summary>
public class FileEmailTemplateProvider : IEmailTemplateProvider
{
    public Task<string> GetTemplateAsync(string fileName, CancellationToken cancellationToken = default) =>
        File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Templates", fileName), cancellationToken);
}
