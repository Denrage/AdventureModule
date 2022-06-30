using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Generic;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class AddMapMarkersMessage : Message
    {
        public List<MapMarker> Markers { get; set; }
    }
}