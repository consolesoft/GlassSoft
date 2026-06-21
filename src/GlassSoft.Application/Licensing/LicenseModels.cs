namespace GlassSoft.Application.Licensing;

public sealed class LicenseOptions
{
    public const string SectionName = "License";
    public string LicenseFile { get; set; } = "App_Data/license.json";
    public string InstallationFile { get; set; } = "App_Data/installation.id";
    public string PublicKeyPem { get; set; } = string.Empty;
    public bool BypassInDevelopment { get; set; }
}

public sealed class LicensePayload
{
    public string LicenseId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string InstallationId { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public int MaxUsers { get; set; }
    public List<string> Modules { get; set; } = new();
}

public sealed class LicenseEnvelope
{
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public sealed class LicenseStatus
{
    public bool IsValid { get; init; }
    public bool IsDevelopmentBypass { get; init; }
    public string Message { get; init; } = string.Empty;
    public string InstallationId { get; init; } = string.Empty;
    public LicensePayload? License { get; init; }

    public bool HasModule(string module) => IsValid &&
        (IsDevelopmentBypass || License?.Modules.Contains("*", StringComparer.OrdinalIgnoreCase) == true ||
         License?.Modules.Contains(module, StringComparer.OrdinalIgnoreCase) == true);
}

public interface ILicenseService
{
    Task<LicenseStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<LicenseStatus> ValidateAsync(string licenseText, CancellationToken cancellationToken = default);
    Task<LicenseStatus> ActivateAsync(string licenseText, CancellationToken cancellationToken = default);
}
