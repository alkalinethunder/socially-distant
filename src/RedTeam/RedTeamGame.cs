using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RedTeam.Input;

namespace RedTeam
{
    public class RedTeamGame : Game
    {
        private static RedTeamGame _instance;

        public static RedTeamGame Instance
            => _instance;

        private TimeSpan _upTime;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _white;
        private Scene _activeScene;
        
        private List<GlobalComponent> _components = new List<GlobalComponent>();
        
        public Texture2D White => _white;

        public TimeSpan UpTime => _upTime;
        
        public SpriteBatch SpriteBatch => _spriteBatch;

        public int ScreenWidth
            => GraphicsDevice.PresentationParameters.BackBufferWidth;
        
        public int ScreenHeight
            => GraphicsDevice.PresentationParameters.BackBufferHeight;
        
        
        public RedTeamGame()
        {
            _instance = this;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public T GetComponent<T>() where T : GlobalComponent, new()
        {
            return _components.OfType<T>().FirstOrDefault() ?? RegisterComponent<T>();
        }
        
        public T RegisterComponent<T>() where T : GlobalComponent, new()
        {
            if (_components.Any(x => x is T))
                throw new InvalidOperationException("Component is already registered.");
            
            var instance = new T();
            _components.Add(instance);
            instance.Initialize(this);
            return instance;
        }

        public void LoadScene(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            if (_activeScene != null)
                _activeScene.Unload();

            _activeScene = scene;
            scene.Load(this);
        }
        
        public void LoadScene<T>() where T : Scene, new()
        {
            var scene = new T();
            LoadScene(scene);
        }
        
        protected override void Initialize()
        {
            RegisterComponent<InputManager>();

            // use native screen resolution
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            Window.IsBorderless = true;
            
            _graphics.ApplyChanges();

            _white = new Texture2D(GraphicsDevice, 1, 1);
            _white.SetData<uint>(new[] {0xFFFFFFFF});

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadScene<ConsoleScene>();
        }

        protected override void UnloadContent()
        {
            _white.Dispose();

            while (_components.Any())
            {
                _components.First().Unload();
                _components.RemoveAt(0);
            }

            if (_activeScene != null)
            {
                _activeScene.Unload();
                _activeScene = null;
            }
            
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _upTime = gameTime.TotalGameTime;
            
            foreach (var component in _components.ToArray())
            {
                component.Update(gameTime);
            }

            _activeScene?.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _activeScene?.Draw(gameTime);
            
            base.Draw(gameTime);
        }
    }
}
