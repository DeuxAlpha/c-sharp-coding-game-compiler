using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Application.Models;
using CommandLine;

namespace Application
{
    internal static class Program
    {
        private static CliOptions _options;

        private static List<ProgramFile> _usedCode = new List<ProgramFile>();

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args).WithParsed(options =>
            {
                _options = options;
                var watcher = new FileSystemWatcher(options.WorkingDirectory)
                    {Filter = "*.cs", IncludeSubdirectories = true};
                watcher.Changed += OnWatcherTriggered;
                watcher.Created += OnWatcherTriggered;
                watcher.Deleted += OnWatcherTriggered;
                watcher.Renamed += OnWatcherTriggered;
                watcher.EnableRaisingEvents = true;
            });

            Compile();

            Console.WriteLine("Press ESC to stop.");

            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {

            }

            Console.WriteLine("Program closed.");
        }

        private static void OnWatcherTriggered(object sender, FileSystemEventArgs e)
        {
            try
            {
                Compile();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void Compile()
        {
            var directoryInfo = new DirectoryInfo(_options.WorkingDirectory);
            var outputFile = new FileInfo(_options.OutputFile);
            var programFiles = Directory.GetFiles(directoryInfo.FullName, "*.cs", SearchOption.AllDirectories)
                .Where(file => !file.Contains(@"\obj\") && !file.Contains(@"\bin\") && file != outputFile.FullName)
                .Select(file =>
                {
                    using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var streamReader = new StreamReader(fileStream);
                    var fileContent = "";
                    while (streamReader.Peek() >= 0)
                    {
                        fileContent += $"{streamReader.ReadLine()}{Environment.NewLine}";
                    }

                    var namespaceMatch = Regex.Match(fileContent, @"^namespace\s.+$", RegexOptions.Multiline);
                    var importMatch = Regex.Matches(fileContent, @"^using\s.+$", RegexOptions.Multiline);
                    return new ProgramFile
                    {
                        FileInfo = new FileInfo(file),
                        FileContent = fileContent.Trim(),
                        Namespace = namespaceMatch.Value.Replace("namespace ", "").Replace("\r", ""),
                        ApplicationImports = importMatch.Select(match =>
                            match.Value.Replace("using ", "").Replace(";\r", "")),
                        ProgramFileType = GetProgramFileType(fileContent)
                    };
                }).ToList();
            var cleansedFiles = CleanseImports(
                    programFiles,
                    programFiles.Select(file => file.Namespace).ToList())
                .ToList();
            var entryFile = cleansedFiles.First(file =>
                file.FileInfo.Name == _options.EntryFile || file.FileInfo.FullName == _options.EntryFile);

            FilterUnusedCode(entryFile, cleansedFiles);

            var code = GetCode(_usedCode);

            File.WriteAllText(outputFile.FullName, code);

            _usedCode.Clear();
        }

        // Remove imports provided by the system.
        // In other words, only keep the imports that are provided by the application itself.
        private static IEnumerable<ProgramFile> CleanseImports(
            IEnumerable<ProgramFile> programFiles,
            ICollection<string> namespaces)
        {
            return programFiles.Select(file =>
            {
                file.SystemImports = file.ApplicationImports.Where(import => !namespaces.Contains(import));
                file.ApplicationImports = file.ApplicationImports.Where(namespaces.Contains);
                return file;
            });
        }

        private static void FilterUnusedCode(
            ProgramFile entryFile,
            ICollection<ProgramFile> programFiles)
        {
            if (_usedCode.Contains(entryFile)) return;
            _usedCode.Add(entryFile);

            foreach (var import in entryFile.ApplicationImports)
            {
                foreach (var file in programFiles.Where(file => file.Namespace == import))
                {
                    FilterUnusedCode(file, programFiles);
                }
            }
        }

        private static string GetCode(IEnumerable<ProgramFile> programFiles)
        {
            var code = programFiles.ToList();
            var importCollection = code.SelectMany(c => c.SystemImports).Distinct();
            var imports = importCollection
                .Aggregate("", (current, systemImport) => current + $"using {systemImport};{Environment.NewLine}");

            var codeContent = code
                .Select(file => file.FileContent)
                .Select(content => content
                    .Remove(content.LastIndexOf(Environment.NewLine, StringComparison.Ordinal)))
                .Select(contentWithoutLastLine =>
                {
                    var type = GetProgramFileType(contentWithoutLastLine);
                    var typeString = ProgramFileTypeHelper.GetClassType(type);
                    return contentWithoutLastLine
                        .Split(typeString)
                        .Last()
                        .Insert(0, typeString);
                })
                .Aggregate("", (current, contentClass) => current + $"\t{contentClass}{Environment.NewLine}{Environment.NewLine}");

            return $"{imports}\r\nnamespace CodinGame\r\n{{\r\n{codeContent.TrimEnd()}\r\n}}";
        }

        private static ProgramFileType GetProgramFileType(string content)
        {
            var indices = new Dictionary<ProgramFileType, int?>
            {
                {ProgramFileType.Class, content.IndexOf("class", StringComparison.Ordinal)},
                {ProgramFileType.PartialClass, content.IndexOf("partial class", StringComparison.Ordinal)},
                {ProgramFileType.StaticClass, content.IndexOf("static class", StringComparison.Ordinal)},
                {ProgramFileType.Interface, content.IndexOf("interface", StringComparison.Ordinal)},
                {ProgramFileType.Enum, content.IndexOf("enum", StringComparison.Ordinal)},
                {ProgramFileType.Struct, content.IndexOf("struct", StringComparison.Ordinal)}
            };

            return indices
                .Where(index => index.Value >= 0)
                .OrderBy(index => index.Value)
                .Select(index => index.Key)
                .First();
        }
    }
}