using Blish_HUD.Controls;
using Denrage.AdventureModule.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface
{
    public class DisconnectBlockControl : Panel
    {
        private Label label;

        public DisconnectBlockControl(IInitializationService initializationService)
        {
            this.Visible = !initializationService.IsInitialized;

            initializationService.Initialize += () => this.Visible = false;
            initializationService.Finalize += () => this.Visible = true;

            this.BackgroundColor = new Color(0, 0, 0, 100);
            this.ZIndex = int.MaxValue;

            this.label = new Label()
            {
                Parent = this,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                BackgroundColor = new Color(0, 0, 0, 200),
                Text = "NOT CONNECTED TO THE SERVER!",
            };
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.label.Location = new Point((this.Width / 2) - (this.label.Width / 2), (this.Height / 2) - (this.label.Height / 2));
        }
    }
}
