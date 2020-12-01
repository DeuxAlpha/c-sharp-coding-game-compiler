using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Application.Models
{
    public class ProgramFile
    {
        public FileInfo FileInfo { get; set; }
        public string FileName => FileInfo.Name;
        public string Namespace { get; set; }
        public string FileContent { get; set; }
        public IEnumerable<string> ApplicationImports { get; set; }
        public IEnumerable<string> SystemImports { get; set; }
    }
}