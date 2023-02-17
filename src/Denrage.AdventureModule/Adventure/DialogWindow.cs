using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Denrage.AdventureModule.Adventure
{
    public class DialogWindow : WindowBase2
    {
        private readonly AdventureDialog dialog;
        private Label npcTextLabel;
        private FlowPanel responseOptionList;

        public DialogWindow(AdventureDialog dialog)
        {
            this.dialog = dialog;
            this.dialog.CurrentNodeChanged += () => this.UpdateNode();
            this.dialog.OnClose += () => this.Hide();
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
        }

        public override void Show()
        {
            base.Show();
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
}

