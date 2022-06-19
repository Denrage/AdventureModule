using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class WhiteboardAddLineMessageHandler : MessageHandler<WhiteboardAddLineMessage>
    {
        private readonly Func<WhiteboardService> getWhiteboardService;

        public WhiteboardAddLineMessageHandler(Func<WhiteboardService> getWhiteboardService)
        {
            this.getWhiteboardService = getWhiteboardService;
        }

        protected override async Task Handle(Guid clientId, WhiteboardAddLineMessage message, CancellationToken ct)
        {
            this.getWhiteboardService().AddServerLines(message.Lines);
            await Task.CompletedTask;
        }
    }
}
