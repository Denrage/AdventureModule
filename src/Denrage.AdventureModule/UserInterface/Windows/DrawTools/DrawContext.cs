using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class DrawContext
    {
        public GameTime GameTime { get; set; }

        public bool LeftMouseDown { get; set; }

        public Point Mouse { get; set; }

        public Container Canvas { get; set; }
    }
}
