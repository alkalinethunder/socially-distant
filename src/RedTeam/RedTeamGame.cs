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
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _white;

        private List<GlobalComponent> _components = new List<GlobalComponent>();
        
        public Texture2D White => _white;

        public RedTeamGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

        protected override void Initialize()
        {
            RegisterComponent<InputManager>();
            
            _white = new Texture2D(GraphicsDevice, 1, 1);
            _white.SetData<uint>(new[] {0xFFFFFFFF});

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            _white.Dispose();

            while (_components.Any())
            {
                _components.First().Unload();
                _components.RemoveAt(0);
            }
            
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            foreach (var component in _components.ToArray())
            {
                component.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            base.Draw(gameTime);
        }
    }
}
