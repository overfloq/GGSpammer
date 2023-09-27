using GGSpammer.DatabaseRecord;
using GGSpammer.Interfaces;
using GGSpammer.Objects;
using LiteDB;
using PastelExtended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GGSpammer.Pages.WebhookPages;
internal partial class ModifyWebhooksPage : PageBase
{
    public override void OnCloseEvent()
    {
    }

    const int PageItems = 11;
    int index = 0;
    int cx, cy;

    void RedrawData()
    {
        var (ox, oy) = Console.GetCursorPosition();
        Console.SetCursorPosition(cx, cy);

        var collection = Database.GetCollection<WebhookRecord>();
        var entriesCount = collection.Count();

        WriteLine($"There {(entriesCount == 1 ? "is" : "are")} {(entriesCount switch
        {
            0 => "no webhooks",
            1 => "one webhook",
            _ => $"{entriesCount} webhooks"
        }).Fg(SecondaryColor)} in your collection!          ".Fg(Color.White));

        Console.WriteLine();
        var webhooks = collection.FindAll().ToList();

        var dest = Math.Min(webhooks.Count, index + PageItems);
        var displayed = 0;
        var slice = webhooks.Skip(index).Take(dest - index);

        foreach (var current in slice)
        {
            var str = $"  {(GetScrollBar(webhooks.Count, index, displayed) ? "|".Fg(PrimaryColor) : " ")} Id {current.Id.ToString().Fg(SecondaryColor)}".Fg(Color.White);

            PastelEx.EraseLine(str);

            const int TokenCut = 50;
            Console.CursorLeft = Console.WindowWidth - 6 - TokenCut;
            //int tokenLength = Console.WindowWidth - Console.CursorLeft - 8;


            var countStr = $"{current.Token.AsSpan(current.Token.Length - TokenCut)}...";
            Console.WriteLine(countStr.Fg(Color.LightGray).Deco(Decoration.Italic));

            displayed++;
        }

        if (displayed < PageItems)
        {
            for (; displayed <= PageItems; displayed++)
            {
                PastelEx.EraseLine();
                Console.WriteLine();
            }
        }

        Console.SetCursorPosition(ox, oy);
    }

    private static bool GetScrollBar(int totalItems, int position, int pageItemIndex)
    {
        if (totalItems <= PageItems)
            return false;

        totalItems -= PageItems;
        var barLocation = Math.Floor(position * PageItems / (double)totalItems);

        return barLocation == pageItemIndex || (position == totalItems && pageItemIndex == PageItems - 1);
    }

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

    public override void OnOpenEvent()
    {
        RedrawScreen();
        (cx, cy) = Console.GetCursorPosition();

        Console.SetCursorPosition(0, Console.WindowHeight - 2);
        RedrawData();

        ManagedInput prompter = new(new()
        {
            UnrecognizedKeyPressed = key =>
            {
                if (key.Key == ConsoleKey.UpArrow)
                {
                    ScrollUp(1);
                    RedrawData();
                }

                if (key.Key == ConsoleKey.DownArrow)
                {
                    ScrollUp(1);
                    RedrawData();
                }
            },
            PromptText = "Command > ".Fg("555")
        });

        while (true)
        {
            var ask = prompter.Ask();
            if (ask is null)
            {
                ClosePage();
                return;
            }

            var span = ask.AsSpan().Trim();
            if (span.StartsWith("NEW", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var webhook = span[3..].Trim();
                    var webhookString = webhook.ToString();
                    var regexUrl = WebhookUrl().Match(webhookString);

                    if (!regexUrl.Success)
                        throw new FormatException();

                    var collection = Database.GetCollection<WebhookRecord>();
                    if (collection.Exists(x => x.Url == webhookString))
                    {
                        prompter.Report("The webhook has already been added!");
                    }
                    else if (!VerifyDiscordWebhook(webhookString))
                    {
                        prompter.Report("The webhook isn't responding!");
                    }
                    else
                    {
                        collection.Insert(new WebhookRecord(webhookString, long.Parse(regexUrl.Groups[1].Value), regexUrl.Groups[2].Value));
                        RedrawData();
                    }
                }
                catch (FormatException)
                {
                    prompter.Report("The webhook has invalid format.");
                }
            }
            else if (span.Equals("CHECK", StringComparison.OrdinalIgnoreCase))
            {
                var collection = Database.GetCollection<WebhookRecord>();
                int count = 0;

                prompter.Updater(updater =>
                {
                    Parallel.ForEach(collection.FindAll(), webhook =>
                    {
                        if (!VerifyDiscordWebhook(webhook.Url))
                        {
                            updater.UpdateText($"{Interlocked.Increment(ref count)} webhooks checked");
                            lock (Lock)
                            {
                                collection.DeleteMany(x => x.Url == webhook.Url);
                                RedrawData();
                            }
                        }
                    });
                });

                if (count == 0)
                    prompter.Report("All webhooks in the database are valid.");
                else
                    prompter.Report($"Removed {count} invalid {(count == 1 ? "entry" : "entries")} from the database.");
            }
            else if (span.StartsWith("DEL", StringComparison.OrdinalIgnoreCase))
            {
                var content = span[3..].Trim();
                var collection = Database.GetCollection<WebhookRecord>();

                if (long.TryParse(content, out var id))
                {
                    // It's an id
                    int count = collection.DeleteMany(x => x.Id == id);

                    if (count == 0)
                        prompter.Report("No database entries have been affected.");
                    else
                        prompter.Report($"Removed {count} {(count == 1 ? "entry" : "entries")} from the database.");
                }
                else if (content.Contains("discord.com/api/webhooks", StringComparison.Ordinal))
                {
                    // It's an url
                    var contentString = content.ToString();
                    int count = collection.DeleteMany(x => x.Url == contentString);

                    if (count == 0)
                        prompter.Report("No database entries have been affected.");
                    else
                        prompter.Report($"Removed {count} {(count == 1 ? "entry" : "entries")} from the database.");
                }
                else
                {
                    // It's a token
                    var contentString = content.ToString();
                    int count = collection.DeleteMany(x => x.Token == contentString);

                    if (count == 0)
                        prompter.Report("No database entries have been affected.");
                    else
                        prompter.Report($"Removed {count} {(count == 1 ? "entry" : "entries")} from the database.");
                }

                RedrawData();
            }
        }
    }

    [GeneratedRegex("https:\\/\\/discord\\.com\\/api\\/webhooks\\/(\\d+)\\/([a-zA-Z0-9_-]+)")]
    private static partial Regex WebhookUrl();

    static bool VerifyDiscordWebhook(string webhookUrl)
    {
        try
        {
            HttpResponseMessage response = httpClient.GetAsync(webhookUrl).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static readonly HttpClient httpClient = new();
}