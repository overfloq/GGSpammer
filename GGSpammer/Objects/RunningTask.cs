using GGSpammer.Enums;

namespace GGSpammer.Objects;
internal class RunningTask
{
    public required string Name { get; init; }
    public DateTime StartTime { get; }
    public int Id { get; }

    public TaskState State { get; set; } = TaskState.Running;

    private static int _incrementingId = 0;

    private readonly Action<RunningTask> _endTask;
    private bool _cancellationRequested = false;

    public bool CancellationRequested => _cancellationRequested;

    RunningTask(Action<RunningTask> endTask)
    {
        StartTime = DateTime.Now;
        _endTask = endTask;
        Id = Interlocked.Increment(ref _incrementingId);

        RunningTasks.Add(this);
    }

    public static RunningTask Create(string name, Action<RunningTask> endTask)
    {
        return new(endTask) { Name = name };
    }

    public static void CreateAndForget(string name, Action<RunningTask> endTask)
    {
        _ = new RunningTask(endTask) { Name = name };
    }

    public void EndTask()
    {
        State = TaskState.Ending;

        _cancellationRequested = true;
        _endTask(this);

        RunningTasks.Remove(this);
    }
}
