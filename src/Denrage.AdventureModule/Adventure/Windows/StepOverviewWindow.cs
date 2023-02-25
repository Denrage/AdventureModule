using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Denrage.AdventureModule.Adventure.Windows
{
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
            this.adventure.MovedToStep += (_, step) => this.RefreshStepList();
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

                if (item.Value.State == StepState.Running)
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

                activateButton.Click += (s, e) => this.adventure.MoveToStep(item.Value);
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
}

