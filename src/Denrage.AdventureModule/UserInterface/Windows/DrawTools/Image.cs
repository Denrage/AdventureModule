﻿using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Services;
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
        private readonly Dictionary<Guid, ImageControl> imageControls = new Dictionary<Guid, ImageControl>();
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private bool isActive = false;
        private bool addingImage = false;
        private bool removeingImage = false;

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
            using var memoryStream = new MemoryStream(serverImage.Data);
            using var graphicContext = GameService.Graphics.LendGraphicsDeviceContext();
            var image = new ImageControl(Texture2D.FromStream(graphicContext.GraphicsDevice, memoryStream))
            {
                Parent = context.Canvas,
                Enabled = false,
            };

            image.Location = new Point(context.Canvas.LocalBounds.Center.X - (image.Width / 2), context.Canvas.LocalBounds.Center.Y - (image.Height / 2));

            return image;
        }

        private void AddImage(byte[] data)
        {
            var serverImage = new Libs.Messages.Data.Image() { Data = data, Username = this.loginService.Name, Id = Guid.NewGuid() };
            this.drawObjectService.Add(new[] { serverImage }, false, default);
        }

        private void RemoveImage()
        {
            var images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>();
            var ownImages = images.Where(x => x.Username == this.loginService.Name).ToArray();
            var image = ownImages[this.imageIndex.Value];
            this.drawObjectService.Remove<Libs.Messages.Data.Image>(new[] { image.Id }, false, default);
        }

        public override void OnUpdate(DrawContext context)
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
                                this.AddImage(memoryStream.ToArray());
                            }
                        }

                        if (ClipboardHelper.TryGetPngData(out var buffer))
                        {
                            this.AddImage(buffer);
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
                        this.RemoveImage();
                    }
                }
                else
                {
                    removeingImage = false;
                }

                var images = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.Image>();

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
