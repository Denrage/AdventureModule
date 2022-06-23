using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class WhiteboardAddOwnLineMessageHandler : MessageHandler<WhiteboardAddOwnLineMessage>
    {
        private readonly Func<WhiteboardService> getWhiteboardService;

        public WhiteboardAddOwnLineMessageHandler(Func<WhiteboardService> getWhiteboardService)
        {
            this.getWhiteboardService = getWhiteboardService;
        }

        protected override async Task Handle(Guid clientId, WhiteboardAddOwnLineMessage message, CancellationToken ct)
        {
            this.getWhiteboardService().AddUserLines(message.Lines);
            await Task.CompletedTask;
        }
    }
}
