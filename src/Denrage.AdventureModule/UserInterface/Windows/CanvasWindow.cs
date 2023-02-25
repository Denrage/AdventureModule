using Blish_HUD;
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
        private bool blockInput;
        private Tool[] tools;
        private bool leftMouseDown = false;
        private DrawObjectService drawObjectService;
        private LoginService loginService;
        private Panel blockInputText;
        private Label blockInputTextLabel;
        private Panel toolControls;
        private Panel canvas;
        private bool? finishedResize;
        private FlowPanel toolbar;
        private Tool tool;

        private Texture2D clipboardTexture;

        public void Initialize(DrawObjectService drawObjectService, LoginService loginService, TcpService tcpService)
        {
            this.drawObjectService = drawObjectService;
            this.loginService = loginService;

            this.blockInputText = new Panel()
            {
                Visible = !tcpService.IsConnected,
                Parent = this,
                ZIndex = 6,
            };

            this.blockInputTextLabel = new Label()
            {
                Parent = blockInputText,
                VerticalAlignment = VerticalAlignment.Middle,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "NOT CONNECTED TO THE SERVER!",
                BackgroundColor = new Color(0,0,0,100),
            };

            tcpService.Connected += () =>
            {
                this.blockInput = false;
                blockInputText.Visible = false;
            };

            tcpService.Disconnected += () =>
            {
                this.blockInput = true;
                blockInputText.Visible = true;
            };

            this.tools = new Tool[]
            {
                new Pen(this.loginService, this.drawObjectService),
                new Eraser(this.loginService, this.drawObjectService),
                new DrawTools.Image(this.loginService, this.drawObjectService),
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
                this.tool?.Deactivate();

                this.toolControls.Children.Clear();
                this.tool = tool;
                this.tool.Activate();
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
                ClipsBounds = true,
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
            };
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            if (!this.blockInput)
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
                    var drawContext = new DrawContext()
                    {
                        Canvas = this.canvas,
                        LeftMouseDown = this.leftMouseDown,
                        Mouse = GameService.Input.Mouse.Position,
                    };

                    this.tool.OnUpdateActive(drawContext);

                    foreach (var item in this.tools)
                    {
                        item.OnUpdateAlways(drawContext, gameTime);
                    }
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

            this.blockInputText.Width = this.Width;
            this.blockInputText.Height = this.Height;
            this.blockInputTextLabel.Width = this.Width;
            this.blockInputTextLabel.Height = this.Height;
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
            var lines = this.drawObjectService.GetDrawObjects<Line>().Where(x => this.canvas.ContentRegion.Contains(x.Start.ToVector()) && this.canvas.ContentRegion.Contains(x.End.ToVector())).OrderBy(x => x.TimeStamp);
            foreach (var line in lines)
            {
                var startRectangle = new Rectangle((int)line.Start.X, (int)line.Start.Y, 1, 1);
                startRectangle = startRectangle.ToBounds(this.canvas.AbsoluteBounds);
                var endRectangle = new Rectangle((int)line.End.X, (int)line.End.Y, 1, 1);
                endRectangle = endRectangle.ToBounds(this.canvas.AbsoluteBounds);
                spriteBatch.DrawLine(new Microsoft.Xna.Framework.Vector2(startRectangle.X, startRectangle.Y), new Microsoft.Xna.Framework.Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), line.StrokeWidth);
            }

            this.tool.Paint(spriteBatch, bounds, this.canvas.AbsoluteBounds, this.SpriteBatchParameters);

            base.PaintAfterChildren(spriteBatch, bounds);
        }
    }
}
