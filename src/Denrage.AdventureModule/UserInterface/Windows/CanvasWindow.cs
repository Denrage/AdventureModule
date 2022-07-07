﻿using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Denrage.AdventureModule.UserInterface.Windows.DrawTools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class CanvasWindow : WindowBase2
    {
        private Tool[] tools;
        private bool leftMouseDown = false;
        private DrawObjectService drawObjectService;
        private LoginService loginService;
        private Panel toolControls;
        private Panel canvas;
        private bool? finishedResize;
        private FlowPanel toolbar;
        private Tool tool;

        private Texture2D clipboardTexture;

        public void Initialize(DrawObjectService drawObjectService, LoginService loginService)
        {
            this.drawObjectService = drawObjectService;
            this.loginService = loginService;

            this.tools = new Tool[]
            {
                new Pen(this.loginService, this.drawObjectService),
                new Eraser(this.loginService, this.drawObjectService),
            };

            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 40, 500, 500 - 40));
            this.Location = new Point(0, 100);
            this.CanResize = true;
            this.toolbar = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Height = 40,
                Width = this.ContentRegion.Width,
                Parent = this,
            };

            this.toolControls = new Panel()
            {
                Height = 40,
                Width = this.ContentRegion.Width,
                Parent = this,
                Location = new Point(0, this.toolbar.Location.Y + this.toolbar.Height),
            };

            void SetTool(Tool tool)
            {
                this.tool?.Reset();

                this.toolControls.Children.Clear();
                this.tool = tool;
                this.tool.Reset();
                this.tool.Controls.Parent = this.toolControls;
                this.tool.Controls.Size = this.toolControls.ContentRegion.Size;
            }

            SetTool(this.tools[0]);
            foreach (var tool in this.tools)
            {
                var button = new StandardButton()
                {
                    Text = tool.Name,
                    Parent = toolbar,
                };

                if (tool == this.tool)
                {
                    button.Enabled = false;
                }

                button.Click += (s, e) =>
                {
                    foreach (var item in this.toolbar)
                    {
                        item.Enabled = true;
                    }

                    SetTool(tool);

                    button.Enabled = false;
                };
            }

            this.canvas = new Panel()
            {
                Parent = this,
                BackgroundColor = Color.White,
            };

            this.SetCanvasSize();

            toolbar.Resized += (s, e) =>
            {
                this.SetCanvasSize();
            };

            this.LeftMouseButtonPressed += (s, e) => this.leftMouseDown = true;
            this.LeftMouseButtonReleased += (s, e) =>
            {
                this.leftMouseDown = false;
                this.tool.Reset();
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) => base.PaintBeforeChildren(spriteBatch, bounds);

        public override void UpdateContainer(GameTime gameTime)
        {
            if (!this.finishedResize.HasValue && this.Resizing)
            {
                this.finishedResize = false;
            }

            if (this.finishedResize.HasValue && !this.finishedResize.Value && !this.Resizing)
            {
                this.finishedResize = true;
            }

            if (this.finishedResize.HasValue && this.finishedResize.Value)
            {
                this.finishedResize = null;
                System.Diagnostics.Debug.WriteLine("Resizing finished");
            }

            if (this.tool != null && !this.Dragging && !this.Resizing)
            {
                this.tool.OnUpdate(new DrawContext()
                {
                    CanvasBounds = this.canvas.AbsoluteBounds,
                    LeftMouseDown = this.leftMouseDown,
                    Mouse = GameService.Input.Mouse.Position,
                });
            }

            if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Ctrl) && GameService.Input.Keyboard.KeysDown.Contains(Microsoft.Xna.Framework.Input.Keys.V))
            {
                var files = ClipboardUtil.WindowsClipboardService.GetFileDropListAsync().Result;

                if (files?.Any() ?? false)
                {
                    var file = files.FirstOrDefault();
                    if (!string.IsNullOrEmpty(file))
                    {
                        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                        using var context = GameService.Graphics.LendGraphicsDeviceContext();
                        this.clipboardTexture = Texture2D.FromStream(context.GraphicsDevice, fileStream);
                    }
                }

                if (ClipboardHelper.TryGetPngData(out var buffer))
                {
                    using var memoryStream = new MemoryStream(buffer);
                    using var context = GameService.Graphics.LendGraphicsDeviceContext();
                    this.clipboardTexture = Texture2D.FromStream(context.GraphicsDevice, memoryStream);
                }
            }

            base.UpdateContainer(gameTime);
        }
        
        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            this.ContentRegion = new Rectangle(this.ContentRegion.X,
                                               this.ContentRegion.Y,
                                               this.Width - this.ContentRegion.X,
                                               this.Height - this.ContentRegion.Y);

            if (canvas != null && toolbar != null)
            {
                this.SetCanvasSize();
            }
        }

        private void SetCanvasSize()
        {
            const int margin = 20;
            canvas.Location = new Point(margin, this.toolControls.Location.Y + this.toolControls.Height + margin);
            canvas.Height = this.ContentRegion.Height - this.toolControls.Height - (margin * 2);
            canvas.Width = this.ContentRegion.Width - (margin * 2);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.clipboardTexture != null)
            {
                spriteBatch.Draw(this.clipboardTexture, bounds, Color.White);
            }

            var lines = this.drawObjectService.GetDrawObjects<Line>().Where(x => this.canvas.ContentRegion.Contains(x.Start.ToVector()) && this.canvas.ContentRegion.Contains(x.End.ToVector())).OrderBy(x => x.TimeStamp);
            foreach (var line in lines)
            {
                var startRectangle = new Rectangle((int)line.Start.X, (int)line.Start.Y, 1, 1);
                startRectangle = startRectangle.ToBounds(this.canvas.AbsoluteBounds);
                var endRectangle = new Rectangle((int)line.End.X, (int)line.End.Y, 1, 1);
                endRectangle = endRectangle.ToBounds(this.canvas.AbsoluteBounds);
                spriteBatch.DrawLine(new Microsoft.Xna.Framework.Vector2(startRectangle.X, startRectangle.Y), new Microsoft.Xna.Framework.Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), line.StrokeWidth);
            }

            this.tool.Paint(spriteBatch);

            base.PaintAfterChildren(spriteBatch, bounds);
        }
    }
}
