using System.Collections.Generic;
using System;
using System.Linq;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;
using aspexam.Models;
using CryptoHelper;

namespace ExamApp.Factory
{
    public class ExamRepository : IFactory<User>
    {
        private string connectionString;
        public ExamRepository()
        {
            connectionString = "server=localhost;userid=root;password=root;port=8889;database=aspexam;SslMode=None";
        }

         internal IDbConnection Connection
        {
            get {
                return new MySqlConnection(connectionString);
            }
        }
        ///Begins here
        public void Add_Network(int id)
        {
             using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT IGNORE INTO network (created_at, updated_at, user_id) VALUES (NOW(), NOW(), '{id}');");

            }
        }
        public Network Network_Last_ID()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Network>("SELECT * FROM network ORDER BY id DESC LIMIT 1").FirstOrDefault();
            }
        }
        public void Add_Joiner(int num1, int num2)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO joiners (network_id, user_id) VALUES ('{num1}', '{num2}')");
            }
        }
        // public IEnumerable<Network> ExceptCurrentUserNetworks(int id)
        // {
        //     using (IDbConnection dbConnection = Connection) 
        //     {
        //         dbConnection.Open();
        //         return dbConnection.Query<Network>($"SELECT users.first_name, network.id from network JOIN joiners ON network.id = joiners.network_id  JOIN users ON users.id = joiners.user_id WHERE joiners.network_id not in (SELECT network.id from network JOIN joiners ON network.id = joiners.network_id  JOIN users ON users.id = joiners.user_id WHERE joiners.user_id = '{id}');");
        //     }
        // }
        public IEnumerable<Network> ExceptCurrentUserNetworks(int id)
        {
            using (IDbConnection dbConnection = Connection) 
            {
                dbConnection.Open();
                return dbConnection.Query<Network>($"Select users.first_name, network.id from users, network, joiners where users.id = joiners.user_id and users.id = network.user_id and network.id = joiners.network_id and network.id  Not in (SELECT network.id from network JOIN joiners ON network.id = joiners.network_id JOIN users ON users.id = joiners.user_id WHERE joiners.user_id = '{id}');");
            }
        }
        public Network Network_Info(string id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var test = dbConnection.Query<Network>($"SELECT network.id, users.description, users.first_name, network.user_id, users.last_name FROM network LEFT JOIN users ON users.id = network.user_id where network.id ='{id}';").FirstOrDefault();
                return test;
            }
        }
        public void Join_Network(string network_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string status = "connected";
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO joiners (network_id, user_id, status) VALUES ('{network_id}', '{user_id}', '{status}')");
            }
        }
        public void Ignore_Network(string network_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string status = "ignored";
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO joiners (network_id, user_id, status) VALUES ('{network_id}', '{user_id}', '{status}')");
            }
        }
        public IEnumerable<Network> CurrentUserNetworks(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Network>($"SELECT network.id, users.first_name, users.last_name from network JOIN joiners ON network.id = joiners.network_id JOIN users ON users.id = network.user_id WHERE joiners.user_id = '{id}'");
            }
        }
        public int  Extract(int num)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var temp = dbConnection.Query<Network>($"SELECT network.id from network where network.user_id = '{num}'").SingleOrDefault();
                return temp.id;
            }
        }
        public IEnumerable<Network> others(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Network>($"SELECT network.id, users.first_name, users.last_name from network JOIN joiners ON network.id = joiners.network_id  JOIN users ON users.id = joiners.user_id WHERE network.id = '{id}' AND users.id NOT IN (SELECT users.id FROM network LEFT JOIN users ON users.id = network.user_id where network.id = '{id}');");
            }
        }
        
        public IEnumerable<Network> Ignored(int id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                string status = "ignored";
                dbConnection.Open();
                return dbConnection.Query<Network>($"SELECT joiners.user_id, joiners.network_id,  users.first_name, users.last_name from joiners, users, network where joiners.network_id = network.id and users.id = network.user_id and joiners.user_id = '{id}' and joiners.status = '{status}' UNION Select joiners.user_id, joiners.network_id,  users.first_name, users.last_name from users, network, joiners where users.id = joiners.user_id and users.id = network.user_id and network.id = joiners.network_id and network.id  Not in (SELECT network.id from network JOIN joiners ON network.id = joiners.network_id JOIN users ON users.id = joiners.user_id WHERE joiners.user_id = '{id}')");
            }
        }
        
        public void Delete_Joiner(string network_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"DELETE FROM joiners WHERE joiners.user_id = '{user_id}' AND joiners.network_id = '{network_id}'");
            }
        }

    }
}
