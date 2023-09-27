using GGSpammer.Objects;
using LiteDB;
using PastelExtended;
using System.Drawing;

namespace GGSpammer;
internal static class StaticUtils
{
    public static readonly Color PrimaryColor = ColorTranslator.FromHtml("#DC143C");
    public static readonly Color SecondaryColor = ColorTranslator.FromHtml("#FF91A4");

    internal static T ReceiveInput<T>(ReadOnlySpan<char> text) where T : struct, ISpanParsable<T> =>
        ReceiveInput<T>(text, true);
    internal static T ReceiveInput<T>(ReadOnlySpan<char> text, bool oneLine) where T : struct, ISpanParsable<T>
    {
        (int X, int Y) cursor;

        string prompt;
        if (oneLine)
        {
            prompt = $"\r  | {$"{text}".Fg(Color.White)} ".Fg(PrimaryColor);
        }
        else
        {
            prompt = "\r  | ".Fg(PrimaryColor);
            StringTypewriter.WriteLine($"  {"|".Fg(PrimaryColor)} {text}".Fg(Color.White));
        }

        StringTypewriter.Write(prompt);
        cursor = Console.GetCursorPosition();

        for (; ; )
        {
            var str = Console.ReadLine().AsSpan();
            if (!str.IsEmpty && T.TryParse(str.Trim(), default, out var result))
                return result;

            Console.SetCursorPosition(cursor.X, cursor.Y);
            Console.Write(new string(' ', str.Length));
            Console.SetCursorPosition(cursor.X, cursor.Y);
            Console.Write(prompt);
        }
    }

    internal static T ReceiveInput<T>(ReadOnlySpan<char> text, T defaultValue) where T : struct, ISpanParsable<T> =>
        ReceiveInput(text, defaultValue, true);
    internal static T ReceiveInput<T>(ReadOnlySpan<char> text, T defaultValue, bool oneLine) where T : struct, ISpanParsable<T>
    {
        (int X, int Y) cursor;

        string prompt;
        if (oneLine)
        {
            prompt = $"\r  | {$"{text}".Fg(Color.White)} ".Fg(PrimaryColor);
        }
        else
        {
            prompt = "\r  | ".Fg(PrimaryColor);
            StringTypewriter.WriteLine($"  {"|".Fg(PrimaryColor)} {text}".Fg(Color.White));
        }

        StringTypewriter.Write(prompt);
        cursor = Console.GetCursorPosition();

        for (; ; )
        {
            var str = Console.ReadLine().AsSpan();
            if (str.IsEmpty) return defaultValue;

            if (T.TryParse(str.Trim(), default, out var result))
                return result;

            Console.SetCursorPosition(cursor.X, cursor.Y);
            Console.Write(new string(' ', str.Length));
            Console.SetCursorPosition(cursor.X, cursor.Y);
            Console.Write(prompt);
        }
    }

    internal static string? ReceiveInput(ReadOnlySpan<char> text) =>
        ReceiveInput(text, true);
    internal static string? ReceiveInput(ReadOnlySpan<char> text, bool oneLine)
    {
        string prompt;
        if (text != default)
        {
            if (oneLine)
            {
                prompt = $"\r  | {$"{text}".Fg(Color.White)} ".Fg(PrimaryColor);
            }
            else
            {
                prompt = "\r  | ".Fg(PrimaryColor);
                StringTypewriter.WriteLine($"  {"|".Fg(PrimaryColor)} {text}".Fg(Color.White));
            }
        }
        else
        {
            prompt = "\r  | ".Fg(PrimaryColor);
        }

        StringTypewriter.Write(prompt);

        var prompter = new ManagedInput(new() { AutoClear = false });

        for (; ; )
        {
            var str = prompter.Ask();
            if (str is null)
                return null;

            if (str.Length > 0)
                return str;

            prompter.ClearLine();
        }
    }

    internal static string? ReceiveInput(ReadOnlySpan<char> text, string defaultValue) =>
        ReceiveInput(text, defaultValue, true);
    internal static string? ReceiveInput(ReadOnlySpan<char> text, string defaultValue, bool oneLine)
    {
        string prompt;
        if (text != default)
        {
            if (oneLine)
            {
                prompt = $"\r  | {$"{text}".Fg(Color.White)} ".Fg(PrimaryColor);
            }
            else
            {
                prompt = "\r  | ".Fg(PrimaryColor);
                StringTypewriter.WriteLine($"  {"|".Fg(PrimaryColor)} {text}".Fg(Color.White));
            }
        }
        else
        {
            prompt = "\r  | ".Fg(PrimaryColor);
        }

        StringTypewriter.Write(prompt);

        var prompter = new ManagedInput(new() { AutoClear = false });

        for (; ; )
        {
            var str = prompter.Ask();
            if (str is null)
                return null;

            if (str.Length > 0) return str;
            return defaultValue;
        }
    }

