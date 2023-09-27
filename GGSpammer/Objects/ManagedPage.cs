using GGSpammer.Interfaces;

namespace GGSpammer.Objects;
internal class ManagedPage
{
    private readonly PageBase _page;

    public ManagedPage(PageBase page)
    {
        _page = page;
    }

    public void Open()
    {
        _page.OpenPage();
    }

    public void Wait()
    {
        _page
            .LocalSignalEvent.Wait();
    }

    public void OpenAndWait()
    {
        Open();
        Wait();
    }

    public static explicit operator ManagedPage(PageBase page)
    {
        return new ManagedPage(page);
    }
}
