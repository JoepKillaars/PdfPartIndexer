using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfPartIndexer.Utils;

internal static class LogHelper
{
    private static LogContext _context = new LogContext();

    public static LogContext Context => _context;

    public static void DisplayContextResult()
    {
        LogInfo("");

        if(!_context.Errors.Any())
            LogMessage("Execution completed.", ConsoleColor.Green);
        else
            LogInfo("Execution completed.");

        if (_context.Warnings.Any())
            LogMessage($"    {_context.Warnings.Count()} Warning(s)", ConsoleColor.DarkYellow);
        else
            LogInfo("    0 Warning(s)");

        if (_context.Errors.Any())
            LogMessage($"    {_context.Errors.Count()} Error(s)", ConsoleColor.Red);
        else
            LogInfo("    0 Error(s)");

        // if(_context.Warnings.Any())
        // {
        //     LogInfo("");
        //     foreach (var w in _context.Warnings)
        //         LogWarning(w);
        // }
        // 
        // if (_context.Errors.Any())
        // {
        //     LogInfo("");
        //     foreach (var e in _context.Errors)
        //         LogException(null, e);
        // }
    }

    public static void LogException(Exception ex)
        => LogException(ex, ex?.Message ?? "-");

    public static void LogException(string message)
        => LogException(null, message);

    public static void LogException(Exception? ex, string message)
    {
        _context.AddError(message);

        LogMessage(message, ConsoleColor.Red);
    }

    public static void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public static void LogDebug(string message)
    {
        LogMessage($"  {message}", ConsoleColor.DarkYellow);
    }

    public static void LogWarning(string message)
    {
        _context.AddWarning(message);

        LogMessage($"  {message}", ConsoleColor.Yellow);
    }

    private static void LogMessage(string message, ConsoleColor color)
    {
        var clr = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = clr;
    }
}
