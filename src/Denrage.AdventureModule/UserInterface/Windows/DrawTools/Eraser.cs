﻿using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.UserInterface.Windows.DrawTools
{
    public class Eraser : Tool
    {
        private readonly LoginService loginService;
        private readonly DrawObjectService drawObjectService;
        private readonly CounterBox eraserSize;

        public override string Name => "Eraser";

        public override Container Controls { get; } = new FlowPanel()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
        };

        public Eraser(LoginService loginService, DrawObjectService drawObjectService)
        {
            this.loginService = loginService;
            this.drawObjectService = drawObjectService;

            this.eraserSize = new CounterBox()
            {
                MinValue = 0,
                MaxValue = 20,
                Value = 5,
                Parent = this.Controls,
            };
        }

        public override void OnUpdate(DrawContext context)
        {
            if (context.LeftMouseDown && context.CanvasBounds.Contains(context.Mouse.ToVector2()))
            {
                var rectangleX = context.Mouse.X - this.eraserSize.Value - context.CanvasBounds.X;
                var rectangleY = context.Mouse.Y - this.eraserSize.Value - context.CanvasBounds.Y;
                var mouseArea = new Rectangle(rectangleX, rectangleY, this.eraserSize.Value * 2, this.eraserSize.Value * 2);
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

        public override void Paint(SpriteBatch spriteBatch)
        {
            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;

            var rectangleX = mouseX - this.eraserSize.Value;
            var rectangleY = mouseY - this.eraserSize.Value;
            var mouseArea = new Rectangle(rectangleX, rectangleY, mouseX - rectangleX + this.eraserSize.Value, mouseY - rectangleY + this.eraserSize.Value);
            spriteBatch.DrawRectangle(mouseArea, Color.Black);

        }
    }
}
