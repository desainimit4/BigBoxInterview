using Npgsql;
using System;

namespace BigBoxInterview
{
    public class PostgresProvider
    {
        // Obtain connection string information from the portal
        //
        private static string Host = "bigboxpostgres.postgres.database.azure.com";
        private static string User = "nidesai@bigboxpostgres";
        private static string DBname = "postgres";
        private static string Password = "b1gboxvr!";
        private static string Port = "5432";

        private string connString = "";

        public PostgresProvider()
        {
            connString =
                            String.Format(
                                "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Disable",
                                Host,
                                User,
                                DBname,
                                Port,
                                Password);
        }

        public void CreateTable()
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                Console.Out.WriteLine("Opening connection");
                conn.Open();
                using (var command = new NpgsqlCommand("CREATE TABLE bbEvents" +
                    "(id serial PRIMARY KEY, " +
                    "app VARCHAR(50), " +
                    "user_id VARCHAR(50), " +
                    "session_id VARCHAR(50), " +
                    "local_time VARCHAR(50), " +
                    "action VARCHAR(50), " +
                    "context VARCHAR(50), " +
                    "value VARCHAR(50))", conn))
                {
                    command.ExecuteNonQuery();
                    Console.Out.WriteLine("Finished creating table");
                }
            }
        }
        public void InsertIntoTable(Event @event)
        {
            using (var conn = new NpgsqlConnection(connString))

            {
                Console.Out.WriteLine("Opening connection");
                conn.Open();
                using (var command = new NpgsqlCommand("INSERT INTO bbEvents (app, user_id, session_id, local_time, action, context, value) " +
                    "VALUES (@app, @user_id, @session_id, @local_time, @action, @context, @value)", conn))
                {
                    command.Parameters.AddWithValue("app", @event.app);
                    command.Parameters.AddWithValue("user_id", @event.user_id);
                    command.Parameters.AddWithValue("session_id", @event.session_id);
                    command.Parameters.AddWithValue("local_time", @event.local_time);
                    command.Parameters.AddWithValue("action", @event.action);
                    command.Parameters.AddWithValue("context", @event.context);
                    command.Parameters.AddWithValue("value", @event.value);

                    int nRows = command.ExecuteNonQuery();
                    Console.Out.WriteLine(String.Format("Number of rows inserted={0}", nRows));
                }
            }
        }

        public Event GetActionNameCount(string actionName)
        {
            Event e = new Event();
            using (var conn = new NpgsqlConnection(connString))
            {
                Console.Out.WriteLine("Opening connection");
                conn.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(@actionName) FROM bbEvents", conn))
                {
                    command.Parameters.AddWithValue("action", actionName);
                                        
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        e = new Event()
                        {
                            app = reader.GetString(0),
                            user_id = reader.GetString(0),
                            session_id = reader.GetString(0),
                            local_time = reader.GetString(0),
                            action = reader.GetString(0),
                            context = reader.GetString(0),
                            value = reader.GetString(0),
                        };
                    }
                    reader.Close();
                }
            }

            return e;
        }

        //ToDo: Implement function that gets values by series of params
    }
}