    public static void Write(ReadOnlySpan<char> chars)
    {
        Console.Write($"  {"|".Fg(PrimaryColor)} {chars}".Fg(Color.White));
    }

    public static void WriteSlow(ReadOnlySpan<char> chars)
    {
        Console.Write($"  {"|".Fg(PrimaryColor)} ");
        StringTypewriter.Write($"{chars}".Fg(Color.White));
    }

    public static void WriteLine(ReadOnlySpan<char> chars)
    {
        Console.WriteLine($"  {"|".Fg(PrimaryColor)} {chars}".Fg(Color.White));
    }

    public static void WriteLineSlow(ReadOnlySpan<char> chars)
    {
        Console.Write($"  {"|".Fg(PrimaryColor)} ");
        StringTypewriter.WriteLine($"{chars}".Fg(Color.White));
    }

    public static void RedrawScreen(bool clear = true)
    {
        if (clear)
            PastelEx.Clear();

        Console.WriteLine("""

             ____  ____   ____                                            
            / ___|/ ___| / ___| _ __   __ _ _ __ ___  _ __ ___   ___ _ __ 
           | |  _| |  _  \___ \| '_ \ / _` | '_ ` _ \| '_ ` _ \ / _ \ '__|
           | |_| | |_| |  ___) | |_) | (_| | | | | | | | | | | |  __/ |   
            \____|\____| |____/| .__/ \__,_|_| |_| |_|_| |_| |_|\___|_|   
                               |_|                                        

        """.Fg(PrimaryColor));
    }

    static readonly string[] loadingStrings = new[]
    {
        "Getting all the things ready ...",
        "You can take a cup of the coffee, while we're working on it ...",
        "Initializing tool to give you the best expecience ...",
        "We're almost there, give us a moment ...",
        "Take a glass of water, while we're working on it ...",
        "We're almost there, just a few more moments ...",
        "Please hold on while we get everything set up ...",
        "We're getting things ready behind the scenes ..."
    };

    public static void ShowLoadingScreen(Action action)
    {
        int totalProgressLength = Console.WindowWidth - 4;
        int marqueeLength = totalProgressLength / 4;

        RedrawScreen();

        Console.SetCursorPosition(2, Console.WindowHeight - 3);
        Console.WriteLine(loadingStrings[Random.Shared.Next(loadingStrings.Length)].Fg(Color.White));

        Console.CursorVisible = false;
        bool progressCancellationRequested = false;

        using var task = Task.Run(action)
            .ContinueWith(task =>
        {
            progressCancellationRequested = true;
        });

        while (!progressCancellationRequested)
        {
            for (int i = -marqueeLength; i < totalProgressLength + marqueeLength; i++)
            {
                if (progressCancellationRequested)
                    break;

                PrintProgress(i, totalProgressLength, marqueeLength);
                Thread.Sleep(10);
            }
        }

        Console.CursorVisible = true;
    }

    const char ProgressChar = '█';
    static void PrintProgress(int offset,
        int totalProgressLength, int marqueeLength)
    {
        Console.Write("\r  ");

        if (offset < 0)
        {
            int len = offset + marqueeLength;
            Console.Write(new string(ProgressChar, len).Fg(PrimaryColor));
        }
        else if (offset == 0)
        {
            Console.Write(new string(ProgressChar, marqueeLength).Fg(PrimaryColor));
        }
        else if (offset > totalProgressLength - marqueeLength)
        {
            int len = totalProgressLength - offset;

            if (len > 0)
                Console.Write($"{new string(' ', offset)}{new string(ProgressChar, len).Fg(PrimaryColor)}");
            else
                Console.Write(new string(' ', totalProgressLength));
        }
        else
        {
            Console.Write($"{new string(' ', offset)}{new string(ProgressChar, marqueeLength).Fg(PrimaryColor)}");
        }
    }

