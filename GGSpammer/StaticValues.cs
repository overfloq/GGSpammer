using GGSpammer.Objects;
using LiteDB;

namespace GGSpammer;
internal static class StaticValues
{
    const string DatabaseFilename = "storage.db";

    public static ConcurrentList<RunningTask> RunningTasks { get; } = new();
    public static LiteDatabase Database { get; } = new(new ConnectionString
    {
        Filename = Path.Join(GetCoreDirectory().FullName, DatabaseFilename)
    });

    public static object Lock { get; } = new();
}
