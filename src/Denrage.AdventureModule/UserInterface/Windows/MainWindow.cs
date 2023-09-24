using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Adventure;
using Denrage.AdventureModule.Interfaces;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class MainWindow : WindowBase2
    {
        public MainWindow(IInitializationService initializationService, TcpService tcpService, DrawObjectService drawObjectService, LoginService loginService, AdventureScript adventureScript)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 40, 500, 500 - 40));
            this.Location = new Point(0, 100);
            this.CanResize = true;

            var panel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
            };

            var usernamePanel = new FlowPanel()
            {
                Parent = panel,
                FlowDirection = ControlFlowDirection.LeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _ = new Label()
            {
                Parent = usernamePanel,
                Text = "Username: ",
                AutoSizeWidth = true,
            };

            var usernameTextbox = new TextBox()
            {
                Parent = usernamePanel,
            };

            usernameTextbox.TextChanged += (s, e) => loginService.Name = usernameTextbox.Text;

            var connectButton = new StandardButton()
            {
                Parent = panel,
                Text = "Connect",
                Enabled = !initializationService.IsInitialized,
            };

            var disconnectButton = new StandardButton()
            {
                Parent = panel,
                Text = "Disconnect",
                Enabled = initializationService.IsInitialized,
            };

            var isConnectedLabel = new Label()
            {
                Parent = panel,
                Text = initializationService.IsInitialized ? "Connected" : "Discsonnected",
            };

            var canvasWindowButton = new StandardButton()
            {
                Parent = panel,
                Text = "Canvas",
            };

            var adventureButton = new StandardButton()
            {
                Parent = panel,
                Text = "Adventure",
            };

            initializationService.Initialize += () =>
            {
                isConnectedLabel.Text = "Connected";
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
            };

            initializationService.Finalize += () =>
            {
                isConnectedLabel.Text = "Disconnected";
                connectButton.Enabled = true;
                disconnectButton.Enabled = false;
            };

            connectButton.Click += async (s, e) 
                => await tcpService.Initialize();

            disconnectButton.Click += (s, e)
                => tcpService.Disconnect();

            var canvasWindow = new CanvasWindow()
            {
                Parent = GameService.Graphics.SpriteScreen,
            };
            canvasWindow.Initialize(drawObjectService, loginService, initializationService);

            canvasWindowButton.Click += (s, e)
                => canvasWindow.Show();

            adventureButton.Click += (s, e)
                => adventureScript.Initialize();
        }
    }
}
