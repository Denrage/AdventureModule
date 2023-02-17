using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Interfaces.Mumble;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Linq;

namespace Denrage.AdventureModule.Entities
{
    public class MarkerEntity : IEntity
    {
        private static DynamicVertexBuffer _sharedVertexBuffer;
        private static readonly Vector4[] _screenVerts = new Vector4[4];

        private static readonly Vector3[] _faceVerts = {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0),
        };

        private static TestEffect sharedEffect;

        private readonly AsyncTexture2D texture;

        public float DrawOrder => default;

        public Vector3 Position { get; set; }

        public Vector3? Rotation { get; set; }

        static MarkerEntity()
        {
            CreateSharedVertexBuffer();
        }

        public MarkerEntity(AsyncTexture2D texture, IGw2Mumble gw2Mumble)
        {
            this.texture = texture;
            if (sharedEffect is null)
            {
                sharedEffect = new TestEffect(Module.Instance.ContentsManager.GetEffect("marker.mgfx"), gw2Mumble);
            }
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

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

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
                matrix *= Matrix.CreateRotationX(this.Rotation.Value.X)
                 * Matrix.CreateRotationY(this.Rotation.Value.Y)
                 * Matrix.CreateRotationZ(this.Rotation.Value.Z)
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

    public class LineEntity : IEntity
    {
        private BasicEffect effect;
        private readonly IGw2Mumble gw2Mumble;

        public float DrawOrder => default;

        public LineEntity(IGw2Mumble gw2Mumble)
        {
            var context = GameService.Graphics.LendGraphicsDeviceContext();
            this.effect = new BasicEffect(context.GraphicsDevice);
            this.gw2Mumble = gw2Mumble;
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
        {

            this.effect.CurrentTechnique.Passes[0].Apply();

            var lineEnd = new Vector3(
                this.gw2Mumble.PlayerCharacter.Position.X + this.gw2Mumble.PlayerCamera.Forward.X * 10 + 20,
                this.gw2Mumble.PlayerCharacter.Position.Y + this.gw2Mumble.PlayerCamera.Forward.Y * 10,
                this.gw2Mumble.PlayerCharacter.Position.Z + this.gw2Mumble.PlayerCamera.Forward.Z * 10);

            var vertices = new[] { new VertexPositionColor(this.gw2Mumble.PlayerCharacter.Position, Color.White), new VertexPositionColor(lineEnd, Color.White) };
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public void Update(GameTime gameTime)
        {
            this.effect.View = this.gw2Mumble.PlayerCamera.View;
            this.effect.Projection = this.gw2Mumble.PlayerCamera.Projection;
        }
    }
}
