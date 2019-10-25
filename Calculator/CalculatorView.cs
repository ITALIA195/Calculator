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
        private readonly object[] _keyMapping =
        {
            Action.ClearEntry, Action.Clear, Action.Delete, Operator.Division,
            7, 8, 9, Operator.Moltiplication,
            4, 5, 6, Operator.Subtraction,
            1, 2, 3, Operator.Sum,
            Action.PlusMinus, 0, null, Action.Compute
        };
        
        private int _selectedButton = -1;

        private readonly CalculatorViewModel _viewModel;
        
        private readonly Brush _whiteBrush = new SolidBrush(Color.White);
        private readonly Brush _buttonNormal = new SolidBrush(Color.FromArgb(22, 22, 22));
        private readonly Brush _buttonHover = new SolidBrush(Color.FromArgb(80, 80, 80));
        private readonly Brush _buttonKeypad = new SolidBrush(Color.FromArgb(7, 7, 7));

        public CalculatorView() : this(new CalculatorViewModel()) { }

        public CalculatorView(CalculatorViewModel viewModel)
        {
            _viewModel = viewModel;

            viewModel.PropertyChanged += (a, b) => Invalidate();
            SetStyle(ControlStyles.DoubleBuffer, true);
        }

        private void DrawTextArea(Graphics g)
        {
            float height = ClientSize.Height * TextAreaHeightRatio;

            string str;
            if (_viewModel.Display < 0)
                str = (-_viewModel.Display).ToString(CultureInfo.InvariantCulture) + '-';
            else
                str = _viewModel.Display.ToString(CultureInfo.InvariantCulture);
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
                var key = _keyMapping[_selectedButton];
                _viewModel.OnKeyPressed(key);
                
                Invalidate();
            }
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