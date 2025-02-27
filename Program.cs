using System.Security.Cryptography;

namespace lösenordshanterare
{
    internal class Program
    {
        static void Main(string[] args)
        {

            client Client = new client();
            Client.generateSecret();
        }
    }
}
