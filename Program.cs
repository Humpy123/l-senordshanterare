using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace lösenordshanterare
{
    internal class Program
    
    {
        static string GetPassword()
        {
            Console.WriteLine("Enter password: ");
            string password = Console.ReadLine();
            return password;
        }

        static void Main(string[] args)
        {
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);

            client Client = new client();
            server Server = new server();

            Server.GenerateIV();
            Client.generateSecret();
            string masterPassword = GetPassword();

            string path = @"files/client.json";
            string json = File.ReadAllText(path);
            client c = JsonSerializer.Deserialize<client>(json);
            string secretKey = c.SecretKey;
            byte[] secretKeyBytes = Encoding.ASCII.GetBytes(secretKey);

            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(masterPassword, secretKeyBytes);

            path = @"files/server.json";
            json = File.ReadAllText(path);
            server s = JsonSerializer.Deserialize<server>(json);
            byte[] IV = Convert.FromBase64String(s.IV);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = vaultKey.GetBytes(16);
                aesAlg.IV = IV;

                // Create an encryptor from the AES instance
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Encrypt data (let's say we want to encrypt the master password or any other string)
                string dataToEncrypt = "hej";  // You can change this to any data you want to encrypt
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(dataToEncrypt);
                        }
                    }

                    byte[] encrypted = msEncrypt.ToArray();

                    // Convert the encrypted data to base64 and output it
                    string encryptedBase64 = Convert.ToBase64String(encrypted);
                    Console.WriteLine($"Encrypted data (Base64): {encryptedBase64}");

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted data
                                string decryptedData = srDecrypt.ReadToEnd();
                                Console.Write(decryptedData);
                            }
                        }
                    }
                }
            }
        }
    }
}
