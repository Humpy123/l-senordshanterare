using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace lösenordshanterare
{
    internal class client
    {
        public string SecretKey { get; set; }

        public void generateSecret()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[32]; // You can choose the length you need
                rng.GetBytes(randomBytes); // Fill the byte array with random bytes

                SecretKey = Convert.ToBase64String(randomBytes);
                string json = JsonSerializer.Serialize(this);
                string path = @"files\client.json";
                File.WriteAllText(path, json);
                Console.WriteLine();
            }
        }

        public client(string newPath)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[32]; // You can choose the length you need
                rng.GetBytes(randomBytes); // Fill the byte array with random bytes

                SecretKey = Convert.ToBase64String(randomBytes);
                string json = JsonSerializer.Serialize(this);
                string path = @newPath;
                File.WriteAllText(path, json);
                Console.WriteLine();
            }
        }

        public client() { }
    }
}
