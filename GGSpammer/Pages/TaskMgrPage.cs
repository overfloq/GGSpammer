using GGSpammer.Interfaces;
using GGSpammer.Objects;
using PastelExtended;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace GGSpammer.Pages;
internal sealed class TaskMgrPage : PageBase
{
    int index = 0;
    int cx, cy;
    const int PageItems = 13;

    static bool acceptsInput = true;

    Timer? redrawTimer;

    public void ScrollUp(int amount)
    {
        if (index > 0)
        {
            index = Math.Max(index - amount, 0);
        }
    }

    public void ScrollDown(int amount)
    {
        if (index < RunningTasks.Count - PageItems)
        {
            index = Math.Min(index + amount, RunningTasks.Count - PageItems);
        }
    }

    public void EnsureIndex(int count)
    {
        if (index < 0)
        {
            index = 0;
        }
        else if (index > count - PageItems)
        {
            index = Math.Max(0, count - PageItems);
        }
    }

    private void RedrawData()
    {
        lock (Lock)
        {
            Console.CursorVisible = false;
            var (ox, oy) = Console.GetCursorPosition();
            Console.SetCursorPosition(cx, cy);

            int displayed = 0;
            var tasksCopy = RunningTasks.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            EnsureIndex(tasksCopy.Count);
            int dest = Math.Min(tasksCopy.Count, index + PageItems);

            for (int i = index; i < dest; i++)
            {
                var runningTask = tasksCopy[i];

                /*var str = $"  {(GetScrollBar(tasksCopy.Count, index, displayed) ? "|".Fg(PrimaryColor) : " ")} {$"{runningTask.Name.Fg(SecondaryColor)} [#{runningTask.Id}]".Fg(SecondaryColor)} {$"started {GetFriendlyTimespanRepresentation(DateTime.Now - runningTask.StartTime)
                    .Fg(Color.LightGray).Deco(Decoration.Italic)}".Fg(Color.White)} {(runningTask.State == Enums.TaskState.Ending ?
                    "[stopping]".Fg(SecondaryColor).Deco(Decoration.Bold, Decoration.Italic) : " ")}";

                var originalLength = PastelEx.GetInformation(str).OriginalLength;

                Console.WriteLine($"{str}{new string(' ', Console.WindowWidth - originalLength - 1)}");*/
                var str = $"  {(GetScrollBar(tasksCopy.Count, index, displayed) ? "|".Fg(PrimaryColor) : " ")} " +
                    $"#{runningTask.Id}".PadLeft(4).Fg(SecondaryColor) +
                    $" {runningTask.Name.Fg(Color.White)}";

                string status = runningTask.State == Enums.TaskState.Running ?
                    GetFriendlyTimespanRepresentation(DateTime.Now - runningTask.StartTime).Fg(Color.LightGray).Deco(Decoration.Italic)
                    : "stopping".Fg(SecondaryColor).Deco(Decoration.Italic);


                int spacesToOverwrite = Console.WindowWidth -
                    PastelEx.GetInformation(status).OriginalLength - PastelEx.GetInformation(str).OriginalLength - 5;

                // Generate a string with spaces
                string spaces = new string(' ', spacesToOverwrite);

                // Write the modified string followed by the spaces
                Console.Write(str);
                Console.Write(spaces);

                Console.WriteLine(status);

                displayed++;
            }

            if (displayed < PageItems)
            {
                // Less than 12, needs to overwrite leading positions.
                for (; displayed <= PageItems; displayed++)
                {
                    PastelEx.EraseLine();
                    Console.WriteLine();
                }
            }

            Console.SetCursorPosition(ox, oy);
            Console.CursorVisible = true;
        }
    }

    private static bool GetScrollBar(int totalItems, int position, int pageItemIndex)
    {
        if (totalItems <= PageItems)
            return false;

        totalItems -= PageItems;
        var barLocation = Math.Floor(position * PageItems / (double)totalItems);

        return barLocation == pageItemIndex || (position == totalItems && pageItemIndex == PageItems - 1);
    }

