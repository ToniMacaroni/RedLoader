using System.Collections.Generic;
using System.Reflection;

namespace RedLoader.Bootstrap;

public interface IModProcessor
{
    List<ModBase> LoadPlugins();

    void InitAfterUnity();
}
