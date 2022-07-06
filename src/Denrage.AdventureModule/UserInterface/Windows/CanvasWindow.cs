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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class CanvasWindow : WindowBase2
    {
        private bool leftMouseDown = false;
        private DrawObjectService drawObjectService;
        private LoginService loginService;
        private Panel canvas;
        private bool? finishedResize;
        private FlowPanel toolbar;
        private Tool tool;

        public void Initialize(DrawObjectService drawObjectService, LoginService loginService)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 40, 500, 500 - 40));
            this.Location = new Point(0, 100);
            this.CanResize = true;
            this.toolbar = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
                Width = this.ContentRegion.Width,
                Parent = this,
            };

            var eraserButton = new StandardButton()
            {
                Text = "Eraser",
                Parent = toolbar,
            };

            eraserButton.Click += (s, e) =>
            {
                if (this.tool is Pen)
                {
                    this.tool = new Eraser(this.loginService, this.drawObjectService);
                }
                else
                {
                    this.tool = new Pen(this.loginService, this.drawObjectService);
                }
            };

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
            this.drawObjectService = drawObjectService;
            this.loginService = loginService;
            this.tool = new Pen(this.loginService, this.drawObjectService);
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

            if (!this.Dragging && !this.Resizing)
            {
                this.tool.OnUpdate(new DrawContext()
                {
                    CanvasBounds = this.canvas.AbsoluteBounds,
                    LeftMouseDown = this.leftMouseDown,
                    Mouse = GameService.Input.Mouse.Position,
                });
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
            canvas.Location = new Point(margin, toolbar.Location.Y + toolbar.Height + margin);
            canvas.Height = this.ContentRegion.Height - toolbar.Height - (margin * 2);
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
                spriteBatch.DrawLine(new Microsoft.Xna.Framework.Vector2(startRectangle.X, startRectangle.Y), new Microsoft.Xna.Framework.Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), 5);
            }

            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;
            if (this.tool is Eraser)
            {
                var rectangleX = mouseX - 5;
                var rectangleY = mouseY - 5;
                var mouseArea = new Rectangle(rectangleX, rectangleY, mouseX - rectangleX + 5, mouseY - rectangleY + 5);
                spriteBatch.DrawRectangle(mouseArea, Color.Black);
            }

            base.PaintAfterChildren(spriteBatch, bounds);
        }
    }
}
