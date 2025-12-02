using System;
using System.IO;
using System.Text.Json;

namespace FulbankApp.Services
{
    public class LocalAuth
    {
        public string Username { get; set; }
        public string PinHash { get; set; }
        public bool HasLoggedBefore { get; set; }

        private static string FilePath = "auth.json";

        public void Save()
        {
            string json = JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(FilePath, json);
        }

        public static LocalAuth Load()
        {
            if (!File.Exists(FilePath))
                return null;

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<LocalAuth>(json);
        }
    }
}
