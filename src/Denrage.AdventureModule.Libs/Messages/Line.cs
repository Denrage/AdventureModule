namespace Denrage.AdventureModule.Libs.Messages
{
    public struct Line
    {
        public struct Color
        {
            public byte R { get; set; }

            public byte G { get; set; }

            public byte B { get; set; }

            public byte A { get; set; }
        }

        public struct Point
        {
            public int X { get; set; }

            public int Y { get; set; }
        }

        public Point Start { get; set; }

        public Point End { get; set; }

        public Color LineColor { get; set; }
    }
}