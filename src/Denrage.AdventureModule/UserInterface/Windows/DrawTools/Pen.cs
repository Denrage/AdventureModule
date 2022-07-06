using Blish_HUD.Controls;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Pen : Tool
    {
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private Line currentLine = default;

        public override string Name => "Pen";

        public override Container Controls { get; } = new Panel();

        public Pen(LoginService loginService, DrawObjectService drawObjectService)
        {
            this.loginService = loginService;
            this.drawObjectService = drawObjectService;
        }

        public override void OnUpdate(DrawContext context)
        {
            if (context.LeftMouseDown && context.CanvasBounds.Contains(context.Mouse.ToVector2()))
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
                        X = context.Mouse.X - context.CanvasBounds.X,
                        Y = context.Mouse.Y - context.CanvasBounds.Y,
                    };
                }
                else if (currentLine.End is null)
                {
                    currentLine.End = new Vector2()
                    {
                        X = context.Mouse.X - context.CanvasBounds.X,
                        Y = context.Mouse.Y - context.CanvasBounds.Y,
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
                        Start = new Vector2() { X = currentLine.End.X, Y = currentLine.End.Y },
                    };
                }
            }
        }

        public override void Reset()
        {
            this.currentLine = null;
        }
    }
}
