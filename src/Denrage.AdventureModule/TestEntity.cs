using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Denrage.AdventureModule
{
    public class TestEntity : IEntity
    {
        private static DynamicVertexBuffer _sharedVertexBuffer;
        private static readonly Vector4[] _screenVerts = new Vector4[4];

        private static readonly Vector3[] _faceVerts = {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0),
        };

        private static readonly TestEffect sharedEffect;

        private readonly Texture2D texture;

        public float DrawOrder => default;

        static TestEntity()
        {
            sharedEffect = new TestEffect(Module.Instance.ContentsManager.GetEffect("marker.mgfx"));
            CreateSharedVertexBuffer();
        }

        public TestEntity()
        {
            this.texture = Module.Instance.ContentsManager.GetTexture("marker.png");
        }

        private static void CreateSharedVertexBuffer()
        {
            _sharedVertexBuffer = new DynamicVertexBuffer(GameService.Graphics.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);

            var verts = new VertexPositionTexture[_faceVerts.Length];

            for (int i = 0; i < _faceVerts.Length; i++)
            {
                ref var vert = ref _faceVerts[i];

                verts[i] = new VertexPositionTexture(vert, new Vector2(vert.X < 0 ? 1 : 0, vert.Y < 0 ? 1 : 0));
            }

            _sharedVertexBuffer.SetData(verts);
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
        {
            const float heightOffset = 1.5f;
            const float minSize = 5f;
            const float maxSize = 2048f;
            var position = new Vector3(-26.014f, -4.329f, 121f);
            var direction = new Vector3(-0.999f, -0.048f, 0.02f);


            var matrix = Matrix.CreateScale(1f, 1f, 1f);


            //matrix *= Matrix.CreateBillboard(position,
            //                          new Vector3(camera.Position.X,
            //                                      camera.Position.Y,
            //                                      camera.Position.Z),
            //                          new Vector3(0, 0, 1),
            //                          camera.Forward);

            //// Enforce min/max size
            //var transformMatrix = Matrix.Multiply(Matrix.Multiply(matrix, sharedEffect.View),
            //                                      sharedEffect.Projection);

            //for (int i = 0; i < _faceVerts.Length; i++)
            //{
            //    _screenVerts[i] = Vector4.Transform(_faceVerts[i], transformMatrix);
            //    _screenVerts[i] /= _screenVerts[i].W;
            //}

            //// Very alloc heavy
            //var bounds = BoundingRectangle.CreateFrom(_screenVerts.Select(s => new Point2(s.X, s.Y)).ToArray());

            //float pixelSizeY = bounds.HalfExtents.Y * 2 * sharedEffect.GraphicsDevice.Viewport.Height;
            //float limitY = MathHelper.Clamp(pixelSizeY, minSize * 4, maxSize * 4);

            //// Eww
            //matrix *= Matrix.CreateTranslation(-position)
            //             * Matrix.CreateScale(limitY / pixelSizeY)
            //             * Matrix.CreateTranslation(position);

            matrix *= Matrix.CreateRotationX(direction.X)
             * Matrix.CreateRotationY(direction.Y)
             * Matrix.CreateRotationZ(direction.Z)
             * Matrix.CreateTranslation(position);

            sharedEffect.SetEntityState(matrix, this.texture, 1f, 0f, 2500f, false, Color.White, true);

            graphicsDevice.SetVertexBuffer(_sharedVertexBuffer);

            foreach (var pass in sharedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

        }

        public void Update(GameTime gameTime)
        { }
    }
}
