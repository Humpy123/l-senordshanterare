﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace lösenordshanterare
{
    internal class server
    {
        public string PasswordVault { get; set; }
        public string IV { get; set; }


        public void WriteToJson()
        {
            string json = JsonSerializer.Serialize(this);
            string path = "files/server.json";
            File.WriteAllText(path, json);
        }

        public server(string serverPath)
        {
            using (Aes aesAlg = Aes.Create())
            {
                string ivBase64 = Convert.ToBase64String(aesAlg.IV);
                IV = ivBase64;
                WriteToJson();
            }
        }

        public server() { }

    }
}
