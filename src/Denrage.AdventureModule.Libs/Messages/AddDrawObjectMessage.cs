using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages
{
    public class AddDrawObjectMessage<TDrawObject> : Message
        where TDrawObject : DrawObject
    {
        public TDrawObject[] DrawObjects;
    }
}
