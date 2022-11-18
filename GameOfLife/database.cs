using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using Renci.SshNet.Security.Cryptography;

namespace GameOfLife
{
    internal class database
    {

        private string server;
        private string bsededonnee;
        private string uid;
        private string password;

        public string Initialize()
        {
            server = "192.168.20.130";
            bsededonnee = "Portes-ouvertes";
            uid = "cp-21sgr";
            password = "Pa$$w0rd";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + " DATABASE=" + bsededonnee + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            return connectionString;
        }

        public string Insert_pannel()
        {
            
            string cmd = $"INSERT INTO formes (name, form) VALUES (@name, @form);";
            return cmd;
        }

        public string GetNumberForm()
        {
            return "SELECT COUNT(*) FROM formes;";
        }

        public string GetFormValue(int id)
        {
            return "SELECT CONVERT(form USING utf8) AS form FROM formes WHERE idformes=" + id + ";";
        }
    }
}
