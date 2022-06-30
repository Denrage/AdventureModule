using Denrage.AdventureModule.Libs.Messages.Data;
using System;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class RemoveDrawObjectMessage<TDrawObject> : Message
        where TDrawObject: DrawObject
    {
        public Guid[] Ids { get; set; }
    }
}
