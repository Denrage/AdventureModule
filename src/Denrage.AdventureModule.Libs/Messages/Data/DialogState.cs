using System;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public struct DialogState : IState
    {
        public Guid Id { get; set; }

        public bool IsOpen { get; set; }

        public int CurrentNode { get; set; }
    }
}
