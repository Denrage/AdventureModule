using System;
using System.Collections.Generic;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public struct LuaVariablesState : IState
    {
        public Guid Id { get; set; }

        public KeyValuePair<string, object>[] Variables { get; set; }

        public string StepName { get; set; }
    }
}
