using System;

public class Program
{
    public static void Main()
    {
        string password = "Password@123";
        string hash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
    }
}
