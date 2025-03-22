using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace lösenordshanterare
{
    internal class Program
    
    {
        static void Main(string[] args)
        {

            //files/client.json files/server.json
            if (args[0] == "init")
            {
                string clientPath = args[1];
                string serverPath = args[2];

                if (!File.Exists(clientPath))
                {
                    var newClient = new client(clientPath);
                    File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
                }

                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.Write("Write your password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);

                Console.WriteLine(Convert.ToBase64String(secretKey));
                Console.WriteLine(Convert.ToBase64String(vaultKey));

                Dictionary<string, string> vaultData = new Dictionary<string, string>();
                vaultData.Add("hej", "då");

                string updatedVaultJson = JsonSerializer.Serialize(vaultData);
                byte[] updatedVault = CryptoHelper.Encrypt(updatedVaultJson, vaultKey, iv);
                serv.PasswordVault = Convert.ToBase64String(updatedVault);

                File.WriteAllText(clientPath, cl.SecretKey);
                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            }

            if (args[0] == "create")
            {
                string clientPath = args[1];
                string serverPath = args[2];

                if (!File.Exists(clientPath))
                {
                    var newClient = new client(clientPath);
                    File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
                }

                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.WriteLine("Enter Password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);
                byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

                Console.WriteLine(Convert.ToBase64String(secretKey));
                Console.WriteLine(Convert.ToBase64String(vaultKey));

                string decryptedVault;
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
                Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                foreach (var kvp in vaultData)
                {
                    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                }
            }
            
            if (args[0]== "set")
            {
                string clientPath = args[1];
                string serverPath = args[2];
                string property = args[3];



                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.WriteLine("Enter Password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);
                byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

                Console.WriteLine(Convert.ToBase64String(secretKey));
                Console.WriteLine(Convert.ToBase64String(vaultKey));

                string decryptedVault;
               
                /*try
                {
                    decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
                }
                catch (CryptographicException)
                {
                    Console.WriteLine("Crypto error");
                }*/
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);


                Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                string password;
                if (args.Length == 5/* && args[4] == "-g" && args[4] == "-generate"*/)
                {
                    if (args[4] == "-g" && args[4] == "-generate")
                    {
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                        Random random = new Random();
                        vaultData[property] = new string(Enumerable.Repeat(chars, 20)
                                                   .Select(s => s[random.Next(s.Length)])
                                                   .ToArray());
                    }

                    else
                        Console.WriteLine("Invalid flag");
                }

                else
                {
                    Console.WriteLine("Enter password for new property)");
                    vaultData[property] = Console.ReadLine();
                }                

                string updatedVaultJson = JsonSerializer.Serialize(vaultData);
                byte[] updatedEncryptedVault = CryptoHelper.Encrypt(updatedVaultJson, vaultKey, iv);
                serv.PasswordVault = Convert.ToBase64String(updatedEncryptedVault);


                foreach (var kvp in vaultData)
                {
                    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                }

                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));             
            }

            if (args[0] == "get")
            {
                string clientPath = args[1];
                string serverPath = args[2];

                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.WriteLine("Enter Password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);
                byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

                Console.WriteLine(Convert.ToBase64String(secretKey));
                Console.WriteLine(Convert.ToBase64String(vaultKey));

                string decryptedVault;
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
                Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                if(args.Length == 3)
                {
                    Console.WriteLine("List of properties:");
                    foreach (var kvp in vaultData)
                        Console.WriteLine(kvp.Key);
                }

                else if (args.Length == 4)
                {
                    string property = args[3];

                    foreach (var kvp in vaultData)
                    {
                        if (kvp.Key == property)
                            Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                    }
                }
            }

            if (args[0] == "delete")
            {
                string clientPath = args[1];
                string serverPath = args[2];
                string property = args[3];



                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.WriteLine("Enter Password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);
                byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

                Console.WriteLine(Convert.ToBase64String(secretKey));
                Console.WriteLine(Convert.ToBase64String(vaultKey));

                string decryptedVault;

                /*try
                {
                    decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
                }
                catch (CryptographicException)
                {
                    Console.WriteLine("Crypto error");
                }*/
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);


                Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);
                vaultData.Remove(property);

                string updatedVaultJson = JsonSerializer.Serialize(vaultData);
                byte[] updatedEncryptedVault = CryptoHelper.Encrypt(updatedVaultJson, vaultKey, iv);
                serv.PasswordVault = Convert.ToBase64String(updatedEncryptedVault);

                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            }

            if (args[0] == "secret")
            {
                string clientPath = args[1];
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
                Console.WriteLine(cl.SecretKey);
            }

            if (args[0] == "change")
            {
                string clientPath = args[1];
                string serverPath = args[2];

                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

                Console.WriteLine("Enter Password: ");

                byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
                byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] iv = Convert.FromBase64String(serv.IV);
                byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

                string decryptedVault;
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
  
                Console.WriteLine("Enter new master password: ");
                byte[] newVaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] newPasswordVault = CryptoHelper.Encrypt(decryptedVault, newVaultKey, iv);
                serv.PasswordVault = Convert.ToBase64String(newPasswordVault);

                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            }

            foreach (string arg  in args)
                Console.WriteLine(arg);
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);
        }
    }
}
