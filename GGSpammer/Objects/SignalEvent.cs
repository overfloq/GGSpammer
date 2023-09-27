namespace GGSpammer.Objects;
internal class SignalEvent
{
    bool _released = false;
    public void Release()
    {
        _released = true;
    }

    public void Wait()
    {
        while (!_released)
        {
            Thread.Sleep(100);
        }
    }
}
