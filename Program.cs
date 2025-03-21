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
        //static void PrintMenu()
        //{
        //    Console.WriteLine("Commands:\n");

        //    Console.WriteLine("init - create a new vault\n");

        //    Console.WriteLine("create - Create a new client file (e.g., on another device) to an already existing vault.");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: create < client > < server > { < pwd >} { < secret >\n");
        //    Console.ResetColor();

        //    Console.WriteLine("get - Show stored values for some property or list properties in vault.");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: get < client > < server > [ < prop >] { < pwd >}\n");
        //    Console.ResetColor();

        //    Console.WriteLine("set - Store value for some ( possibly new ) property in vault.");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: set < client > < server > < prop > [ - g ] { < pwd >} { < value >}\n");
        //    Console.ResetColor();

        //    Console.WriteLine("delete - Drop some property from vault.");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: delete < client > < server > < prop > { < pwd >}\n");
        //    Console.ResetColor();

        //    Console.WriteLine("secret - Show secret key.");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: secret < client >\n");
        //    Console.ResetColor();


        //    Console.WriteLine("change - Change the master password");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("syntax: change < client > < server > { < pwd >} { < new_pwd >}");
        //    Console.ResetColor();
        //}
        static bool IsBase64String(string s)
        {
            Span<byte> buffer = new Span<byte>(new byte[s.Length]);
            return Convert.TryFromBase64String(s, buffer, out _);
        }

        static void Main(string[] args)
        {

            //files/client.json files/server.json
            if (args[0] == "init")
            {
                string clientPath = args[1];
                string serverPath = args[2];

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

                File.WriteAllText(serverPath, JsonSerializer.Serialize(serv));
            }

            if (args[0] == "create")
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

            if (args[0]== "geft")
            {
                string clientPath = args[1];
                string serverPath = args[2];
                string property = args[3];

                if (!File.Exists(clientPath) || !File.Exists(serverPath))
                {
                    Console.WriteLine("Error, client or server file missing");
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
                decryptedVault = CryptoHelper.Decrypt(encryptedVault, secretKey, iv);
                var vaultData = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedVault);

                foreach (var kvp in vaultData)
                {
                    Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                }
            }


            foreach (string arg  in args)
                Console.WriteLine(arg);
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);
        }
    }
}
