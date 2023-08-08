using UnityEngine;

namespace SUI;

public class SPanelOptions : SContainerOptions
{
    public string Id { get; internal set; }
    public SPanelOptions(GameObject root) : base(root)
    {
    }
}