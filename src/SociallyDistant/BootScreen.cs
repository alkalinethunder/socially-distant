using System;
using System.IO;
using System.Numerics;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Components;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant
{
    public class BootScreen : Scene
    {
        #region GLOBAL REFERENCES

        private SaveManager _saveManager;
        private RedConfigManager _redConf;

        #endregion

        #region COMPONENTS

        private Transform2D _logoTransform1 = new();
        private Transform2D _logoTransform2 = new();
        private Sprite _logoSprite1 = new();
        private Sprite _logoSprite2 = new();

        #endregion
        
        #region SYSTEMS

        private WindowManager _winManager;

        #endregion

        #region STATE

        private int _bootState;
        private string[] _kmsgList = Array.Empty<string>();
        private int _kmsgIndex;
        private double _kmsgNextTime;
        private int _installState;
        private string _username;
        private string _hostname;
        private string _password;
        private int _gbootState;
        private double _gbootTime;
        
        #endregion

        #region UI ELEMENTS

        private Stacker _master = new();
        private TextBlock _statusHeader = new();
        private ProgressBar _progress = new();
        private ConsoleControl _console = new();
        private ProgressBar _bootProgress = new();
        
        #endregion

        #region WINDOWS

        private OobeWindow _oobeWindow;

        #endregion
        
        protected override void OnLoad()
        {
            // Grab app references.
            _saveManager = Game.GetComponent<SaveManager>();
            _redConf = Game.GetComponent<RedConfigManager>();
            
            // Add the gui system to the scene.
            var logo1 = SpawnObject();
            var logo2 = SpawnObject();
            
            // Add components to these entities.
            logo1.AddComponent(_logoTransform1);
            logo2.AddComponent(_logoTransform2);
            logo1.AddComponent(_logoSprite1);
            logo2.AddComponent(_logoSprite2);
            
            // Set up the GUI.
            _master.Children.Add(_statusHeader);
            _master.Children.Add(_progress);
            _master.Children.Add(_console);
            Gui.AddToViewport(_master);
            Gui.AddToViewport(_bootProgress);
            
            // Window manager.
            _winManager = RegisterSystem<WindowManager>();
            
            // Boot progress bar setup.
            _bootProgress.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _bootProgress.Properties.SetValue(FreePanel.AnchorProperty, FreePanel.CanvasAnchor.Center);
            _bootProgress.Properties.SetValue(FreePanel.PositionProperty, new Vector2(0, 96));
            _bootProgress.Properties.SetValue(FreePanel.AlignmentProperty, new Vector2(0.5f, 0.5f));
            _bootProgress.FixedWidth = 192;
            
            
            // Let redconf set the proper console fonts.
            _redConf.SetConsoleFonts(_console);
            
            // Style the GUI
            _statusHeader.Font = _console.Font;
            _statusHeader.ForeColor = Color.White;
            _statusHeader.Visibility = Visibility.Collapsed;
            _progress.Visibility = Visibility.Collapsed;
            _progress.Padding = new Padding(0, 5, 0, 10);
            _console.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            // Prepare the kmsg text.
            LoadKernelMessages();

            // Logo setup.
            _logoSprite1.Texture = Texture2D.FromResource(Game.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Textures.rtos_boot_1.png");
            _logoSprite2.Texture = Texture2D.FromResource(Game.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Textures.rtos_boot_2.png");
            _logoSprite1.Size = new Vector2(128, 128);
            _logoSprite2.Size = new Vector2(128, 128);
            
            
            base.OnLoad();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _logoSprite1.Color = Color.Transparent;
            _logoSprite2.Color = Color.Transparent;
            _bootProgress.Opacity = 0;
            
            switch (_bootState)
            {
                case 0:
                    KernelBootUpdate(gameTime);
                    break;
                case 1:
                    InstallationUpdate(gameTime);
                    break;
                case 2:
                    GraphicalBootUpdate(gameTime);
                    break;
            }
            
            base.OnUpdate(gameTime);
        }

        private void InstallationUpdate(GameTime gameTime)
        {
            switch (_installState)
            {
                case 0:
                    _statusHeader.Text = "INSTALLING BASE SYSTEM...";
                    _console.WriteLine(" * partitioning /dev/sda... *");
                    _installState++;
                    break;
                case 1:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 4;
                    if (_progress.Value >= 0.19f)
                    {
                        _progress.Value = 0.19f;
                        _console.WriteLine(" * /dev/sda1: creating EFI System Partition * ");
                        _installState++;
                    }
                    break;
                case 2:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 4;
                    if (_progress.Value >= 0.26f)
                    {
                        _progress.Value = 0.26f;
                        _console.WriteLine(" * /dev/sda2: creating Linux filesystem * ");
                        _installState++;
                    }
                    break;
                case 3:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 4;
                    if (_progress.Value >= 0.38f)
                    {
                        _progress.Value = 0.38f;
                        _console.WriteLine(" * /dev/sda3: creating swap partition * ");
                        _installState++;
                    }
                    break;
                case 4:
                    _statusHeader.Text = "FORMATTING...";
                    _console.WriteLine(" * Beginning disk format... *");
                    _installState++;
                    break;
                case 5:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.39f)
                    {
                        _progress.Value = 0.39f;
                        _console.WriteLine("mkfs.fat32 /dev/sda1");
                        _installState++;
                    }
                    break;
                case 6:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.40f)
                    {
                        _progress.Value = 0.40f;
                        _console.WriteLine("mkfs.ext4 /dev/sda2");
                        _installState++;
                    }
                    break;
                case 7:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.69f)
                    {
                        _progress.Value = 0.69f;
                        _console.WriteLine("mkswap /dev/sda3");
                        _installState++;
                    }
                    break;
                case 8:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.74f)
                    {
                        _progress.Value = 0.74f;
                        _console.WriteLine(" * mounting system drives... * ");
                        _statusHeader.Text = "INSTALLING RED-OS ON /dev/sda...";
                        _installState++;
                    }
                    break;
                case 9:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.75f)
                    {
                        _progress.Value = 0.75f;
                        _console.WriteLine("mount /dev/sda2 /mnt");
                        _installState++;
                    }
                    break;
                case 10:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.77f)
                    {
                        _progress.Value = 0.77f;
                        _console.WriteLine("mount /dev/sda1 /mnt/boot");
                        _installState++;
                    }
                    break;
                case 11:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.78f)
                    {
                        _progress.Value = 0.78f;
                        _console.WriteLine("swapon /dev/sda3");
                        _installState++;
                    }
                    break;
                case 12:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 12;
                    if (_progress.Value >= 0.80f)
                    {
                        _progress.Value = 0.80f;
                        _console.WriteLine("genfstab /mnt > /mnt/etc/fstab");
                        _installState++;
                    }
                    break;
                case 13:
                    _statusHeader.Text = "INSTALLING BOOTLOADER...";
                    _console.WriteLine(" * installing redefined bootloader... *");
                    _installState++;
                    break;
                case 14:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 8;
                    if (_progress.Value >= 0.98f)
                    {
                        _progress.Value = 0.98f;
                        _console.WriteLine(" * bootloader installation completed. *");
                        _statusHeader.Text = "BASE INSTALLATION COMPLETED.";
                        _installState++;
                    }
                    break;
                case 15:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 8;
                    if (_progress.Value >= 0.99f)
                    {
                        _progress.Value = 0.99f;
                        _console.WriteLine(" * preparing to install packages... *");
                        _installState++;
                    }
                    break;
                case 16:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 8;
                    if (_progress.Value >= 1f)
                    {
                        _progress.Value = 0f;
                        _console.WriteLine(" * now installing packages... *");
                        _statusHeader.Text = "Installing system packages...";
                        _installState++;
                    }
                    break;
                case 17:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 16;
                    _statusHeader.Text = $"Installing packages... ({Math.Round(_progress.Value * 200)} %)";
                    if (_progress.Value >= 0.5f)
                    {
                        _progress.Value = 0.5f;
                        _console.WriteLine(" * building homes... *");
                        _statusHeader.Text = "Creating user data...";
                        BeginOobe();
                    }
                    break;
                case 18:
                    if (_oobeWindow == null)
                    {
                        _installState++;
                        _console.WriteLine("echo {0} > /mnt/etc/hostname", _hostname);
                    }
                    break;
                case 19:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 6;
                    if (_progress.Value >= 0.52f)
                    {
                        _progress.Value = 0.52f;
                        _console.WriteLine("/bin/bash -c \"chroot /mnt & useradd -G wheel {0}\"", _username);
                        _installState++;
                    }
                    break;
                case 20:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.75f)
                    {
                        _progress.Value = 0.75f;
                        _console.WriteLine("/bin/bash -c \"chroot /mnt; echo '{0}\\n{0}' | passwd {1}\"", _password, _username);
                        _installState++;
                    }
                    break;
                case 21:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.76f)
                    {
                        _progress.Value = 0.76f;
                        _console.WriteLine("mkdir /mnt/{0}/Desktop", _username);
                        _installState++;
                    }
                    break;
                case 22:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.77f)
                    {
                        _progress.Value = 0.77f;
                        _console.WriteLine("mkdir /mnt/{0}/Documents", _username);
                        _installState++;
                    }
                    break;
                case 23:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.78f)
                    {
                        _progress.Value = 0.78f;
                        _console.WriteLine("mkdir /mnt/{0}/Downloads", _username);
                        _installState++;
                    }
                    break;
                case 24:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.79f)
                    {
                        _progress.Value = 0.79f;
                        _console.WriteLine("mkdir /mnt/{0}/Music", _username);
                        _installState++;
                    }
                    break;
                case 25:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.80f)
                    {
                        _progress.Value = 0.80f;
                        _console.WriteLine("mkdir /mnt/{0}/Pictures", _username);
                        _installState++;
                    }
                    break;
                case 26:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 0.81f)
                    {
                        _progress.Value = 0.81f;
                        _console.WriteLine("mkdir /mnt/{0}/Videos", _username);
                        _console.WriteLine(" * installation complete, cleaning up and rebooting... *");
                        _statusHeader.Text = "Cleaning up...";
                        _installState++;
                    }
                    break;
                case 27:
                    _progress.Value += (float) gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_progress.Value >= 1f)
                    {
                        _progress.Value = 1f;
                        _console.WriteLine("mkdir /mnt/{0}/Videos", _username);
                        SavePlayerInformation();
                        _bootState++;
                    }
                    break;
                    
            }   
        }

        private void SavePlayerInformation()
        {
            _saveManager.SpawnInitialWorld(_username, _password, _hostname);
        }
        
        private void BeginOobe()
        {
            _installState++;

            if (_saveManager.ContentPack.RequiresOutOfBoxExperience)
            {
                BuildOobeWindow();
            }
            else
            {
                _installState++;
            }
        }

        private void BuildOobeWindow()
        {
            _oobeWindow = _winManager.OpenWindow<OobeWindow>();
            _oobeWindow.WindowClosed += OobeWindowOnWindowClosed;
        }

        private void OobeWindowOnWindowClosed(object sender, EventArgs e)
        {
            _username = _oobeWindow.Username;
            _password = _oobeWindow.Password;
            _hostname = _oobeWindow.Hostname;

            _oobeWindow = null;
        }

        private void GraphicalBootUpdate(GameTime gameTime)
        {
            _master.Visibility = Visibility.Collapsed;

            _gbootTime += gameTime.ElapsedGameTime.TotalSeconds;
            
            switch (_gbootState)
            {
                case 0:
                    if (_gbootTime >= 1)
                    {
                        _gbootTime = 0;
                        _gbootState++;
                    }
                    break;
                case 1:
                    _bootProgress.Opacity = (float) (_gbootTime / 0.5f);

                    var halfWidth = ViewportBounds.Width / 2;

                    _logoSprite1.Color = Color.White * _bootProgress.Opacity;
                    _logoSprite2.Color = _logoSprite1.Color;

                    _logoTransform1.Position = new Vector2(-(halfWidth * _bootProgress.Opacity), 0);
                    _logoTransform2.Position = new Vector2(-_logoTransform1.Position.X, 0);

                    if (_gbootTime >= 0.5)
                    {
                        _gbootTime = 0;
                        _gbootState++;
                        _logoTransform1.Position = Vector2.Zero;
                        _logoTransform2.Position = Vector2.Zero;
                    }
                    
                    break;
                case 2:
                    _logoSprite1.Color = Color.White;
                    _logoSprite2.Color = Color.White;
                    _bootProgress.Opacity = 1;

                    _bootProgress.Value = (float) (_gbootTime / 3);
                    if (_gbootTime >= 3)
                    {
                        GoToScene<Workspace>();
                    }
                    
                    break;
            }
        }
        
        private void KernelBootUpdate(GameTime gameTime)
        {
            if (_kmsgNextTime <= 0)
            {
                if (_kmsgIndex >= _kmsgList.Length)
                {
                    _console.Clear();
                    ContinueBootSequence();
                    return;
                }
                
                var text = _kmsgList[_kmsgIndex];
                _console.WriteLine("#f" + text + "&0");

                _kmsgIndex++;

                if (_kmsgIndex >= _kmsgList.Length)
                {
                    _kmsgNextTime = 1;
                }
                else
                {
                    var nextText = _kmsgList[_kmsgIndex];
                    
                    // each character is 10 milliseconds.
                    _kmsgNextTime = (double) nextText.Length / 4000;
                }
            }
            else
            {
                _kmsgNextTime -= gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
        
        private void ContinueBootSequence()
        {
            // Check if the game has player data. If so, go straight to graphical logo show.
            // If not, start the game's preinstall screen.
            if (_saveManager.IsPlayerReady)
            {
                _bootState = 2;
            }
            else
            {
                _master.Padding = 15;
                _statusHeader.Visibility = Visibility.Visible;
                _progress.Visibility = Visibility.Visible;
                _bootState = 1;
            }
        }
        
        private void LoadKernelMessages()
        {
            var asm = GetType().Assembly;
            using var resource = asm.GetManifestResourceStream("SociallyDistant.Resources.kmsg.txt");

            var text = string.Empty;
            
            if (resource == null)
            {
                text = @"I've failed you.

I wanted to make this boot screen realistic. The game's binary was supposed to
have a bunch of UNIX boot messages in it.  But you're seeing this message instead.
That's because my crappy-ass code that you most likely paid for, is crappy. And it couldn't
find the boot messages. You should get a refund.

But hey, at least I wrote a special case for that. Right?

I mean, the game could've crashed. But instead you get this message.

Oh well. I'ma throw ya into the game UI now.";
            }
            else
            {
                using var reader = new StreamReader(resource);
                text = reader.ReadToEnd();
            }

            _kmsgList = text.Split(Environment.NewLine);
        }
    }
}