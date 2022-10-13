﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class MarkerElement : AdventureElement, IMarkerLua
    {
        private const float INTERACT_RADIUS = 2.5f;

        private readonly MarkerEntity marker;
        private readonly InteractCuboidEntity interactCuboid;
        private readonly Vector3 position;
        private readonly int mapId;
        private readonly AdventureDebugService debugService;
        private bool interactPressed;

        public event Action Interacted;

        public void FlipNinetyDegrees()
        {
            this.marker.Rotation = this.marker.Rotation + new Vector3(0, 90, 0);
        }

        public override void Dispose()
        {
            GameService.Graphics.World.RemoveEntity(this.marker);

            if (this.debugService.IsDebug)
            {
                GameService.Graphics.World.RemoveEntity(this.interactCuboid);
            }
        }

        public MarkerElement(Vector3 position, Vector3 rotation, int mapId, AdventureDebugService debugService, float fadeNear = -1, float fadeFar = -1)
        {
            this.debugService = debugService;
            this.position = position;
            this.mapId = mapId;

            this.marker = new MarkerEntity(Module.Instance.ContentsManager.GetTexture("marker.png"), this.mapId, this.debugService)
            {
                Position = position,
                Rotation = rotation,
            };

            this.interactCuboid = new InteractCuboidEntity(this.mapId)
            {
                Position = position - new Vector3(INTERACT_RADIUS),
                Dimensions = new Vector3(INTERACT_RADIUS * 2f),
            };

            if (fadeNear != -1)
            {
                this.marker.FadeNear = fadeNear;
            }

            if (fadeFar != -1)
            {
                this.marker.FadeFar = fadeFar;
            }

            GameService.Graphics.World.AddEntity(this.marker);

            this.debugService.DebugActivated += () => GameService.Graphics.World.AddEntity(this.interactCuboid);
            this.debugService.DebugDeactivated += () => GameService.Graphics.World.RemoveEntity(this.interactCuboid);

            if (this.debugService.IsDebug)
            {
                GameService.Graphics.World.AddEntity(this.interactCuboid);
            }

            GameService.Input.Keyboard.KeyPressed += (s, e) =>
            {
                if (e.Key == Microsoft.Xna.Framework.Input.Keys.F)
                {
                    this.interactPressed = true;
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (this.mapId == GameService.Gw2Mumble.CurrentMap.Id)
            {
                if (this.interactPressed)
                {
                    if (Vector3.Distance(GameService.Gw2Mumble.PlayerCharacter.Position, this.position) < INTERACT_RADIUS)
                    {
                        this.Interacted?.Invoke();
                    }

                    this.interactPressed = false;
                }
                base.Update(gameTime);
            }
        }

        private class InteractCuboidEntity : IEntity
        {
            private readonly (Vector3 Start, Vector3 End)[] edges = new (Vector3 Start, Vector3 End)[]
            {
                (new Vector3(0,0,0), new Vector3(1,0,0)),
                (new Vector3(0,0,0), new Vector3(0,1,0)),
                (new Vector3(0,0,0), new Vector3(0,0,1)),

                (new Vector3(1,0,0), new Vector3(1,1,0)),
                (new Vector3(1,0,0), new Vector3(1,0,1)),

                (new Vector3(0,1,0), new Vector3(1,1,0)),
                (new Vector3(0,1,0), new Vector3(0,1,1)),

                (new Vector3(0,0,1), new Vector3(0,1,1)),
                (new Vector3(0,0,1), new Vector3(1,0,1)),

                (new Vector3(1,1,1), new Vector3(1,1,0)),
                (new Vector3(1,1,1), new Vector3(0,1,1)),
                (new Vector3(1,1,1), new Vector3(1,0,1)),
            };

            private readonly BasicEffect effect;
            private readonly int mapId;

            public Vector3 Position { get; set; }

            public Vector3 Dimensions { get; set; }

            public float DrawOrder => default;

            public InteractCuboidEntity(int mapId)
            {
                var context = GameService.Graphics.LendGraphicsDeviceContext();
                this.effect = new BasicEffect(context.GraphicsDevice)
                {
                    VertexColorEnabled = true
                };
                context.Dispose();
                this.mapId = mapId;
            }

            public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
            {
                if (this.mapId == GameService.Gw2Mumble.CurrentMap.Id)
                {
                    this.effect.CurrentTechnique.Passes[0].Apply();
                    var vertices = new List<VertexPositionColor>();
                    var playerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;
                    var color = Color.White;

                    var lowerX = Math.Min(this.Position.X, this.Position.X + this.Dimensions.X);
                    var lowerY = Math.Min(this.Position.Y, this.Position.Y + this.Dimensions.Y);
                    var lowerZ = Math.Min(this.Position.Z, this.Position.Z + this.Dimensions.Z);

                    var higherX = Math.Max(this.Position.X, this.Position.X + this.Dimensions.X);
                    var higherY = Math.Max(this.Position.Y, this.Position.Y + this.Dimensions.Y);
                    var higherZ = Math.Max(this.Position.Z, this.Position.Z + this.Dimensions.Z);

                    if (playerPosition.X > lowerX && playerPosition.Y > lowerY && playerPosition.Z > lowerZ &&
                        playerPosition.X < higherX && playerPosition.Y < higherY && playerPosition.Z < higherZ)
                    {
                        color = Color.DarkOrange;
                    }

                    foreach (var item in this.edges)
                    {
                        vertices.Add(new VertexPositionColor(this.Position + new Vector3(this.Dimensions.X * item.Start.X, this.Dimensions.Y * item.Start.Y, this.Dimensions.Z * item.Start.Z), color));
                        vertices.Add(new VertexPositionColor(this.Position + (this.Dimensions * item.End), color));
                    }

                    graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.ToArray(), 0, this.edges.Count());
                }
            }

            public void Update(GameTime gameTime)
            {
                if (this.mapId == GameService.Gw2Mumble.CurrentMap.Id)
                {
                    this.effect.View = GameService.Gw2Mumble.PlayerCamera.View;
                    this.effect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
                }
            }
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
            private readonly int mapId;
            private readonly AdventureDebugService debugService;

            public float DrawOrder => default;

            public Vector3 Position { get; set; }

            public Vector3? Rotation { get; set; }

            public float FadeNear { get; set; } = 0f;

            public float FadeFar { get; set; } = float.MaxValue;

            static MarkerEntity()
            {
                sharedEffect = new TestEffect(Module.Instance.ContentsManager.GetEffect("marker.mgfx"));
                CreateSharedVertexBuffer();
            }

            public MarkerEntity(AsyncTexture2D texture, int mapId, AdventureDebugService debugService)
            {
                this.texture = texture;
                this.mapId = mapId;
                this.debugService = debugService;
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
                if (this.mapId == GameService.Gw2Mumble.CurrentMap.Id)
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

                    sharedEffect.SetEntityState(matrix, this.texture, 1f, this.FadeNear, this.FadeFar, false, Color.White, this.debugService.IsDebug);

                    graphicsDevice.SetVertexBuffer(_sharedVertexBuffer);

                    foreach (var pass in sharedEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    }
                }
            }

            public void Update(GameTime gameTime)
            {
            }
        }
    }
}
