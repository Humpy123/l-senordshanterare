using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace losenordshanterare
{
    internal class client
    {
        public string SecretKey { get; set; }

        public void generateSecret(string path)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[32]; // You can choose the length you need
                rng.GetBytes(randomBytes); // Fill the byte array with random bytes

                SecretKey = Convert.ToBase64String(randomBytes);
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(path, json);
                Console.WriteLine();
            }
        }

        public client(string path, string existingSecretKey = null)
        {
            if (!string.IsNullOrEmpty(existingSecretKey))
            {
                // Use the provided existing SecretKey
                SecretKey = existingSecretKey;
            }
            else
            {
                // Generate a new random SecretKey if none provided
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    byte[] randomBytes = new byte[32];
                    rng.GetBytes(randomBytes);
                    SecretKey = Convert.ToBase64String(randomBytes);
                }
            }

            // Save the client object to the specified path
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(path, json);
        }

        public client() { }
    }
}
