namespace ModManager;

internal abstract class BaseInstaller
{
    protected readonly string _name;

    protected BaseInstaller(string name)
    {
        _name = name;
    }
    
    public virtual void Install()
    {
    }
}