using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Generic;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class WhiteboardAddLineMessage : Message
    {
        public List<Line> Lines { get; set; }
    }
}