using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Win32.SafeHandles;

namespace lösenordshanterare
{
    internal class Program
    
    {
        static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 20)
                                                           .Select(s => s[random.Next(s.Length)])
                                                           .ToArray());
        }
        static void EncryptVault(string vaultData, string clientPath, string serverPath)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

            Console.WriteLine("Enter Password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
            byte[] iv = Convert.FromBase64String(serv.IV);

            serv.PasswordVault = Convert.ToBase64String(CryptoHelper.Encrypt(vaultData, vaultKey, iv));

            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
        }
        static Dictionary<string, string> DecryptVault(string clientPath, string serverPath)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

            Console.WriteLine("Enter Password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
            byte[] iv = Convert.FromBase64String(serv.IV);
            byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

            string decryptedVault;
            decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
            Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

            return vaultData;
        }
        static void Init(string clientPath, string serverPath)
        {
            if (!File.Exists(clientPath))
            {
                var newClient = new client(clientPath);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            if (!File.Exists(serverPath))
            {
                var newServer = new server(serverPath);
                File.WriteAllText(serverPath, JsonSerializer.Serialize(newServer));
            }

            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

            Console.Write("Write your password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
            byte[] iv = Convert.FromBase64String(serv.IV);

            Dictionary<string, string> vaultData = new Dictionary<string, string>();
            string updatedVaultJson = JsonSerializer.Serialize(vaultData);
            byte[] updatedVault = CryptoHelper.Encrypt(updatedVaultJson, vaultKey, iv);
            serv.PasswordVault = Convert.ToBase64String(updatedVault);

            File.WriteAllText(clientPath, JsonSerializer.Serialize(cl));
            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
        }

        static void Create(string clientPath, string serverPath)
        {
            if (!File.Exists(clientPath))
            {
                var newClient = new client(clientPath);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            if (!File.Exists(clientPath))
            {
                var newClient = new client(clientPath);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            var vaultData = DecryptVault(clientPath, serverPath);
        }

        static void Set(string clientPath, string serverPath, string property, string password)
        {
            Dictionary<string, string> vaultData = DecryptVault(clientPath, serverPath);
            vaultData.Add(property, password);

            EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath);
        }

        static void Get(string clientPath, string serverPath)
        {
            var vaultData = DecryptVault(clientPath, serverPath);

            Console.WriteLine("List of properties:");
            foreach (var kvp in vaultData)
                Console.WriteLine(kvp.Key);
        }

        static void Get(string clientPath, string serverPath, string property)
        {
            var vaultData = DecryptVault(clientPath, serverPath);

            foreach (var kvp in vaultData)
            {
                if (kvp.Key == property)
                    Console.WriteLine("Password for " + kvp.Key + ": ");
                    Console.Write(kvp.Value);
            }
        }

        static void Delete(string clientPath, string serverPath, string property)
        {
            var vaultData = DecryptVault(clientPath, serverPath);
            vaultData.Remove(property);
            EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath);
        }

        static void Change(string clientPath, string serverPath)
        {

            string decryptedVault = JsonSerializer.Serialize(DecryptVault(clientPath, serverPath));

            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
            byte[] iv = Convert.FromBase64String(serv.IV);

            Console.WriteLine("Enter new master password: ");
            byte[] newVaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
            byte[] newPasswordVault = CryptoHelper.Encrypt(decryptedVault, newVaultKey, iv);
            serv.PasswordVault = Convert.ToBase64String(newPasswordVault);

            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
        }
        static void Main(string[] args)
        {
            string clientPath = args[1];
            string serverPath = args[2];    

            switch (args[0])
            {
                case "init":
                    Init(args[1], args[2]);
                    break;
                case "create":
                    Create(args[1], args[2]);
                    break;
                case "set":
                    string property = args[3];
                    if (args.Length == 5)
                    {
                        if (args[4] == "-g" && args[4] == "-generate")
                            Set(clientPath, serverPath, property, GenerateRandomPassword());
                        Console.WriteLine("Error: unrecognized flag");
                    }

                    Console.WriteLine("Please enter your desired password for the new property: ");
                    Set(clientPath, serverPath, property, Console.ReadLine());

                    break;
                case "get":                 
                    if (args.Length == 3)
                        Get(clientPath, serverPath);

                    else if (args.Length == 4)
                    {
                        property = args[3];
                        Get(clientPath, serverPath, property);
                    }
                    break;
                case "secret":
                    clientPath = args[1];
                    client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
                    Console.WriteLine("Secret Key: " + cl.SecretKey);
                    break;
                case "delete":
                    property = args[3];
                    Delete(clientPath, serverPath, property);
                    break;
                case "change":
                    Change(clientPath, serverPath);
                    break;

            }

            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);
        }
    }
}
