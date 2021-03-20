using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace RedTeam
{
    public abstract class Scene
    {
        private RedTeamGame _game;
        private List<SceneComponent> _components = new List<SceneComponent>();

        public RedTeamGame Game => _game;
        
        public T AddComponent<T>() where T : SceneComponent, new()
        {
            var component = new T();
            _components.Add(component);
            component.Load(this);
            return component;
        }

        public void RemoveComponent(SceneComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            component.Unload();
            _components.Remove(component);
        }
        
        public void Load(RedTeamGame game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            OnLoad();
        }
        
        public void Unload()
        {
            while (_components.Any())
                RemoveComponent(_components.First());
            
            OnUnload();
            _game = null;
        }

        public void Update(GameTime gameTime)
        {
            OnUpdate(gameTime);
            
            foreach (var component in _components.ToArray())
            {
                component.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Draw(gameTime, _game.SpriteBatch);
            }
        }
        
        
        protected virtual void OnUpdate(GameTime gameTime) {}
        protected virtual void OnLoad() {}
        protected virtual void OnUnload() {}
    }
}