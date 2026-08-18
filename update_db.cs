using System;
using MySqlConnector;

class Program {
    static void Main() {
        string connStr = "Server=db45494.public.databaseasp.net;Database=db45494;Uid=db45494;Pwd=V$R~5j+y78%M2r;Port=3306;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        // 1. Update Product 28 name to "Wooden Table"
        using (var cmd = new MySqlCommand("UPDATE Products SET Name = 'Wooden Table', ModelNumber = 'Wooden Table' WHERE Id = 28", conn))
        {
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Updated {rows} row(s) in Products.");
        }

        // 2. Update AssetInforming 63 status to "GRN Recorded"
        using (var cmd = new MySqlCommand("UPDATE AssetInformings SET Status = 'GRN Recorded' WHERE Id = 63", conn))
        {
            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine($"Updated {rows} row(s) in AssetInformings.");
        }
    }
}
