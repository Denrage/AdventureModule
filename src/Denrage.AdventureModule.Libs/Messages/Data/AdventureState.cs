using System;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public struct AdventureState : IState
    {
        public Guid Id { get; set; }

        public string StepName { get; set; }
    }


}


