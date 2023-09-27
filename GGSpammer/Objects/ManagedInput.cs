using GGSpammer.DatabaseRecord;
using PastelExtended;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGSpammer.Objects;
internal sealed class ManagedInput
{
    private readonly ManagedInputParameters _parameters;

    public ManagedInput(ManagedInputParameters parameters)
    {
        _parameters = parameters;
    }

    readonly StringBuilder builder = new();

    private void RedrawPrompt()
    {
        PastelEx.EraseLine();
        Console.CursorLeft = _parameters.ConsoleOffset;
        Console.Write(_parameters.PromptText);
    }

    public string? Ask()
    {
        RedrawPrompt();

        while (true)
        {
            var readKey = Console.ReadKey(true);

            if (readKey.Key == ConsoleKey.Escape)
            {
                return null;
            }

            if (readKey.Key == ConsoleKey.Enter)
            {
                var stringValue = builder.ToString();

                if (_parameters.AutoClear)
                {
                    lock (Lock)
                    {
                        for (int i = 0; i < Math.Min(builder.Length, _parameters.MaximumSize); i++)
                        {
                            Console.Write("\b \b");
                        }
                    }
                }

                builder.Clear();
                return stringValue;
            }

            lock (Lock)
            {
                if (readKey.Key == ConsoleKey.Backspace && builder.Length > 0)
                {
                    if (builder.Length > _parameters.MaximumSize)
                    {
                        builder.Remove(builder.Length - 1, 1);
                        Console.CursorLeft -= _parameters.MaximumSize;
                        Console.Out.Write(builder.ToString().AsSpan(builder.Length - _parameters.MaximumSize));
                    }
                    else
                    {
                        Console.Write("\b \b");
                        builder.Remove(builder.Length - 1, 1);
                    }
                }
                else if (!char.IsControl(readKey.KeyChar))
                {
                    builder.Append(readKey.KeyChar);

                    if (builder.Length > _parameters.MaximumSize)
                    {
                        Console.CursorLeft -= _parameters.MaximumSize;
                        Console.Out.Write(builder.ToString().AsSpan(builder.Length - _parameters.MaximumSize));
                    }
                    else
                    {
                        Console.Write(readKey.KeyChar);
                    }
                }
                else
                {
                    _parameters.UnrecognizedKeyPressed?.Invoke(readKey);
                }
            }
        }
    }

    public void Report(string message, ReportSeverity severity = ReportSeverity.None)
    {
        if (builder.Length == 0)
        {
            lock (Lock)
            {
                PastelEx.EraseLine();
                Console.CursorLeft = _parameters.ConsoleOffset;

                Console.Write(message.Fg(severity switch
                {
                    ReportSeverity.Error => SecondaryColor,
                    _ => Color.LightGray
                }).Deco(Decoration.Italic));

                Console.ReadKey(true);
                for (int i = 0; i < message.Length; i++)
                {
                    Console.Write("\b \b");
                }
            }
            return;
        }
        else
        {
            throw new NotSupportedException("Message must be empty to show a report message!");
        }
    }

    public void Updater(Action<RealtimeUpdater> action)
    {
        var updater = RealtimeUpdater.Init(_parameters);
        action(updater);

        ClearLine();
    }

    public void ClearLine()
    {
        lock (Lock)
        {
            for (int i = 0; i < Math.Min(builder.Length, _parameters.MaximumSize); i++)
            {
                Console.Write("\b \b");
            }
        }
    }

    internal enum ReportSeverity
    {
        None,
        Error
    }
}