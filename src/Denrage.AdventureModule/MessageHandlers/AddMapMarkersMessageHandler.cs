using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class AddMapMarkersMessageHandler : MessageHandler<AddMapMarkersMessage>
    {
        private readonly Func<MapMarkerService> getMapMarkerService;

        public AddMapMarkersMessageHandler(Func<MapMarkerService> getMapMarkerService)
        {
            this.getMapMarkerService = getMapMarkerService;
        }

        protected override async Task Handle(Guid clientId, AddMapMarkersMessage message, CancellationToken ct)
        {
            this.getMapMarkerService().AddServerMarker(message.Markers);
            await Task.CompletedTask;
        }
    }
}
