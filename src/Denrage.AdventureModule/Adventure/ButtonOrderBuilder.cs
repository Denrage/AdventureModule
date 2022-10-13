using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{
    public class ButtonOrderBuilder : IButtonOrderBuilderLua
    {
        private readonly List<ButtonState> buttonStates = new List<ButtonState>();
        private ButtonState initialState;
        private ButtonState currentState;

        public event Func<IMarkerLua[], LuaResult> StateChanged;

        public event Func<LuaResult> Finished;

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
                _ = (this.StateChanged?.Invoke(Array.Empty<IMarkerLua>()));
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

            _ = (this.StateChanged?.Invoke(pressedButtons.ToArray()));

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


}


