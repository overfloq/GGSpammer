using GGSpammer.Objects;

namespace GGSpammer.Interfaces;
internal abstract class PageBase
{
    /// <summary>
    /// This method should always, no matter what, call <see cref="ClosePage"/> method to signal this page is closed
    /// and waiting handle can be released.
    /// </summary>
    public abstract void OnOpenEvent();

    /// <summary>
    /// This method is called before the actual page is closed. In this method, all resources should be disposed, all
    /// timers should be ended (and should be waited for it's exit).
    /// </summary>
    public abstract void OnCloseEvent();

    public SignalEvent LocalSignalEvent = new();

    public void ClosePage()
    {
        LocalSignalEvent.Release();
        OnCloseEvent();
    }

    public void OpenPage()
    {
        OnOpenEvent();
    }
}
