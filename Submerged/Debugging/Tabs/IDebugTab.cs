namespace Submerged.Debugging.Tabs;

public interface IDebugTab
{
    string Name { get; }
    bool ShouldShow { get; }

    void BuildGUI();
}
