using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;



namespace JSONtoSQL
{
    public class Users
    {
        public int postId { get; set; }
        public int id { get; set; }
        public String name { get; set; }
        public String email { get; set; }
        public String body { get; set; }
    }

    public class AllItems
    {
        public List<Users> Items { get; set; }
        public bool HasMoreResults { get; set; }
    }
    class Program
    {
        private static T JSONtoGENtype<T>(string url) where T : new()
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString(url);
                }
                catch (Exception) { }
                // if string with JSON data is not empty, deserialize it to class and return its instance  
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
        }



        private static void Main(string[] args)
        {

            var url = "https://jsonplaceholder.typicode.com/comments";
            var Tabela = JSONtoGENtype<List<Users>>(url);



            SqlConnection conn = new SqlConnection();
            conn.ConnectionString =
            @"Data Source = NAIDA-PC\SQLEXPRESS; Integrated Security = True;";

            conn.Open();
            Console.WriteLine("ServerVersion: {0}", conn.ServerVersion);
            Console.WriteLine("State: {0}", conn.State);

            string makedb = @"USE master " + @"IF EXISTS(select * from sys.databases where name = 'JSONdata') " +
@"DROP DATABASE JSONdata " + " CREATE DATABASE JSONdata";

            using (SqlCommand cmd = new SqlCommand(makedb, conn))

                cmd.ExecuteNonQuery();


            string use = @"Use JSONdata";
            using (SqlCommand cmd = new SqlCommand(use, conn))

                cmd.ExecuteNonQuery();

            string maketb = @"IF OBJECT_ID('AllItemsTable', 'U') IS NOT NULL
  DROP TABLE AllItemsTable " +  @"Create table AllItemsTable (
     postId int ,
     id int,
     name text,
     body text,
     email text

     PRIMARY KEY (id)

 );";
            using (SqlCommand cmd = new SqlCommand(maketb, conn))

                cmd.ExecuteNonQuery();

            foreach (var Users in Tabela)
            {
                //Save/Insert to database
                if (SaveToDatabase(conn, Users))
                {
                    Console.WriteLine("Success : " + Users.id + " Saved into database");
                }
                else
                {
                    Console.WriteLine("Error : " + Users.id + " unable to Saved into database");
                }
            }

            Console.Read();


        }
        static bool SaveToDatabase(SqlConnection con, Users aItemObj)
        {
            try
            {

                string insertQuery = @"Insert into AllItemsTable(postId,id,name,email,body) Values(@postId,@id,@name,@email,@body)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    cmd.Parameters.Add(new SqlParameter("@postId", aItemObj.postId));
                    cmd.Parameters.Add(new SqlParameter("@id", aItemObj.id));
                    cmd.Parameters.Add(new SqlParameter("@name", aItemObj.name));
                    cmd.Parameters.Add(new SqlParameter("@email", aItemObj.email));
                    cmd.Parameters.Add(new SqlParameter("@body", aItemObj.body));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception objEx)
            {
                Console.WriteLine("\nMessage ---\n{0}", objEx.Message);
                /* Console.WriteLine(
                     "\nHelpLink ---\n{0}", objEx.HelpLink);
                 Console.WriteLine("\nSource ---\n{0}", objEx.Source);
                 Console.WriteLine(
                     "\nStackTrace ---\n{0}", objEx.StackTrace);
                 Console.WriteLine(
                     "\nTargetSite ---\n{0}", objEx.TargetSite);
                     */
                return false;
            }
        }
        

    }
}


