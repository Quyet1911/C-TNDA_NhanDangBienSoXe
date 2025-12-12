// File này để generate BCrypt hash cho password
// Chạy: dotnet run

using System;

class Program
{
    static void Main()
    {
        // Password cần hash
        string password = "Admin@123";

        // Tạo BCrypt hash
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 11);

        Console.WriteLine("========================================");
        Console.WriteLine("BCrypt Password Hash Generator");
        Console.WriteLine("========================================");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash:     {hash}");
        Console.WriteLine("========================================");
        Console.WriteLine("\nCHẠY SCRIPT SQL SAU ĐÂY:");
        Console.WriteLine("========================================");
        Console.WriteLine($"UPDATE pr.Users");
        Console.WriteLine($"SET PasswordHash = N'{hash}'");
        Console.WriteLine($"WHERE UserName = N'admin';");
        Console.WriteLine("========================================");

        // Test verify
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine($"\n✅ Test verify: {isValid}");
    }
}
