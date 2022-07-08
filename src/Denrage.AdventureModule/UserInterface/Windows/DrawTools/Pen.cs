using Blish_HUD;
using Blish_HUD._Extensions;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Pen : Tool
    {
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private readonly CounterBox strokeWidth;
        private readonly ColorBox colorBox;
        private Line currentLine = default;

        public override string Name => "Pen";

        public override Container Controls { get; } = new FlowPanel()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
        };

        public Pen(LoginService loginService, DrawObjectService drawObjectService)
        {
            this.loginService = loginService;
            this.drawObjectService = drawObjectService;

            this.strokeWidth = new CounterBox()
            {
                MinValue = 1,
                MaxValue = 20,
                Value = 5,
                Parent = this.Controls,
            };

            this.colorBox = new ColorBox()
            {
                Parent = this.Controls,
            };

            this.colorBox.Click += (s, e) =>
            {
                var test = new ColorPickerPopup()
                {
                    Parent = GraphicsService.Graphics.SpriteScreen,
                    Width = 800,
                    Height = 600,
                };

                test.Initialize(this.colorBox);
                test.Show();
            };
        }

        public override void OnUpdateActive(DrawContext context)
        {
            if (context.LeftMouseDown && context.Canvas.AbsoluteBounds.Contains(context.Mouse.ToVector2()))
            {
                var color = this.colorBox.Color?.Cloth?.ToXnaColor() ?? Color.Black;
                
                if (currentLine is null)
                {
                    currentLine = new Line();
                    currentLine.LineColor = new Line.Color()
                    {
                        A = color.A,
                        B = color.B,
                        G = color.G,
                        R = color.R,
                    };
                    currentLine.Start = new Libs.Messages.Data.Vector2()
                    {
                        X = context.Mouse.X - context.Canvas.AbsoluteBounds.X,
                        Y = context.Mouse.Y - context.Canvas.AbsoluteBounds.Y,
                    };
                }
                else if (currentLine.End is null)
                {
                    currentLine.End = new Libs.Messages.Data.Vector2()
                    {
                        X = context.Mouse.X - context.Canvas.AbsoluteBounds.X,
                        Y = context.Mouse.Y - context.Canvas.AbsoluteBounds.Y,
                    };

                    currentLine.TimeStamp = System.DateTime.UtcNow;
                    currentLine.Id = System.Guid.NewGuid();
                    currentLine.Username = this.loginService.Name;
                    currentLine.StrokeWidth = this.strokeWidth.Value;

                    this.drawObjectService.Add(new[] { currentLine }, false, default);

                    currentLine = new Line()
                    {
                        LineColor = new Line.Color()
                        {
                            A = color.A,
                            B = color.B,
                            G = color.G,
                            R = color.R,
                        },
                        Start = new Libs.Messages.Data.Vector2() { X = currentLine.End.X, Y = currentLine.End.Y },
                    };
                }
            }
            else
            {
                currentLine = null;
            }
        }

        public override void Activate() => 
            this.currentLine = null;

        public override void Deactivate() =>
            this.currentLine = null;
    }
}
