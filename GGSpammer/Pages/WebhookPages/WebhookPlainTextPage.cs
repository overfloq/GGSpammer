using GGSpammer.DatabaseRecord;
using GGSpammer.Interfaces;
using GGSpammer.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GGSpammer.Pages.WebhookPages;
internal class WebhookPlainTextPage : PageBase
{
    public override void OnCloseEvent()
    {
    }

    static readonly HttpClient httpClient = new();

    public override void OnOpenEvent()
    {
        RedrawScreen();

        List<string> lines = new();

        bool showPrompt = true;
        while (true)
        {
            var stringContent = ReceiveInput(showPrompt ? "Your message (empty to complete):" : default, string.Empty, false);
            showPrompt = false;
            Console.WriteLine();

            if (stringContent is null)
            {
                ClosePage();
                return;
            }

            if (stringContent.Length == 0)
            {
                if (lines.Count > 0)
                {
                    break;
                }

                ClosePage();
                return;
            }

            lines.Add(stringContent);
        }

        var httpContent = new StringContent(JsonSerializer.Serialize(
            new
            {
                content = string.Join('\n', lines)
            }), Encoding.UTF8, "application/json");

        CancellationTokenSource cts = new();
        RunningTask.CreateAndForget("Plain-Content Webhook Spammer", _ =>
        {
            cts.Cancel();
        });

        _ = Task.Factory.StartNew(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                foreach (var webhook in Database.GetCollection<WebhookRecord>().FindAll())
                {
                    if (cts.IsCancellationRequested)
                        break;

                    await httpClient.PostAsync(webhook.Url, httpContent, cts.Token);
                }

                Thread.Sleep(3000);
            }
        }, TaskCreationOptions.LongRunning);

        ClosePage();
    }
}
