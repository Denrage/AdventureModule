using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class StateChangedMessage<TState> : Message
        where TState : IState
    {
        public TState State { get; set; }
    }
}
