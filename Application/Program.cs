using System;
using System.IO;
using System.Linq;
using Application.Models;
using CommandLine;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args).WithParsed(options =>
            {
                var directoryInfo = new DirectoryInfo(options.WorkingDirectory);
                var cSharpFiles = Directory.GetFiles(directoryInfo.FullName, "*.cs", SearchOption.AllDirectories);

            });
        }
    }
}