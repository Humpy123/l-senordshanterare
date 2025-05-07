using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Win32.SafeHandles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace losenordshanterare
{
    public class Program
    
    {
        static string GenerateRandomPassword(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new RNGCryptoServiceProvider();
            var result = new StringBuilder(length);

            while (result.Length < length)
            {
                byte[] bytes = new byte[1];
                random.GetBytes(bytes);
                char c = chars[bytes[0] % chars.Length];
                if (char.IsLetterOrDigit(c)) // Ensure we only get alphanumeric characters
                {
                    result.Append(c);
                }
            }

            return result.ToString();
            /*
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new RNGCryptoServiceProvider();
                var bytes = new byte[length];
                random.GetBytes(bytes);
                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    result[i] = chars[bytes[i] % chars.Length];
                }
                return new string(result);
            }
            
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 20)
                                                           .Select(s => s[random.Next(s.Length)])
                                                           .ToArray());*/
        }
        static void EncryptVault(string vaultData, string clientPath, string serverPath, string pwd)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

          //  Console.WriteLine("Enter Password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, pwd);
            byte[] iv = Convert.FromBase64String(serv.IV);

            serv.PasswordVault = Convert.ToBase64String(CryptoHelper.Encrypt(vaultData, vaultKey, iv));

            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
        }
        static Dictionary<string, string> DecryptVault(string clientPath, string serverPath, string pwd)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

            //Console.WriteLine("Enter Password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, pwd);
            byte[] iv = Convert.FromBase64String(serv.IV);
            byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

            string decryptedVault;
            decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
            Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

            return vaultData;
        }

        static Dictionary<string, string> DecryptVault(string clientPath, string serverPath, string pwd, string secret)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));

            byte[] vaultKey = CryptoHelper.DeriveKey(secret, pwd);
            byte[] iv = Convert.FromBase64String(serv.IV);
            byte[] encryptedVault = Convert.FromBase64String(serv.PasswordVault);

            string decryptedVault;
            decryptedVault = CryptoHelper.Decrypt(encryptedVault, vaultKey, iv);
            Dictionary<string, string> vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

            return vaultData;
        }
        static void Init(string clientPath, string serverPath, string pwd)
        {

                var newClient = new client(clientPath);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
  
                var newServer = new server(serverPath);
                File.WriteAllText(serverPath, JsonSerializer.Serialize(newServer));


            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

            //Console.Write("Write your password: ");

            byte[] secretKey = Convert.FromBase64String(cl.SecretKey.Replace("\\u002b", "+"));
            byte[] vaultKey = CryptoHelper.DeriveKey(cl.SecretKey, pwd);
            byte[] iv = Convert.FromBase64String(serv.IV);

            Dictionary<string, string> vaultData = new Dictionary<string, string>();
            string updatedVaultJson = JsonSerializer.Serialize(vaultData);
            byte[] updatedVault = CryptoHelper.Encrypt(updatedVaultJson, vaultKey, iv);
            serv.PasswordVault = Convert.ToBase64String(updatedVault);

            File.WriteAllText(clientPath, JsonSerializer.Serialize(cl));
            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            Console.WriteLine(cl.SecretKey);
        }

        static void Create(string clientPath, string serverPath, string pwd, string secret)
        {
            if (!File.Exists(clientPath))
            {
                var newClient = new client(clientPath, secret);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            if (!File.Exists(clientPath))
            {
                var newClient = new client(clientPath, secret);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd, secret);

                if (vaultData == null)
                    throw new Exception("Decryption returned null vault data.");
            }
            catch
            {
                if (File.Exists(clientPath))
                    File.Delete(clientPath);

                throw new Exception("Failed to decrypt vault: possibly wrong secret key.");
            }
        }

        static void Set(string clientPath, string serverPath, string pwd, string property, string password)
        {
            Dictionary<string, string> vaultData = DecryptVault(clientPath, serverPath, pwd);

            if (vaultData.ContainsKey(property))
                vaultData[property] = password;
            else
                vaultData.Add(property, password);
  
            EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath, pwd);
        }

        static void Get(string clientPath, string serverPath, string pwd)
        {
            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd);

                //Console.WriteLine("List of properties:");
                foreach (var kvp in vaultData)
                    Console.WriteLine(kvp.Key);
            }

            catch
            {
                Console.WriteLine("error");
            } 
        }

        static void Get(string clientPath, string serverPath, string pwd, string property)
        {
            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd);

                foreach (var kvp in vaultData)
                {
                    if (kvp.Key == property)
                        Console.WriteLine(kvp.Value);
                }
            }

            catch
            {
                Console.WriteLine("error");
            }
        }

        static void Delete(string clientPath, string serverPath, string pwd, string property)
        {
            var vaultData = DecryptVault(clientPath, serverPath, pwd);
            vaultData.Remove(property);
            EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath, pwd);
        }

        static void Change(string clientPath, string serverPath, string pwd)
        {

            string decryptedVault = JsonSerializer.Serialize(DecryptVault(clientPath, serverPath, pwd));

            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
            byte[] iv = Convert.FromBase64String(serv.IV);

            Console.WriteLine("Enter new master password: ");
            byte[] newVaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
            byte[] newPasswordVault = CryptoHelper.Encrypt(decryptedVault, newVaultKey, iv);
            serv.PasswordVault = Convert.ToBase64String(newPasswordVault);

            File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
        }
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid command. Usage: [command] [arguments...]");
                return;
            }

            try
            {
                string clientPath = args[1];


                switch (args[0])
                {
                    case "init":
                        string serverPath = args[2];

                        Console.WriteLine("Enter master password: ");
                        Init(args[1], args[2], Console.ReadLine());
                        break;
                    case "create":
                        serverPath = args[2];

                        Console.WriteLine("Enter master password, then enter your secret key: ");
                        Create(args[1], args[2], Console.ReadLine(), Console.ReadLine());
                        break;
                    case "set":
                        serverPath = args[2];

                        string property = args[3];
                        if (args.Length == 5)
                        {
                            if (args[4] == "-g" || args[4] == "--generate")
                            {
                                // Console.WriteLine("Enter master password: ");
                                string randomPassword = GenerateRandomPassword();
                                Set(clientPath, serverPath, Console.ReadLine(), property, randomPassword);
                                Console.WriteLine(randomPassword);
                            }
                        }
                        else
                        {
                          //  Console.WriteLine("Enter your desired password for the property, then enter your master password: ");
                            
                            Set(clientPath, serverPath, Console.ReadLine(), property, Console.ReadLine());
                        }


                        break;
                    case "get":
                        serverPath = args[2];

                        //Console.WriteLine("Enter master password: ");
                        if (args.Length == 3)
                            Get(clientPath, serverPath, Console.ReadLine());

                        else if (args.Length == 4)
                        {
                            property = args[3];
                            Get(clientPath, serverPath, Console.ReadLine(), property);
                        }
                        break;
                    case "secret":
                        client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
                        Console.WriteLine(cl.SecretKey);
                        break;
                    case "delete":
                        serverPath = args[2];

                        property = args[3];
                        Console.WriteLine("Enter master password: ");
                        Delete(clientPath, serverPath, Console.ReadLine(), property);
                        break;
                    case "change":
                        serverPath = args[2];

                        Console.WriteLine("Enter master password: ");
                        Change(clientPath, serverPath, Console.ReadLine());
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

