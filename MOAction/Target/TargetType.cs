﻿using Dalamud.Game.ClientState.Objects.Types;

namespace MOAction.Target
{
    public abstract class TargetType
    {
        public delegate IGameObject PtrFunc();
        public PtrFunc getPtr;
        public string TargetName;
        public bool ObjectNeeded;

        public TargetType(PtrFunc function, string name)
        {
            getPtr = function;
            TargetName = name;
            ObjectNeeded = true;
        }

        public TargetType(PtrFunc function, string name, bool objneed)
        {
            getPtr = function;
            TargetName = name;
            ObjectNeeded = objneed;
        }

        public abstract IGameObject GetTarget();
        public abstract bool IsTargetValid();

        public override string ToString() => TargetName;
    }
}
