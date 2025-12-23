using Dalamud.Game.ClientState.Objects.Types;

namespace MOAction.Target;

public abstract class TargetType(TargetType.PtrFunc function, string name, bool needsObject = true)
{
    public delegate IGameObject? PtrFunc();

    protected readonly PtrFunc GetPtr = function;
    public readonly string TargetName = name;
    public readonly bool ObjectNeeded = needsObject;

    public abstract IGameObject? GetTarget();
    public abstract bool IsTargetValid();

    public override string ToString() => TargetName;
}