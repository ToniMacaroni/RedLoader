using RedLoader;
using UnityEngine;
using UnityEngine.UI;

namespace SUI;

public class STabControllerOptions : SContainerOptions
{
    public Dictionary<string, TabDefinition> Tabs = new();
    
    public SContainerOptions TabHeader;
    public SContainerOptions TabDivider;
    public SContainerOptions TabContainer;
    
    private TabDefinition? _currentTab;
    private SUI.BackgroundDefinition _buttonBackground;

    private static readonly Color InactiveColor = new(0.3f, 0.3f, 0.3f);
    private static readonly Color ActiveColor = Color.white;

    public STabControllerOptions(GameObject root) : base(root)
    {
        TabHeader = new SContainerOptions(root.transform.Find("TabHeader").gameObject);
        TabDivider = new SContainerOptions(root.transform.Find("TabDivider").gameObject);
        TabContainer = new SContainerOptions(root.transform.Find("TabContent").gameObject);

        _buttonBackground =
            new SUI.BackgroundDefinition(SUI.ColorFromString("#222222"), SUI.GetBackgroundSprite(EBackground.Round28), Image.Type.Sliced);
    }

    public STabControllerOptions AddTab(TabDefinition tab)
    {
        var btn = SUI.SBgButton.Text(tab.Name).Background(_buttonBackground).Ppu(6).Notify(() => ShowTab(tab.Id)).FontColor(InactiveColor).Color(Color.clear);
        
        tab.Tab = btn;

        Tabs.Add(tab.Id, tab);
        TabHeader.Add(btn);
        TabContainer.Add(tab.TabContent);

        if (Tabs.Count == 1) 
            ShowTab(tab.Id);

        return this;
    }

    public STabControllerOptions ShowTab(string id)
    {
        if (_currentTab != null)
        {
            _currentTab.Value.Tab.As<SBgButtonOptions>().FontColor(InactiveColor).Color(Color.clear);
            _currentTab.Value.TabContent.Active(false);
        }

        if (!Tabs.TryGetValue(id, out var tab))
            return this;

        tab.TabContent.Active(true);
        _currentTab = tab;
        
        tab.Tab.As<SBgButtonOptions>().FontColor(ActiveColor).Color("#222222");
        
        return this;
    }
    
    public STabControllerOptions HideDivider(bool hide = true)
    {
        TabDivider.Active(!hide);
        return this;
    }

    public struct TabDefinition
    {
        public string Id;
        public string Name;
        public SUiElement Tab;
        public SContainerOptions TabContent;
        
        public TabDefinition(string id, string name, SUiElement tab, SContainerOptions tabContent)
        {
            Id = id;
            Name = name;
            Tab = tab;
            TabContent = tabContent;
            tabContent.Active(false);
        }
        
        public TabDefinition(string id, string name, SContainerOptions tabContent)
        {
            Id = id;
            Name = name;
            TabContent = tabContent;
            tabContent.Active(false);
        }
    }
}