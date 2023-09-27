namespace GGSpammer.Objects;

internal sealed class ManagedInputParameters
{
    public Action<ConsoleKeyInfo>? UnrecognizedKeyPressed { get; init; }
    public int MaximumSize { get; init; } = -1;
    public bool AutoClear { get; init; } = true;
    public string PromptText { get; init; } = string.Empty;
    public int ConsoleOffset { get; init; } = 2;

    public ManagedInputParameters()
    {
        if (MaximumSize == -1)
        {
            MaximumSize = Console.WindowWidth - Console.CursorLeft - 2;
        }
    }
}