using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace losenordshanterare
{
    internal class server
    {
        public string PasswordVault { get; set; }
        public string IV { get; set; }


        public void WriteToJson(string serverPath)
        {
            string json = JsonSerializer.Serialize(this);
            string path = serverPath;
            File.WriteAllText(path, json);
        }

        public server(string serverPath)
        {
            using (Aes aesAlg = Aes.Create())
            {
                string ivBase64 = Convert.ToBase64String(aesAlg.IV);
                IV = ivBase64;
                WriteToJson(serverPath);
            }
        }

        public server() { }

    }
}
