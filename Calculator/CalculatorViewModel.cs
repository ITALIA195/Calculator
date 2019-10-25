﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Calculator
{
    public sealed class CalculatorViewModel : INotifyPropertyChanged
    {
        private Operator _operator;
        private readonly double[] _operands = new double[2];
        private bool _resetNext;
        private double _display;

        public double Display
        {
            get => _display;
            set
            {
                if (value.Equals(_display)) return;
                _display = value;
                OnPropertyChanged();
            }
        }

        private double CurrentOperand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _operands[_operator == Operator.None ? 0 : 1];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _operands[_operator == Operator.None ? 0 : 1] = value;
        }

        public void OnKeyPressed(object key)
        {
            switch (key)
            {
                case int num:
                {
                    if (_resetNext)
                    {
                        CurrentOperand = 0;
                        _display = 0;
                        _resetNext = false;
                    }

                    CurrentOperand = CurrentOperand * 10 + (CurrentOperand < 0 ? -num : num);
                    _display = CurrentOperand;
                    break;
                }
                case Operator op:
                    if (_operator != Operator.None)
                        _operands[0] = Compute(_operator, _operands);
                    
                    _operator = op;
                    _operands[1] = _operands[0];
                    _display = _operands[0];
                    _resetNext = true;
                    break;
                case Action action:
                    switch (action)
                    {
                        case Action.Compute:
                            if (_operator == Operator.None)
                                break;

                            _operands[0] = Compute(_operator, _operands);
                            _display = _operands[0];
                            _operator = Operator.None;
                            _resetNext = true;
                            break;
                        case Action.ClearEntry:
                            _display = 0;
                            CurrentOperand = 0;
                            break;
                        case Action.Clear:
                            _display = 0;
                            _operands[0] = 0;
                            _operator = Operator.None;
                            _resetNext = false;
                            break;
                        case Action.Delete:
                            if (_resetNext)
                            {
                                CurrentOperand = 0;
                                _display = 0;
                                _resetNext = false;
                            }
                            _display = CurrentOperand = (CurrentOperand - CurrentOperand % 10) / 10;
                            break;
                        case Action.PlusMinus:
                            _display = CurrentOperand = -CurrentOperand;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(action), "Unexpected requested action");
                    }
                    break;
            }
        }

        private static double Compute(Operator op, params double[] operands)
        {
            return op switch
            {
                Operator.Division => operands[0] / operands[1],
                Operator.Moltiplication => operands[0] * operands[1],
                Operator.Subtraction => operands[0] - operands[1],
                Operator.Sum => operands[0] + operands[1],
                _ => throw new ArgumentOutOfRangeException(nameof(_operator), "The current operator is not supported.")
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}