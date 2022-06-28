using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class PlayerPositionMessage : Message
    {
        public Vector3 Position { get; set; }
    }
}
