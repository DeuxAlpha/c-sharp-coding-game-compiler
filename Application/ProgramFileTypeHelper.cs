using System;
using Application.Models;

namespace Application
{
    public class ProgramFileTypeHelper
    {
        public static string GetClassType(ProgramFileType fileType)
        {
            switch (fileType)
            {
                case ProgramFileType.Class:
                    return "class";
                case ProgramFileType.PartialClass:
                    return "partial class";
                case ProgramFileType.StaticClass:
                    return "static class";
                case ProgramFileType.Interface:
                    return "interface";
                case ProgramFileType.Enum:
                    return "enum";
                case ProgramFileType.Struct:
                    return "struct";
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
        }
    }
}