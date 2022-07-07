using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Image : Tool
    {
        private readonly CounterBox imageIndex;
        private readonly List<ImageWindow> images = new List<ImageWindow>();

        private bool addingImage = false;
        private bool removeingImage = false;

        public override string Name => "Image";

        public override Container Controls { get; } = new FlowPanel();

        public Image()
        {
            this.imageIndex = new CounterBox()
            {
                MinValue = 0,
                MaxValue = 0,
                Value = 0,
                Parent = this.Controls,
            };
        }

        public override void OnUpdate(DrawContext context)
        {
            if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Ctrl) && GameService.Input.Keyboard.KeysDown.Contains(Microsoft.Xna.Framework.Input.Keys.V))
            {
                if (!addingImage)
                {
                    addingImage = true;

                    var files = ClipboardUtil.WindowsClipboardService.GetFileDropListAsync().Result;

                    if (files?.Any() ?? false)
                    {
                        var file = files.FirstOrDefault();
                        if (!string.IsNullOrEmpty(file))
                        {
                            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                            using var graphicContext = GameService.Graphics.LendGraphicsDeviceContext();
                            var image = new ImageWindow(Texture2D.FromStream(graphicContext.GraphicsDevice, fileStream))
                            {
                                Parent = context.Canvas,
                            };

                            image.Location = new Point(context.Canvas.LocalBounds.Center.X - (image.Width / 2), context.Canvas.LocalBounds.Center.Y - (image.Height / 2));
                            this.images.Add(image);
                            if (this.images.Count != 1)
                            {
                                this.imageIndex.MaxValue++;
                                this.imageIndex.Value = this.imageIndex.MaxValue;
                            }
                        }
                    }

                    if (ClipboardHelper.TryGetPngData(out var buffer))
                    {
                        using var memoryStream = new MemoryStream(buffer);
                        using var graphicContext = GameService.Graphics.LendGraphicsDeviceContext();
                        var image = new ImageWindow(Texture2D.FromStream(graphicContext.GraphicsDevice, memoryStream))
                        {
                            Parent = context.Canvas,
                        };

                        image.Location = new Point(context.Canvas.LocalBounds.Center.X - (image.Width / 2), context.Canvas.LocalBounds.Center.Y - (image.Height / 2));

                        this.images.Add(image);
                        if (this.images.Count != 1)
                        {
                            this.imageIndex.MaxValue++;
                            this.imageIndex.Value = this.imageIndex.MaxValue;
                        }
                    }
                }
            }
            else
            {
                addingImage = false;
            }

            if (GameService.Input.Keyboard.KeysDown.Contains(Microsoft.Xna.Framework.Input.Keys.Delete))
            {
                if (!removeingImage)
                {
                    removeingImage = true;
                    var currentImage = this.images[this.imageIndex.Value];
                    currentImage.Dispose();
                    _ = this.images.Remove(currentImage);

                    if (this.imageIndex.Value > 0)
                    {
                        this.imageIndex.Value--;
                    }

                    if (this.imageIndex.MaxValue > 0)
                    {
                        this.imageIndex.MaxValue--;
                    }
                }
            }
            else
            {
                removeingImage = false;
            }

            if (this.images.Any())
            {
                foreach (var item in this.images)
                {
                    item.Enabled = false;
                }

                this.images[this.imageIndex.Value].Enabled = true;
            }
        }

        public override void Activate()
        {
            if (this.images.Any())
            {
                this.images[this.imageIndex.Value].Enabled = true;
            }
        }

        public override void Deactivate()
        {
            foreach (var item in this.images)
            {
                item.Enabled = false;
            }
        }
    }
}
