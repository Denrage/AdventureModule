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
        private readonly ConcurrentBag<Line> lines = new ConcurrentBag<Line>();
        private readonly List<Line> diffLines = new List<Line>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool leftMouseDown = false;

        public void Initialize(WhiteboardService whiteboardService, TcpService tcpService)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 0, 500, 500 - 30));
            //this.ClipsBounds = true;
            this.LeftMouseButtonPressed += (s, e) => this.leftMouseDown = true;
            this.LeftMouseButtonReleased += (s, e) =>
            {
                this.leftMouseDown = false;
                currentLine = default;
            };
            this.whiteboardService = whiteboardService;
            this.tcpService = tcpService;

            Task.Run(async () => await this.LineTask());
        }

        private async Task LineTask()
        {
            while (true)
            {
                this.cancellationTokenSource.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, this.cancellationTokenSource.Token);
                var linesToSend = new List<Line>();
                lock (this.diffLines)
                {
                    linesToSend = this.diffLines.ToList();
                    this.diffLines.Clear();
                }

                if (linesToSend.Any())
                {
                    await this.tcpService.Send(new WhiteboardAddLineMessage()
                    {
                        Lines = linesToSend,
                    }, this.cancellationTokenSource.Token);
                }
            }
        }

        private Line currentLine = default;
        private WhiteboardService whiteboardService;
        private TcpService tcpService;

        public override void UpdateContainer(GameTime gameTime)
        {
            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;
            if (this.leftMouseDown && mouseX > this.AbsoluteBounds.X && mouseX < this.AbsoluteBounds.X + this.AbsoluteBounds.Width && mouseY > this.AbsoluteBounds.Y + 60 && mouseY < this.AbsoluteBounds.Y + this.AbsoluteBounds.Height + 60)
            {
                if (currentLine.Start.X == 0 && currentLine.Start.Y == 0)
                {
                    currentLine.LineColor = new Line.Color()
                    {
                        A = 255,
                        B = 255,
                        G = 255,
                        R = 255,
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
                    this.lines.Add(currentLine);
                    lock (this.diffLines)
                    {
                        diffLines.Add(currentLine);
                    }
                    currentLine = new Line()
                    {
                        LineColor = new Line.Color()
                        {
                            A = 255,
                            B = 255,
                            G = 255,
                            R = 255,
                        },
                        Start = new Line.Point()
                        {
                            X = mouseX - this.AbsoluteBounds.X,
                            Y = mouseY - this.AbsoluteBounds.Y,
                        },
                    };
                }
            }
            base.UpdateContainer(gameTime);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var line in this.lines.Concat(this.whiteboardService.Lines))
            {
                var startRectangle = new Rectangle(line.Start.X, line.Start.Y, 1, 1);
                startRectangle = startRectangle.ToBounds(this.AbsoluteBounds);
                var endRectangle = new Rectangle(line.End.X, line.End.Y, 1, 1);
                endRectangle = endRectangle.ToBounds(this.AbsoluteBounds);
                spriteBatch.DrawLine(new Vector2(startRectangle.X, startRectangle.Y), new Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), 5);
            }
        }

        protected override void DisposeControl()
        {
            this.cancellationTokenSource.Cancel();
            base.DisposeControl();
        }
    }
}
