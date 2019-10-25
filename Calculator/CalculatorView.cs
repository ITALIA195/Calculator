using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Calculator
{
    internal class CalculatorView : Control
    {
        private const float TextAreaHeightRatio = 0.2f;
        private const float KeypadHeightRatio = 1 - TextAreaHeightRatio;
            
        private const int KeypadColumns = 4;
        private const int KeypadRows = 5;
        
        private readonly RectangleF[] _buttons = new RectangleF[KeypadRows * KeypadColumns];
        private readonly string[] _buttonsText = 
        {
            "CE", "C", "DEL", "/",
            "7", "8", "9", "x",
            "4", "5", "6", "-",
            "1", "2", "3", "+",
            "\xB1", "0", "", "="
        };
        
        private double _displayedNumber;
        private int _selectedButton = -1;

        private bool _resetNext;
        
        private double _firstOperand;
        private double _secondOperand;
        private Operator _operator;
        
        private readonly Brush _whiteBrush = new SolidBrush(Color.White);
        private readonly Brush _buttonNormal = new SolidBrush(Color.FromArgb(22, 22, 22));
        private readonly Brush _buttonHover = new SolidBrush(Color.FromArgb(80, 80, 80));
        private readonly Brush _buttonKeypad = new SolidBrush(Color.FromArgb(7, 7, 7));

        public CalculatorView()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
        }

        private void DrawTextArea(Graphics g)
        {
            float height = ClientSize.Height * TextAreaHeightRatio;

            string str;
            if (_displayedNumber < 0)
                str = (-_displayedNumber).ToString(CultureInfo.InvariantCulture) + '-';
            else
                str = _displayedNumber.ToString(CultureInfo.InvariantCulture);
            var font = new Font(Font.FontFamily, 26, FontStyle.Regular);
            var size = g.MeasureString(str, font);
            g.DrawString(
                str,
                font,
                _whiteBrush,
                new RectangleF(0, height / 2f - size.Height / 2f, ClientSize.Width, height),
                new StringFormat(StringFormatFlags.DirectionRightToLeft |
                                 StringFormatFlags.NoWrap |
                                 StringFormatFlags.FitBlackBox));
        }

        private void DrawKeypadArea(Graphics g)
        {
            var font = new Font(Font.FontFamily, 12, FontStyle.Regular);
            for (var i = 0; i < _buttons.Length; i++)
            {
                var rect = _buttons[i];
                if (_selectedButton == i)
                    g.FillRectangle(_buttonHover, rect);
                else if (i / 4 >= 1 && i % 4 < 3) 
                    g.FillRectangle(_buttonKeypad, rect);
                else
                    g.FillRectangle(_buttonNormal, rect);
                var size = g.MeasureString(_buttonsText[i], font);
                g.DrawString(
                    _buttonsText[i], 
                    font, 
                    _whiteBrush, 
                    rect.X + rect.Width / 2f - size.Width / 2f,
                    rect.Y + rect.Height / 2f - size.Height / 2f);
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawTextArea(e.Graphics);
            DrawKeypadArea(e.Graphics);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_selectedButton < 0)
                return;
            
            if ((e.Button & MouseButtons.Left) != 0)
            {
                var row = _selectedButton / KeypadColumns;
                var column = _selectedButton % KeypadColumns;
                if (row >= 1 && row <= 3 && column < 3)
                {
                    if (_resetNext)
                    {
                        _displayedNumber = 0;
                        _resetNext = false;
                    }
                    var num = (3 - row) * 3 + column;
                    _displayedNumber = _displayedNumber * 10 + num + 1;
                }

                switch (column, row)
                {
                    case (0, 4):
                        _displayedNumber = -_displayedNumber;
                        break;
                    case (0, 0):
                        _displayedNumber = 0;
                        break;
                    case (1, 0):
                        _displayedNumber = 0;
                        _firstOperand = 0;
                        _secondOperand = 0;
                        _operator = Operator.None;
                        _resetNext = false;
                        break;
                    case (2, 0):
                        _displayedNumber = (_displayedNumber - _displayedNumber % 10) / 10;
                        break;
                    case (1, 4):
                        if (_resetNext)
                        {
                            _displayedNumber = 0;
                            _resetNext = false;
                        }
                        _displayedNumber *= 10;
                        break;
                }
                
                if (column == 3 && row < 4)
                {
                    if (_operator != Operator.None)
                    {
                        _secondOperand = _displayedNumber;
                        ComputeOperation();
                    }
                    
                    _operator = (Operator) row + 1;
                    _firstOperand = _displayedNumber;
                    _resetNext = true;
                }

                if (column == 3 && row == 4)
                {
                    if (_operator != Operator.None)
                    {
                        _secondOperand = _displayedNumber;
                        ComputeOperation();
                    }
                }
                
                Invalidate();
            }
        }

        private void ComputeOperation()
        {
            _displayedNumber = _operator switch
            {
                Operator.Division => _firstOperand / _secondOperand,
                Operator.Moltiplication => _firstOperand * _secondOperand,
                Operator.Subtraction => _firstOperand - _secondOperand,
                Operator.Sum => _firstOperand + _secondOperand,
                _ => throw new ArgumentOutOfRangeException(nameof(_operator), "The current operator is not supported.")
            };
            _operator = Operator.None;
            _resetNext = true;
        }

        protected override void OnResize(EventArgs e)
        {
            const float startX = 0;
            var startY = ClientSize.Height * TextAreaHeightRatio;

            const float windowMargin = 3;
            
            var width = ClientSize.Width - 2 * windowMargin;
            var height = ClientSize.Height - startY - 2 * windowMargin;
            
            const float spacing = 3;
            
            var buttonWidth = (width - spacing * (KeypadColumns - 1)) / KeypadColumns;
            var buttonHeight = (height - spacing * (KeypadRows - 1)) / KeypadRows;

            for (var i = 0; i < _buttons.Length; i++)
            {
                // ReSharper disable once PossibleLossOfFraction
                _buttons[i] = new RectangleF(
                    startX + windowMargin + (buttonWidth + spacing) * (i % 4),
                    startY + (buttonHeight + spacing) * (i / 4),
                    buttonWidth,
                    buttonHeight);
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var previousSelected = _selectedButton;
            _selectedButton = -1;
            for (var i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i].Contains(e.Location))
                    _selectedButton = i;
            }
            
            if (previousSelected != _selectedButton)
                Invalidate();
        }
    }
}