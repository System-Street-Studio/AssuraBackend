using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string[] users = {
            "admin:Password@123:admin@assura.com:System:Admin:1:7",
            "procurement:Procurement@123:proc@assura.com:Procurement:Officer:2:9",
            "test_storekeeper:StorekeeperPass123!:storekeeper@assura.com:Test:Storekeeper:5:10",
            "test_accountant:AccountantPass123!:accountant@assura.com:Test:Accountant:9:8",
            "test_super:SuperintendentPass123!:super@assura.com:Test:Super:4:7"
        };

        foreach (var user in users)
        {
            var parts = user.Split(':');
            var hash = BCrypt.Net.BCrypt.HashPassword(parts[1]);
            Console.WriteLine($"INSERT INTO users (Username, PasswordHash, Email, FirstName, LastName, Role, DivisionId, IsActive, IsDeleted, CreatedAt) VALUES ('{parts[0]}', '{hash}', '{parts[2]}', '{parts[3]}', '{parts[4]}', {parts[5]}, {parts[6]}, 1, 0, NOW());");
        }
    }
}
