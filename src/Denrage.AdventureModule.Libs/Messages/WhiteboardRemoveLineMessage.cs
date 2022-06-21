using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class WhiteboardRemoveLineMessage : Message
    {
        public List<Guid> Ids { get; set; }
    }
}
