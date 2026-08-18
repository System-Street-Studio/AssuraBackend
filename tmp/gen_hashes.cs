using BCrypt.Net;
using System;

class Program
{
    static void Main()
    {
        string[] passwords = {
            "TestStore123!",
            "TestDivHead123!",
            "TestEmp123!",
            "TestSuper123!",
            "TestAcc123!"
        };

        foreach (var pwd in passwords)
        {
            Console.WriteLine($"{pwd}:{BCrypt.Net.BCrypt.HashPassword(pwd)}");
        }
    }
}
