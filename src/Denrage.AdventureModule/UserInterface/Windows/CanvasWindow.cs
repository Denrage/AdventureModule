using Blish_HUD;
using Blish_HUD.Controls;
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
        private WhiteboardService whiteboardService;
        private Panel canvas;
        private Mode currentMode = Mode.Paint;

        public void Initialize(WhiteboardService whiteboardService)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 0, 500, 500 - 30));
            var toolbar = new FlowPanel()
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
                Location = new Point(0, toolbar.Location.Y + toolbar.Height),
                Parent = this,
                Width = this.ContentRegion.Width,
                Height = this.ContentRegion.Height - toolbar.Height,
                BackgroundColor = Color.White,
            };

            toolbar.Resized += (s, e) =>
            {
                canvas.Location = new Point(0, toolbar.Location.Y + toolbar.Height);
                canvas.Height = this.ContentRegion.Height - toolbar.Height;
            };
            //this.ClipsBounds = true;
            this.LeftMouseButtonPressed += (s, e) => this.leftMouseDown = true;
            this.LeftMouseButtonReleased += (s, e) =>
            {
                this.leftMouseDown = false;
                currentLine = default;
            };
            this.whiteboardService = whiteboardService;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;

            if (currentMode == Mode.Paint)
            {
                this.Paint(mouseX, mouseY);
            }
            else if (currentMode == Mode.Erase)
            {
                this.Erase(mouseX, mouseY);
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
                foreach (var line in this.whiteboardService.UserLines.Values)
                {
                    var currentLine = line;
                    if (Helper.CohenSutherland.IsIntersecting(ref currentLine, ref mouseArea))
                    {
                        linesToDelete.Add(line);
                    }
                }

                this.whiteboardService.DeleteUserLines(linesToDelete);
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
                if (currentLine.Start.X == 0 && currentLine.Start.Y == 0)
                {
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

                    this.whiteboardService.AddUserLine(currentLine);

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

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var line in this.whiteboardService.UserLines.Values.Concat(this.whiteboardService.ServerLines.Values).OrderBy(x => x.TimeStamp))
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
        }
    }
}
