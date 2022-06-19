using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Graphics;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AdventureModule.Libs.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Denrage.AdventureModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {

        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly WhiteboardService whiteboardService;
        private readonly TcpService tcpService;

        internal static Module Instance { get; private set; }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
            this.whiteboardService = new WhiteboardService();
            this.tcpService = new TcpService(this.whiteboardService);
        }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            //GameService.Graphics.World.AddEntity(new TestEntity());
            var window = new CanvasWindow()
            {
                Parent = GraphicsService.Graphics.SpriteScreen,
            };
            await this.tcpService.Initialize();
            window.Initialize(this.whiteboardService, this.tcpService);
            window.Show();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {

        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }

    public class TcpService
    {
        private TcpClient client;
        private NetworkStream stream;
        private readonly Dictionary<string, (Type Type, MessageHandler Handler)> messageTypes;

        public TcpService(WhiteboardService whiteboardService)
        {
            this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
            {
                { typeof(WhiteboardAddLineMessage).Name, (typeof(WhiteboardAddLineMessage), new WhiteboardAddLineMessageHandler(whiteboardService)) },
            };
        }

        public async Task Initialize()
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5837);
            this.stream = client.GetStream();
            _ = Task.Run(async () => this.ReadTask(this.stream));
        }

        private async Task ReadTask(NetworkStream networkStream)
        {
            const string EndOfMessageToken = "✷";
            var messageToken = System.Text.Encoding.UTF8.GetBytes(EndOfMessageToken);
            while (client.Connected)
            {
                if (client.Available == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }

                using (var memoryStream = new MemoryStream())
                {
                    var bytesRead = -1;
                    var buffer = new byte[4096];
                    while (bytesRead != 0)
                    {
                        bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        _ = memoryStream.Seek(0, SeekOrigin.Begin);

                        var messageTokenPosition = -1;
                        do
                        {
                            messageTokenPosition = -1;
                            var tempData = memoryStream.ToArray();
                            for (int i = 0; i < tempData.Length; i++)
                            {
                                if (tempData[i] == messageToken[0] && tempData.Length - i >= messageToken.Length)
                                {
                                    var setToken = true;
                                    for (int j = 0; j < messageToken.Length; j++)
                                    {
                                        if (tempData[i + j] != messageToken[j])
                                        {
                                            setToken = false;
                                        }
                                    }

                                    if (setToken)
                                    {
                                        messageTokenPosition = i;
                                        break;
                                    }
                                }
                            }

                            if (messageTokenPosition != -1)
                            {
                                _ = memoryStream.Seek(0, SeekOrigin.Begin);
                                var data = new byte[messageTokenPosition];
                                _ = await memoryStream.ReadAsync(data, 0, data.Length);

                                _ = memoryStream.Seek(0, SeekOrigin.Begin);
                                var indexNextMessage = memoryStream.Length - (messageTokenPosition + messageToken.Length);
                                if (indexNextMessage != 0)
                                {
                                    var remainingData = new byte[memoryStream.Length - indexNextMessage];
                                    _ = memoryStream.Seek(messageTokenPosition + messageToken.Length, SeekOrigin.Begin);
                                    _ = await memoryStream.ReadAsync(remainingData, 0, remainingData.Length);
                                    _ = memoryStream.Seek(0, SeekOrigin.Begin);
                                    memoryStream.SetLength(0);
                                    await memoryStream.WriteAsync(remainingData, 0, remainingData.Length);
                                }
                                else
                                {
                                    memoryStream.SetLength(0);
                                }

                                _ = Task.Run(() => this.Handle(data));
                            }
                        } while (messageTokenPosition != -1);
                    }
                }
            }
        }

        private async Task Handle(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
            var message = System.Text.Json.JsonSerializer.Deserialize(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type);
            await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(message);
        }

        public async Task Send<T>(T message)
            where T : Message
        {
            const string EndOfMessageToken = "✷";
            var tcpMessage = new TcpMessage();

            tcpMessage.TypeIdentifier = typeof(T).Name;
            tcpMessage.Data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
            
            var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);

            data = data + EndOfMessageToken;

            if (this.client.Connected)
            {
                using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
                {
                    await memoryStream.CopyToAsync(this.stream);
                }
            }
        }
    }


    public abstract class MessageHandler
    {
        public abstract Task Handle(object message);
    }

    public abstract class MessageHandler<T> : MessageHandler
        where T : Message
    {
        public override async Task Handle(object message)
            => await this.Handle((T)message);

        protected abstract Task Handle(T message);
    }

    public class WhiteboardAddLineMessageHandler : MessageHandler<WhiteboardAddLineMessage>
    {
        private readonly WhiteboardService whiteboardService;

        public WhiteboardAddLineMessageHandler(WhiteboardService whiteboardService)
        {
            this.whiteboardService = whiteboardService;
        }

        protected override async Task Handle(WhiteboardAddLineMessage message)
        {
            this.whiteboardService.AddLines(message.Lines);
            await Task.CompletedTask;
        }
    }

    public class WhiteboardService
    {
        public ConcurrentBag<Line> Lines = new ConcurrentBag<Line>();

        public void AddLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                this.Lines.Add(line);
            }
        }
    }

    public class CanvasWindow : WindowBase2
    {
        private readonly ConcurrentBag<Line> lines = new ConcurrentBag<Line>();
        private readonly List<Line> diffLines = new List<Line>();
        private bool leftMouseDown = false;

        public void Initialize(WhiteboardService whiteboardService, TcpService tcpService)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 0, 500, 500 - 30));
            //this.ClipsBounds = true;
            this.LeftMouseButtonPressed += (s, e) => this.leftMouseDown = true;
            this.LeftMouseButtonReleased += (s, e) =>
            {
                this.leftMouseDown = false;
                currentLine = default;
            };
            this.whiteboardService = whiteboardService;
            this.tcpService = tcpService;

            Task.Run(async () => await this.LineTask());
        }

        private async Task LineTask()
        {
            while (true)
            {
                await Task.Delay(10);
                var linesToSend = new List<Line>();
                lock (this.diffLines)
                {
                    linesToSend = this.diffLines.ToList();
                    this.diffLines.Clear();
                }

                if (linesToSend.Any())
                {
                    await this.tcpService.Send(new WhiteboardAddLineMessage()
                    {
                        Lines = linesToSend,
                    });
                }
            }
        }

        private Line currentLine = default;
        private WhiteboardService whiteboardService;
        private TcpService tcpService;
        
        public override void UpdateContainer(GameTime gameTime)
        {
            var mouseX = GameService.Input.Mouse.Position.X;
            var mouseY = GameService.Input.Mouse.Position.Y;
            if (this.leftMouseDown && mouseX > this.AbsoluteBounds.X && mouseX < this.AbsoluteBounds.X + this.AbsoluteBounds.Width && mouseY > this.AbsoluteBounds.Y + 60 && mouseY < this.AbsoluteBounds.Y + this.AbsoluteBounds.Height + 60)
            {
                if (currentLine.Start.X == 0 && currentLine.Start.Y == 0)
                {
                    currentLine.LineColor = new Line.Color()
                    {
                        A = 255,
                        B = 255,
                        G = 255,
                        R = 255,
                    };
                    currentLine.Start = new Line.Point()
                    {
                        X = mouseX - this.AbsoluteBounds.X,
                        Y = mouseY - this.AbsoluteBounds.Y,
                    };
                }
                else if (currentLine.End.X == 0 && currentLine.End.Y == 0)
                {
                    currentLine.End = new Line.Point()
                    {
                        X = mouseX - this.AbsoluteBounds.X,
                        Y = mouseY - this.AbsoluteBounds.Y,
                    };
                    this.lines.Add(currentLine);
                    lock(this.diffLines)
                    {
                        diffLines.Add(currentLine);
                    }
                    currentLine = new Line()
                    {
                        LineColor = new Line.Color()
                        {
                            A = 255,
                            B = 255,
                            G = 255,
                            R = 255,
                        },
                        Start = new Line.Point()
                        {
                            X = mouseX - this.AbsoluteBounds.X,
                            Y = mouseY - this.AbsoluteBounds.Y,
                        },
                    };
                }
            }
            base.UpdateContainer(gameTime);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var line in this.lines.Concat(this.whiteboardService.Lines))
            {
                var startRectangle = new Rectangle(line.Start.X, line.Start.Y, 1, 1);
                startRectangle = startRectangle.ToBounds(this.AbsoluteBounds);
                var endRectangle = new Rectangle(line.End.X, line.End.Y, 1, 1);
                endRectangle = endRectangle.ToBounds(this.AbsoluteBounds);
                spriteBatch.DrawLine(new Vector2(startRectangle.X, startRectangle.Y), new Vector2(endRectangle.X, endRectangle.Y), new Color(line.LineColor.R, line.LineColor.G, line.LineColor.B, line.LineColor.A), 5);
            }
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

    public class TestEffect : SharedEffect
    {
        // Per-effect parameters
        private const string PARAMETER_VIEW = "View";
        private const string PARAMETER_PROJECTION = "Projection";
        private const string PARAMETER_PLAYERVIEW = "PlayerView";
        private const string PARAMETER_PLAYERPOSITION = "PlayerPosition";
        private const string PARAMETER_CAMERAPOSITION = "CameraPosition";

        private Matrix _view, _projection, _playerView;
        private Vector3 _playerPosition;
        private Vector3 _cameraPosition;

        public Matrix View
        {
            get => _view;
            set => SetParameter(PARAMETER_VIEW, ref _view, value);
        }

        public Matrix Projection
        {
            get => _projection;
            set => SetParameter(PARAMETER_PROJECTION, ref _projection, value);
        }

        public Matrix PlayerView
        {
            get => _playerView;
            set => SetParameter(PARAMETER_PLAYERVIEW, ref _playerView, value);
        }

        public Vector3 PlayerPosition
        {
            get => _playerPosition;
            set => SetParameter(PARAMETER_PLAYERPOSITION, ref _playerPosition, value);
        }

        public Vector3 CameraPosition
        {
            get => _cameraPosition;
            set => SetParameter(PARAMETER_CAMERAPOSITION, ref _cameraPosition, value);
        }

        // Universal

        private const string PARAMETER_RACE = "Race";
        private const string PARAMETER_MOUNT = "Mount";
        private const string PARAMETER_FADENEARCAMERA = "FadeNearCamera";

        private int _race;
        private int _mount;
        private bool _fadeNearCamera;

        public int Race
        {
            get => _race;
            set => SetParameter(PARAMETER_RACE, ref _race, value);
        }

        public int Mount
        {
            get => _mount;
            set => SetParameter(PARAMETER_MOUNT, ref _mount, value);
        }

        // Entity-unique parameters
        private const string PARAMETER_WORLD = "World";
        private const string PARAMETER_TEXTURE = "Texture";
        private const string PARAMETER_FADETEXTURE = "FadeTexture";
        private const string PARAMETER_OPACITY = "Opacity";
        private const string PARAMETER_FADENEAR = "FadeNear";
        private const string PARAMETER_FADEFAR = "FadeFar";
        private const string PARAMETER_PLAYERFADERADIUS = "PlayerFadeRadius";
        private const string PARAMETER_FADECENTER = "FadeCenter";
        private const string PARAMETER_TINTCOLOR = "TintColor";
        private const string PARAMETER_SHOWDEBUGWIREFRAME = "ShowDebugWireframe";

        private Matrix _world;
        private Texture2D _texture;
        private Texture2D _fadeTexture;
        private float _opacity;
        private float _fadeNear, _fadeFar;
        private float _playerFadeRadius;
        private bool _fadeCenter;
        private Color _tintColor;
        private bool _showDebugWireframe;

        public Matrix World
        {
            get => _world;
            set => SetParameter(PARAMETER_WORLD, ref _world, value);
        }

        public Texture2D Texture
        {
            get => _texture;
            set => SetParameter(PARAMETER_TEXTURE, ref _texture, value);
        }

        public Texture2D FadeTexture
        {
            get => _fadeTexture;
            set => SetParameter(PARAMETER_FADETEXTURE, ref _fadeTexture, value);
        }

        public float Opacity
        {
            get => _opacity;
            set => SetParameter(PARAMETER_OPACITY, ref _opacity, value);
        }

        public float FadeNear
        {
            get => _fadeNear;
            set => SetParameter(PARAMETER_FADENEAR, ref _fadeNear, value);
        }

        public float FadeFar
        {
            get => _fadeFar;
            set => SetParameter(PARAMETER_FADEFAR, ref _fadeFar, value);
        }

        public float PlayerFadeRadius
        {
            get => _playerFadeRadius;
            set => SetParameter(PARAMETER_PLAYERFADERADIUS, ref _playerFadeRadius, value);
        }

        public bool FadeCenter
        {
            get => _fadeCenter;
            set => SetParameter(PARAMETER_FADECENTER, ref _fadeCenter, value);
        }

        public bool FadeNearCamera
        {
            get => _fadeNearCamera;
            set => SetParameter(PARAMETER_FADENEARCAMERA, ref _fadeNearCamera, value);
        }

        public Color TintColor
        {
            get => _tintColor;
            set => SetParameter(PARAMETER_TINTCOLOR, ref _tintColor, value);
        }

        public bool ShowDebugWireframe
        {
            get => _showDebugWireframe;
            set => SetParameter(PARAMETER_SHOWDEBUGWIREFRAME, ref _showDebugWireframe, value);
        }

        #region ctors

        public TestEffect(Effect baseEffect) : base(baseEffect) { }

        private TestEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { }

        private TestEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { }

        #endregion

        public void SetEntityState(Matrix world, Texture2D texture, float opacity, float fadeNear, float fadeFar, bool fadeNearCamera, Color tintColor, bool showDebugWireframe)
        {
            this.World = world;
            this.Texture = texture;
            this.Opacity = opacity;
            this.FadeNear = fadeNear;
            this.FadeFar = fadeFar;
            this.FadeNearCamera = fadeNearCamera;
            this.TintColor = tintColor;
            this.ShowDebugWireframe = showDebugWireframe;
        }

        protected override void Update(GameTime gameTime)
        {
            this.PlayerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;
            this.CameraPosition = GameService.Gw2Mumble.PlayerCamera.Position;

            // Universal
            this.Mount = (int)GameService.Gw2Mumble.PlayerCharacter.CurrentMount;
            this.Race = (int)GameService.Gw2Mumble.PlayerCharacter.Race;

            this.View = GameService.Gw2Mumble.PlayerCamera.View;
            this.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
            this.PlayerView = GameService.Gw2Mumble.PlayerCamera.PlayerView;
        }
    }
}
