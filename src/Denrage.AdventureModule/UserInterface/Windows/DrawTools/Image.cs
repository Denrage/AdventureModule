using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class DrawValueChangeContext
    {
        public Guid Id { get; set; }

        public Libs.Messages.Data.Vector2 Location { get; set; }

        public Libs.Messages.Data.Vector2 Size { get; set; }

        public float? Rotation { get; set; }

        public float? Opacity { get; set; }
    }

    public class Image : Tool
    {
        private readonly CounterBox imageIndex;
        private readonly Dictionary<Guid, ImageControl> imageControls = new Dictionary<Guid, ImageControl>();
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private readonly Dictionary<Guid, DrawValueChangeContext> valueChangeContext = new Dictionary<Guid, DrawValueChangeContext>();
        private bool isActive = false;
        private bool addingImage = false;
        private bool removingImage = false;

        public override string Name => "Image";

        public override Container Controls { get; } = new FlowPanel();

        public Image(LoginService loginService, DrawObjectService drawObjectService)
        {
            this.imageIndex = new CounterBox()
            {
                MinValue = 0,
                MaxValue = 0,
                Value = 0,
                Parent = this.Controls,
            };
            this.loginService = loginService;
            this.drawObjectService = drawObjectService;
        }

        private ImageControl CreateImageControl(DrawContext context, Libs.Messages.Data.Image serverImage)
        {
            var images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>();
            var ownImages = images.Where(x => x.Username == this.loginService.Name).ToArray();
            using var memoryStream = new MemoryStream(serverImage.Data);
            using var graphicContext = GameService.Graphics.LendGraphicsDeviceContext();
            var image = new ImageControl(Texture2D.FromStream(graphicContext.GraphicsDevice, memoryStream))
            {
                Parent = context.Canvas,
                Enabled = false,
            };

            image.Location = new Point(context.Canvas.LocalBounds.Center.X - (image.Width / 2), context.Canvas.LocalBounds.Center.Y - (image.Height / 2));

            if (ownImages.Select(x => x.Id).Contains(serverImage.Id))
            {
                image.Moved += (s, e) =>
                {
                    if (!this.valueChangeContext.TryGetValue(serverImage.Id, out var context))
                    {
                        context = new DrawValueChangeContext();
                        this.valueChangeContext[serverImage.Id] = context;
                    }

                    context.Location = new Libs.Messages.Data.Vector2() { X = e.CurrentLocation.X, Y = e.CurrentLocation.Y };
                };
                image.Resized += (s, e) =>
                {
                    if (!this.valueChangeContext.TryGetValue(serverImage.Id, out var context))
                    {
                        context = new DrawValueChangeContext();
                        this.valueChangeContext[serverImage.Id] = context;
                    }

                    context.Size = new Libs.Messages.Data.Vector2() { X = e.CurrentSize.X, Y = e.CurrentSize.Y };
                };
                image.OpacityChanged += opacity =>
                {
                    if (!this.valueChangeContext.TryGetValue(serverImage.Id, out var context))
                    {
                        context = new DrawValueChangeContext();
                        this.valueChangeContext[serverImage.Id] = context;
                    }

                    context.Opacity = opacity;
                };
                image.RotationChanged += rotation =>
                {
                    if (!this.valueChangeContext.TryGetValue(serverImage.Id, out var context))
                    {
                        context = new DrawValueChangeContext();
                        this.valueChangeContext[serverImage.Id] = context;
                    }

                    context.Rotation = rotation;
                };
            }

            return image;
        }

        private void AddImage(byte[] data, DrawContext context)
        {
            var serverImage = new Libs.Messages.Data.Image() { Data = data, Username = this.loginService.Name, Id = Guid.NewGuid(), Location = new Libs.Messages.Data.Vector2 { X = context.Canvas.LocalBounds.Center.X - (400 / 2), Y = context.Canvas.LocalBounds.Center.Y - (400 / 2) }, Opacity = 1f, Rotation = 0f, Size = new Libs.Messages.Data.Vector2() { X = 400, Y = 400 } };
            this.drawObjectService.Add(new[] { serverImage }, false, default);
        }

        private void RemoveImage()
        {
            var images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>();
            var ownImages = images.Where(x => x.Username == this.loginService.Name).ToArray();
            var image = ownImages[this.imageIndex.Value];
            this.drawObjectService.Remove<Libs.Messages.Data.Image>(new[] { image.Id }, false, default);
        }

        public override void OnUpdateActive(DrawContext context)
        {
            if (this.isActive)
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
                                using var memoryStream = new MemoryStream();
                                fileStream.CopyTo(memoryStream);
                                this.AddImage(memoryStream.ToArray(), context);
                            }
                        }

                        if (ClipboardHelper.TryGetPngData(out var buffer))
                        {
                            this.AddImage(buffer, context);
                        }
                    }
                }
                else
                {
                    addingImage = false;
                }

                if (GameService.Input.Keyboard.KeysDown.Contains(Microsoft.Xna.Framework.Input.Keys.Delete))
                {
                    if (!removingImage)
                    {
                        removingImage = true;
                        this.RemoveImage();
                    }
                }
                else
                {
                    removingImage = false;
                }
            }
        }

        private double previousGameTime;

        public override void OnUpdateAlways(DrawContext context, GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds - previousGameTime > 20)
            {
                var watch = Stopwatch.StartNew();
                var images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>().ToArray();
                this.drawObjectService.Update(this.valueChangeContext.Select(x => new Libs.Messages.Data.Image()
                {
                    Id = x.Key,
                    Location = x.Value.Location,
                    Opacity = x.Value.Opacity,
                    Rotation = x.Value.Rotation,
                    Size = x.Value.Size,
                }), false, default);

                images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>().ToArray();

                foreach (var item in images)
                {
                    if (!this.imageControls.ContainsKey(item.Id))
                    {
                        this.imageControls[item.Id] = this.CreateImageControl(context, item);
                    }
                }

                var toRemove = new List<Guid>();

                foreach (var item in this.imageControls)
                {
                    if (!images.Select(x => x.Id).Contains(item.Key))
                    {
                        toRemove.Add(item.Key);
                    }
                }

                foreach (var item in toRemove)
                {
                    this.imageControls[item].Dispose();
                    _ = this.imageControls.Remove(item);
                }

                foreach (var item in images.Where(x => x.Username != this.loginService.Name))
                {
                    var imageControl = this.imageControls[item.Id];
                    imageControl.Size = new Point((int)item.Size.X, (int)item.Size.Y);
                    imageControl.Location = new Point((int)item.Location.X, (int)item.Location.Y);
                    imageControl.Rotation = item.Rotation.Value;
                    imageControl.Opacity = item.Opacity.Value;
                }

                var ownImages = images.Where(x => x.Username == this.loginService.Name).ToArray();

                if (ownImages.Length > 0)
                {
                    this.imageIndex.MaxValue = ownImages.Length - 1;
                    if (this.imageIndex.Value > this.imageIndex.MaxValue)
                    {
                        this.imageIndex.Value = this.imageIndex.MaxValue;
                    }

                    foreach (var item in ownImages)
                    {
                        var imageControl = this.imageControls[item.Id];
                        imageControl.Enabled = false;
                    }

                    this.imageControls[ownImages[this.imageIndex.Value].Id].Enabled = true;
                }

                this.previousGameTime = gameTime.TotalGameTime.TotalMilliseconds;
                watch.Stop();
                //Debug.WriteLine("Time took: " + watch.ElapsedTicks);
            }
        }

        public override void Activate()
        {
            this.isActive = true;
        }

        public override void Deactivate()
        {
            this.isActive = false;
            foreach (var item in this.imageControls)
            {
                item.Value.Enabled = false;
            }
        }
    }
}
