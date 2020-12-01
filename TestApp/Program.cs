using System;
using System.IO;
using TestApp.Services;

namespace TestApp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            TestService.Test("Testing");
            Console.WriteLine("Something");
            Console.WriteLine("Something else yoss");
        }
    }
}