using System;
using System.Collections.Generic;
using System.Text;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public class Image : DrawObject
    {
        public byte[] Data { get; set; }

        public Vector2 Location { get; set; }

        public Vector2 Size { get; set; }

        public float Rotation { get; set; }

        public float Opacity { get; set; }
    }
}
