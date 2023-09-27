using GGSpammer.Interfaces;
using GGSpammer.Objects;
using GGSpammer.Pages.WebhookPages;

namespace GGSpammer.Pages;
internal class WebhookMenuPage : PageBase
{
    CollectionListener<RunningTask>? listener;

    public override void OnCloseEvent()
    {
        listener?.Dispose();
    }

    public override void OnOpenEvent()
    {
        RedrawScreen();

        var path = new[] { "webhooks" };

        listener = RunningTasks.GetListener();
        listener.ChangeEvent += (x, y) =>
        {
            if (x == ChangeAction.Add || x == ChangeAction.Remove)
            {
                lock (Lock)
                {
                    PrintMenuBottomText(path);
                }
            }
        };

        if (
            ShowMenuOptions(path,
            new MenuItem('E', this, (ManagedPage)new TaskMgrPage())
            { Category = "Webhook Spammer", Name = "Images", Description = "Spams images with optional plain content. Can be laggy and slower." },
            new MenuItem('P', this, (ManagedPage)new WebhookPlainTextPage())
            { Category = "Webhook Spammer", Name = "Plain Content", Description = "Spams using a basic plain messages, without any embeds or images." },

            new MenuItem('S', this, (ManagedPage)new ModifyWebhooksPage())
            { Name = "See webhooks", Description = "Display, add or remove webhooks. Webhooks are stored persistently on the disk\nand can be modified using a basic text editor." }

        ) == default)
        {
            ClosePage();
            return;
        }

        ClosePage();
        new ManagedPage(new WebhookMenuPage())
            .Open();
    }
}
