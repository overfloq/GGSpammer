using GGSpammer.Interfaces;
using GGSpammer.Objects;

namespace GGSpammer.Pages;
internal class MainMenuPage : PageBase
{
    CollectionListener<RunningTask>? listener;

    public override void OnCloseEvent()
    {
        listener?.Dispose();
    }

    public override void OnOpenEvent()
    {
        RedrawScreen();

        listener = RunningTasks.GetListener();
        listener.ChangeEvent += (x, y) =>
        {
            if (x == ChangeAction.Add || x == ChangeAction.Remove)
            {
                lock(Lock)
                {
                    PrintMenuBottomText(default);
                }
            }
        };

        ShowMenuOptions(default,

            new MenuItem('W', this, (ManagedPage)new WebhookMenuPage())
            { Category = "Raiding Tools", Name = "Webhook Spammer", Description = "Basic tools for spamming through webhooks. You can add or remove\ndifferent webhooks depending on your requirements." },

            new MenuItem('T', this, (ManagedPage)new TaskMgrPage())
            { Name = "Task Manager", Description = "Displays or ends currently running tasks, which have recently been started.\nIt's not recommended to use this due to beta testing of this feature!" });

        ClosePage();
    }
}
