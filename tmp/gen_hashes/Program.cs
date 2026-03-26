using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        Console.WriteLine("Admin@123: " + BCrypt.Net.BCrypt.HashPassword("Admin@123"));
        Console.WriteLine("Procurement@123: " + BCrypt.Net.BCrypt.HashPassword("Procurement@123"));
    }
}
