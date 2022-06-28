﻿using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class PlayerPositionsMessage : Message
    {
        public Dictionary<string, Vector3> Positions { get; set; }
    }
}
