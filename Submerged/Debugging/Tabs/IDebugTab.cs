namespace Submerged.Debugging.Tabs;

public interface IDebugTab
{
    public string Name { get; }
    public bool ShouldShow { get; }

    public void BuildGUI();
}
