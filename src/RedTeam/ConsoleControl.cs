using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RedTeam.Gui;
using RedTeam.Gui.Elements;
using RedTeam.Input;

namespace RedTeam
{
    public sealed class ConsoleControl : Element, IConsole
    {
        public static readonly char BackgroundColorCode = '$';
        public static readonly char ForegroundColorCode = '#';
        public static readonly char AttributeCode = '&';
        
        private const double _cursorBlinkTime = 0.75;
        private const double _blinkTime = 1;

        private double _blink;
        private double _cursorBlink;
        private bool _blinkShow = true;
        private bool _cursorShow = true;

        
        private bool _textIsDirty = true;
        
        private SpriteFont _regularFont;
        private SpriteFont _boldFont;
        private SpriteFont _italicFont;
        private SpriteFont _boldItalicFont;

        private ColorPalette _fallbackPalette = new ColorPalette();
        private ColorPalette _activePalette = null;

        private string _text = "";
        private string _input = string.Empty;
        private int _inputPos = 0;

        private string[] _relevantCompletions = Array.Empty<string>();
        private int _activeCompletion = 0;

        private int _completionsPerPage = 10;
        private int _scrollbarWidth = 3;
        private float _scrollbackMax;
        private float _height;
        private float _scrollback;
        private Color _scrollFG = Color.Cyan;
        private Color _scrollBG = Color.Gray;
        private Color _foreground = Color.LightGray;
        private Color _background = new Color(22, 22, 22);
        private Color _cursorColor = Color.White;
        private Color _cursorFG = Color.Black;
        private Color _highlight = Color.White;
        private Color _highlightFG = Color.Black;
        private Vector2 _completionY;
        private bool _paintCompletions;
        private float _completionsWidth;
        private int _completionPageStart;
        
        private const char CURSOR_SIGNAL = (char) 0xFF;

        private List<TextElement> _elements = new List<TextElement>();

        public IAutoCompleteSource AutoCompleteSource { get; set; }

        public ColorPalette ColorPalette
        {
            get => _activePalette ?? _fallbackPalette;
            set
            {
                _activePalette = value;
                _textIsDirty = true;
            }
        }

        public ConsoleControl()
        {
            _regularFont = RedTeamGame.Instance.Content.Load<SpriteFont>("Fonts/Console/Regular");
            _boldFont = RedTeamGame.Instance.Content.Load<SpriteFont>("Fonts/Console/Bold");
            _boldItalicFont = RedTeamGame.Instance.Content.Load<SpriteFont>("Fonts/Console/BoldItalic");
            _italicFont = RedTeamGame.Instance.Content.Load<SpriteFont>("Fonts/Console/Italic");
        }

