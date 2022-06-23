using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class ImageWindow : Control
    {
        private const int SCALE_STEP = 5;
        private readonly Texture2D texture;
        private readonly IEnumerable<ScaleRectangle> scaleRectangles = new List<ScaleRectangle>()
        {
            new ScaleRectangle() // Left-Top
            {
                Offset = new Vector2(0, 0),
                Scale = new Vector2(-1, -1),
            },
            new ScaleRectangle() // Top
            {
                Offset = new Vector2(0.5f, 0),
                Scale = new Vector2(0, -1),
            },
            new ScaleRectangle() // Right-Top
            {
                Offset = new Vector2(1f, 0f),
                Scale = new Vector2(1, -1),
            },
            new ScaleRectangle() // Left
            {
                Offset = new Vector2(0f, 0.5f),
                Scale = new Vector2(-1, 0),
            },
            new ScaleRectangle() // Left-Bottom
            {
                Offset = new Vector2(0, 1),
                Scale = new Vector2(-1, 1),
            },
            new ScaleRectangle() // Bottom
            {
                Offset = new Vector2(0.5f, 1),
                Scale = new Vector2(0, 1),
            },
            new ScaleRectangle() // Right-Bottom
            {
                Offset = new Vector2(1, 1),
                Scale = new Vector2(1, 1),
            },
            new ScaleRectangle() // Right
            {
                Offset = new Vector2(1, 0.5f),
                Scale = new Vector2(1, 0),
            }
        };

        private Rectangle positionChangeRectangle = new Rectangle(0, 0, 10, 10);

        private bool leftMouseDown;
        private Vector2 currentScale;
        private Point lastMousePosition;
        private float rotation = 0f;
        private bool currrentlyMoving = false;
        private float opacity = 1f;

        string debug = "";

        public ImageWindow(ContentsManager contentsManager)
        {
            this.texture = contentsManager.GetTexture("testimage.jpg");
            this.ClipsBounds = false;
            GameService.Input.Mouse.LeftMouseButtonPressed += (s, e) => this.leftMouseDown = true;
            GameService.Input.Mouse.LeftMouseButtonReleased += (s, e) =>
            {
                this.leftMouseDown = false;
                this.currentScale = new Vector2(0, 0);
                this.currrentlyMoving = false;
            };

            GameService.Debug.OverlayTexts.TryAdd("currentScale", _ => $"CurrentScale: X: {this.currentScale.X} Y: {this.currentScale.Y}");
            GameService.Debug.OverlayTexts.TryAdd("debugSomething", _ => $"Debug: {this.debug}");
        }

        public override void DoUpdate(GameTime gameTime)
        {
            var currentMousePosition = GameService.Input.Mouse.Position;
            var currentScrollWheelValue = GameService.Input.Mouse.State.ScrollWheelValue;
            this.positionChangeRectangle.X = this.AbsoluteBounds.X + ((this.AbsoluteBounds.Width / 2) - 5);
            this.positionChangeRectangle.Y = this.AbsoluteBounds.Y + ((this.AbsoluteBounds.Height / 2) - 5);

            if (!this.currrentlyMoving)
            {
                if (this.leftMouseDown && this.currentScale.X == 0 && this.currentScale.Y == 0)
                {
                    var scaling = false;
                    foreach (var item in this.scaleRectangles)
                    {
                        var rectangle = new Rectangle(this.AbsoluteBounds.X - 10 + (int)(this.AbsoluteBounds.Width * item.Offset.X), this.AbsoluteBounds.Y - 10 + (int)(this.AbsoluteBounds.Height * item.Offset.Y), 20, 20);
                        if (rectangle.Contains(currentMousePosition))
                        {
                            this.currentScale = item.Scale;
                            scaling = true;
                            break;
                        }
                    }

                    if (this.AbsoluteBounds.Contains(currentMousePosition) && !scaling)
                    {
                        if (this.positionChangeRectangle.Contains(currentMousePosition))
                        {
                            this.currrentlyMoving = true;
                        }
                        else
                        {
                            if (currentMousePosition.X != lastMousePosition.X || currentMousePosition.Y != lastMousePosition.Y)
                            {
                                var angle = Math.Atan2(currentMousePosition.Y - (this.positionChangeRectangle.Location.Y + this.positionChangeRectangle.Height / 2), currentMousePosition.X - (this.positionChangeRectangle.Location.X + this.positionChangeRectangle.Width / 2)) - Math.Atan2(lastMousePosition.Y - (this.positionChangeRectangle.Location.Y + this.positionChangeRectangle.Height / 2), lastMousePosition.X - (this.positionChangeRectangle.Location.X + this.positionChangeRectangle.Width / 2));
                                this.rotation += (float)angle;
                            }
                        }
                    }
                }

                if (lastMousePosition != currentMousePosition && (this.currentScale.X != 0 || this.currentScale.Y != 0))
                {
                    var newX = this.Location.X;
                    var newY = this.Location.Y;
                    var newWidth = this.Width;
                    var newHeight = this.Height;

                    var step = 0d;

                    if (this.currentScale.X != 0)
                    {
                        step += Math.Abs(currentMousePosition.X - lastMousePosition.X);
                    }

                    if (this.currentScale.Y != 0)
                    {
                        step += Math.Abs(currentMousePosition.Y - lastMousePosition.Y);
                    }

                    var normalizedMouseVector = new Vector2(currentMousePosition.X, currentMousePosition.Y) - new Vector2(lastMousePosition.X, lastMousePosition.Y);
                    normalizedMouseVector.Normalize();
                    var scaleVector = new Vector2(this.currentScale.X, this.currentScale.Y);
                    scaleVector.Normalize();
                    step *= Math.Abs(Math.Acos(scaleVector.Dot(normalizedMouseVector))) < 1.57908 ? 1 : -1;

                    if (this.currentScale.X == -1)
                    {
                        newX -= (int)step;
                        newWidth += (int)step;
                    }
                    else if (this.currentScale.X == 1)
                    {
                        newWidth += (int)step;
                    }

                    if (this.currentScale.Y == -1)
                    {
                        newY -= (int)step;
                        newHeight += (int)step;
                    }
                    else if (this.currentScale.Y == 1)
                    {
                        newHeight += (int)step;

                    }

                    this.Location = new Point(newX, newY);
                    this.Size = new Point(newWidth, newHeight);
                }
            }
            else
            {
                this.Location = new Point(currentMousePosition.X - (this.AbsoluteBounds.Width / 2), currentMousePosition.Y - (this.AbsoluteBounds.Height / 2));
            }


            if (currentScrollWheelValue != 0 && this.AbsoluteBounds.Contains(currentMousePosition))
            {
                this.opacity += Math.Sign(currentScrollWheelValue) * 0.10f;

                this.opacity = MathHelper.Clamp(this.opacity, 0f, 1f);
            }

            lastMousePosition = currentMousePosition;
            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Scale
            var scale = new Vector2((float)this.AbsoluteBounds.Width / (float)this.texture.Width, (float)this.AbsoluteBounds.Height / (float)this.texture.Height);

            var origin = new Vector2((float)this.texture.Width / 2f, (float)this.texture.Height / 2f);
            var scaledOrigin = scale * origin;
            var destination = new Vector2(this.AbsoluteBounds.X, this.AbsoluteBounds.Y);
            destination.X += (int)scaledOrigin.X;
            destination.Y += (int)scaledOrigin.Y;

            // Transparency
            var color = Color.White * this.opacity;

            // Rotation
            spriteBatch.Draw(this.texture, destination, null, color, this.rotation, origin, scale, SpriteEffects.None, 0);
            //spriteBatch.DrawRectangle(this.AbsoluteBounds, Color.OrangeRed, 3, 0);
            //spriteBatch.DrawCircle(new Vector2(scaledOrigin.X + this.AbsoluteBounds.X, scaledOrigin.Y + this.AbsoluteBounds.Y), 5, 10, Color.OrangeRed, 2, 0);

            spriteBatch.DrawRectangle(this.AbsoluteBounds, Color.OrangeRed, 2);
            this.DrawScaleRectangles(spriteBatch);
            this.DrawPositionChangeRectangle(spriteBatch);
        }

        private void DrawScaleRectangles(SpriteBatch spriteBatch)
        {
            foreach (var item in this.scaleRectangles)
            {
                spriteBatch.DrawRectangle(new Rectangle(this.AbsoluteBounds.X - 10 + (int)(this.AbsoluteBounds.Width * item.Offset.X), this.AbsoluteBounds.Y - 10 + (int)(this.AbsoluteBounds.Height * item.Offset.Y), 20, 20), Color.OrangeRed, 2);
            }
        }

        private void DrawPositionChangeRectangle(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(this.positionChangeRectangle, Color.OrangeRed, 2);
        }
    }

    public class ScaleRectangle
    {
        public Vector2 Offset { get; set; }

        public Vector2 Scale { get; set; }
    }
}
