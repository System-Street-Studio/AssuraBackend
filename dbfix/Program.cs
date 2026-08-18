using System;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connStr = "Server=db45494.public.databaseasp.net;Database=db45494;Uid=db45494;Pwd=K-h2t9?AjS7;Port=3306;";
        using var conn = new MySqlConnection(connStr);
        try
        {
            conn.Open();
            Console.WriteLine("Successfully connected to the database!");

            // 1. Check QueueItems
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM QueueItems", conn))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"QueueItems Count: {count}");
            }

            using (var cmd = new MySqlCommand("SELECT Id, Name, Status, Date FROM QueueItems LIMIT 10", conn))
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine("QueueItems (Limit 10):");
                while (reader.Read())
                {
                    Console.WriteLine($"- ID: {reader["Id"]}, Name: {reader["Name"]}, Status: {reader["Status"]}, Date: {reader["Date"]}");
                }
            }

            // 2. Check DiscardedNotes
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM DiscardedNotes", conn))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"DiscardedNotes Count: {count}");
            }

            using (var cmd = new MySqlCommand("SELECT Id, Name, Status, Date FROM DiscardedNotes LIMIT 10", conn))
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine("DiscardedNotes (Limit 10):");
                while (reader.Read())
                {
                    Console.WriteLine($"- ID: {reader["Id"]}, Name: {reader["Name"]}, Status: {reader["Status"]}, Date: {reader["Date"]}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database query error: {ex.Message}");
        }
    }
}