    public static void PrintMenuBottomText(string[]? path)
    {
        var (ox, oy) = Console.GetCursorPosition();

        Console.SetCursorPosition(0, Console.WindowHeight - 3);

        var runningTasksString = RunningTasks.Count switch
        {
            0 => "no running tasks",
            1 => "1 running task",
            _ => $"{RunningTasks.Count} running tasks"
        };

        Console.WriteLine($"  /{(path == default ? "home" : $"home/{string.Join('/', path)}")}/".Fg("444"));

        Console.Write(runningTasksString.PadLeft(Console.WindowWidth - 2)
            .Fg("444"));

        Console.Write($"\r  version {VERSION} ({CHANNEL})".Fg("444"));

        Console.SetCursorPosition(ox, oy);
    }

    public static char ShowMenuOptions(string[]? path, params MenuItem[] items)
    {
        if (!items.GroupBy(x => x.Icon).All(x => x.Count() == 1))
            throw new InvalidDataException("Internal Error - Got duplicate icons in the menu!");

        var groups = items.GroupBy(x => x.Category)/*.Select(x => x.Chunk(4)).ToList();*/;
        var maxSize = items.Max(x => x.Name.Length);

        int x = 0, y = 0;
        lock (Lock)
        {
            RedrawScreen();
            PrintMenuBottomText(path);

            foreach (var group in groups)
            {
                var chunks = group.Chunk(4).ToList();
                var groupName = group.Key;

                if (groupName is not null)
                {
                    Console.WriteLine($"   {groupName.Fg(Color.White).Deco(Decoration.Bold, Decoration.Underline)}");
                }

                for (int i = 0; i < chunks.Count; i++)
                {
                    var currentChunk = chunks[i];

                    // Process all items in current chunk and print it to stdout.
                    Console.Write(' ');
                    foreach (var item in currentChunk)
                    {
                        Console.Write($"  {"[".Fg("333")}{item.Icon.ToString().Fg(SecondaryColor)}{"]".Fg("333")} {item.Name.PadRight(maxSize).Fg("ddd")}");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            //Console.WriteLine();
            (x, y) = Console.GetCursorPosition();
        }

        char previousKey = default;
        int previousLines = 0;

        while (true)
        {
            var pressed = Console.ReadKey(true);
            if (pressed.Key == ConsoleKey.Escape)
                return default;

            var pressedChar = char.ToUpper(pressed.KeyChar);

            var menuItem = items.FirstOrDefault(x => x.Icon == pressedChar);
            if (menuItem == default)
                continue;

            if (pressedChar == previousKey)
            {
                menuItem.InvokeSelectedEvent();
                return pressedChar;
            }

            var description = menuItem.Description;
            previousKey = pressedChar;

            lock (Lock)
            {
                Console.SetCursorPosition(x, y);

                var interatorLength = Math.Max(menuItem.DescriptionLines().Length, previousLines);
                for (int i = 0; i < interatorLength; i++)
                {
                    if (i < menuItem.DescriptionLines().Length)
                    {
                        var line = menuItem.DescriptionLines()[i];
                        PastelEx.EraseLine($"   {line}".Fg("#666666").Deco(Decoration.Italic));
                    }
                    else
                    {
                        PastelEx.EraseLine();
                    }

                    Console.WriteLine();
                }

                previousLines = menuItem.DescriptionLines().Length;
            }
        }
    }

    public static string GetFriendlyTimespanRepresentation(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 0)
        {
            return "in the future";
        }
        else if (timeSpan.TotalSeconds < 1)
        {
            return "just now";
        }
        else if (timeSpan.TotalMinutes < 1)
        {
            int seconds = (int)timeSpan.TotalSeconds;
            return $"{seconds} second{(seconds == 1 ? "" : "s")} ago";
        }
        else if (timeSpan.TotalHours < 1)
        {
            int minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }
        else
        {
            int hours = (int)timeSpan.TotalHours;
            return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }
    }

    static readonly DirectoryInfo coreDirectory = new(Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GGSpammer"));

    public static DirectoryInfo GetCoreDirectory()
    {
        if (!coreDirectory.Exists)
            coreDirectory.Create();

        return coreDirectory;
    }

    public static DirectoryInfo GetCoreDirectory(string childFolder)
    {
        var core = GetCoreDirectory();
        DirectoryInfo secondaryPath = new(Path.Join(core.FullName, childFolder));

        if (!secondaryPath.Exists)
            secondaryPath.Create();

        return secondaryPath;
    }
}
