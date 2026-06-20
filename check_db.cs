using System;
using MySqlConnector;

class Program {
    static void Main() {
        string connStr = "Server=db45494.public.databaseasp.net;Database=db45494;Uid=db45494;Pwd=V$R~5j+y78%M2r;Port=3306;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        using var cmd = new MySqlCommand("SHOW COLUMNS FROM Assets", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Console.WriteLine(reader[0]);
        }
    }
}
