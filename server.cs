using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace lösenordshanterare
{
    internal class server
    {
        public string PasswordVault { get; set; }
        public string IV { get; set; }

        public void GenerateIV()
        {
            using (Aes aesAlg = Aes.Create())
            {
                string ivBase64 = Convert.ToBase64String(aesAlg.IV);
                IV = ivBase64;
                string json = JsonSerializer.Serialize(this);
                string path = @"C:\Users\olive\source\repos\lösenordshanterare\lösenordshanterare\files\server.json";
                File.WriteAllText(path, json);
                Console.WriteLine();
            }
        }
    }
}
