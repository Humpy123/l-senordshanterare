using System.Security.Cryptography;

namespace lösenordshanterare
{
    internal class Program
    {
        static string masterPassword()
        {
            Console.WriteLine("Enter password: ")
            string password = Console.ReadLine();
            return password;
        }

        static void Main(string[] args)
        {
            string masterPassword = masterPassword();
            client Client = new client();
            Client.generateSecret();
        }
    }
}
