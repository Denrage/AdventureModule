using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Linq;

namespace Denrage.AdventureModule
{
    public class MapMarkerContainer : Control
    {
        private readonly DrawObjectService drawObjectService;

        public Texture2D Texture { get; set; }

        public MapMarkerContainer(DrawObjectService drawObjectService)
        {
            this.Texture = Module.Instance.ContentsManager.GetTexture("marker.png");
            this.ClipsBounds = false;
            this.drawObjectService = drawObjectService;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var markers = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.MapMarker>();
            foreach (var item in markers)
            {
                var location = MumbleUtils.ContinentToMapScreen(item.Position.ToVector());
                location = new Vector2(location.X - 20, location.Y - 10);
                spriteBatch.Draw(this.Texture, new Rectangle((int)location.X, (int)location.Y, 40, 20), Color.White);
            }

        }

        public static class MumbleUtils
        {
            private const int MinCompassWidth = 170;
            private const int MaxCompassWidth = 362;
            private const int MinCompassHeight = 170;
            private const int MaxCompassHeight = 338;
            private const int MinCompassOffset = 19;
            private const int MaxCompassOffset = 40;
            private const int CompassSeparation = 40;

            private static int GetCompassOffset(float curr, float min, float max)
                => (int)Math.Round(MathUtils.Scale(curr, min, max, MinCompassOffset, MaxCompassOffset));

            public static Rectangle GetMapBounds()
            {
                if (GameService.Gw2Mumble.UI.CompassSize.Width < 1 || GameService.Gw2Mumble.UI.CompassSize.Height < 1)
                    return default;

                if (GameService.Gw2Mumble.UI.IsMapOpen)
                    return new Rectangle(Point.Zero, Graphics.SpriteScreen.Size);

                int offsetWidth = GetCompassOffset(GameService.Gw2Mumble.UI.CompassSize.Width, MinCompassWidth, MaxCompassWidth);
                int offsetHeight = GetCompassOffset(GameService.Gw2Mumble.UI.CompassSize.Height, MinCompassHeight, MaxCompassHeight);

                return new Rectangle(
                    Graphics.SpriteScreen.Width - GameService.Gw2Mumble.UI.CompassSize.Width - offsetWidth,
                    GameService.Gw2Mumble.UI.IsCompassTopRight
                        ? 0
                        : Graphics.SpriteScreen.Height - GameService.Gw2Mumble.UI.CompassSize.Height - offsetHeight - CompassSeparation,
                    GameService.Gw2Mumble.UI.CompassSize.Width + offsetWidth,
                    GameService.Gw2Mumble.UI.CompassSize.Height + offsetHeight);
            }

            public static Vector2 ContinentToMapScreen(Vector2 continentCoords)
            {
                var mapCenter = new Vector2((float)GameService.Gw2Mumble.UI.MapCenter.X, (float)GameService.Gw2Mumble.UI.MapCenter.Y);
                var mapRotation = Matrix.CreateRotationZ(
                    GameService.Gw2Mumble.UI.IsCompassRotationEnabled && !GameService.Gw2Mumble.UI.IsMapOpen
                        ? (float)GameService.Gw2Mumble.UI.CompassRotation
                        : 0);

                var screenBounds = MumbleUtils.GetMapBounds();
                var scale = (float)(GameService.Gw2Mumble.UI.MapScale * 0.897);  // Magic number to transform scale
                var boundsCenter = screenBounds.Location.ToVector2() + screenBounds.Size.ToVector2() / 2f;

                return Vector2.Transform((continentCoords - mapCenter) / scale, mapRotation) + boundsCenter;
            }
        }

        public static class MathUtils
        {
            public const double DegToRad = Math.PI / 180;
            public const double RadToDeg = 1 / DegToRad;

            public const double AngleToRad = Math.PI;
            public const double RadToAngle = 1 / AngleToRad;

            public const double AngleToDeg = 180;
            public const double DegToAngle = 1 / AngleToDeg;

            public const float MetersToInches = 39.3700787f;
            public const float InchesToMeters = 1 / MetersToInches;

            public static double Squared(double x) => x * x;
            public static double Cubed(double x) => x * x * x;
            public static double Biquadrated(double x) => x * x * x * x;

            public static float Squared(float x) => x * x;

            public static double Clamp(double x, double min, double max)
                => Math.Min(Math.Max(x, min), max);

            public static double Clamp01(double x) => Clamp(x, 0, 1);

            public static double InverseLerp(double min, double max, double x, bool clamp = false)
                => ((clamp ? Clamp(x, min, max) : x) - min) / (max - min);

            public static double Lerp(double min, double max, double x, bool clamp = false)
                => min + (max - min) * (clamp ? Clamp01(x) : x);

            public static double Scale(double x, double sourceMin, double sourceMax, double targetMin, double targetMax,
                bool clamp = false)
                => Lerp(targetMin, targetMax, InverseLerp(sourceMin, sourceMax, x), clamp);
        }
    }

    public class PlayerMarker : IEntity
    {
        private static DynamicVertexBuffer _sharedVertexBuffer;
        private static readonly Vector4[] _screenVerts = new Vector4[4];

        private static readonly Vector3[] _faceVerts = {
            new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0),
        };

        private static readonly TestEffect sharedEffect;

        private readonly Texture2D texture;

        public float DrawOrder => default;

        public Vector3 Position { get; set; }

        static PlayerMarker()
        {
            sharedEffect = new TestEffect(Module.Instance.ContentsManager.GetEffect("marker.mgfx"));
            CreateSharedVertexBuffer();
        }

        public PlayerMarker()
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
            //var position = new Vector3(-25.414f, -4.129f, 121.7f);
            var position = this.Position;
            var direction = new Vector3((float)System.Math.PI / 2, 0f, (float)System.Math.PI / 2);


            var matrix = Matrix.CreateScale(1f, 1f, 1f);


            matrix *= Matrix.CreateBillboard(position,
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
            matrix *= Matrix.CreateTranslation(-position)
                         * Matrix.CreateScale(limitY / pixelSizeY)
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
        {
        }
    }

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

        private BasicEffect effect = new BasicEffect(GameService.Graphics.GraphicsDevice);

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
            var position = new Vector3(-25.414f, -4.129f, 121.7f);
            var direction = new Vector3((float)System.Math.PI / 2, 0f, (float)System.Math.PI / 2);


            var matrix = Matrix.CreateScale(1f, 1f, 1f);

            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


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

            this.effect.CurrentTechnique.Passes[0].Apply();

            var lineEnd = new Vector3(
                GameService.Gw2Mumble.PlayerCharacter.Position.X + (GameService.Gw2Mumble.PlayerCamera.Forward.X * 10) + 20,
                GameService.Gw2Mumble.PlayerCharacter.Position.Y + (GameService.Gw2Mumble.PlayerCamera.Forward.Y * 10),
                GameService.Gw2Mumble.PlayerCharacter.Position.Z + (GameService.Gw2Mumble.PlayerCamera.Forward.Z * 10));

            var vertices = new[] { new VertexPositionColor(GameService.Gw2Mumble.PlayerCharacter.Position, Color.White), new VertexPositionColor(lineEnd, Color.White) };
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);

        }

        public void Update(GameTime gameTime)
        {
            this.effect.View = GameService.Gw2Mumble.PlayerCamera.View;
            this.effect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
        }
    }
}
