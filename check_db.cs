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

        Console.WriteLine("\n--- SPECIFIC GRN ---");
        using (var cmd = new MySqlCommand("SELECT Id, GrnNumber, PurchasingOrderId, AssetId, ReceivedDate, ReceivedBy FROM GRNs WHERE GrnNumber = 'GRN-20260909-6882' OR AssetId = 256", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, GrnNo: {reader[1]}, PO_Id: {reader[2]}, AssetId: {reader[3]}, Date: {reader[4]}, By: {reader[5]}");
            }
        }

        Console.WriteLine("\n--- SPECIFIC ASSET ---");
        using (var cmd = new MySqlCommand("SELECT Id, AssetCode, Status, PurchaseValue, Notes, ProductId, DivisionId, ReservedForUserId, AssignedUserId, IsDeleted FROM Assets WHERE AssetCode LIKE '%3970%' OR Id = 256 OR AssetCode LIKE '%0004%'", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, Code: {reader[1]}, Status: {reader[2]}, Value: {reader[3]}, Notes: {reader[4]}, ProductId: {reader[5]}, Div: {reader[6]}, ReservedFor: {reader[7]}, AssignedUser: {reader[8]}, IsDeleted: {reader[9]}");
            }
        }

        Console.WriteLine("\n--- INFORMING FOR PO 32 OR 3970 ---");
        using (var cmd = new MySqlCommand("SELECT Id, ItemName, Model, Status, AssetId, PurchasingOrderId, TargetEmployeeId FROM AssetInformings WHERE PurchasingOrderId = 32 OR AssetId = 256 OR ItemName LIKE '%3970%'", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"ID: {reader[0]}, Item: {reader[1]}, Model: {reader[2]}, Status: {reader[3]}, AssetId: {reader[4]}, PO: {reader[5]}, TargetEmp: {reader[6]}");
            }
        }
    }
}
