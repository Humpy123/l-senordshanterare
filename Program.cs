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
using System.Linq.Expressions;

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
        }
        static void EncryptVault(string vaultData, string clientPath, string serverPath, string pwd)
        {
            server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));

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

            // Cancels the command if a server does not exist at the path.
            if (!File.Exists(serverPath))
            {
                Console.WriteLine("Error: Invalid server path.");
                return;
            }

            // If decryption is successful, the client is written/overwritten, otherwise an error message is printed.
            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd, secret);
                File.Delete(clientPath);
                var newClient = new client(clientPath, secret);
                File.WriteAllText(clientPath, JsonSerializer.Serialize(newClient));
            }

            catch
            {
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
            }            
        }

        static void Set(string clientPath, string serverPath, string pwd, string property, string password)
        {
            try
            {
                Dictionary<string, string> vaultData = DecryptVault(clientPath, serverPath, pwd);

                if (vaultData.ContainsKey(property))
                    vaultData[property] = password;
                else
                    vaultData.Add(property, password);

                EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath, pwd);
            }            

            catch
            {
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
            }
        }

        static void Get(string clientPath, string serverPath, string pwd)
        {
            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd);
                foreach (var kvp in vaultData)
                    Console.WriteLine(kvp.Key);
            }

            catch
            {
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
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
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
            }
        }

        static void Delete(string clientPath, string serverPath, string pwd, string property)
        {
            try
            {
                var vaultData = DecryptVault(clientPath, serverPath, pwd);

                if (!vaultData.ContainsKey(property))
                    Console.WriteLine("Error: Property does not exist");

                vaultData.Remove(property);
                EncryptVault(JsonSerializer.Serialize(vaultData), clientPath, serverPath, pwd);
            }

            catch
            {
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
            }
        }

        static void Change(string clientPath, string serverPath, string pwd)
        {
            try
            {
                string decryptedVault = JsonSerializer.Serialize(DecryptVault(clientPath, serverPath, pwd));

                server serv = JsonSerializer.Deserialize<server>(File.ReadAllText(serverPath));
                client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
                byte[] iv = Convert.FromBase64String(serv.IV);

                Console.WriteLine("Choose your master password: ");
                byte[] newVaultKey = CryptoHelper.DeriveKey(cl.SecretKey, Console.ReadLine());
                byte[] newPasswordVault = CryptoHelper.Encrypt(decryptedVault, newVaultKey, iv);
                serv.PasswordVault = Convert.ToBase64String(newPasswordVault);

                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            }        
            catch
            {
                Console.WriteLine("Error: Failed to decrypt vault, possibly wrong secret key or password.");
            }
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
                string clientPath;
                string serverPath;


                switch (args[0])
                {
                    case "init":
                        if (args.Length != 3)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }
                        else
                        {
                            serverPath = args[2];
                            clientPath = args[1];

                            Console.WriteLine("Enter master password: ");
                            Init(clientPath, serverPath, Console.ReadLine());
                        }
                        
                        break;
                    case "create":
                        if(args.Length != 3)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }
                        else
                        {
                            serverPath = args[2];
                            clientPath = args[1];

                            Console.WriteLine("Enter master password, then enter your secret key: ");
                            Create(clientPath, serverPath, Console.ReadLine(), Console.ReadLine());
                        }
                        
                        break;
                    case "set":
                        if(args.Length != 4 && args.Length != 5)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }
                        else
                        {
                            string property = args[3];
                            serverPath = args[2];
                            clientPath = args[1];

                            if (args.Length == 5)
                            {
                                if (args[4] == "-g" || args[4] == "--generate")
                                {
                                    Console.WriteLine("Enter master password: ");
                                    string randomPassword = GenerateRandomPassword();
                                    Set(clientPath, serverPath, Console.ReadLine(), property, randomPassword);
                                    Console.WriteLine(randomPassword);
                                }

                                else
                                    Console.WriteLine("Error: invalid flag, use -g or --generate to generate a random secure password.");
                            }
                            else
                            {
                                Console.WriteLine("Enter your master password, then your desired password for the property: ");
                                Set(clientPath, serverPath, Console.ReadLine(), property, Console.ReadLine());
                            }
                        }
                                                   
                                      
                        break;
                    case "get":
                        if (args.Length != 4 && args.Length != 3)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }
                        else
                        {
                            clientPath = args[1];
                            serverPath = args[2];

                            Console.WriteLine("Enter master password: ");
                            if (args.Length == 3)
                                Get(clientPath, serverPath, Console.ReadLine());

                            else if (args.Length == 4)
                            {
                                string property = args[3];
                                Get(clientPath, serverPath, Console.ReadLine(), property);
                            }
                        }
                        
                        break;
                    case "secret":
                        if(args.Length != 2)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }

                        else
                        {
                            clientPath = args[1];

                            client cl = JsonSerializer.Deserialize<client>(File.ReadAllText(clientPath));
                            Console.WriteLine(cl.SecretKey);
                        }
                        
                        break;
                    case "delete":
                        if(args.Length != 4)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }

                        else
                        {
                            serverPath = args[2];
                            string property = args[3];
                            clientPath = args[1];

                            Console.WriteLine("Enter master password: ");
                            Delete(clientPath, serverPath, Console.ReadLine(), property);
                        }
                        
                        break;
                    case "change":
                        if(args.Length != 3)
                        {
                            Console.WriteLine("Error: incorrect amount of arguments, see the manual.");
                        }
                        else
                        {
                            serverPath = args[2];
                            clientPath = args[1];

                            Console.WriteLine("Enter master password: ");
                            Change(clientPath, serverPath, Console.ReadLine());
                        }

                        break;
                    default:
                        Console.WriteLine("Error: command does not exist.");
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

