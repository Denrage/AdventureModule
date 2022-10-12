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
    public class Adventure
    {
        private readonly Lua engine;
        private readonly CharacterInformation characterInformation;
        private readonly AdventureElementCreator adventureElementCreator;
        private readonly LuaLogger logger;
        private readonly LogicBuilderCreator logicCreator;
        private readonly Dictionary<string, Step> steps = new Dictionary<string, Step>();

        public event Action StepsChanged;

        public event Action<Step> StepLoaded;

        public event Action<Step> ActiveStepChanged;

        public Step ActiveStep { get; private set; }

        public IReadOnlyDictionary<string, Step> Steps => new ReadOnlyDictionary<string, Step>(this.steps);

        public Adventure(Lua engine, string scriptFolder, CharacterInformation characterInformation, AdventureElementCreator adventureElementCreator, LuaLogger logger, LogicBuilderCreator logicCreator)
        {
            this.engine = engine;
            this.characterInformation = characterInformation;
            this.adventureElementCreator = adventureElementCreator;
            this.logger = logger;
            this.logicCreator = logicCreator;
            this.LoadSteps(scriptFolder);
        }

        private void LoadSteps(string scriptFolder)
        {
            this.steps.Clear();
            this.ActivateStep(null);
            this.StepsChanged?.Invoke();
            foreach (var item in Directory.GetFiles(scriptFolder))
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
            step.Chunk = this.engine.CompileChunk(step.LuaFile, new LuaCompileOptions() { ClrEnabled = true });
            step.Environment = this.engine.CreateEnvironment<AdventureGlobal>();
            step.Environment.Character = this.characterInformation;
            step.Environment.StepLogic = new StepLogic(this);
            step.Environment.Logger = this.logger;
            step.Environment.LogicCreator = this.logicCreator;

            _ = step.Environment.DoChunk(step.Chunk);
            this.StepLoaded?.Invoke(step);
        }

        public void ActivateStep(Step step)
        {
            if (this.ActiveStep != null)
            {
                // Deactivation Logic
                this.adventureElementCreator.ClearFromStep(this.ActiveStep);
            }

            this.ActiveStep = step;
            this.ActiveStepChanged?.Invoke(this.ActiveStep);

            if (this.ActiveStep != null)
            {
                this.ActiveStep.State = StepState.Running;
                this.ActiveStep.Environment.CallMethod("onStart", new AdventureElementCreatorWrapper(this.ActiveStep, this.adventureElementCreator));
            }
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
            this.creator = new AdventureElementCreator();
            this.logger = new LuaLogger();
            this.characterInformation = new CharacterInformation();
            var adventure = new Adventure(this.luaEngine, @"D:\Repos\AdventureModule\Adventure", characterInformation, creator, logger, logicCreator);

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


