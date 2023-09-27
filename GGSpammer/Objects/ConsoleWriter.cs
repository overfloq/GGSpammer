namespace GGSpammer.Objects;
internal class ConsoleWriter
{
    readonly object? _syncObject = default;

    ConsoleWriter(bool synchronized)
    {
        if (synchronized)
        {
            _syncObject = new();
        }
    }

    public static ConsoleWriter CreateSynchronized()
    {
        return new(true);
    }

    private static readonly ConsoleWriter _syncWriter = new(true);
    private static readonly ConsoleWriter _nonSyncWriter = new(false);

    public static ConsoleWriter Synchronized => _syncWriter;
    public static ConsoleWriter NonSynchronized => _nonSyncWriter;

    public void Write(string text)
    {
        if (_syncObject == default)
        {
            Console.Write(text);
            return;
        }

        lock (_syncObject)
        {
            Console.Write(text);
        }
    }

    public void Write(char text)
    {
        if (_syncObject == default)
        {
            Console.Write(text);
            return;
        }

        lock (_syncObject)
        {
            Console.Write(text);
        }
    }

    public void WriteLine(string text)
    {
        if (_syncObject == default)
        {
            Console.WriteLine(text);
            return;
        }

        lock (_syncObject)
        {
            Console.WriteLine(text);
        }
    }

    public void WriteLine(char text)
    {
        if (_syncObject == default)
        {
            Console.WriteLine(text);
            return;
        }

        lock (_syncObject)
        {
            Console.WriteLine(text);
        }
    }

    public void WriteLine()
    {
        if (_syncObject == default)
        {
            Console.WriteLine();
            return;
        }

        lock (_syncObject)
        {
            Console.WriteLine();
        }
    }
}
