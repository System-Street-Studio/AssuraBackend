using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string[] users = { "Storekeeper", "DivHead", "Employee", "Super", "Accountant" };
        string[] passwords = {
            "StorekeeperPass123!",
            "DivHeadPass123!",
            "EmployeePass123!",
            "SuperintendentPass123!",
            "AccountantPass123!"
        };

        for (int i = 0; i < users.Length; i++)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(passwords[i]);
            Console.WriteLine($"{users[i]}|{passwords[i]}|{hash}");
        }
    }
}
