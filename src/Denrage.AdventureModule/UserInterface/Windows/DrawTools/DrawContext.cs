using Microsoft.Xna.Framework;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class DrawContext
    {
        public bool LeftMouseDown { get; set; }

        public Point Mouse { get; set; }

        public Rectangle CanvasBounds { get; set; }
    }
}
