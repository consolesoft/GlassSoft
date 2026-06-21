using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };

if (args.Length == 3 && args[0].Equals("init", StringComparison.OrdinalIgnoreCase))
{
    using var rsa = RSA.Create(3072);
    await File.WriteAllTextAsync(args[1], rsa.ExportPkcs8PrivateKeyPem());
    await File.WriteAllTextAsync(args[2], rsa.ExportSubjectPublicKeyInfoPem());
    Console.WriteLine($"Anahtar çifti üretildi. Private key'i güvenli ve uygulamadan ayrı tutun: {args[1]}");
    return;
}

if (args.Length == 9 && args[0].Equals("issue", StringComparison.OrdinalIgnoreCase))
{
    var privateKeyPath = args[1];
    var installationId = args[2];
    var customerName = args[3];
    if (!int.TryParse(args[4], out var validityDays) || validityDays < 1 ||
        !int.TryParse(args[5], out var maxUsers) || maxUsers < 1)
        throw new ArgumentException("Gün ve kullanıcı limiti pozitif sayı olmalıdır.");

    var modules = args[6].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var licenseId = args[7];
    var outputPath = args[8];
    var issuedAt = DateTime.UtcNow;
    var payload = new
    {
        licenseId,
        customerName,
        installationId,
        issuedAtUtc = issuedAt,
        expiresAtUtc = issuedAt.AddDays(validityDays),
        maxUsers,
        modules
    };
    var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, jsonOptions);

    using var rsa = RSA.Create();
    rsa.ImportFromPem(await File.ReadAllTextAsync(privateKeyPath));
    var signature = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var envelope = new
    {
        payload = Convert.ToBase64String(payloadBytes),
        signature = Convert.ToBase64String(signature)
    };
    await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(envelope, jsonOptions), Encoding.UTF8);
    Console.WriteLine($"Lisans üretildi: {outputPath}");
    return;
}

Console.WriteLine("Kullanım:");
Console.WriteLine("  init <private.pem> <public.pem>");
Console.WriteLine("  issue <private.pem> <installationId> <customer> <days> <maxUsers> <modulesCsv> <licenseId> <output.json>");
Environment.ExitCode = 1;
