using System;
using MySqlConnector;

class Program {
    static void Main() {
        string connStr = "Server=db45494.public.databaseasp.net;Database=db45494;Uid=db45494;Pwd=V$R~5j+y78%M2r;Port=3306;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        Console.WriteLine("--- RECENT ASSET INFORMINGS ---");
        using (var cmd = new MySqlCommand("SELECT Id, ItemName, Model, Status, TargetEmployeeId, DivisionId, Remarks FROM AssetInformings ORDER BY Id DESC LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, Item: {reader[1]}, Model: {reader[2]}, Status: {reader[3]}, TargetEmp: {reader[4]}, Div: {reader[5]}, Remarks: {reader[6]}");
            }
        }

        Console.WriteLine("\n--- RECENT GRNs ---");
        using (var cmd = new MySqlCommand("SELECT Id, GrnNumber, PurchasingOrderId, AssetId, ReceivedDate, ReceivedBy FROM GRNs ORDER BY Id DESC LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, GrnNo: {reader[1]}, PO_Id: {reader[2]}, AssetId: {reader[3]}, Date: {reader[4]}, By: {reader[5]}");
            }
        }

        Console.WriteLine("\n--- RECENT ASSETS ---");
        using (var cmd = new MySqlCommand("SELECT Id, AssetCode, Status, PurchaseValue, Notes, ProductId, DivisionId FROM Assets ORDER BY Id DESC LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, Code: {reader[1]}, Status: {reader[2]}, Value: {reader[3]}, Notes: {reader[4]}, ProductId: {reader[5]}");
            }
        }

        Console.WriteLine("\n--- RECENT PURCHASING ORDERS ---");
        using (var cmd = new MySqlCommand("SELECT Id, OrderNumber, Status, TotalAmount FROM PurchasingOrders ORDER BY Id DESC LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, OrderNo: {reader[1]}, Status: {reader[2]}, Amount: {reader[3]}");
            }
        }
    }
}
