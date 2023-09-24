using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class ImageViewerWindow : WindowBase2
    {
        public void Initialize()
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 200, 500), new Rectangle(0, 40, 200, 500 - 40));
            this.Location = new Point(0, 100);

            var flowPanel = new FlowPanel()
            {
                Parent = this,
                Width = this.ContentRegion.Width,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(10),
            };

            var image = new Image()
            {
                Texture = Module.Instance.ContentsManager.GetTexture("1.jpg"),
                Width = flowPanel.ContentRegion.Width,
                Height = 100,
                Parent = flowPanel,
            };

            image.Click += (s, e) =>
            {
                new ImageControl(image.Texture)
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Width = 800,
                    Height = 600,
                };
            };

            new Image()
            {
                Texture = Module.Instance.ContentsManager.GetTexture("2.jpg"),
                Width = flowPanel.ContentRegion.Width,
                Height = 100,
                Parent = flowPanel,
            };

            new Image()
            {
                Texture = Module.Instance.ContentsManager.GetTexture("3.jpg"),
                Width = flowPanel.ContentRegion.Width,
                Height = 100,
                Parent = flowPanel,
            };

            new Image()
            {
                Texture = Module.Instance.ContentsManager.GetTexture("4.jpg"),
                Width = flowPanel.ContentRegion.Width,
                Height = 100,
                Parent = flowPanel,
            };
        }
    }
}
