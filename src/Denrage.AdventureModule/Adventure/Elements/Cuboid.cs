using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Adventure.Interfaces;
using Denrage.AdventureModule.Adventure.Services;
using Denrage.AdventureModule.Interfaces.Mumble;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure.Elements
{
    public class Cuboid : AdventureElement, ICuboidLua
    {
        private Vector3 position;
        private Vector3 dimensions;
        private readonly CuboidEntity debugEntity;
        private readonly int mapId;
        private readonly AdventureDebugService debugService;
        private readonly IGw2Mumble gw2Mumble;
        private bool entered = false;

        public event Action PlayerEntered;

        public Vector3 Position
        {
            get => position;

            set
            {
                position = value;
                this.debugEntity.Position = value;
            }
        }

        public Vector3 Dimensions
        {
            get => dimensions;

            set
            {
                dimensions = value;
                this.debugEntity.Dimensions = value;
            }
        }

        public bool CharacterInside { get; set; } = false;

        public Cuboid(int mapId, AdventureDebugService debugService, IGw2Mumble gw2Mumble)
        {
            this.mapId = mapId;
            this.debugService = debugService;
            this.gw2Mumble = gw2Mumble;
            this.debugEntity = new CuboidEntity(mapId, gw2Mumble);

            this.debugService.DebugActivated += () => GameService.Graphics.World.AddEntity(this.debugEntity);
            this.debugService.DebugDeactivated += () => GameService.Graphics.World.RemoveEntity(this.debugEntity);

            if (this.debugService.IsDebug)
            {
                GameService.Graphics.World.AddEntity(this.debugEntity);
            }
        }

        public void Test()
        {
            ScreenNotification.ShowNotification("WORKS!");
        }

        public override void Update(GameTime gameTime)
        {
            if (this.mapId == this.gw2Mumble.CurrentMap.Id)
            {
                var playerPosition = this.gw2Mumble.PlayerCharacter.Position;

                var lowerX = Math.Min(this.Position.X, this.Position.X + this.Dimensions.X);
                var lowerY = Math.Min(this.Position.Y, this.Position.Y + this.Dimensions.Y);
                var lowerZ = Math.Min(this.Position.Z, this.Position.Z + this.Dimensions.Z);

                var higherX = Math.Max(this.Position.X, this.Position.X + this.Dimensions.X);
                var higherY = Math.Max(this.Position.Y, this.Position.Y + this.Dimensions.Y);
                var higherZ = Math.Max(this.Position.Z, this.Position.Z + this.Dimensions.Z);

                if (playerPosition.X > lowerX && playerPosition.Y > lowerY && playerPosition.Z > lowerZ &&
                    playerPosition.X < higherX && playerPosition.Y < higherY && playerPosition.Z < higherZ)
                {
                    if (!this.entered)
                    {
                        this.PlayerEntered?.Invoke();
                        this.entered = true;
                        this.CharacterInside = true;
                    }
                }
                else
                {
                    this.entered = false;
                    this.CharacterInside = false;
                }
            }
        }

        public bool IsCharacterInside(string name) => throw new NotImplementedException();

        public override void Dispose()
        {
            if (this.debugService.IsDebug)
            {
                GameService.Graphics.World.RemoveEntity(this.debugEntity);
            }
        }

        private class CuboidEntity : IEntity
        {
            private (Vector3 Start, Vector3 End)[] edges = new (Vector3 Start, Vector3 End)[]
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

            private BasicEffect effect;
            private readonly int mapId;
            private readonly IGw2Mumble gw2Mumble;

            public Vector3 Position { get; set; }

            public Vector3 Dimensions { get; set; }

            public float DrawOrder => default;

            public CuboidEntity(int mapId, IGw2Mumble gw2Mumble)
            {
                var context = GameService.Graphics.LendGraphicsDeviceContext();
                this.effect = new BasicEffect(context.GraphicsDevice);
                this.effect.VertexColorEnabled = true;
                context.Dispose();
                this.mapId = mapId;
                this.gw2Mumble = gw2Mumble;
            }

            public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
            {
                if (this.mapId == this.gw2Mumble.CurrentMap.Id)
                {
                    this.effect.CurrentTechnique.Passes[0].Apply();
                    var vertices = new List<VertexPositionColor>();
                    var playerPosition = this.gw2Mumble.PlayerCharacter.Position;
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
                        vertices.Add(new VertexPositionColor(this.Position + this.Dimensions * item.End, color));
                    }

                    graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices.ToArray(), 0, this.edges.Count());
                }
            }

            public void Update(GameTime gameTime)
            {
                if (this.mapId == this.gw2Mumble.CurrentMap.Id)
                {
                    this.effect.View = this.gw2Mumble.PlayerCamera.View;
                    this.effect.Projection = this.gw2Mumble.PlayerCamera.Projection;
                }
            }
        }
    }
}

