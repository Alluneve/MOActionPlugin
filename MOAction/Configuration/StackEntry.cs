using MOAction.Target;

namespace MOAction.Configuration;

public class StackEntry(Lumina.Excel.Sheets.Action action, TargetType targ)
{
    public Lumina.Excel.Sheets.Action Action = action;
    public TargetType Target { get; set; } = targ;

    public override string ToString() => $"{Action.Name.ExtractText()}@{Target}";
}