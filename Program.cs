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

        static void Main(string[] args)
        {
            if (args[0] == "init")
            {
                string clientPath = args[1];
                string serverPath = args[2];

                server serv = new server(clientPath);
                client cl = new client(serverPath);

                Console.Write("Write your password: ");

  
                byte[] secretKeyBytes = Encoding.ASCII.GetBytes(cl.SecretKey);
                Rfc2898DeriveBytes vK = new Rfc2898DeriveBytes(Console.ReadLine(), secretKeyBytes, 1000); ;

                CryptoHelper.Encrypt(serv.PasswordVault, vK.GetBytes(16), Convert.FromBase64String(serv.IV));
            }

            if (args[0] == "create")
            {
                string clientPath = args[1];
                string serverPath = args[2];
            }


            foreach(string arg  in args)
                Console.WriteLine(arg);
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Directory.SetCurrentDirectory(projectDirectory);
        }
    }
}
