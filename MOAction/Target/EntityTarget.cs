using Dalamud.Game.ClientState.Objects.Types;

namespace MOAction.Target;

public class EntityTarget : TargetType
{
    public EntityTarget(PtrFunc func, string name) : base(func, name) { }
    public EntityTarget(PtrFunc func, string name, bool needsObject) : base(func, name, needsObject) { }

    public override IGameObject? GetTarget()
    {
        var obj = GetPtr();
        return IsTargetValid() ? obj : null;
    }

    public override bool IsTargetValid()
    {
        var obj = GetPtr();
        return obj != null;
    }
}