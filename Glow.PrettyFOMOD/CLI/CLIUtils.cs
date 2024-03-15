using System.Text;
using Glow.PrettyFOMOD.Configuration;
using Sharprompt;
using Spectre.Console;

namespace Glow.PrettyFOMOD.CLI;

public static class CliUtils
{
    private static readonly object MessageLock= new();
    public static void PrintSeparator()
    {
        Console.WriteLine("---------------");
        Console.WriteLine();
    }
    
    public static PrettyFomodConfig ConfigFromCli()
    {
        var config = new PrettyFomodConfig();
        
        Console.OutputEncoding = Encoding.UTF8;
        AnsiConsole.Write(new FigletText("PrettyFOMOD").LeftJustified().Color(Color.MediumOrchid3));
        
        WriteWarningText("PLEASE NOTE: This tool is intended to be as non-destructive as possible.\n" +
                         "That said, it is currently in very early stages of development.\nIf you are working with " +
                         "an active FOMOD in this folder, it will be backed up, but if you want to be extra cautious, " +
                         "make a backup first then run this program again.");
        
        // TODO: Folder select goes here. Set `cwd` based on this.

        const string testPrompt = "Run in test mode? (You probably don't want this unless you're working in an IDE";
        const string dummyPrompt = "Use dummy file names? (Useful for verification, won't work in mod organizers)";
        const string generateFullPrompt = "Generate entire FOMOD?";
        const string smartConditionPrompt = "Generate smart conditions for EXISTING FOMOD?";
        
        config.Test              = Prompt.Confirm(testPrompt, defaultValue: false);
        config.UseDummyFileNames = Prompt.Confirm(dummyPrompt, defaultValue: false);
        config.GenerateFull      = Prompt.Confirm(generateFullPrompt, defaultValue: true);
        if (!config.GenerateFull)
        {
            config.SmartConditions   = Prompt.Confirm(smartConditionPrompt, defaultValue: false);
        }

        if (config is not { GenerateFull: true, SmartConditions: true }) return config;
        
        WriteWarningText("Smart conditions requires a FOMOD created by the creation tool" +
                         "\n Generating a full one does not. Select one or the other please.");
        throw new InvalidProgramException();

    }

    private static string GetFolderPath()
    {
        // FolderBrowserDialogue 
        return "";
    }

    public static void WriteStepwiseText(string text)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold]{text}[/]");
    }
    
    public static void WriteWarningText(string text)
    {
        WriteColoredText(text, ConsoleColor.DarkRed);
    }

    public static void WriteHeaderText(string text)
    {
        WriteColoredText(text, ConsoleColor.DarkGreen);
    }

    private static void WriteColoredText(string text, ConsoleColor backgroundColor)
    {
        lock (MessageLock)
        {
            Console.WriteLine();
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    public static void ConfirmExit(bool happy = true)
    {
        AnsiConsole.MarkupLine(happy
            ? ":cherry_blossom: Process completed! Press any key to exit~"
            : ":confounded_face: Press any key to exit.");
        Console.ReadLine();
    }

    public static void WriteException(Exception e)
    {
        AnsiConsole.MarkupLine("[bold red]An exception occured :([/]");
        AnsiConsole.WriteException(e);
        ConfirmExit(happy: false);
    }
}
     