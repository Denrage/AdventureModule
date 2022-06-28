using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class PlayersMumbleMessage : Message
    {
        public Dictionary<string, MumbleInformation> Information { get; set; }
    }
}
