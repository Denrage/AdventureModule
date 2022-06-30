using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class RemoveMapMarkersMessageHandler : MessageHandler<RemoveMapMarkersMessage>
    {
        private readonly Func<MapMarkerService> getMapMarkerService;

        public RemoveMapMarkersMessageHandler(Func<MapMarkerService> getMapMarkerService)
        {
            this.getMapMarkerService = getMapMarkerService;
        }

        protected override async Task Handle(Guid clientId, RemoveMapMarkersMessage message, CancellationToken ct)
        {
            this.getMapMarkerService().RemoveServerMarker(message.Ids);
            await Task.CompletedTask;
        }
    }
}
