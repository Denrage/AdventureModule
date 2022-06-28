using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public class MumbleInformation
    {
        public Vector2 ContinentPosition { get; set; }
        
        public Vector3 MapPosition { get; set; }

        public Vector3 CameraDirection { get; set; }
        
        public Vector3 CameraPosition { get; set; }

        public int MapId { get; set; }

        public string CharacterName { get; set; }

    }
}
