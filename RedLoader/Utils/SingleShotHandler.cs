namespace RedLoader.Utils;

public struct SingleShotHandler
{
    private bool _hasTriggered;

    public bool HasTriggered => _hasTriggered;

    public void Reset()
    {
        _hasTriggered = false;
    }

    public bool Trigger()
    {
        if (_hasTriggered)
        {
            return false;
        }

        _hasTriggered = true;
        return true;
    }
}