        private Color GetColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return _background;
                case ConsoleColor.Gray:
                    return _foreground;
                default:
                    return ColorPalette.GetColor(color);
            }
        }

        private (int Start, int End, string Text) GetWordAtInputPos()
        {
            var end = 0;
            var start = 0;
            var word = string.Empty;

            if (_input.Length > 0)
            {
                for (var i = _inputPos; i >= 0; i--)
                {
                    if (i < _input.Length)
                    {
                        var ch = _input[i];
                        if (char.IsWhiteSpace(ch))
                            break;
                        start = i;
                    }
                }

                for (var i = _inputPos; i <= _input.Length; i++)
                {
                    if (i == _input.Length)
                    {
                        end = i;
                        break;
                    }
                    else
                    {
                        var ch = _input[i];
                        if (char.IsWhiteSpace(ch))
                            break;
                        end = i;
                    }
                }
            }
            else
            {
                start = 0;
                end = 0;
            }

            word = _input.Substring(start, end - start);
            
            return (start, end, word);
        }

        private IEnumerable<string> GetRelevantCompletions()
        {
            if (AutoCompleteSource != null)
            {
                var (start, end, word) = GetWordAtInputPos();
                if (start != end)
                {
                    foreach (var completion in AutoCompleteSource.GetCompletions(word))
                    {
                        if (completion.ToLower().StartsWith(word.ToLower()) && completion.Length > word.Length)
                            yield return completion;
                    }
                }
            }
        }
        
        public void ScrollToBottom()
        {
            _scrollback = 0;
        }
        
        private string[] BreakWords(string text)
        {
            var words = new List<string>();

            var word = string.Empty;
            foreach (var ch in text)
            {
                word += ch;
                if (char.IsWhiteSpace(ch))
                {
                    words.Add(word);
                    word = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(word))
                words.Add(word);
            
            return words.ToArray();
        }

        private string[] LetterWrap(SpriteFont font, string text, float width)
        {
            var lines = new List<string>();

            var line = string.Empty;
            var w = 0f;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i < text.Length)
                {
                    var ch = text[i];
                    var m = font.MeasureString(ch.ToString()).X;
                    if (w + m >= width)
                    {
                        lines.Add(line);
                        line = "";
                        w = 0;
                    }

                    line += ch;
                    w += m;
                }
                else
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        lines.Add(line);
                    }
                }
            }
            
            return lines.ToArray();
        }

        public void MoveLeft(int amount)
        {
            if (_inputPos - amount < 0)
                _inputPos = 0;
            else
                _inputPos -= amount;
            _textIsDirty = true;
        }

        private bool ParseColorCode(char code, out ConsoleColor color)
        {
            var result = true;
            var c = ConsoleColor.Black;

            int num = code switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'a' => 10,
                'A' => 10,
                'b' => 11,
                'B' => 11,
                'c' => 12,
                'C' => 12,
                'd' => 13,
                'D' => 13,
                'e' => 14,
                'E' => 14,
                'f' => 15,
                'F' => 15,
                _ => 0
            };

            result = num > 0 || (num == 0 && code == '0');
            color = (ConsoleColor) num;
            return result;
        }

        public void MoveRight(int amount)
        {
            if (_inputPos + amount > _input.Length)
                _inputPos = _input.Length;
            else
                _inputPos += amount;
            _textIsDirty = true;
        }

        public void MoveToHome()
        {
            _inputPos = 0;
            _textIsDirty = true;
        }

        public void MoveToEnd()
        {
            _inputPos = _input.Length;
            _textIsDirty = true;
        }

        public void ScrollUp(float amount)
        {
            if (_scrollback + amount > _scrollbackMax)
            {
                _scrollback = _scrollbackMax;
            }
            else
            {
                _scrollback += amount;
            }
        }

        public void ScrollDown(float amount)
        {
            _scrollback -= amount;
            if (_scrollback < 0)
                _scrollback = 0;
        }

        private void ApplyCompletion()
        {
            if (_relevantCompletions.Any())
            {
                var completion = _relevantCompletions[_activeCompletion];
                var (start, end, word) = GetWordAtInputPos();

                _input = _input.Remove(start, word.Length);
                _input = _input.Insert(start, completion);
                _inputPos = start;
                MoveRight(completion.Length + 1);
                _textIsDirty = true;
            }
        }

        private SpriteFont GetFont(bool bold, bool italic)
        {
            if (bold && italic)
                return _boldItalicFont;
            else if (bold)
                return _boldFont;
            else if (italic)
                return _italicFont;
            else
                return _regularFont;
        }

        private bool ParseAttribute(char code, out ConsoleAttribute attribute)
        {
            var attr = code switch
            {
                '0' => ConsoleAttribute.Reset,
                '1' => ConsoleAttribute.ResetFont,
                'u' => ConsoleAttribute.Underline,
                'i' => ConsoleAttribute.FontItalic,
                'b' => ConsoleAttribute.FontBold,
                'B' => ConsoleAttribute.FontNoBold,
                'I' => ConsoleAttribute.FontNoItalic,
                '2' => ConsoleAttribute.Blink,
                'U' => ConsoleAttribute.NoUnderline,
                _ => ConsoleAttribute.Unknown
            };

            attribute = attr;
            return attr != ConsoleAttribute.Unknown;
        }
        
        private void CreateTextElements()
        {
            // This is incredibly fucked
            // Step 1 is clear the old crap
            _elements.Clear();
            _textIsDirty = false;
            _blinkShow = true;
            _blink = 0;
            _cursorBlink = 0;
            _cursorShow = true;
            
            var bg = ConsoleColor.Black;
            var fg = ConsoleColor.Gray;
            var bold = false;
            var italic = false;
            var underline = false;
            var blink = false;
            
            // step 2 is to break the terminal into words.ords
            var outWords = BreakWords(_text + _input.Insert(_inputPos, CURSOR_SIGNAL.ToString()) + " ");
            
            // step 3 is create elements for these words
            for (var i = 0; i < outWords.Length; i++)
            {
                var word = outWords[i];

                var elem = new TextElement();
                elem.Background = GetColor(bg);
                elem.Foreground = GetColor(fg);
                elem.Font = GetFont(bold, italic);
                elem.Underline = underline;
                elem.Blinking = blink;

                if (FindFirstCharacterCode(word, out int colorCode, out ConsoleCode firstCode))
                {
                    var colorCharIndex = colorCode + 1;
                    if (colorCharIndex < word.Length)
                    {
                        var colorChar = word[colorCharIndex];
                        if (firstCode != ConsoleCode.Attr && ParseColorCode(colorChar, out ConsoleColor color))
                        {
                            var preWord = word.Substring(0, colorCharIndex - 1) ?? "";
                            var postWord = word.Substring(colorCharIndex + 1) ?? "";
                            word = preWord;
                            Array.Resize(ref outWords, outWords.Length + 1);
                            for (int j = outWords.Length - 1; j > i; j--)
                            {
                                outWords[j] = outWords[j - 1];
                            }

                            outWords[i + 1] = postWord;
                            outWords[i] = word;

                            if (firstCode == ConsoleCode.Bg)
                                bg = color;
                            else
                                fg = color;
                        }
                        else if (ParseAttribute(colorChar, out ConsoleAttribute attr))
                        {
                            var preWord = word.Substring(0, colorCharIndex - 1) ?? "";
                            var postWord = word.Substring(colorCharIndex + 1) ?? "";
                            word = preWord;
                            Array.Resize(ref outWords, outWords.Length + 1);
                            for (int j = outWords.Length - 1; j > i; j--)
                            {
                                outWords[j] = outWords[j - 1];
                            }

                            outWords[i + 1] = postWord;
                            outWords[i] = word;

                            switch (attr)
                            {
                                case ConsoleAttribute.Reset:
                                    underline = false;
                                    bold = false;
                                    italic = false;
                                    fg = ConsoleColor.Gray;
                                    bg = ConsoleColor.Black;
                                    blink = false;
                                    break;
                                case ConsoleAttribute.ResetFont:
                                    bold = false;
                                    italic = false;
                                    break;
                                case ConsoleAttribute.FontBold:
                                    bold = true;
                                    break;
                                case ConsoleAttribute.FontItalic:
                                    italic = true;
                                    break;
                                case ConsoleAttribute.FontNoBold:
                                    bold = false;
                                    break;
                                case ConsoleAttribute.FontNoItalic:
                                    italic = false;
                                    break;
                                case ConsoleAttribute.Underline:
                                    underline = true;
                                    break;
                                case ConsoleAttribute.Blink:
                                    blink = true;
                                    break;
                                case ConsoleAttribute.NoUnderline:
                                    underline = false;
                                    break;
                            }
                        }
                    }
                }

                elem.Text = word;
                _elements.Add(elem);
            }
            
            // step 4 is to handle the cursor.
            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = _elements[i];

                // handle the cursor
                if (elem.Text.Contains(CURSOR_SIGNAL))
                {
                    var cursor = elem.Text.IndexOf(CURSOR_SIGNAL);

                    var cursorChar = cursor + 1;
                    var afterCursor = cursorChar + 1;

                    var text = elem.Text;

                    // this element gets everything before the cursor
                    elem.Text = text.Substring(0, cursor);

                    // this adds the cursor itself as a text element.
                    i++;
                    var cElem = new TextElement();
                    cElem.Font = elem.Font;
                    if (HasAnyFocus)
                    {
                        cElem.Background = _cursorColor;
                        cElem.Foreground = _cursorFG;
                    }
                    else
                    {
                        cElem.Background = elem.Background;
                        cElem.Foreground = elem.Foreground;
                    }

                    cElem.IsCursor = true;
                    cElem.Underline = elem.Underline;
                    cElem.Text = text[cursorChar].ToString();
                    _elements.Insert(i, cElem);

                    // and this is everything after the cursor.
                    i++;
                    var aElem = new TextElement();
                    aElem.Background = elem.Background;
                    aElem.Foreground = elem.Foreground;
                    aElem.Font = elem.Font;
                    aElem.Underline = elem.Underline;
                    aElem.Text = text.Substring(afterCursor);
                    _elements.Insert(i, aElem);
                }
            }
            
            // step 5 is purging empty elements.
            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = _elements[i];
                if (string.IsNullOrEmpty(elem.Text))
                {
                    _elements.RemoveAt(i);
                    i--;
                }
            }
            
            // step 6 is to position the elements.
            var rect = BoundingBox;
            rect.Width -= _scrollbarWidth;
            var firstLine = true;
            var pos = rect.Location.ToVector2();
            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = _elements[i];

                for (int j = 0; j < elem.Text.Length; j++)
                {
                    if (char.IsWhiteSpace(elem.Text[j]))
                        continue;

                    if (!elem.Font.Characters.Contains(elem.Text[j]))
                        elem.Text = elem.Text.Replace(elem.Text[j], '?');
                }
                
                // Measure the element.
                var measure = elem.Font.MeasureString(elem.Text);

                // wrap to new line if the measurement states we can't fit
                if (!firstLine && pos.X + measure.X >= rect.Right)
                {
                    pos.X = rect.Left;
                    pos.Y += elem.Font.LineSpacing;
                }

                elem.Position = pos;
                
                // is the element larger than a lie?
                if (measure.X >= rect.Width)
                {
                    // letter-wrap the text
                    var lines = LetterWrap(elem.Font, elem.Text, rect.Width);
                    
                    // this element gets the first line
                    elem.Text = lines.First();
                    
                    // this is some seriously fucked shit
                    foreach (var line in lines.Skip(1))
                    {
                        // what the fuck?
                        i++;
                        
                        // oh my fucking god.
                        var wtf = new TextElement();
                        wtf.Font = elem.Font;
                        wtf.Text = line;
                        wtf.Foreground = elem.Foreground;
                        wtf.Background = elem.Background;
                        wtf.Underline = elem.Underline;
                        wtf.Position = elem.Position;
                        
                        // I wanna die
                        wtf.Position.Y += wtf.Font.LineSpacing;
                        
                        // fuck this
                        pos = wtf.Position;
                        
                        // my god I'm screwed
                        _elements.Insert(i, wtf);
                        elem = wtf;
                        
                        // SWEET MOTHER OF FUCK
                        measure = elem.Font.MeasureString(elem.Text);
                    }
                }

                if (elem.Text.EndsWith('\n'))
                {
                    pos.X = rect.Left;
                    pos.Y += elem.Font.LineSpacing;
                }
                else
                {
                    pos.X += measure.X;
                }
                
                firstLine = false;
            }
            
            // final step is figuring out how tall the text is.
            var lineY = -1f;
            var height = 0;
            foreach (var elem in _elements)
            {
                var y = elem.Position.Y;
                if (MathF.Abs(lineY - y) >= 0.00001f)
                {
                    lineY = y;
                    height += elem.Font.LineSpacing;
                }
            }

            _height = height;
            
            // Oh and let's figure out what the max scrollback is.
            if (_height > BoundingBox.Height)
            {
                _scrollbackMax = _height - BoundingBox.Height;
            }
            else
            {
                _scrollbackMax = 0;
            }
            
            // finally, if the text is taller than the bounds then we need to make sure we account for that.
            // this is literally how the auto-scroll code plays in.
            foreach (var elem in _elements)
            {
                elem.Position.Y -= _scrollbackMax;
            }
            
            // God there will never be a final step to this behemoth of an algorithm.
            // Update auto-completions.
            UpdateCompletions();
        }

        private bool FindFirstCharacterCode(string word, out int firstIndex, out ConsoleCode firstCode)
        {
            var result = false;
            var index = 0;
            var code = ConsoleCode.Attr;
            
            for (var i = 0; i < word.Length; i++)
            {
                var ch = word[i];
                if (ch == BackgroundColorCode)
                {
                    index = i;
                    code = ConsoleCode.Bg;
                    result = true;
                    break;
                }

                if (ch == ForegroundColorCode)
                {
                    index = i;
                    code = ConsoleCode.Fg;
                    result = true;
                    break;
                }

                if (ch == AttributeCode)
                {
                    index = i;
                    code = ConsoleCode.Attr;
                    result = true;
                    break;
                }
            }

            firstIndex = index;
            firstCode = code;
            return result;
        }
        
        private void UpdateCompletions()
        {
            _relevantCompletions = GetRelevantCompletions().ToArray();
            _activeCompletion = 0;
            
            // find the cursor.
            var cursor = _elements.First(x => x.IsCursor);
            
            // is it the first element on the line?
            if (cursor.Position.X <= BoundingBox.Left)
            {
                // use this location as the auto-complete start position
                _completionY = cursor.Position;
            }
            else
            {
                // use the element before it.
                var index = _elements.IndexOf(cursor);
                if (index > 0)
                {
                    var elem = _elements[index - 1];
                    _completionY = elem.Position;
                }
                else
                {
                    _completionY = cursor.Position;
                }
            }
            
            _completionY.Y += _regularFont.LineSpacing;
            _paintCompletions = _relevantCompletions.Any();
            _completionPageStart = 0;
            
            if (_paintCompletions)
            {
                _completionsWidth = _relevantCompletions.Select(x => _regularFont.MeasureString(x + " ").X)
                    .OrderByDescending(x => x).First();
            }
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            _cursorBlink += gameTime.ElapsedGameTime.TotalSeconds;
            _blink += gameTime.ElapsedGameTime.TotalSeconds;

            if (_cursorBlink >= _cursorBlinkTime)
            {
                _cursorBlink = 0;
                _cursorShow = !_cursorShow;
            }

            if (_blink >= _blinkTime)
            {
                _blink = 0;
                _blinkShow = !_blinkShow;
            }

            if (_textIsDirty)
            {
                CreateTextElements();
            }
        }

        protected override bool OnMouseScroll(MouseScrollEventArgs e)
        {
            var result = base.OnMouseScroll(e);

            var sb = _scrollback + (e.WheelDelta / 8);
            if (sb > _scrollbackMax)
                sb = _scrollbackMax;
            else if (sb < 0)
                sb = 0;
            _scrollback = sb;
            
            return result;
        }

        protected override void OnPaint(GameTime gameTime, GuiRenderer renderer)
        {
            renderer.FillRectangle(BoundingBox, _background);

            foreach (var elem in _elements)
            {
                var measure = elem.Font.MeasureString(elem.Text);
                var rect = new Rectangle((int) elem.Position.X, (int) elem.Position.Y, (int) measure.X, (int) measure.Y);

                var bg = elem.Background;
                var fg = elem.Foreground;
                
                if (_height > BoundingBox.Height)
                {
                    rect.Y += (int) _scrollback;
                }

                if (elem.IsCursor)
                {
                    if (!_cursorShow)
                    {
                        var s = bg;
                        bg = fg;
                        fg = s;
                    }
                }

                if (elem.Blinking && !_blinkShow)
                {
                    fg = Color.Transparent;
                }
                
                renderer.FillRectangle(rect, bg);

                renderer.DrawString(elem.Font, elem.Text, rect.Location.ToVector2(), fg);

                if (elem.Underline)
                {
                    rect.Height = 2;
                    rect.Y += (int) measure.Y - rect.Height;
                    renderer.FillRectangle(rect, fg);
                }
            }
            
            // Draw the completions menu
            if (_paintCompletions && IsFocused)
            {
                var cHeight = Math.Min(_relevantCompletions.Length, _completionsPerPage) * _regularFont.LineSpacing;
                var cPos = _completionY;

                // Scrolling
                if (_height > BoundingBox.Height)
                {
                    cPos.Y += (int) _scrollback;
                }

                // Make sure the menu doesn't go beyond the width of the terminal
                if (cPos.X + _completionsWidth > BoundingBox.Right)
                {
                    var back = BoundingBox.Right - (cPos.X + _completionsWidth);
                    cPos.X -= back;
                }
                
                // if the page height's going to go below the terminal bounds then we're going to render above the cursor.
                if (cPos.Y + cHeight > BoundingBox.Bottom)
                {
                    // cursor
                    cPos.Y -= _regularFont.LineSpacing;
                    // menu
                    cPos.Y -= cHeight;
                }

                // paint the background
                var bgRect = new Rectangle((int) cPos.X, (int) cPos.Y, (int) _completionsWidth, (int) cHeight);
                renderer.FillRectangle(bgRect, _background);
                
                // paint each line
                var c = 0;
                bgRect.Height = _regularFont.LineSpacing;
                for (int i = _completionPageStart; i < _relevantCompletions.Length; i++)
                {
                    if (c > _completionsPerPage)
                        break;

                    c++;
                    
                    // render the background if we're the active element
                    var color = _foreground;
                    if (i == _activeCompletion)
                    {
                        renderer.FillRectangle(bgRect, _highlight);
                        color = _highlightFG;
                    }
                    
                    // draw the text
                    renderer.DrawString(_regularFont, _relevantCompletions[i], bgRect.Location.ToVector2(), color);

                    bgRect.Y += _regularFont.LineSpacing;
                }
                


            }
            
            // paint the scroollbar.
            if (_scrollbarWidth > 0 && _height > BoundingBox.Height)
            {
                var sRect = BoundingBox;
                sRect.X = sRect.Right - _scrollbarWidth;
                sRect.Width = _scrollbarWidth;

                renderer.FillRectangle(sRect, _scrollBG);

                var height = BoundingBox.Height / _height;

                var pos = _height - BoundingBox.Height;
                pos -= _scrollback;

                sRect.Y = BoundingBox.Top + (int) ((pos / _height) * BoundingBox.Height);
                sRect.Height = (int) (BoundingBox.Height * height);
                renderer.FillRectangle(sRect, _scrollFG);
            }
        }

        protected override bool OnBlurred(FocusChangedEventArgs e)
        {
            base.OnBlurred(e);
            _textIsDirty = true;
            return true;
        }

        protected override bool OnFocused(FocusChangedEventArgs e)
        {
            base.OnFocused(e);
            _textIsDirty = true;
            return true;
        }

        protected override bool OnKeyDown(KeyEventArgs e)
        {
            var result = false;

            switch (e.Key)
            {
                case Keys.Left:
                    MoveLeft(1);
                    break;
                case Keys.Right:
                    MoveRight(1);
                    break;
                case Keys.Home:
                    MoveToHome();
                    break;
                case Keys.End:
                    MoveToEnd();
                    break;
                case Keys.Up:
                    if (_activeCompletion > 0)
                    {
                        _activeCompletion--;
                        if (_completionPageStart > _activeCompletion)
                        {
                            _completionPageStart--;
                        }
                    }

                    break;
                case Keys.Down:
                    if (_activeCompletion < _relevantCompletions.Length - 1)
                    {
                        _activeCompletion++;
                        if (_completionPageStart + _completionsPerPage < _activeCompletion)
                        {
                            _completionPageStart++;
                        }
                    }

                    break;
                case Keys.PageUp:
                    ScrollUp(BoundingBox.Height);
                    break;
                case Keys.PageDown:
                    ScrollDown(BoundingBox.Height);
                    break;
                case Keys.Delete:
                    if (_inputPos < _input.Length)
                    {
                        _input = _input.Remove(_inputPos, 1);
                        _textIsDirty = true;
                    }
                    break;
                case Keys.Tab:
                    ApplyCompletion();
                    break;
                default:
                    result = base.OnKeyDown(e);
                    break;
            }
            
            return result;
        }

        protected override bool OnKeyChar(KeyCharEventArgs e)
        {
            var result = base.OnKeyChar(e);

            ScrollToBottom();
            
            if (e.Key == Keys.Enter)
            {
                var nl = Environment.NewLine;
                _input += nl;
                MoveToEnd();
                result = true;
            }
            
            if (e.Character == '\b')
            {
                if (_inputPos > 0)
                {
                    _inputPos--;
                    _input = _input.Remove(_inputPos, 1);
                    _textIsDirty = true;
                }

                return true;
            }
            
            if (_regularFont.Characters.Contains(e.Character))
            {
                _input = _input.Insert(_inputPos, e.Character.ToString());
                _inputPos += 1;
                _textIsDirty = true;
                result = true;
            }

            return result;
        }

        private class TextElement
        {
            public SpriteFont Font;
            public string Text;
            public Color Background;
            public Color Foreground;
            public bool Underline;
            public Vector2 Position;
            public bool IsCursor;
            public bool Blinking;
        }

        public void Write(object value)
        {
            var text = string.Empty;
            if (value == null)
                text = "null";
            else
                text = value.ToString();
            Write(text);
        }

        public void Write(string text)
        {
            _text += text;
            _textIsDirty = true;
        }
        
        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }
        
        public void Write(string format, params object[] values)
        {
            var text = string.Format(format, values);
            _text += text;
            _textIsDirty = true;
        }

        public void WriteLine(string format, params object[] values)
        {
            Write(format, values);
            WriteLine();
        }

        public void WriteLine()
        {
            _text += Environment.NewLine;
            _textIsDirty = true;
        }

        public void Clear()
        {
            _text = string.Empty;
            _textIsDirty = true;
        }

        public bool GetLine(out string text)
        {
            // shorthand for newline character
            var nl = Environment.NewLine;
            
            // check if a full line of text has been entered.
            if (_input.Contains(nl))
            {
                // Get the first line of text in the input.
                // This also removes that line from the input.
                var index = _input.IndexOf(nl);
                text = _input.Substring(0, index);
                _input = _input.Substring(text.Length + nl.Length);

                // Write the extracted line to output.
                WriteLine(text);
                
                // Move the cursor to the left by the removed amount of characters.
                MoveLeft(text.Length + nl.Length);
                
                return true;
            }

            text = string.Empty;
            return false;
        }

        public bool GetCharacter(out char character)
        {
            // do we have any input?
            if (!string.IsNullOrWhiteSpace(_input))
            {
                // get the first character.
                character = _input[0];
                
                // remove it from input.
                _input = _input.Substring(1);
                
                // echo it
                Write(character);
                
                // Move the cursor to the left by 1.
                MoveLeft(1);

                return true;
            }

            character = '\0';
            return false;
        }
    }

    public class ColorPalette
    {
        private Dictionary<ConsoleColor, Color> _map = new Dictionary<ConsoleColor, Color>();

        public ColorPalette()
        {
            _map.Add(ConsoleColor.Black, Color.Black);
            _map.Add(ConsoleColor.White, Color.White);
            _map.Add(ConsoleColor.Red, Color.Red);
            _map.Add(ConsoleColor.Green, Color.Green);
            _map.Add(ConsoleColor.Blue, Color.Blue);
            _map.Add(ConsoleColor.Gray, Color.Gray);
            _map.Add(ConsoleColor.DarkGray, Color.DarkGray);
            _map.Add(ConsoleColor.Cyan, Color.Cyan);
            _map.Add(ConsoleColor.Magenta, Color.Magenta);
            _map.Add(ConsoleColor.Yellow, Color.Yellow);
            _map.Add(ConsoleColor.DarkYellow, Color.Orange);
            _map.Add(ConsoleColor.DarkRed, Color.DarkRed);
            _map.Add(ConsoleColor.DarkGreen, Color.DarkGreen);
            _map.Add(ConsoleColor.DarkBlue, Color.DarkBlue);
            _map.Add(ConsoleColor.DarkCyan, Color.DarkCyan);
            _map.Add(ConsoleColor.DarkMagenta, Color.DarkMagenta);
        }
        
        public Color GetColor(ConsoleColor consoleColor)
        {
            return _map[consoleColor];
        }

        public void SetColor(ConsoleColor consoleColor, Color color)
        {
            _map[consoleColor] = new Color(color.R, color.G, color.B);
        }
    }

    public enum ConsoleCode
    {
        Bg,
        Fg,
        Attr
    }
    
    public enum ConsoleAttribute
    {
        Unknown,
        Reset,
        ResetFont,
        FontBold,
        FontNoBold,
        FontItalic,
        FontNoItalic,
        Underline,
        NoUnderline,
        Blink
    }
}