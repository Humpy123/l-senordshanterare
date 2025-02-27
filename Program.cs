using System.Security.Cryptography;

namespace lösenordshanterare
{
    internal class Program
    {
        static void Main(string[] args)
        {

            client Client = new client();
            server Server = new server();
            Server.GenerateIV();
            Client.generateSecret();
            
        }
    }
}
