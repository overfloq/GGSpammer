using GGSpammer.Interfaces;

namespace GGSpammer.Objects;
internal class MenuItem
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public char Icon { get; }
    public string? Category { get; set; }

    public Action<MenuItem>? SelectedEvent { get; }

    public MenuItem(char icon, Action<MenuItem> selectedEvent)
    {
        Icon = char.ToUpper(icon);
        SelectedEvent = selectedEvent;
    }

    public MenuItem(char icon)
    {
        Icon = char.ToUpper(icon);
        SelectedEvent = default;
    }

    public MenuItem(char icon, ManagedPage displayPage)
    {
        Icon = char.ToUpper(icon);
        SelectedEvent = _ =>
        {
            displayPage.OpenAndWait();
        };
    }

    public MenuItem(char icon, PageBase parent, ManagedPage displayPage)
    {
        Icon = char.ToUpper(icon);
        SelectedEvent = _ =>
        {
            parent.ClosePage();
            displayPage.OpenAndWait();
        };
    }

    public void InvokeSelectedEvent()
    {
        SelectedEvent?.Invoke(this);
    }

    private string[]? _modDescription = null;
    public string[] DescriptionLines()
    {
        _modDescription ??= Description.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        return _modDescription;
    }
}
