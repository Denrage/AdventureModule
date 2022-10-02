using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Stateless;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Denrage.AdventureModule.Adventure
{
    public abstract class AdventureElement : IDisposable
    {
        public abstract IEntity EditEntity { get; }

        public abstract void Dispose();

        public virtual void Update(GameTime gameTime) { }
    }
}


