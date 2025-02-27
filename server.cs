using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace lösenordshanterare
{
    internal class server
    {
        static void random()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16]; // You can choose the length you need
                rng.GetBytes(randomBytes); // Fill the byte array with random bytes

                // Example: Convert to a random integer (use the first 4 bytes)
                int randomInt = BitConverter.ToInt32(randomBytes, 0);
                Console.WriteLine("Random Integer: " + randomInt);
            }
        }
    }
}
