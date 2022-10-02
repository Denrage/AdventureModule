using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class MarkerElement : AdventureElement, IMarkerLua
    {
        private readonly IEntity internalEditEntity;
        private readonly Vector3 position;
        private bool interactPressed;

        public override IEntity EditEntity => this.internalEditEntity;

        public event Action Interacted;

        public void FlipNinetyDegrees()
        {
            ((MarkerEntity)this.internalEditEntity).Rotation = ((MarkerEntity)this.internalEditEntity).Rotation + new Vector3(0, 90, 0);
        }

        public override void Dispose()
        {
            GameService.Graphics.World.RemoveEntity(this.EditEntity);
        }

        public MarkerElement(Vector3 position, Vector3 rotation)
        {
            this.internalEditEntity = new MarkerEntity(Module.Instance.ContentsManager.GetTexture("marker.png"))
            {
                Position = position,
                Rotation = rotation,
            };

            GameService.Graphics.World.AddEntity(this.EditEntity);

            GameService.Input.Keyboard.KeyPressed += (s, e) =>
            {
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.F)
                {
                    this.interactPressed = true;
                }
            };
            this.position = position;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.interactPressed)
            {
                if (Vector3.Distance(GameService.Gw2Mumble.PlayerCharacter.Position, this.position) < 5)
                {
                    this.Interacted?.Invoke();
                }

                this.interactPressed = false;
            }
            base.Update(gameTime);
        }

        private class MarkerEntity : IEntity
        {
            private static DynamicVertexBuffer _sharedVertexBuffer;
            private static readonly Vector4[] _screenVerts = new Vector4[4];

            private static readonly Vector3[] _faceVerts = {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0),
        };

            private static readonly TestEffect sharedEffect;

            private readonly AsyncTexture2D texture;

            public float DrawOrder => default;

            public Vector3 Position { get; set; }

            public Vector3? Rotation { get; set; }

            static MarkerEntity()
            {
                sharedEffect = new TestEffect(Module.Instance.ContentsManager.GetEffect("marker.mgfx"));
                CreateSharedVertexBuffer();
            }

            public MarkerEntity(AsyncTexture2D texture)
            {
                this.texture = texture;
                //this.texture = Module.Instance.ContentsManager.GetTexture("marker.png");
            }

            private static void CreateSharedVertexBuffer()
            {
                using (var context = GameService.Graphics.LendGraphicsDeviceContext())
                {
                    _sharedVertexBuffer = new DynamicVertexBuffer(context.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                }

                var verts = new VertexPositionTexture[_faceVerts.Length];

                for (var i = 0; i < _faceVerts.Length; i++)
                {
                    ref var vert = ref _faceVerts[i];

                    verts[i] = new VertexPositionTexture(vert, new Vector2(vert.X < 0 ? 1 : 0, vert.Y < 0 ? 1 : 0));
                }

                _sharedVertexBuffer.SetData(verts);
            }

            public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
            {
                const float minSize = 5f;
                const float maxSize = 2048f;
                //var position = new Vector3(-25.414f, -4.129f, 121.7f);
                //var direction = new Vector3((float)System.Math.PI / 2, 0f, (float)System.Math.PI / 2);

                var matrix = Matrix.CreateScale(1f, 1f, 1f);

                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                if (!this.Rotation.HasValue)
                {
                    matrix *= Matrix.CreateBillboard(this.Position,
                                              new Vector3(camera.Position.X,
                                                          camera.Position.Y,
                                                          camera.Position.Z),
                                              new Vector3(0, 0, 1),
                                              camera.Forward);

                    // Enforce min/max size
                    var transformMatrix = Matrix.Multiply(Matrix.Multiply(matrix, sharedEffect.View),
                                                          sharedEffect.Projection);

                    for (int i = 0; i < _faceVerts.Length; i++)
                    {
                        _screenVerts[i] = Vector4.Transform(_faceVerts[i], transformMatrix);
                        _screenVerts[i] /= _screenVerts[i].W;
                    }

                    // Very alloc heavy
                    var bounds = BoundingRectangle.CreateFrom(_screenVerts.Select(s => new Point2(s.X, s.Y)).ToArray());

                    float pixelSizeY = bounds.HalfExtents.Y * 2 * sharedEffect.GraphicsDevice.Viewport.Height;
                    float limitY = MathHelper.Clamp(pixelSizeY, minSize * 4, maxSize * 4);

                    // Eww
                    matrix *= Matrix.CreateTranslation(-this.Position)
                                 * Matrix.CreateScale(limitY / pixelSizeY)
                                 * Matrix.CreateTranslation(this.Position);
                }
                else
                {
                    matrix *= Matrix.CreateRotationX(MathHelper.ToRadians(this.Rotation.Value.X))
                     * Matrix.CreateRotationY(MathHelper.ToRadians(this.Rotation.Value.Y))
                     * Matrix.CreateRotationZ(MathHelper.ToRadians(this.Rotation.Value.Z))
                     * Matrix.CreateTranslation(this.Position);
                }

                sharedEffect.SetEntityState(matrix, this.texture, 1f, 0f, 2500f, false, Color.White, true);

                graphicsDevice.SetVertexBuffer(_sharedVertexBuffer);

                foreach (var pass in sharedEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                }
            }

            public void Update(GameTime gameTime)
            {
            }
        }
    }


}


