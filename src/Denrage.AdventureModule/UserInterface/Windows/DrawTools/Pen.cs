using Blish_HUD.Controls;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Pen : Tool
    {
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private readonly CounterBox strokeWidth;
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
        }

        public override void OnUpdate(DrawContext context)
        {
            if (context.LeftMouseDown && context.Canvas.AbsoluteBounds.Contains(context.Mouse.ToVector2()))
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
                    currentLine.Start = new Vector2()
                    {
                        X = context.Mouse.X - context.Canvas.AbsoluteBounds.X,
                        Y = context.Mouse.Y - context.Canvas.AbsoluteBounds.Y,
                    };
                }
                else if (currentLine.End is null)
                {
                    currentLine.End = new Vector2()
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
                            A = 255,
                            B = 0,
                            G = 0,
                            R = 0,
                        },
                        Start = new Vector2() { X = currentLine.End.X, Y = currentLine.End.Y },
                    };
                }
            }
        }

        public override void Activate() => 
            this.currentLine = null;

        public override void Deactivate() =>
            this.currentLine = null;
    }
}
