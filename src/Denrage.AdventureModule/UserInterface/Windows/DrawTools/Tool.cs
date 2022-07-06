using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public abstract class Tool
    {
        public abstract void OnUpdate(DrawContext context);

        public virtual void Reset() { }

        public abstract string Name { get; }

        public abstract Container Controls { get; }

        public virtual void Paint(SpriteBatch spriteBatch) { }
    }
}
