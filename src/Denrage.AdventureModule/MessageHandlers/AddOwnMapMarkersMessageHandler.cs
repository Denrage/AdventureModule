using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class AddOwnMapMarkersMessageHandler : MessageHandler<AddOwnMapMarkersMessage>
    {
        private readonly Func<MapMarkerService> getMapMarkerService;

        public AddOwnMapMarkersMessageHandler(Func<MapMarkerService> getMapMarkerService)
        {
            this.getMapMarkerService = getMapMarkerService;
        }

        protected override async Task Handle(Guid clientId, AddOwnMapMarkersMessage message, CancellationToken ct)
        {
            this.getMapMarkerService().AddUserMarker(message.Markers);
            await Task.CompletedTask;
        }
    }
}
