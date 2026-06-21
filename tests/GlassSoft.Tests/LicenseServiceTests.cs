using System.Security.Cryptography;
using System.Text.Json;
using GlassSoft.Application.Licensing;
using GlassSoft.Infrastructure.Licensing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GlassSoft.Tests;

public class LicenseServiceTests
{
    [Fact]
    public async Task SignedLicense_IsAccepted_AndTamperedPayloadIsRejected()
    {
        var root = Path.Combine(Path.GetTempPath(), "glasssoft-license-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var rsa = RSA.Create(2048);
            var options = new LicenseOptions
            {
                InstallationFile = "installation.id",
                LicenseFile = "license.json",
                PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem()
            };
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new FileLicenseService(Options.Create(options), new TestHostEnvironment(root), cache);

            await service.GetStatusAsync();
            var installationId = (await File.ReadAllTextAsync(Path.Combine(root, "installation.id"))).Trim();
            var payload = new LicensePayload
            {
                LicenseId = "TEST-1",
                CustomerName = "Test",
                InstallationId = installationId,
                IssuedAtUtc = DateTime.UtcNow.AddMinutes(-1),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
                MaxUsers = 5,
                Modules = ["*"]
            };
            var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var envelope = new LicenseEnvelope
            {
                Payload = Convert.ToBase64String(payloadBytes),
                Signature = Convert.ToBase64String(rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            };

            var valid = await service.ValidateAsync(JsonSerializer.Serialize(envelope));
            Assert.True(valid.IsValid, valid.Message);

            payloadBytes[0] ^= 1;
            envelope.Payload = Convert.ToBase64String(payloadBytes);
            var tampered = await service.ValidateAsync(JsonSerializer.Serialize(envelope));
            Assert.False(tampered.IsValid);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private sealed class TestHostEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "GlassSoft.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
