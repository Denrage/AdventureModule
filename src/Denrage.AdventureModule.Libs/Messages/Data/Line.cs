using System;

namespace Denrage.AdventureModule.Libs.Messages.Data
{
    public class Line : DrawObject
    {
        public struct Color
        {
            public byte R { get; set; }

            public byte G { get; set; }

            public byte B { get; set; }

            public byte A { get; set; }
        }

        public int StrokeWidth { get; set; }

        public Vector2 Start { get; set; }

        public Vector2 End { get; set; }

        public DateTime TimeStamp { get; set; }

        public Color LineColor { get; set; }
    }
}