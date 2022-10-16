using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{
    public class DialogBuilder
    {
        public DialogGraph Create()
            => new DialogGraph();
    }

    public class DialogGraph
    {
        private readonly List<Node> nodes = new List<Node>();
        private readonly List<Edge> edges = new List<Edge>();

        public void AddNode(int id, string text)
        {
            this.nodes.Add(new Node()
            {
                Id = id,
                Text = text,
            });
        }

        public void AddEdge(int node, int nextNode, string text, Func<bool> predicate = null, Func<LuaResult> action = null)
        {
            if (predicate is null)
            {
                predicate = () => true;
            }

            this.edges.Add(new Edge()
            {
                NextNodeId = nextNode,
                NodeId = node,
                Predicate = predicate,
                Text = text,
                Action = action,
            });
        }

        public class Node
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public List<Edge> IncomingEdges { get; } = new List<Edge>();

            public List<Edge> OutgoingEdges { get; } = new List<Edge>();
        }

        public class Edge
        {
            public int NodeId { get; set; }

            public int NextNodeId { get; set; }

            public Func<bool> Predicate { get; set; }

            public Func<LuaResult> Action { get; set; }

            public string Text { get; set; }

            public Node PreviousNode { get; set; }

            public Node NextNode { get; set; }
        }

        public AdventureDialog Build()
        {
            foreach (var node in this.nodes)
            {
                foreach (var edge in this.edges)
                {
                    if (edge.NodeId == node.Id)
                    {
                        node.OutgoingEdges.Add(edge);
                        edge.PreviousNode = node;
                    }
                    else if (edge.NextNodeId == node.Id)
                    {
                        node.IncomingEdges.Add(edge);
                        edge.NextNode = node;
                    }
                }
            }

            return new AdventureDialog(this.nodes.First());

        }
    }

    public class AdventureDialog
    {
        private readonly DialogGraph.Node node;

        public DialogGraph.Node CurrentNode { get; private set; }

        public event Action CurrentNodeChanged;

        public event Action OnClose;

        public void MoveNext(DialogGraph.Edge edge)
        {
            _ = edge.Action?.Invoke();

            if (edge.NextNode is null)
            {
                this.OnClose?.Invoke();
                this.CurrentNode = this.node;
            }
            else
            {
                this.CurrentNode = edge.NextNode;
                this.CurrentNodeChanged?.Invoke();
            }
        }

        public AdventureDialog(DialogGraph.Node node)
        {
            this.node = node;
            this.CurrentNode = this.node;
        }

        public void Show()
        {
            var dialog = new DialogWindow(this)
            {
                Parent = GameService.Graphics.SpriteScreen,
            };
            dialog.Show();
        }
    }

    public class DialogWindow : WindowBase2
    {
        private readonly AdventureDialog dialog;
        private Label npcTextLabel;
        private FlowPanel responseOptionList;

        public DialogWindow(AdventureDialog dialog)
        {
            this.dialog = dialog;
            this.dialog.CurrentNodeChanged += () => this.UpdateNode();
            this.dialog.OnClose += () => this.Dispose();
            this.CanClose = false;
            this.CanCloseWithEscape = false;

            this.ConstructWindow(Module.Instance.ContentsManager.GetTexture("background2.png"), new Rectangle(0, 0, 400, 300), new Rectangle(0, 0, 400, 300));
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            var mainPanel = new FlowPanel()
            {
                Parent = this,
                Height = this.ContentRegion.Height,
                Width = this.ContentRegion.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            this.npcTextLabel = new Label()
            {
                Text = "Placeholder",
                AutoSizeHeight = true,
                WrapText = true,
                Width = mainPanel.ContentRegion.Width,
                Parent = mainPanel,
            };

            this.responseOptionList = new FlowPanel()
            {
                Parent = mainPanel,
                HeightSizingMode = SizingMode.Fill,
                Width = mainPanel.ContentRegion.Width,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            this.UpdateNode();
        }

        private void UpdateNode()
        {
            var toDispose = this.responseOptionList.Children.ToArray();
            foreach (var item in toDispose)
            {
                item.Dispose();
            }

            this.npcTextLabel.Text = this.dialog.CurrentNode.Text;

            foreach (var item in this.dialog.CurrentNode.OutgoingEdges)
            {
                if (item.Predicate())
                {
                    var button = new StandardButton()
                    {
                        Parent = this.responseOptionList,
                        Text = item.Text,
                        Width = 300,
                    };

                    button.Click += (s, e) => this.dialog.MoveNext(item);
                }
            }
        }
    }

    public class AdventureDebugService
    {
        public event Action DebugActivated;

        public event Action DebugDeactivated;

        public bool IsDebug { get; private set; } = true;

        public void ToggleDebug()
        {
            if (this.IsDebug)
            {
                this.IsDebug = false;
                this.DebugDeactivated?.Invoke();
            }
            else
            {
                this.IsDebug = true;
                this.DebugActivated?.Invoke();
            }
        }
    }

    public class AdventureSynchronizationService
    {
        public event Action<string, string, object> PropertyChanged;

        public AdventureSynchronizationService(Adventure adventure)
        {
            // Get current state from Server
            // Adjust step table with that from Server (all ServerVariable Properties)
            // Activate OnChange Handlers and start sending updates to the server
            // Add Synchronization with object states (button logic, etc.)
            // Last one wins
        }

        public void UpdateStepVariable(string stepName, string propertyName, object propertyValue)
        {

        }
    }

    public class Adventure
    {
        private readonly Lua engine;
        private readonly CharacterInformation characterInformation;
        private readonly AdventureElementCreator adventureElementCreator;
        private readonly LuaLogger logger;
        private readonly LogicBuilderCreator logicCreator;
        private readonly DialogBuilder dialog;
        private readonly Dictionary<string, Step> steps = new Dictionary<string, Step>();
        private readonly AdventureSynchronizationService synchronizationService;

        public event Action StepsChanged;

        public event Action<Step> StepLoaded;

        public event Action<Step> ActiveStepChanged;

        public Step ActiveStep { get; private set; }

        public IReadOnlyDictionary<string, Step> Steps => new ReadOnlyDictionary<string, Step>(this.steps);

        public Adventure(Lua engine, string scriptFolder, CharacterInformation characterInformation, AdventureElementCreator adventureElementCreator, LuaLogger logger, LogicBuilderCreator logicCreator, DialogBuilder dialog)
        {
            this.engine = engine;
            this.characterInformation = characterInformation;
            this.adventureElementCreator = adventureElementCreator;
            this.logger = logger;
            this.logicCreator = logicCreator;
            this.dialog = dialog;
            this.synchronizationService = new AdventureSynchronizationService(this);
            this.synchronizationService.PropertyChanged += (stepName, property, value) => this.steps[stepName].Environment.SetServerVariable(property, value);
            this.LoadSteps(scriptFolder);
        }

        private void LoadSteps(string scriptFolder)
        {
            this.steps.Clear();
            this.ActivateStep(null);
            this.StepsChanged?.Invoke();
            foreach (var item in Directory.GetFiles(scriptFolder, "*.lua"))
            {
                var step = new Step()
                {
                    LuaFile = item,
                    Name = Path.GetFileNameWithoutExtension(item),
                    State = StepState.NotStarted,
                };

                this.ReloadStep(step);

                this.steps.Add(step.Name, step);
                this.StepsChanged?.Invoke();
            }

            this.ActivateStep(this.Steps.Values.First());
        }

        public void ReloadStep(Step step)
        {
            var setActive = step == this.ActiveStep;
            if (setActive)
            {
                this.ActivateStep(null);
            }
            step.Chunk = this.engine.CompileChunk(step.LuaFile, new LuaCompileOptions() { ClrEnabled = true });
            step.Environment = this.engine.CreateEnvironment<AdventureGlobal>();
            step.Environment.Step = step;
            step.Environment.Character = this.characterInformation;
            step.Environment.StepLogic = new StepLogic(this);
            step.Environment.Logger = this.logger;
            step.Environment.LogicCreator = this.logicCreator;
            step.Environment.Dialog = this.dialog;

            _ = step.Environment.DoChunk(step.Chunk);
            this.StepLoaded?.Invoke(step);

            if (setActive)
            {
                this.ActivateStep(step);
            }
        }

        public void ActivateStep(Step step)
        {
            if (this.ActiveStep != null)
            {
                // Deactivation Logic
                this.adventureElementCreator.ClearFromStep(this.ActiveStep);
                this.ActiveStep.Environment.CallMethod("onUnload");
                this.ActiveStep.State = StepState.Completed;
                this.ActiveStep.Environment.ServerVariablesChanged -= this.OnStepServerVariableChanged;
            }

            this.ActiveStep = step;
            this.ActiveStepChanged?.Invoke(this.ActiveStep);

            if (this.ActiveStep != null)
            {
                this.ActiveStep.State = StepState.Running;
                this.ActiveStep.Environment.CallMethod("onStart", new AdventureElementCreatorWrapper(this.ActiveStep, this.adventureElementCreator));
                this.ActiveStep.Environment.ServerVariablesChanged += this.OnStepServerVariableChanged;
            }
        }

        private void OnStepServerVariableChanged(Step step, string propertyName)
        {
            this.synchronizationService.UpdateStepVariable(step.Name, propertyName, step.Environment.ServerVariables[propertyName]);
        }
    }

    public class StepOverviewWindow : WindowBase2
    {
        private readonly Adventure adventure;
        private FlowPanel panel;

        public StepOverviewWindow(Adventure adventure)
        {
            this.ConstructWindow(Module.Instance.ContentsManager.GetTexture("background.png"), new Rectangle(0, 0, 350, 600), new Rectangle(0, 0, 350, 600));
            this.Parent = GameService.Graphics.SpriteScreen;
            this.adventure = adventure;
            this.adventure.StepsChanged += () => this.RefreshStepList();
            this.adventure.ActiveStepChanged += _ => this.RefreshStepList();
            this.BuildWindow();
        }

        private void RefreshStepList()
        {
            var toDispose = this.panel.Children.ToArray();
            foreach (var item in toDispose)
            {
                item.Dispose();
            }

            foreach (var item in this.adventure.Steps)
            {
                var itemPanel = new FlowPanel()
                {
                    Parent = this.panel,
                    Width = this.panel.Width,
                    Height = 30,
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                };

                if (this.adventure.ActiveStep == item.Value)
                {
                    itemPanel.BackgroundColor = Color.LightGreen;
                }


                _ = new Label()
                {
                    Text = item.Value.Name,
                    AutoSizeWidth = true,
                    Parent = itemPanel,
                };

                var reloadButton = new StandardButton()
                {
                    Text = "Reload",
                    Parent = itemPanel,
                };

                reloadButton.Click += (s, e) => this.adventure.ReloadStep(item.Value);

                var activateButton = new StandardButton()
                {
                    Text = "Activate",
                    Parent = itemPanel,
                };

                activateButton.Click += (s, e) => this.adventure.ActivateStep(item.Value);
            }
        }

        private void BuildWindow()
        {
            this.panel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
                Width = this.ContentRegion.Width,
                HeightSizingMode = SizingMode.AutoSize,
                CanScroll = true,
            };

            this.RefreshStepList();
        }
    }

    public class AdventureScript
    {
        private AdventureElementCreator creator;
        private readonly CharacterInformation characterInformation;
        private readonly LogicBuilderCreator logicCreator;
        private readonly LuaLogger logger;
        private readonly Lua luaEngine;

        public Dictionary<string, Step> Steps { get; } = new Dictionary<string, Step>();


        public void Update(GameTime gameTime)
        {
            var elements = this.creator.Elements;
            foreach (var item in elements)
            {
                item.Update(gameTime);
            }
        }

        public void EmoteUsed()
        {
            this.characterInformation.FireEmoteUsed();
        }

        public AdventureScript()
        {
            this.luaEngine = new Lua();

            this.logicCreator = new LogicBuilderCreator();
            this.creator = new AdventureElementCreator(new AdventureDebugService());
            this.logger = new LuaLogger();
            this.characterInformation = new CharacterInformation();
            var dialog = new DialogBuilder();
            var adventure = new Adventure(this.luaEngine, @"D:\Repos\AdventureModule\Adventure2", characterInformation, creator, logger, logicCreator, dialog);

            //foreach (var file in Directory.GetFiles(@"D:\Repos\AdventureModule\Adventure"))
            //{
            //    this.Steps.Add(file, this.luaEngine.CreateEnvironment<AdventureGlobal>().InitializeStep(file));
            //}

            var scriptWindow = new StepOverviewWindow(adventure);
            scriptWindow.Show();

            //var fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
            //fileWatcher.Changed += (s, e) =>
            //{
            //    this.LoadScript(file);
            //};
            //fileWatcher.EnableRaisingEvents = true;

            //// create an environment, that is associated to the lua scripts
            //this.luaEnvironment = this.luaEngine.CreateEnvironment<LuaGlobal>();


            //this.luaEnvironment["Character"] = this.characterInformation;
            //this.luaEnvironment["LogicCreator"] = this.logicCreator;
            //this.luaEnvironment["Logger"] = this.logger;
            //this.luaEnvironment["CreateVector"] = new Func<float, float, float, Vector3>((x, y, z) => new Vector3(x, y, z));

            //this.LoadScript(file);
        }

        //private void LoadScript(string path)
        //{
        //    try
        //    {
        //        scriptValid = false;
        //        this.creator.Clear();
        //        var chunk = this.luaEngine.CompileChunk(path, new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default });
        //        _ = this.luaEnvironment.DoChunk(chunk);



        //        if (this.luaEnvironment.ContainsKey("onStart"))
        //        {
        //            (this.luaEnvironment["onStart"] as Func<AdventureElementCreator, LuaResult>)(this.creator);
        //        }

        //        if (this.luaEnvironment.ContainsKey("onStarted"))
        //        {
        //            (this.luaEnvironment["onStarted"] as Func<LuaResult>)();
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Expception: {e.Message}");
        //        var d = LuaExceptionData.GetData(e); // get stack trace
        //        System.Diagnostics.Debug.WriteLine("StackTrace: {0}", d.FormatStackTrace(0, false));
        //    }
        //    scriptValid = true;
        //}
    }


}


