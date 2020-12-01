using CommandLine;

namespace Application.Models
{
    public class CliOptions
    {
        [Option('w', "working-directory", Default = ".", HelpText = "The directory which will be used to compile the C# source code.")]
        public string WorkingDirectory { get; set; }
        [Option('e', "entry-file", Default = "./Program.cs", HelpText = "The entry file from which all other files will be compiled.")]
        public string EntryFile { get; set; }
        [Option('o', "output-file", Default = "./dist.cs", HelpText = "The output file that will hold all of the C# source code.")]
        public string OutputFile { get; set; }
    }
}