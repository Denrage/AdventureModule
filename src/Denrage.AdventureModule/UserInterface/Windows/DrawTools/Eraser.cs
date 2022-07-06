using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Eraser : Tool
    {
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;

        public Eraser(LoginService loginService, DrawObjectService drawObjectService)
        {
            this.loginService = loginService;
            this.drawObjectService = drawObjectService;
        }

        public override void OnUpdate(DrawContext context)
        {
            if (context.LeftMouseDown && context.CanvasBounds.Contains(context.Mouse.ToVector2()))
            {
                var rectangleX = context.Mouse.X - 5 - context.CanvasBounds.X;
                var rectangleY = context.Mouse.Y - 5 - context.CanvasBounds.Y;
                var mouseArea = new Rectangle(rectangleX, rectangleY, 10, 10);
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
    }
}
