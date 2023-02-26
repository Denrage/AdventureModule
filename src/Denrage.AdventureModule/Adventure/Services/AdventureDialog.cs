using Blish_HUD;
using Denrage.AdventureModule.Adventure.Elements;
using Denrage.AdventureModule.Adventure.Windows;
using Denrage.AdventureModule.Libs.Messages.Data;
using System;

namespace Denrage.AdventureModule.Adventure.Services
{
    public class AdventureDialog
    {
        private readonly DialogWindow window;
        private readonly DialogGraph graph;

        private DialogState state;

        protected DialogState State
        {
            get => this.state;

            set => this.StateChanged?.Invoke(value);
        }

        public Guid Id { get; set; }

        public DialogGraph.Node CurrentNode => this.graph.GetNodeById(this.state.CurrentNode);

        public event Action CurrentNodeChanged;

        public event Action OnClose;

        public event Action<DialogState> StateChanged;

        public AdventureDialog(DialogGraph graph)
        {
            this.graph = graph;
            this.state = new DialogState()
            {
                CurrentNode = this.graph.InitialNode.Id,
                Id = this.Id,
                IsOpen = false,
            };

            this.window = new DialogWindow(this)
            {
                Parent = GameService.Graphics.SpriteScreen,
            };
        }

        public void SetState(DialogState state)
        {
            var previousState = this.state;
            this.state = state;

            if (previousState.CurrentNode != this.State.CurrentNode)
            {
                this.CurrentNodeChanged?.Invoke();
            }

            if (!this.window.Visible && state.IsOpen)
            {
                this.window.Show();
            }

            if (this.window.Visible && !state.IsOpen)
            {
                this.window.Hide();
            }
        }

        public void MoveNext(DialogGraph.Edge edge)
        {
            _ = edge.Action?.Invoke();

            if (edge.NextNode is null)
            {
                this.Close();
            }
            else
            {
                this.State = new DialogState() { CurrentNode = edge.NextNode.Id, Id = this.Id, IsOpen = this.State.IsOpen };
            }
        }

        private void Close()
            => this.State = new DialogState() { CurrentNode = this.graph.InitialNode.Id, Id = this.Id, IsOpen = false };

        public void Show()
            => this.State = new DialogState() { CurrentNode = this.State.CurrentNode, Id = this.Id, IsOpen = true };
    }
}

