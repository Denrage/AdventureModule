using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class CanvasWindow : WindowBase2
    {
        private enum Mode
        {
            Paint,
            Erase,
        }

        private bool leftMouseDown = false;
        private Line currentLine = default;
        private DrawObjectService drawObjectService;
        private LoginService loginService;
        private Panel canvas;
        private Mode currentMode = Mode.Paint;
        private bool? finishedResize;
        private FlowPanel toolbar;

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
                if (this.currentMode == Mode.Paint)
                {
                    this.currentLine = default;
                    this.currentMode = Mode.Erase;
                }
                else
                {
                    this.currentMode = Mode.Paint;
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
                currentLine = default;
            };
            this.drawObjectService = drawObjectService;
            this.loginService = loginService;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) => base.PaintBeforeChildren(spriteBatch, bounds);

        public override void UpdateContainer(GameTime gameTime)
        {
            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;

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
                if (currentMode == Mode.Paint)
                {
                    this.Paint(mouseX, mouseY);
                }
                else if (currentMode == Mode.Erase)
                {
                    this.Erase(mouseX, mouseY);
                }
            }

            base.UpdateContainer(gameTime);
        }

        private void Erase(int mouseX, int mouseY)
        {
            if (this.leftMouseDown &&
                mouseX > this.canvas.AbsoluteBounds.X &&
                mouseX < this.canvas.AbsoluteBounds.X + this.canvas.AbsoluteBounds.Width &&
                mouseY > this.canvas.AbsoluteBounds.Y &&
                mouseY < this.canvas.AbsoluteBounds.Y + this.canvas.AbsoluteBounds.Height)
            {
                var rectangleX = mouseX - 5;
                var rectangleY = mouseY - 5;
                var mouseArea = new Rectangle(rectangleX, rectangleY, mouseX - rectangleX + 5, mouseY - rectangleY + 5);
                var linesToDelete = new List<Line>();
                foreach (var line in this.drawObjectService.GetDrawObjects<Line>().Where(x => x.Username == this.loginService.Name))
                {
                    var currentLine = line;
                    if (Helper.CohenSutherland.IsIntersecting(ref currentLine, ref mouseArea))
                    {
                        linesToDelete.Add(line);
                    }
                }

                this.drawObjectService.Remove<Line>(linesToDelete.Select(x => x.Id), false, default);
            }
        }

        private void Paint(int mouseX, int mouseY)
        {
            if (this.leftMouseDown &&
                mouseX > this.canvas.AbsoluteBounds.X &&
                mouseX < this.canvas.AbsoluteBounds.X + this.canvas.AbsoluteBounds.Width &&
                mouseY > this.canvas.AbsoluteBounds.Y &&
                mouseY < this.canvas.AbsoluteBounds.Y + this.canvas.AbsoluteBounds.Height)
            {
                if (currentLine is null)
                {
                    currentLine = new Line();
                    currentLine.LineColor = new Line.Color()
                    {
                        A = 255,
                        B = 0,
                        G = 0,
                        R = 0,
                    };
                    currentLine.Start = new Line.Point()
                    {
                        X = mouseX - this.AbsoluteBounds.X,
                        Y = mouseY - this.AbsoluteBounds.Y,
                    };
                }
                else if (currentLine.End.X == 0 && currentLine.End.Y == 0)
                {
                    currentLine.End = new Line.Point()
                    {
                        X = mouseX - this.AbsoluteBounds.X,
                        Y = mouseY - this.AbsoluteBounds.Y,
                    };

                    currentLine.TimeStamp = System.DateTime.UtcNow;
                    currentLine.Id = System.Guid.NewGuid();
                    currentLine.Username = this.loginService.Name;

                    this.drawObjectService.Add(new[] { currentLine }, false, default);

                    currentLine = new Line()
                    {
                        LineColor = new Line.Color()
                        {
                            A = 255,
                            B = 0,
                            G = 0,
                            R = 0,
                        },
                        Start = new Line.Point()
                        {
                            X = mouseX - this.AbsoluteBounds.X,
                            Y = mouseY - this.AbsoluteBounds.Y,
                        },
                    };
                }
            }
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
            foreach (var line in this.drawObjectService.GetDrawObjects<Line>().Where(x => this.canvas.AbsoluteBounds.Contains(x.Start.ToVector()) && this.canvas.AbsoluteBounds.Contains(x.End.ToVector())).OrderBy(x => x.TimeStamp))
            {
                var startRectangle = new Rectangle(line.Start.X, line.Start.Y, 1, 1);
                startRectangle = startRectangle.ToBounds(this.AbsoluteBounds);
                var endRectangle = new Rectangle(line.End.X, line.End.Y, 1, 1);
                endRectangle = endRectangle.ToBounds(this.AbsoluteBounds);
                spriteBatch.DrawLine(new Microsoft.Xna.Framework.Vector2(startRectangle.X, startRectangle.Y), new Microsoft.Xna.Framework.Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), 5);
            }

            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;
            if (this.currentMode == Mode.Erase)
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
