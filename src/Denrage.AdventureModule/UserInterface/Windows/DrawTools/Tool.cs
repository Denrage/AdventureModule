using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public abstract class Tool
    {
        public abstract void OnUpdateActive(DrawContext context);

        public virtual void OnUpdateAlways(DrawContext context, GameTime gameTime) { }

        public abstract string Name { get; }

        public abstract Container Controls { get; }

        public virtual void Activate() { }

        public virtual void Deactivate() { }

        public virtual void Paint(SpriteBatch spriteBatch, Rectangle bounds, Rectangle canvasBounds, SpriteBatchParameters parameters) { }
    }
}
