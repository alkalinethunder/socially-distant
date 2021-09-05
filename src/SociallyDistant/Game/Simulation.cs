using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using SociallyDistant.SaveData;
using SociallyDistant.WorldObjects;
using Thundershock;
using Thundershock.Core;
using Thundershock.Gui.Elements;
using Thundershock.IO;

namespace SociallyDistant.Game
{
    public class Simulation : ISystem
    {
        private Scene _scene;
        private SaveManager _saveManager;

        private Stacker _traceStacker = new();
        private TextBlock _traceTimer = new();
        private TextBlock _traceLabel = new();

        public void Init(Scene scene)
        {
            _scene = scene;
            _saveManager = scene.Game.GetComponent<SaveManager>();

            var assetRegistry = _saveManager.GetAssetRegistry();

            SpawnAgents(assetRegistry);
            
            _traceStacker.Children.Add(_traceLabel);
            _traceStacker.Children.Add(_traceTimer);

            _traceLabel.Text = "TRACE DETECTED - EMERGENCY REBOOT";

            _traceStacker.ViewportAnchor = new FreePanel.CanvasAnchor(1, 1, 0, 0);
            _traceStacker.ViewportAlignment = Vector2.One;
            _traceStacker.ViewportPosition = new Vector2(-25, -25);
            
            _scene.Gui.AddToViewport(_traceStacker);
        }

        public void Unload()
        {
        }

        public void Load()
        {
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render(GameTime gameTime)
        {
        }

        private void SpawnAgents(AssetRegistry assetRegistry)
        {
            // Start with the player.
            var playerEntity = _scene.Registry.Create();
            
            // Attach the player state to the player entity.
            _scene.Registry.AddComponent(playerEntity, _saveManager.CurrentGame.PlayerState);

            // Attach both the agent and the device to the player.
            _scene.Registry.AddComponent(playerEntity, _saveManager.CurrentGame.PlayerAgent);
            _scene.Registry.AddComponent(playerEntity, _saveManager.CurrentGame.PlayerDevice);
            
            // Create a filesystem node for the root of the vOS for this device.
            var vOSRoot = new VOSRootNode(_saveManager, _saveManager.CurrentGame.PlayerDevice);
            
            // Create and attach a file system for this node to the entity.
            var fs = FileSystem.FromNode(vOSRoot);
            _scene.Registry.AddComponent(playerEntity, fs);
        }

        public uint GetPlayerEntity()
        {
            return _scene.Registry.View<PlayerState>().First();
        }

        public static async Task BeginPreload(SaveManager saveManager)
        {
            await Task.Run(() =>
            {
                Preload(saveManager);
            });
        }

        private static void Preload(SaveManager saveManager)
        {
            var save = saveManager.CurrentGame;
            if (!save.HasPlayer)
                throw new InvalidOperationException("Simulation  preload was started without a player in the save.");
            
            // create the player agent if there isn't one already.
            if (save.PlayerAgent == null)
            {
                var playerAgent = new AgentData();

                playerAgent.Id = Guid.NewGuid();
                playerAgent.IsPlayer = true;

                save.PlayerAgent = playerAgent;
                
                saveManager.App.EnqueueAction(() =>
                {
                    saveManager.Save();
                });
            }

            // Update the player agent information if it's somehow changed.
            save.PlayerAgent.Name = save.PlayerName;
            save.PlayerAgent.Pronouns = save.PlayerPronoun;
            
            // Create the player device if it's not there yet.
            if (save.PlayerDevice == null)
            {
                var playerDevice = new DeviceData();
                playerDevice.Id = Guid.NewGuid();

                var firstName = save.PlayerName.Split(' ').First().ToUnixUsername().ToLower();

                var hostName = firstName + "-pc";
                playerDevice.HostName = hostName;
                playerDevice.DeviceType = DeviceType.Player;
                playerDevice.Users.Add(firstName);
                
                save.PlayerDevice = playerDevice;
                
                // Copy the skeleton home directory over.
                CopyHomeData(saveManager, save.PlayerDevice);
            }
            
            
            // TODO: Player network creation.
            // TODO: Player ISP linking.
            // TODO: City network route map generation.
        }

        private static void CopyHomeData(SaveManager saveManager, DeviceData device)
        {
            var pakfs = saveManager.GetWorldData();
            var homeFolder = saveManager.GetHomeDirectory(device);

            var homefs = FileSystem.FromHostDirectory(homeFolder);

            if (!homefs.DirectoryExists("/root"))
                homefs.CreateDirectory("/root");

            pakfs.BulkCopy(homefs, "/skel/base", "/root");
            
            foreach (var user in device.Users)
            {
                if (!homefs.DirectoryExists("/" + user))
                    homefs.CreateDirectory("/" + user);
                pakfs.BulkCopy(homefs, "/skel/base", "/" + user);
            }
        }
    }
}