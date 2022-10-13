using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class Cuboid : AdventureElement, ICuboidLua
    {
        private Vector3 position;
        private Vector3 dimensions;
        private readonly CuboidEntity internalEditEntity;
        private readonly int mapId;
        private bool entered = false;

        public event Action PlayerEntered;

        public Vector3 Position
        {
            get => position;

            set
            {
                position = value;
                this.internalEditEntity.Position = value;
            }
        }

        public Vector3 Dimensions
        {
            get => dimensions;

            set
            {
                dimensions = value;
                this.internalEditEntity.Dimensions = value;
            }
        }

        public override IEntity EditEntity => this.internalEditEntity;

        public bool CharacterInside { get; set; } = false;

        public Cuboid(int mapId)
        {
            this.mapId = mapId;
            this.internalEditEntity = new CuboidEntity(mapId);
            GameService.Graphics.World.AddEntity(this.EditEntity);
        }

        public void Test()
        {
            ScreenNotification.ShowNotification("WORKS!");
        }

        public override void Update(GameTime gameTime)
        {
            if (this.mapId == GameService.Gw2Mumble.CurrentMap.Id)
            {
                var playerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;

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
            GameService.Graphics.World.RemoveEntity(this.EditEntity);
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

            public Vector3 Position { get; set; }

            public Vector3 Dimensions { get; set; }

            public float DrawOrder => default;

            public CuboidEntity(int mapId)
            {
                var context = GameService.Graphics.LendGraphicsDeviceContext();
                this.effect = new BasicEffect(context.GraphicsDevice);
                this.effect.VertexColorEnabled = true;
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
    }


}


