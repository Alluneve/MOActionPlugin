using Lumina.Excel.Sheets;
using MOAction.Target;

namespace MOAction.Configuration;

public class StackEntry(Action action, TargetType targ)
{
    public Action Action = action;
    public TargetType Target { get; set; } = targ;

    public override string ToString() => $"{Action.Name.ToString()}@{Target}";
}