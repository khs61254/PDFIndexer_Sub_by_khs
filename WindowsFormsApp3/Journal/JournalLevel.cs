using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp3.Journal
{
    public enum JournalLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal,
    }

    //public static class LevelExtensions
    //{
    //    public static string ToText(this JournalLevel level)
    //    {
    //        switch (level)
    //        {
    //            case JournalLevel.Debug:
    //                return "Debug";
    //            case JournalLevel.Info:
    //                return "Info";
    //            case JournalLevel.Warning:
    //                return "Warning";
    //            case JournalLevel.Error:
    //                return "Error";
    //            case JournalLevel.Fatal:
    //                return "Fatal";
    //            default:
    //                return "Unknown";
    //        }
    //    }
    //}
}
