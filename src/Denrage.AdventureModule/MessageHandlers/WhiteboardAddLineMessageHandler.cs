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
        private readonly WhiteboardService whiteboardService;

        public WhiteboardAddLineMessageHandler(WhiteboardService whiteboardService)
        {
            this.whiteboardService = whiteboardService;
        }

        protected override async Task Handle(Guid clientId, WhiteboardAddLineMessage message, CancellationToken ct)
        {
            this.whiteboardService.AddLines(message.Lines);
            await Task.CompletedTask;
        }
    }
}
