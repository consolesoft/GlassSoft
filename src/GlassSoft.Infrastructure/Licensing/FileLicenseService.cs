using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GlassSoft.Application.Licensing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace GlassSoft.Infrastructure.Licensing;

public sealed class FileLicenseService : ILicenseService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly LicenseOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "GlassSoft.License.Status";

    public FileLicenseService(IOptions<LicenseOptions> options, IHostEnvironment environment, IMemoryCache cache)
    {
        _options = options.Value;
        _environment = environment;
        _cache = cache;
    }

    public async Task<LicenseStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var installationId = await GetInstallationIdAsync(cancellationToken);
        if (_environment.IsDevelopment() && _options.BypassInDevelopment)
            return new LicenseStatus
            {
                IsValid = true,
                IsDevelopmentBypass = true,
                InstallationId = installationId,
                Message = "Geliştirme ortamı lisans geçişi etkin."
            };

        var path = ResolvePath(_options.LicenseFile);
        if (!File.Exists(path))
            return Invalid("Lisans dosyası bulunamadı.", installationId);

        var lastWriteUtc = File.GetLastWriteTimeUtc(path);
        if (_cache.TryGetValue(CacheKey, out CachedLicenseStatus? cached) && cached?.LastWriteUtc == lastWriteUtc)
            return cached.Status;

        var status = await ValidateAsync(await File.ReadAllTextAsync(path, cancellationToken), cancellationToken);
        var cacheUntil = status.License?.ExpiresAtUtc < DateTime.UtcNow.AddMinutes(5)
            ? status.License.ExpiresAtUtc
            : DateTime.UtcNow.AddMinutes(5);
        if (cacheUntil > DateTime.UtcNow)
            _cache.Set(CacheKey, new CachedLicenseStatus(lastWriteUtc, status), cacheUntil);
        return status;
    }

    public async Task<LicenseStatus> ValidateAsync(string licenseText, CancellationToken cancellationToken = default)
    {
        var installationId = await GetInstallationIdAsync(cancellationToken);
        try
        {
            var envelope = JsonSerializer.Deserialize<LicenseEnvelope>(licenseText, JsonOptions);
            if (envelope == null || string.IsNullOrWhiteSpace(envelope.Payload) || string.IsNullOrWhiteSpace(envelope.Signature))
                return Invalid("Lisans paketi biçimi geçersiz.", installationId);

            var payloadBytes = Convert.FromBase64String(envelope.Payload);
            var signatureBytes = Convert.FromBase64String(envelope.Signature);
            if (string.IsNullOrWhiteSpace(_options.PublicKeyPem))
                return Invalid("Lisans public key yapılandırılmamış.", installationId);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(_options.PublicKeyPem);
            if (!rsa.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return Invalid("Lisans imzası doğrulanamadı.", installationId);

            var payload = JsonSerializer.Deserialize<LicensePayload>(payloadBytes, JsonOptions);
            if (payload == null || string.IsNullOrWhiteSpace(payload.LicenseId))
                return Invalid("Lisans içeriği geçersiz.", installationId);
            if (!string.Equals(payload.InstallationId, installationId, StringComparison.OrdinalIgnoreCase))
                return Invalid("Lisans bu kurulum için üretilmemiş.", installationId, payload);
            if (payload.IssuedAtUtc > DateTime.UtcNow.AddMinutes(5))
                return Invalid("Lisans başlangıç tarihi henüz gelmemiş.", installationId, payload);
            if (payload.ExpiresAtUtc <= DateTime.UtcNow)
                return Invalid("Lisans süresi dolmuş.", installationId, payload);
            if (payload.MaxUsers < 1)
                return Invalid("Lisans kullanıcı limiti geçersiz.", installationId, payload);

            return new LicenseStatus
            {
                IsValid = true,
                InstallationId = installationId,
                License = payload,
                Message = "Lisans geçerli."
            };
        }
        catch (Exception ex) when (ex is JsonException or FormatException or CryptographicException)
        {
            return Invalid("Lisans paketi okunamadı veya bozuk.", installationId);
        }
    }

    public async Task<LicenseStatus> ActivateAsync(string licenseText, CancellationToken cancellationToken = default)
    {
        var status = await ValidateAsync(licenseText, cancellationToken);
        if (!status.IsValid) return status;

        var path = ResolvePath(_options.LicenseFile);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, licenseText, Encoding.UTF8, cancellationToken);
        File.Move(temporaryPath, path, true);
        _cache.Remove(CacheKey);
        return status;
    }

    private async Task<string> GetInstallationIdAsync(CancellationToken cancellationToken)
    {
        var path = ResolvePath(_options.InstallationFile);
        if (File.Exists(path)) return (await File.ReadAllTextAsync(path, cancellationToken)).Trim();

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var id = Guid.NewGuid().ToString("N");
        await File.WriteAllTextAsync(path, id, cancellationToken);
        return id;
    }

    private string ResolvePath(string path) => Path.IsPathRooted(path) ? path : Path.Combine(_environment.ContentRootPath, path);

    private static LicenseStatus Invalid(string message, string installationId, LicensePayload? payload = null) => new()
    {
        IsValid = false,
        Message = message,
        InstallationId = installationId,
        License = payload
    };

    private sealed record CachedLicenseStatus(DateTime LastWriteUtc, LicenseStatus Status);
}
