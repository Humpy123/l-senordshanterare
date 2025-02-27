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

            byte[] IV = Encoding.ASCII.GetBytes(s.IV);
            
            using(Aes a = Aes.Create())
            {
                a.Key = vaultKey.GetBytes(16);
                a.IV = IV;
            }

        }
    }
}
