using Neo.IronLua;
using System;

namespace Denrage.AdventureModule.Adventure.Interfaces
{
    public interface IButtonOrderBuilderLua
    {
        event Func<LuaResult> Finished;

        event Func<IMarkerLua[], LuaResult> StateChanged;

        IButtonOrderBuilderLua Add(IMarkerLua button, int delay = 0);

        void Build();
    }
}

