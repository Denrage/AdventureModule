using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class WhiteboardRemoveLineMessageHandler : MessageHandler<WhiteboardRemoveLineMessage>
    {
        private readonly Func<WhiteboardService> getWhiteboardService;

        public WhiteboardRemoveLineMessageHandler(Func<WhiteboardService> getWhiteboardService)
        {
            this.getWhiteboardService = getWhiteboardService;
        }

        protected override async Task Handle(Guid clientId, WhiteboardRemoveLineMessage message, CancellationToken ct)
        {
            this.getWhiteboardService().DeleteServerLines(message.Ids);
            await Task.CompletedTask;
        }
    }
}
