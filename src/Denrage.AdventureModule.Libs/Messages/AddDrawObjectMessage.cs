using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public abstract class DrawObject
    {
        public Guid Id { get; set; }

        public string Username { get; set; }
    }

    public class AddDrawObjectMessage<TDrawObject> : Message
        where TDrawObject : DrawObject
    {
        public TDrawObject[] DrawObjects;
    }

    public class RemoveDrawObjectMessage<TDrawObject> : Message
        where TDrawObject: DrawObject
    {
        public Guid[] Ids { get; set; }
    }
}
