﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Gui.Elements;
using RedTeam.Input;

namespace RedTeam.Gui
{
    public sealed class GuiSystem : SceneComponent
    {
        private RootElement _rootElement;
        private bool _debugShowBounds;
        private SpriteFont _debugFont;
        private Element _focused;
        private Element _hovered;
        private Element _down;
        private InputManager _input;

        public Element FocusedElement => _focused;
        
        protected override void OnLoad()
        {
            base.OnLoad();
            _input = Game.GetComponent<InputManager>();
            _rootElement = new RootElement(this);

            _debugFont = Game.Content.Load<SpriteFont>("Fonts/DebugSmall");
            
            _input.MouseMove += HandleMouseMove;
            _input.MouseDown += HandleMouseDown;
            _input.MouseUp += HandleMouseUp;
            _input.KeyChar += HandleKeyChar;
        }

        private void HandleKeyChar(object? sender, KeyCharEventArgs e)
        {
            Bubble(_focused, x => x.FireKeyChar(e));
        }

        private void Bubble(Element element, Func<Element, bool> predicate)
        {
            var e = element;
            while (e != null)
            {
                if (predicate(e))
                    break;
                e = e.Parent;
            }
        }

        
        public void SetFocus(Element element)
        {
            if (_focused != element)
            {
                var evt = new FocusChangedEventArgs(_focused, element);

                if (_focused != null)
                {
                    _focused.FireBlurred(evt);
                }

                if (element != null)
                {
                    if (element.FireFocused(evt))
                    {
                        _focused = element;
                    }
                }
            }
        }
        
        private void HandleMouseUp(object? sender, MouseButtonEventArgs e)
        {
            var hovered = FindElement(e.XPosition, e.YPosition);

            if (_down == hovered)
            {
                SetFocus(hovered);
                _down = null;
            }
        }

        private void HandleMouseDown(object? sender, MouseButtonEventArgs e)
        {
            var hovered = FindElement(e.XPosition, e.YPosition);

            _down = hovered;
        }

        private void HandleMouseMove(object? sender, MouseMoveEventArgs e)
        {
            var hovered = FindElement(e.XPosition, e.YPosition);

            _hovered = hovered;
        }

        public void AddToViewport(Element element)
        {
            _rootElement.Children.Add(element);
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            PerformLayout();

            _rootElement.Update(gameTime);
        }

        private float ComputeElementOpacity(Element element)
        {
            var opacity = element.Opacity;
            var parent = element.Parent;
            while (parent != null)
            {
                opacity = opacity * parent.Opacity;
                parent = parent.Parent;
            }

            return opacity;
        }

        private void PerformLayout()
        {
            var screenRectangle = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight);

            var rootLayout = _rootElement.LayoutManager;

            rootLayout.SetBounds(screenRectangle);
        }
        
        private Color ComputeElementTint(Element element)
        {
            var color = element.Enabled ? Color.White : Color.Gray;
            var parent = element.Parent;
            while (parent != null)
            {
                var pColor = parent.Enabled ? Color.White : Color.Gray;

                var r = (float) pColor.R / 255f;
                var g = (float) pColor.G / 255f;
                var b = (float) pColor.B / 255f;

                var br = (byte) (color.R * r);
                var bg = (byte) (color.G * g);
                var bb = (byte) (color.B * b);

                color = new Color(br, bg, bb);
                
                parent = parent.Parent;
            }

            return color;
        }

        protected override void OnDraw(GameTime gameTime, SpriteBatch batch)
        {
            base.OnDraw(gameTime, batch);

            foreach (var element in _rootElement.CollapseElements())
            {
                var opacity = ComputeElementOpacity(element);
                var masterTint = ComputeElementTint(element);
                var clip = Rectangle.Empty;
                
                var renderer = new GuiRenderer(Game.GraphicsDevice, Game.White, batch, opacity, masterTint, clip);

                batch.Begin();
                element.Paint(gameTime, renderer);
                batch.End();

                if (_debugShowBounds)
                {
                    var debugRenderer = new GuiRenderer(Game.GraphicsDevice, Game.White, batch, 1, Color.White,
                        Rectangle.Empty);

                    batch.Begin();

                    debugRenderer.DrawRectangle(element.BoundingBox, Color.White, 1);

                    var text = $"{element.Name}{Environment.NewLine}BoundingBox={element.BoundingBox}";
                    var measure = _debugFont.MeasureString(text);
                    var pos = new Vector2((element.BoundingBox.Left + ((element.BoundingBox.Width - measure.X) / 2)),
                        element.BoundingBox.Top + ((element.BoundingBox.Height - measure.Y) / 2));

                    debugRenderer.DrawString(_debugFont, text, pos, Color.White, TextAlign.Center, 2);
                    
                    
                    batch.End();
                }
            }
        }

        private Element FindElement(int x, int y)
        {
            return FindElement(_rootElement, x, y);
        }

        private Element FindElement(Element elem, int x, int y)
        {
            foreach (var child in elem.Children.ToArray().Reverse())
            {
                var f = FindElement(child, x, y);
                if (f != null)
                    return f;
            }

            var b = elem.BoundingBox;

            if (x >= b.Left && x <= b.Right && y >= b.Top && y <= b.Bottom)
            {
                return elem;
            }
            
            return null;
        }
    }
}