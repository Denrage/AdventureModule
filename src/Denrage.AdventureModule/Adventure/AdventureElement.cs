using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Denrage.AdventureModule.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Neo.IronLua;
using Stateless;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Denrage.AdventureModule.Adventure
{
    public abstract class AdventureElement : IDisposable
    {
        public abstract IEntity EditEntity { get; }

        public abstract void Dispose();

        public virtual void Update(GameTime gameTime) { }
    }

    public class LogicBuilderCreator
    {
        public object CreateButtonOrderLogic()
        {
            return new ButtonOrderBuilder();
        }
    }

    public class AdventureElementCreator
    {
        private Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();

        public IEnumerable<AdventureElement> Elements
        {
            get
            {
                lock (this.elements)
                {
                    return this.elements.Values.ToArray();
                }
            }
        }

        public object CreateCuboid(string name, Vector3 position, Vector3 dimension)
        {
            var result = new Cuboid()
            {
                Position = position,
                Dimensions = dimension,
            };

            this.elements[name] = result;
            return result;
        }

        public object CreateMarker(string name, Vector3 position, Vector3 rotation)
        {
            lock (this.elements)
            {
                var result = new MarkerElement(position, rotation);
                this.elements[name] = result;
                return result;
            }
        }

        public void Clear()
        {
            lock (this.elements)
            {
                var elements = this.Elements;
                this.elements.Clear();
                foreach (var item in elements)
                {
                    item.Dispose();
                }
            }
        }
    }

    public interface IButtonOrderBuilderLua
    {
        event Action Finished;

        event Action<IMarkerLua[]> StateChanged;

        IButtonOrderBuilderLua Add(IMarkerLua button, int delay = 0);

        void Build();
    }

    public class ButtonOrderBuilder : IButtonOrderBuilderLua
    {
        private readonly List<ButtonState> buttonStates = new List<ButtonState>();
        private ButtonState initialState;
        private ButtonState currentState;

        public event Action<IMarkerLua[]> StateChanged;

        public event Action Finished;

        public IButtonOrderBuilderLua Add(IMarkerLua button, int delay = 0)
        {
            var state = new ButtonState()
            {
                Delay = delay,
                Button = button,
            };

            state.OnFailure += () =>
            {
                System.Diagnostics.Debug.WriteLine("Failed. Return to initial");
                this.currentState = this.initialState;
            };
            state.OnSuccess += () => this.MoveToNextStep();

            button.Interacted += () => this.currentState.OnPressed(button);

            if (this.buttonStates.Count == 0)
            {
                state.Delay = default;
                this.initialState = state;
            }

            this.buttonStates.Add(state);
            return this;
        }

        private void MoveToNextStep()
        {
            if (this.currentState is null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("Switch to next step");

            this.currentState = this.currentState.NextState;

            var pressedButtons = new List<IMarkerLua>();
            var currentState = this.initialState;
            while (currentState != this.currentState)
            {
                pressedButtons.Add(currentState.Button);
                currentState = currentState.NextState;
            }

            this.StateChanged?.Invoke(pressedButtons.ToArray());

            if (this.currentState is null)
            {
                Finished?.Invoke();
                return;
            }

            this.currentState.OnEnter();
        }

        public void Build()
        {
            for (int i = 0; i < this.buttonStates.Count - 1; i++)
            {
                this.buttonStates[i].NextState = this.buttonStates[i + 1];
            }

            this.currentState = this.initialState;
        }

        private class ButtonState
        {
            private CancellationTokenSource delayToken;

            public event Action OnSuccess;

            public event Action OnFailure;

            public IMarkerLua Button { get; set; }

            public int Delay { get; set; }

            public ButtonState NextState { get; set; }

            public void OnPressed(IMarkerLua button)
            {
                this.delayToken?.Cancel();
                if (button == this.Button)
                {
                    this.OnSuccess?.Invoke();
                }
                else
                {
                    this.OnFailure?.Invoke();
                }
            }

            public void OnEnter()
            {
                if (this.Delay != default)
                {
                    this.delayToken = new CancellationTokenSource();
                    try
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(this.Delay), delayToken.Token);
                            this.OnFailure.Invoke();
                        });
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }
        }
    }

    public class CharacterInformation : ICharacterLua
    {
        public Vector3 Position => GameService.Gw2Mumble.PlayerCharacter.Position;

        public event Action<string> EmoteUsed;

        public void FireEmoteUsed()
        {
            this.EmoteUsed?.Invoke("hello world");
        }
    }

    public interface ICharacterLua
    {
        event Action<string> EmoteUsed;

        Vector3 Position { get; }
    }

    public interface IMarkerLua
    {
        event Action Interacted;

        void FlipNinetyDegrees();
    }

    public interface ICuboidLua
    {
        event Action PlayerEntered;

        bool CharacterInside { get; }

        bool IsCharacterInside(string name);

        void Test();
    }

    public class LuaLogger
    {
        public void Log(object message) => System.Diagnostics.Debug.WriteLine(message.ToString());
    }

    public class AdventureScript
    {
        private AdventureElementCreator creator;
        private readonly dynamic luaEnvironment;
        private readonly CharacterInformation characterInformation;
        private readonly LogicBuilderCreator logicCreator;
        private readonly LuaLogger logger;
        private readonly Lua luaEngine;
        private bool scriptValid = false;

        public void Update(GameTime gameTime)
        {
            if (scriptValid)
            {
                var elements = this.creator.Elements;
                foreach (var item in elements)
                {
                    item.Update(gameTime);
                }

                //if (script.Globals.Keys.Contains(DynValue.NewString("update")))
                //{
                //    this.script.Call(script.Globals["update"]);
                //}
            }
        }

        public void EmoteUsed()
        {
            this.characterInformation.FireEmoteUsed();
        }

        public AdventureScript()
        {
            var file = @"D:\Repos\AdventureModule\test.lua";
            var fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
            fileWatcher.Changed += (s, e) =>
            {
                this.LoadScript(file);
            };
            fileWatcher.EnableRaisingEvents = true;
            this.creator = new AdventureElementCreator();
            this.characterInformation = new CharacterInformation();
            this.logicCreator = new LogicBuilderCreator();
            this.logger = new LuaLogger();
            this.luaEngine = new Lua();

            // create an environment, that is associated to the lua scripts
            this.luaEnvironment = this.luaEngine.CreateEnvironment<LuaGlobal>();


            this.luaEnvironment.character = this.characterInformation;
            this.luaEnvironment.logicCreator = this.logicCreator;
            this.luaEnvironment.logger = this.logger;

            this.LoadScript(file);
        }

        private void LoadScript(string path)
        {
            try
            {
                scriptValid = false;
                this.creator.Clear();
                var chunk = this.luaEngine.CompileChunk(path, new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default });
                this.luaEnvironment.dochunk(chunk);



                if (((LuaGlobal)this.luaEnvironment).ContainsKey("onStart"))
                {
                    this.luaEnvironment.onStart(this.creator);
                }

                if (((LuaGlobal)this.luaEnvironment).ContainsKey("onStarted"))
                {
                    this.luaEnvironment.onStarted();
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Expception: {0}", e.Message);
                var d = LuaExceptionData.GetData(e); // get stack trace
                System.Diagnostics.Debug.WriteLine("StackTrace: {0}", d.FormatStackTrace(0, false));
            }
            scriptValid = true;
        }
    }

    public class Cuboid : AdventureElement, ICuboidLua
    {
        private Vector3 position;
        private Vector3 dimensions;
        private readonly CuboidEntity internalEditEntity;
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

        public Cuboid()
        {
            this.internalEditEntity = new CuboidEntity();
            GameService.Graphics.World.AddEntity(this.EditEntity);
        }

        public void Test()
        {
            ScreenNotification.ShowNotification("WORKS!");
        }

        public override void Update(GameTime gameTime)
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

            public Vector3 Position { get; set; }

            public Vector3 Dimensions { get; set; }

            public float DrawOrder => default;

            public CuboidEntity()
            {
                var context = GameService.Graphics.LendGraphicsDeviceContext();
                this.effect = new BasicEffect(context.GraphicsDevice);
                this.effect.VertexColorEnabled = true;
                context.Dispose();
            }

            public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera)
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

            public void Update(GameTime gameTime)
            {
                this.effect.View = GameService.Gw2Mumble.PlayerCamera.View;
                this.effect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
            }
        }
    }

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




    public class AdventureParser
    {
        private readonly Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();

        public AdventureParser()
        {
            Task.Run(async () => await this.Watcher());
        }

        private async Task Watcher()
        {
            try
            {

                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));

                    using (var file = new FileStream(@"D:\Repos\AdventureModule\test.yaml", FileMode.Open, FileAccess.Read))
                    using (var streamReader = new StreamReader(file))
                    {
                        var yamlStream = new YamlStream();
                        yamlStream.Load(streamReader);
                        var mapping = yamlStream.Documents[0].RootNode as YamlSequenceNode;

                        foreach (var item in mapping.Cast<YamlMappingNode>().SelectMany(x => x))
                        {
                            var name = (item.Key as YamlScalarNode).Value;
                            var type = this.GetNode<YamlScalarNode>(item.Value, "Type");
                            if (type.Value.Equals(nameof(Cuboid), StringComparison.OrdinalIgnoreCase))
                            {
                                var dimensionNode = this.GetNode<YamlScalarNode>(item.Value, nameof(Cuboid.Dimensions));
                                var positionNode = this.GetNode<YamlScalarNode>(item.Value, nameof(Cuboid.Position));

                                var dimensions = this.GetVector3(dimensionNode);
                                var position = this.GetVector3(positionNode);
                                if (!this.elements.TryGetValue(name, out var element))
                                {
                                    element = new Cuboid();
                                    this.elements.Add(name, element);
                                }

                                var cuboid = element as Cuboid;
                                cuboid.Dimensions = dimensions;
                                cuboid.Position = position;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private T GetNode<T>(YamlNode node, string property)
            where T : YamlNode
        {
            var mappingNode = node as YamlMappingNode;
            return mappingNode[new YamlScalarNode(property)] as T;
        }

        private Vector3 GetVector3(YamlNode node)
        {
            var vectorNode = node as YamlScalarNode;
            var parts = vectorNode.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
            return new Vector3(parts[0], parts[1], parts[2]);
        }
    }


}


