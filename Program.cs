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
     /*   static string GetPassword()
        {
            Console.WriteLine("Enter a master password: ");
            string password = Console.ReadLine();
            return password;
        }*/

        static void PrintMenu()
        {
            Console.WriteLine("Commands:\n");

            Console.WriteLine("init - create a new vault\n");

            Console.WriteLine("create - Create a new client file (e.g., on another device) to an already existing vault.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: create < client > < server > { < pwd >} { < secret >\n");
            Console.ResetColor();

            Console.WriteLine("get - Show stored values for some property or list properties in vault.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: get < client > < server > [ < prop >] { < pwd >}\n");
            Console.ResetColor();

            Console.WriteLine("set - Store value for some ( possibly new ) property in vault.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: set < client > < server > < prop > [ - g ] { < pwd >} { < value >}\n");
            Console.ResetColor();

            Console.WriteLine("delete - Drop some property from vault.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: delete < client > < server > < prop > { < pwd >}\n");
            Console.ResetColor();

            Console.WriteLine("secret - Show secret key.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: secret < client >\n");
            Console.ResetColor();


            Console.WriteLine("change - Change the master password");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("syntax: change < client > < server > { < pwd >} { < new_pwd >}");
            Console.ResetColor();
        }

        static void Main(string[] args)
        {
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);

            client Client = new client();
            server Server = new server();

            Server.GenerateIV();
            Client.generateSecret();

            string path = @"files/client.json";
            string json = File.ReadAllText(path);
            client c = JsonSerializer.Deserialize<client>(json);
            string secretKey = c.SecretKey;
            byte[] secretKeyBytes = Encoding.ASCII.GetBytes(secretKey);

            Console.WriteLine("Enter a master password: ");
            Rfc2898DeriveBytes vaultKey = new Rfc2898DeriveBytes(Console.ReadLine(), secretKeyBytes, 1000);

            path = @"files/server.json";
            json = File.ReadAllText(path);
            server s = JsonSerializer.Deserialize<server>(json);
            byte[] IV = Convert.FromBase64String(s.IV);

            Console.Clear();
            Console.WriteLine("Succesfully logged in\n\n");
            PrintMenu();      

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
