using Denrage.AdventureModule.Libs.Messages.Data;
using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class Adventure
    {
        private readonly Lua engine;
        private readonly string scriptFolder;
        private readonly CharacterInformation characterInformation;
        private readonly AdventureElementCreator adventureElementCreator;
        private readonly LuaLogger logger;
        private readonly LogicBuilderCreator logicCreator;
        private readonly DialogBuilder dialog;
        private readonly Dictionary<string, Step> steps = new Dictionary<string, Step>();
        private readonly SynchronizationService synchronizationService;

        public event Action StepsChanged;

        public event Action<Step> StepLoaded;

        public event Action<Step> StepDeactivated;

        public event Action<Step> StepActivated;

        public event Action<Step, Step> MovedToStep;

        public IReadOnlyDictionary<string, Step> Steps => new ReadOnlyDictionary<string, Step>(this.steps);

        public Adventure(Lua engine, string scriptFolder, CharacterInformation characterInformation, AdventureElementCreator creator, LuaLogger logger, LogicBuilderCreator logicCreator, DialogBuilder dialog, SynchronizationService synchronizationService)
        {
            this.synchronizationService = synchronizationService;
            this.adventureElementCreator = creator;
            this.engine = engine;
            this.scriptFolder = scriptFolder;
            this.characterInformation = characterInformation;
            this.logger = logger;
            this.logicCreator = logicCreator;
            this.dialog = dialog;

            this.synchronizationService.Register(typeof(LuaVariablesState), state =>
            {
                var luaState = (LuaVariablesState)state;
                this.steps[luaState.StepName].Environment.SetServerVariables(luaState.Variables);
            });

            this.synchronizationService.Register(typeof(AdventureState), state =>
            {
                var adventureState = (AdventureState)state;
                if (adventureState.StepName == null)
                {
                    this.ActuallyMoveToStep(null);
                }
                else
                {
                    this.ActuallyMoveToStep(this.steps[adventureState.StepName]);
                }
            });

            //this.synchronizationService.PropertyChanged += (stepName, property, value) => this.steps[stepName].Environment.SetServerVariable(property, value);
            this.LoadSteps(scriptFolder);
        }

        private void LoadSteps(string scriptFolder)
        {
            this.steps.Clear();
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
            var setActive = step.State == StepState.Running;
            if (setActive)
            {
                this.MoveToStep(null);
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
                this.MoveToStep(step);
            }
        }

        public void DeactivateStep(Step step)
        {
            this.adventureElementCreator.ClearFromStep(step);
            _ = step.Environment.CallMethod("onUnload");
            step.State = StepState.Completed;
            step.Environment.ServerVariablesChanged -= this.OnStepServerVariableChanged;
            this.StepDeactivated?.Invoke(step);
        }

        public void MoveToStep(Step nextStep)
        {
            this.synchronizationService.SendNewState(new AdventureState() { Id = ToGuid(Path.GetDirectoryName(this.scriptFolder)), StepName = nextStep?.Name }, default);
        }

        public void ActuallyMoveToStep(Step nextStep)
        {

            var previousStep = this.Steps.Values.FirstOrDefault(x => x.State == StepState.Running);
            if (previousStep != null)
            {
                this.DeactivateStep(previousStep);
            }

            this.ActivateStep(nextStep);

            this.MovedToStep?.Invoke(previousStep, nextStep);
        }

        public void ActivateStep(Step step)
        {
            if (step != null)
            {
                step.State = StepState.Running;
                step.Environment.CallMethod("onStart", new AdventureElementCreatorWrapper(step, this.adventureElementCreator));
                step.Environment.ServerVariablesChanged += this.OnStepServerVariableChanged;
            }

            this.StepActivated?.Invoke(step);
        }

        private void OnStepServerVariableChanged(Step step)
        {
            this.synchronizationService.SendNewState(new LuaVariablesState()
            {
                Id = ToGuid(System.IO.Path.GetFileName(step.LuaFile)),
                Variables = step.Environment.ServerVariables.Select(x => new KeyValuePair<string, object>((string)x.Key, x.Value)).ToArray(),
                StepName = step.Name,
            }, default);
        }

        public static Guid ToGuid(string src)
        {
            var stringbytes = System.Text.Encoding.UTF8.GetBytes(src);
            var hashedBytes = new System.Security.Cryptography
                .SHA1CryptoServiceProvider()
                .ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }
    }
}