    public override void OnOpenEvent()
    {
        RedrawScreen();
        (cx, cy) = Console.GetCursorPosition();

        Console.SetCursorPosition(0, Console.WindowHeight - 2);
        Console.Write("  Command > ".Fg("555"));

        redrawTimer = new(_ => RedrawData(), default, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        using var collectionListener = RunningTasks.GetListener();
        collectionListener.ChangeEvent += (x, y) =>
        {
            if (x != ChangeAction.Update)
                RedrawData();
        };

        StringBuilder builder = new();
        while (true)
        {
            var readKey = Console.ReadKey(true);

            if (readKey.Key == ConsoleKey.Escape)
            {
                ClosePage();
                return;
            }

            if (readKey.Key == ConsoleKey.UpArrow)
            {
                ScrollUp(1);
                RedrawData();
                continue;
            }

            if (readKey.Key == ConsoleKey.DownArrow)
            {
                ScrollDown(1);
                RedrawData();
                continue;
            }

            if (readKey.Key == ConsoleKey.Enter)
            {
                if (!acceptsInput)
                    continue;

                acceptsInput = false;
                var outputString = builder.ToString();

                Task.Run(() =>
                {
                    var outputSpan = outputString.AsSpan().Trim();
                    if (outputSpan.StartsWith("END", StringComparison.OrdinalIgnoreCase))
                    {
                        var statement = outputSpan[3..].Trim();

                        if (statement.Length == 1 &&
                            statement[0] == '*')
                        {
                            // End all the tasks.
                            Parallel.ForEach(RunningTasks.ToList(), task => task.EndTask());
                        }

                        if (statement.StartsWith("REGEX", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var regex = statement[5..].Trim()[1..].Trim();
                                var regularExpression = new Regex(regex.ToString());

                                Parallel.ForEach(RunningTasks.ToList().Where(x => regularExpression.Match(x.Name).Success),
                                    task => task.EndTask());
                            }
                            catch (FormatException) { }
                        }
                        else
                        {
                            try
                            {
                                var id = statement.Trim();
                                int rangeIndex;
                                if ((rangeIndex = id.IndexOf("..")) == -1)
                                {
                                    try
                                    {
                                        if (int.TryParse(id, out var result))
                                        {
                                            var runningTask = RunningTasks[result - 1];
                                            runningTask.EndTask();
                                        }
                                    }
                                    catch (IndexOutOfRangeException) { }
                                }
                                else
                                {
                                    var from = id[..rangeIndex];
                                    var to = id[(rangeIndex + 2)..];

                                    if (from.Length == 0)
                                        from = "0";

                                    if (to.Length == 0)
                                        to = RunningTasks.Max(x => x.Id).ToString();

                                    try
                                    {
                                        if (int.TryParse(from, out var fromInt) &&
                                            int.TryParse(to, out var toInt))
                                        {
                                            var list = RunningTasks.Where(x =>
                                                x.Id >= fromInt && x.Id <= toInt).ToList();

                                            Parallel.ForEach(list, task => task.EndTask());
                                        }
                                    }
                                    catch (IndexOutOfRangeException) { }
                                }
                            }
                            catch (FormatException) { }
                        }
                    }
                }).ContinueWith(_ =>
                {
                    acceptsInput = true;
                });

                // -------------------

                lock (Lock)
                {
                    for (int i = 0; i < builder.Length; i++)
                    {
                        Console.Write("\b \b");
                    }
                }

                builder.Clear();
            }

            lock (Lock)
            {
                if (readKey.Key == ConsoleKey.Backspace && builder.Length > 0)
                {
                    Console.Write("\b \b");
                    builder.Remove(builder.Length - 1, 1);
                }
                else if (!char.IsControl(readKey.KeyChar) && builder.Length < 76)
                {
                    Console.Write(readKey.KeyChar);
                    builder.Append(readKey.KeyChar);
                }
            }
        }
    }

    public override void OnCloseEvent()
    {
        lock (Lock)
        {
            redrawTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            redrawTimer?.Dispose();
        }
    }
}
