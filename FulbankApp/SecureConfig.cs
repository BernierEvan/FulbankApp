using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class SecureConfig
{
    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "db.secure");

    public static string GetConnectionString()
    {
        if (!File.Exists(ConfigPath))
            throw new FileNotFoundException("Le fichier sécurisé db.secure est introuvable : " + ConfigPath);

        byte[] encrypted = File.ReadAllBytes(ConfigPath);

        byte[] decrypted = ProtectedData.Unprotect(
            encrypted,
            null,
            DataProtectionScope.LocalMachine
        );

        return Encoding.UTF8.GetString(decrypted);
    }
}
