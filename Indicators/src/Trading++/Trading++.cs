// TTPlugins
// Copyright (C) 2024  TTPlugins
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CustomCommon.Draw;
using TigerTrade.Chart.Alerts;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Data;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Indicators.Enums;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace IndicatorsPlusPlus.Trading
{
    [DataContract(
        Name = "TradingPlusPlusIndicator",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom"
    )]
    [Indicator("TradingPlusPlusIndicator", "*Trading++", true, Type = typeof(Trading))]
    internal sealed class Trading : IndicatorBase
    {
        public enum Type
        {
            [EnumMember(Value = "Percent"), Description("%")]
            Percent,

            [EnumMember(Value = "Ticks"), Description("Ticks")]
            Ticks,
        }

        private Type _baseType;

        [DataMember(Name = "BaseType")]
        [Category("General"), DisplayName("Base Type")]
        public Type BaseType
        {
            get => _baseType;
            set
            {
                if (value == _baseType)
                {
                    return;
                }

                _baseType = value;

                OnPropertyChanged();
            }
        }

        private double _stopValue;

        [DataMember(Name = "StopValue")]
        [Category("General"), DisplayName("Stop Value")]
        public double StopValue
        {
            get => _stopValue;
            set
            {
                value = Math.Max(0, value);

                if (value == _stopValue)
                {
                    return;
                }

                _stopValue = value;

                OnPropertyChanged();
            }
        }

        private int _profits;

        [DataMember(Name = "Profits")]
        [Category("General"), DisplayName("N. Profits")]
        public int Profits
        {
            get => _profits;
            set
            {
                value = Math.Max(1, value);

                if (value == _profits)
                {
                    return;
                }

                _profits = value;

                OnPropertyChanged();
            }
        }

        private double _profitsMultiplier;

        [DataMember(Name = "ProfitsMultiplier")]
        [Category("General"), DisplayName("Profits Multiplier")]
        public double ProfitsMultiplier
        {
            get => _profitsMultiplier;
            set
            {
                value = Math.Max(0, value);

                if (value == _profitsMultiplier)
                {
                    return;
                }

                _profitsMultiplier = value;

                OnPropertyChanged();
            }
        }

        private double _fontSize;

        [DataMember(Name = "FontSize")]
        [Category("General"), DisplayName("Font Size")]
        public double FontSize
        {
            get => _fontSize;
            set
            {
                value = Math.Max(1, value);

                if (value == _fontSize)
                {
                    return;
                }

                _fontSize = value;

                OnPropertyChanged();
            }
        }

        private bool _showRisk;

        [DataMember(Name = "ShowRisk")]
        [Category("Risk Calculator"), DisplayName("Show")]
        public bool ShowRisk
        {
            get => _showRisk;
            set
            {
                if (value == _showRisk)
                {
                    return;
                }

                _showRisk = value;

                OnPropertyChanged();
            }
        }

        private double _riskBalance;

        [DataMember(Name = "RiskBalance")]
        [Category("Risk Calculator"), DisplayName("Balance")]
        public double RiskBalance
        {
            get => _riskBalance;
            set
            {
                value = Math.Max(0, value);

                if (value == _riskBalance)
                {
                    return;
                }

                _riskBalance = value;

                OnPropertyChanged();
            }
        }

        private double _riskPercentValue;

        [DataMember(Name = "RiskPercentValue")]
        [Category("Risk Calculator"), DisplayName("Max Risk Value %")]
        public double RiskPercentValue
        {
            get => _riskPercentValue;
            set
            {
                value = Math.Max(0, value);

                if (value == _riskPercentValue)
                {
                    return;
                }

                _riskPercentValue = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionBox;

        [DataMember(Name = "ShowPositionBox")]
        [Category("Position Box"), DisplayName("Show")]
        public bool ShowPositionBox
        {
            get => _showPositionBox;
            set
            {
                if (value == _showPositionBox)
                {
                    return;
                }

                _showPositionBox = value;

                OnPropertyChanged();
            }
        }

        private double _positionBoxHorizontalMargin;

        [DataMember(Name = "PositionBoxHorizontalMargin")]
        [Category("Position Box"), DisplayName("Horizontal Margin")]
        public double PositionBoxHorizontalMargin
        {
            get => _positionBoxHorizontalMargin;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionBoxHorizontalMargin)
                {
                    return;
                }

                _positionBoxHorizontalMargin = value;

                OnPropertyChanged();
            }
        }

        private double _positionBoxVerticalMargin;

        [DataMember(Name = "PositionBoxVerticalMargin")]
        [Category("Position Box"), DisplayName("Vertical Margin")]
        public double PositionBoxVerticalMargin
        {
            get => _positionBoxVerticalMargin;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionBoxVerticalMargin)
                {
                    return;
                }

                _positionBoxVerticalMargin = value;

                OnPropertyChanged();
            }
        }

        private double _positionBoxHorizontalPadding;

        [DataMember(Name = "PositionBoxHorizontalPadding")]
        [Category("Position Box"), DisplayName("Horizontal Padding")]
        public double PositionBoxHorizontalPadding
        {
            get => _positionBoxHorizontalPadding;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionBoxHorizontalPadding)
                {
                    return;
                }

                _positionBoxHorizontalPadding = value;

                OnPropertyChanged();
            }
        }

        private double _positionBoxVerticalPadding;

        [DataMember(Name = "PositionBoxVerticalPadding")]
        [Category("Position Box"), DisplayName("Vertical Padding")]
        public double PositionBoxVerticalPadding
        {
            get => _positionBoxVerticalPadding;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionBoxVerticalPadding)
                {
                    return;
                }

                _positionBoxVerticalPadding = value;

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxBackgroundBrush;

        private XColor _positionBoxBackgroundColor;

        [DataMember(Name = "PositionBoxBackgroundColor")]
        [Category("Position Box"), DisplayName("Background")]
        public XColor PositionBoxBackgroundColor
        {
            get => _positionBoxBackgroundColor;
            set
            {
                if (value == _positionBoxBackgroundColor)
                {
                    return;
                }

                _positionBoxBackgroundColor = value;

                _positionBoxBackgroundBrush = new XBrush(_positionBoxBackgroundColor);

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxBorderBrush;

        private XPen _positionBoxBorderPen;

        private XColor _positionBoxBorderColor;

        [DataMember(Name = "PositionBoxBorderColor")]
        [Category("Position Box"), DisplayName("Border Color")]
        public XColor PositionBoxBorderColor
        {
            get => _positionBoxBorderColor;
            set
            {
                if (value == _positionBoxBorderColor)
                {
                    return;
                }

                _positionBoxBorderColor = value;

                _positionBoxBorderBrush = new XBrush(_positionBoxBorderColor);
                _positionBoxBorderPen = new XPen(
                    _positionBoxBorderBrush,
                    PositionBoxBorderThickness,
                    PositionBoxBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _positionBoxBorderThickness;

        [DataMember(Name = "PositionBoxBorderThickness")]
        [Category("Position Box"), DisplayName("Border Thickness")]
        public int PositionBoxBorderThickness
        {
            get => _positionBoxBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionBoxBorderThickness)
                {
                    return;
                }

                _positionBoxBorderThickness = value;

                _positionBoxBorderPen = new XPen(
                    _positionBoxBorderBrush,
                    _positionBoxBorderThickness,
                    PositionBoxBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _positionBoxBorderStyle;

        [DataMember(Name = "PositionBoxBorderStyle")]
        [Category("Position Box"), DisplayName("Border Style")]
        public XDashStyle PositionBoxBorderStyle
        {
            get => _positionBoxBorderStyle;
            set
            {
                if (value == _positionBoxBorderStyle)
                {
                    return;
                }

                _positionBoxBorderStyle = value;

                _positionBoxBorderPen = new XPen(
                    _positionBoxBorderBrush,
                    PositionBoxBorderThickness,
                    _positionBoxBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxNeutralTextBrush;

        private XColor _positionBoxNeutralTextColor;

        [DataMember(Name = "PositionBoxNeutralTextColor")]
        [Category("Position Box"), DisplayName("Neutral Text Color")]
        public XColor PositionBoxNeutralTextColor
        {
            get => _positionBoxNeutralTextColor;
            set
            {
                if (value == _positionBoxNeutralTextColor)
                {
                    return;
                }

                _positionBoxNeutralTextColor = value;

                _positionBoxNeutralTextBrush = new XBrush(_positionBoxNeutralTextColor);

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxPositiveTextBrush;

        private XColor _positionBoxPositiveTextColor;

        [DataMember(Name = "PositionBoxPositiveTextColor")]
        [Category("Position Box"), DisplayName("Positive Text Color")]
        public XColor PositionBoxPositiveTextColor
        {
            get => _positionBoxPositiveTextColor;
            set
            {
                if (value == _positionBoxPositiveTextColor)
                {
                    return;
                }

                _positionBoxPositiveTextColor = value;

                _positionBoxPositiveTextBrush = new XBrush(_positionBoxPositiveTextColor);

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxNegativeTextBrush;

        private XColor _positionBoxNegativeTextColor;

        [DataMember(Name = "PositionBoxNegativeTextColor")]
        [Category("Position Box"), DisplayName("Negative Text Color")]
        public XColor PositionBoxNegativeTextColor
        {
            get => _positionBoxNegativeTextColor;
            set
            {
                if (value == _positionBoxNegativeTextColor)
                {
                    return;
                }

                _positionBoxNegativeTextColor = value;

                _positionBoxNegativeTextBrush = new XBrush(_positionBoxNegativeTextColor);

                OnPropertyChanged();
            }
        }

        private XBrush _positionBoxQtyValueTextBrush;

        private XColor _positionBoxQtyValueTextColor;

        [DataMember(Name = "PositionBoxQtyValueTextColor")]
        [Category("Position Box"), DisplayName("Qty/Value Text Color")]
        public XColor PositionBoxQtyValueTextColor
        {
            get => _positionBoxQtyValueTextColor;
            set
            {
                if (value == _positionBoxQtyValueTextColor)
                {
                    return;
                }

                _positionBoxQtyValueTextColor = value;

                _positionBoxQtyValueTextBrush = new XBrush(_positionBoxQtyValueTextColor);

                OnPropertyChanged();
            }
        }

        private double _positionLinesMargin;

        [DataMember(Name = "PositionLinesMargin")]
        [Category("Position Lines"), DisplayName("Margin")]
        public double PositionLinesMargin
        {
            get => _positionLinesMargin;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionLinesMargin)
                {
                    return;
                }

                _positionLinesMargin = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesEntry;

        [DataMember(Name = "ShowPositionLinesEntry")]
        [Category("Position Lines"), DisplayName("Show Entry")]
        public bool ShowPositionLinesEntry
        {
            get => _showPositionLinesEntry;
            set
            {
                if (value == _showPositionLinesEntry)
                {
                    return;
                }

                _showPositionLinesEntry = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesStop;

        [DataMember(Name = "ShowPositionLinesStop")]
        [Category("Position Lines"), DisplayName("Show Stop")]
        public bool ShowPositionLinesStop
        {
            get => _showPositionLinesStop;
            set
            {
                if (value == _showPositionLinesStop)
                {
                    return;
                }

                _showPositionLinesStop = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesProfits;

        [DataMember(Name = "ShowPositionLinesProfits")]
        [Category("Position Lines"), DisplayName("Show Profits")]
        public bool ShowPositionLinesProfits
        {
            get => _showPositionLinesProfits;
            set
            {
                if (value == _showPositionLinesProfits)
                {
                    return;
                }

                _showPositionLinesProfits = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesEntryMarker;

        [DataMember(Name = "ShowPositionLinesEntryMarker")]
        [Category("Position Lines"), DisplayName("Show Entry Marker")]
        public bool ShowPositionLinesEntryMarker
        {
            get => _showPositionLinesEntryMarker;
            set
            {
                if (value == _showPositionLinesEntryMarker)
                {
                    return;
                }

                _showPositionLinesEntryMarker = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesStopMarker;

        [DataMember(Name = "ShowPositionLinesStopMarker")]
        [Category("Position Lines"), DisplayName("Show Stop Marker")]
        public bool ShowPositionLinesStopMarker
        {
            get => _showPositionLinesStopMarker;
            set
            {
                if (value == _showPositionLinesStopMarker)
                {
                    return;
                }

                _showPositionLinesStopMarker = value;

                OnPropertyChanged();
            }
        }

        private bool _showPositionLinesProfitsMarkers;

        [DataMember(Name = "ShowPositionLinesProfitsMarkers")]
        [Category("Position Lines"), DisplayName("Show Profits Markers")]
        public bool ShowPositionLinesProfitsMarkers
        {
            get => _showPositionLinesProfitsMarkers;
            set
            {
                if (value == _showPositionLinesProfitsMarkers)
                {
                    return;
                }

                _showPositionLinesProfitsMarkers = value;

                OnPropertyChanged();
            }
        }

        private XBrush _positionLinesEntryBorderBrush;

        private XPen _positionLinesEntryBorderPen;

        private XColor _positionLinesEntryBorderColor;

        [DataMember(Name = "PositionLinesEntryBorderColor")]
        [Category("Position Lines"), DisplayName("Entry Border Color")]
        public XColor PositionLinesEntryBorderColor
        {
            get => _positionLinesEntryBorderColor;
            set
            {
                if (value == _positionLinesEntryBorderColor)
                {
                    return;
                }

                _positionLinesEntryBorderColor = value;

                _positionLinesEntryBorderBrush = new XBrush(_positionLinesEntryBorderColor);
                _positionLinesEntryBorderPen = new XPen(
                    _positionLinesEntryBorderBrush,
                    PositionLinesEntryBorderThickness,
                    PositionLinesEntryBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _positionLinesEntryBorderThickness;

        [DataMember(Name = "PositionLinesEntryBorderThickness")]
        [Category("Position Lines"), DisplayName("Entry Border Thickness")]
        public int PositionLinesEntryBorderThickness
        {
            get => _positionLinesEntryBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionLinesEntryBorderThickness)
                {
                    return;
                }

                _positionLinesEntryBorderThickness = value;

                _positionLinesEntryBorderPen = new XPen(
                    _positionLinesEntryBorderBrush,
                    _positionLinesEntryBorderThickness,
                    PositionLinesEntryBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _positionLinesEntryBorderStyle;

        [DataMember(Name = "PositionLinesEntryBorderStyle")]
        [Category("Position Lines"), DisplayName("Entry Border Style")]
        public XDashStyle PositionLinesEntryBorderStyle
        {
            get => _positionLinesEntryBorderStyle;
            set
            {
                if (value == _positionLinesEntryBorderStyle)
                {
                    return;
                }

                _positionLinesEntryBorderStyle = value;

                _positionLinesEntryBorderPen = new XPen(
                    _positionLinesEntryBorderBrush,
                    PositionLinesEntryBorderThickness,
                    _positionLinesEntryBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XBrush _positionLinesStopBorderBrush;

        private XPen _positionLinesStopBorderPen;

        private XColor _positionLinesStopBorderColor;

        [DataMember(Name = "PositionLinesStopBorderColor")]
        [Category("Position Lines"), DisplayName("Stop Border Color")]
        public XColor PositionLinesStopBorderColor
        {
            get => _positionLinesStopBorderColor;
            set
            {
                if (value == _positionLinesStopBorderColor)
                {
                    return;
                }

                _positionLinesStopBorderColor = value;

                _positionLinesStopBorderBrush = new XBrush(_positionLinesStopBorderColor);
                _positionLinesStopBorderPen = new XPen(
                    _positionLinesStopBorderBrush,
                    PositionLinesStopBorderThickness,
                    PositionLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _positionLinesStopBorderThickness;

        [DataMember(Name = "PositionLinesStopBorderThickness")]
        [Category("Position Lines"), DisplayName("Stop Border Thickness")]
        public int PositionLinesStopBorderThickness
        {
            get => _positionLinesStopBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionLinesStopBorderThickness)
                {
                    return;
                }

                _positionLinesStopBorderThickness = value;

                _positionLinesStopBorderPen = new XPen(
                    _positionLinesStopBorderBrush,
                    _positionLinesStopBorderThickness,
                    PositionLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _positionLinesStopBorderStyle;

        [DataMember(Name = "PositionLinesStopBorderStyle")]
        [Category("Position Lines"), DisplayName("Stop Border Style")]
        public XDashStyle PositionLinesStopBorderStyle
        {
            get => _positionLinesStopBorderStyle;
            set
            {
                if (value == _positionLinesStopBorderStyle)
                {
                    return;
                }

                _positionLinesStopBorderStyle = value;

                _positionLinesStopBorderPen = new XPen(
                    _positionLinesStopBorderBrush,
                    PositionLinesStopBorderThickness,
                    _positionLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XBrush _positionLinesProfitsBorderBrush;

        private XPen _positionLinesProfitsBorderPen;

        private XColor _positionLinesProfitsBorderColor;

        [DataMember(Name = "PositionLinesProfitsBorderColor")]
        [Category("Position Lines"), DisplayName("Profits Border Color")]
        public XColor PositionLinesProfitsBorderColor
        {
            get => _positionLinesProfitsBorderColor;
            set
            {
                if (value == _positionLinesProfitsBorderColor)
                {
                    return;
                }

                _positionLinesProfitsBorderColor = value;

                _positionLinesProfitsBorderBrush = new XBrush(_positionLinesProfitsBorderColor);
                _positionLinesProfitsBorderPen = new XPen(
                    _positionLinesProfitsBorderBrush,
                    PositionLinesProfitsBorderThickness,
                    PositionLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _positionLinesProfitsBorderThickness;

        [DataMember(Name = "PositionLinesProfitsBorderThickness")]
        [Category("Position Lines"), DisplayName("Profits Border Thickness")]
        public int PositionLinesProfitsBorderThickness
        {
            get => _positionLinesProfitsBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _positionLinesProfitsBorderThickness)
                {
                    return;
                }

                _positionLinesProfitsBorderThickness = value;

                _positionLinesProfitsBorderPen = new XPen(
                    _positionLinesProfitsBorderBrush,
                    _positionLinesProfitsBorderThickness,
                    PositionLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _positionLinesProfitsBorderStyle;

        [DataMember(Name = "PositionLinesProfitsBorderStyle")]
        [Category("Position Lines"), DisplayName("Profits Border Style")]
        public XDashStyle PositionLinesProfitsBorderStyle
        {
            get => _positionLinesProfitsBorderStyle;
            set
            {
                if (value == _positionLinesProfitsBorderStyle)
                {
                    return;
                }

                _positionLinesProfitsBorderStyle = value;

                _positionLinesProfitsBorderPen = new XPen(
                    _positionLinesProfitsBorderBrush,
                    PositionLinesProfitsBorderThickness,
                    _positionLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XBrush _positionLinesTextBackgroundBrush;
        private XColor _positionLinesTextBackgroundColor;

        [DataMember(Name = "PositionLinesTextBackgroundColor")]
        [Category("Position Lines"), DisplayName("Text Background")]
        public XColor PositionLinesTextBackgroundColor
        {
            get => _positionLinesTextBackgroundColor;
            set
            {
                if (value == _positionLinesTextBackgroundColor)
                {
                    return;
                }

                _positionLinesTextBackgroundColor = value;

                _positionLinesTextBackgroundBrush = new XBrush(_positionLinesTextBackgroundColor);

                OnPropertyChanged();
            }
        }

        public enum CustomKey
        {
            None = 0,
            A = 44,
            B = 45,
            C = 46,
            D = 47,
            E = 48,
            F = 49,
            G = 50,
            H = 51,
            I = 52,
            J = 53,
            K = 54,
            L = 55,
            M = 56,
            N = 57,
            O = 58,
            P = 59,
            Q = 60,
            R = 61,
            S = 62,
            T = 63,
            U = 64,
            V = 65,
            W = 66,
            X = 67,
            Y = 68,
            Z = 69,
        }

        private CustomKey _cursorLinesActivationKey;

        [DataMember(Name = "CursorLinesActivationKey")]
        [Category("Cursor Lines"), DisplayName("Activation Key")]
        public CustomKey CursorLinesActivationKey
        {
            get => _cursorLinesActivationKey;
            set
            {
                if (value == _cursorLinesActivationKey)
                {
                    return;
                }

                _cursorLinesActivationKey = value;

                OnPropertyChanged();
            }
        }

        public enum CustomModifierKeys
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
        }

        private CustomModifierKeys _cursorLinesOppositeKey;

        [DataMember(Name = "CursorLinesOppositeKey")]
        [Category("Cursor Lines"), DisplayName("Opposite Key")]
        public CustomModifierKeys CursorLinesOppositeKey
        {
            get => _cursorLinesOppositeKey;
            set
            {
                if (value == _cursorLinesOppositeKey)
                {
                    return;
                }

                _cursorLinesOppositeKey = value;

                OnPropertyChanged();
            }
        }

        private bool _showCursorLinesStop;

        [DataMember(Name = "ShowCursorLinesStop")]
        [Category("Cursor Lines"), DisplayName("Show Stop")]
        public bool ShowCursorLinesStop
        {
            get => _showCursorLinesStop;
            set
            {
                if (value == _showCursorLinesStop)
                {
                    return;
                }

                _showCursorLinesStop = value;

                OnPropertyChanged();
            }
        }

        private bool _showCursorLinesProfits;

        [DataMember(Name = "ShowCursorLinesProfits")]
        [Category("Cursor Lines"), DisplayName("Show Profits")]
        public bool ShowCursorLinesProfits
        {
            get => _showCursorLinesProfits;
            set
            {
                if (value == _showCursorLinesProfits)
                {
                    return;
                }

                _showCursorLinesProfits = value;

                OnPropertyChanged();
            }
        }

        private XBrush _cursorLinesStopBorderBrush;

        private XPen _cursorLinesStopBorderPen;

        private XColor _cursorLinesStopBorderColor;

        [DataMember(Name = "CursorLinesStopBorderColor")]
        [Category("Cursor Lines"), DisplayName("Stop Border Color")]
        public XColor CursorLinesStopBorderColor
        {
            get => _cursorLinesStopBorderColor;
            set
            {
                if (value == _cursorLinesStopBorderColor)
                {
                    return;
                }

                _cursorLinesStopBorderColor = value;

                _cursorLinesStopBorderBrush = new XBrush(_cursorLinesStopBorderColor);
                _cursorLinesStopBorderPen = new XPen(
                    _cursorLinesStopBorderBrush,
                    CursorLinesStopBorderThickness,
                    CursorLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _cursorLinesStopBorderThickness;

        [DataMember(Name = "CursorLinesStopBorderThickness")]
        [Category("Cursor Lines"), DisplayName("Stop Border Thickness")]
        public int CursorLinesStopBorderThickness
        {
            get => _cursorLinesStopBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _cursorLinesStopBorderThickness)
                {
                    return;
                }

                _cursorLinesStopBorderThickness = value;

                _cursorLinesStopBorderPen = new XPen(
                    _cursorLinesStopBorderBrush,
                    _cursorLinesStopBorderThickness,
                    CursorLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _cursorLinesStopBorderStyle;

        [DataMember(Name = "CursorLinesStopBorderStyle")]
        [Category("Cursor Lines"), DisplayName("Stop Border Style")]
        public XDashStyle CursorLinesStopBorderStyle
        {
            get => _cursorLinesStopBorderStyle;
            set
            {
                if (value == _cursorLinesStopBorderStyle)
                {
                    return;
                }

                _cursorLinesStopBorderStyle = value;

                _cursorLinesStopBorderPen = new XPen(
                    _cursorLinesStopBorderBrush,
                    CursorLinesStopBorderThickness,
                    _cursorLinesStopBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XBrush _cursorLinesProfitsBorderBrush;

        private XPen _cursorLinesProfitsBorderPen;

        private XColor _cursorLinesProfitsBorderColor;

        [DataMember(Name = "CursorLinesProfitsBorderColor")]
        [Category("Cursor Lines"), DisplayName("Profits Border Color")]
        public XColor CursorLinesProfitsBorderColor
        {
            get => _cursorLinesProfitsBorderColor;
            set
            {
                if (value == _cursorLinesProfitsBorderColor)
                {
                    return;
                }

                _cursorLinesProfitsBorderColor = value;

                _cursorLinesProfitsBorderBrush = new XBrush(_cursorLinesProfitsBorderColor);
                _cursorLinesProfitsBorderPen = new XPen(
                    _cursorLinesProfitsBorderBrush,
                    CursorLinesProfitsBorderThickness,
                    CursorLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private int _cursorLinesProfitsBorderThickness;

        [DataMember(Name = "CursorLinesProfitsBorderThickness")]
        [Category("Cursor Lines"), DisplayName("Profits Border Thickness")]
        public int CursorLinesProfitsBorderThickness
        {
            get => _cursorLinesProfitsBorderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _cursorLinesProfitsBorderThickness)
                {
                    return;
                }

                _cursorLinesProfitsBorderThickness = value;

                _cursorLinesProfitsBorderPen = new XPen(
                    _cursorLinesProfitsBorderBrush,
                    _cursorLinesProfitsBorderThickness,
                    CursorLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private XDashStyle _cursorLinesProfitsBorderStyle;

        [DataMember(Name = "CursorLinesProfitsBorderStyle")]
        [Category("Cursor Lines"), DisplayName("Profits Border Style")]
        public XDashStyle CursorLinesProfitsBorderStyle
        {
            get => _cursorLinesProfitsBorderStyle;
            set
            {
                if (value == _cursorLinesProfitsBorderStyle)
                {
                    return;
                }

                _cursorLinesProfitsBorderStyle = value;

                _cursorLinesProfitsBorderPen = new XPen(
                    _cursorLinesProfitsBorderBrush,
                    CursorLinesProfitsBorderThickness,
                    _cursorLinesProfitsBorderStyle
                );

                OnPropertyChanged();
            }
        }

        private ChartAlertSettings _alert;

        [DataMember(Name = "Alert"), Browsable(true)]
        [Category("Alert"), DisplayName("Alert")]
        public ChartAlertSettings Alert
        {
            get => _alert ?? (_alert = new ChartAlertSettings());
            set
            {
                if (Equals(value, _alert))
                {
                    return;
                }

                _alert = value;

                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public override bool ShowIndicatorTitle => false;

        [Browsable(false)]
        public override bool ShowIndicatorValues => true;

        [Browsable(false)]
        public override bool ShowIndicatorLabels => true;

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnPriceChange;

        private int _lastIndex = 0;
        private Position _position = new Position();

        private class Position
        {
            private double Size;
            private double Value;
            private double Precision;
            private double TickSize;
            private double LastPrice;

            public Position()
            {
                Size = 0;
                Value = 0;
                Precision = 0;
                TickSize = 0;
                LastPrice = 0;
            }

            public bool IsOpen()
            {
                return Size != 0;
            }

            public bool IsLong()
            {
                return Size >= 0;
            }

            public double GetSize()
            {
                return Size;
            }

            public double GetValue()
            {
                return Value;
            }

            public void Buy(double size, double price)
            {
                Size = Math.Round((Size + size) / Precision) * Precision;
                Value += size * price;
                CheckReset();
            }

            public void Sell(double size, double price)
            {
                Size = Math.Round((Size - size) / Precision) * Precision;
                Value -= size * price;
                CheckReset();
            }

            private void CheckReset()
            {
                if (Size == 0)
                {
                    Size = 0;
                    Value = 0;
                }
            }

            public double GetAvgPrice()
            {
                if (Size == 0)
                    return 0;

                return Value / Size;
            }

            public double GetPoints()
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                if (IsLong())
                {
                    return Math.Round((LastPrice - GetAvgPrice()) / TickSize) * TickSize;
                }
                else
                {
                    return Math.Round((GetAvgPrice() - LastPrice) / TickSize) * TickSize;
                }
            }

            public long GetTicks()
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                if (IsLong())
                {
                    return (long)Math.Round((LastPrice - GetAvgPrice()) / TickSize);
                }
                else
                {
                    return (long)Math.Round((GetAvgPrice() - LastPrice) / TickSize);
                }
            }

            public double GetPnL()
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                if (IsLong())
                {
                    return Size * (LastPrice - GetAvgPrice());
                }
                else
                {
                    return -Size * (GetAvgPrice() - LastPrice);
                }
            }

            public double GetPnLPercent()
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                if (IsLong())
                {
                    return ((LastPrice - GetAvgPrice()) / GetAvgPrice()) * 100;
                }
                else
                {
                    return ((GetAvgPrice() - LastPrice) / GetAvgPrice()) * 100;
                }
            }

            public double GetRiskReward(double stopTicks)
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                return GetTicks() / stopTicks;
            }

            public double GetRiskRewardPercent(double stopPercent)
            {
                if (Size == 0)
                    return 0;
                if (LastPrice == 0)
                    return 0;

                return GetPnLPercent() / stopPercent;
            }

            public bool AreConstantsSet()
            {
                return Precision != 0 && TickSize != 0;
            }

            public double GetPrecision()
            {
                return Precision;
            }

            public double GetTickSize()
            {
                return TickSize;
            }

            public double GetLastPrice()
            {
                return LastPrice;
            }

            public void SetPrecision(double precision)
            {
                Precision = precision;
            }

            public void SetTickSize(double tickSize)
            {
                TickSize = tickSize;
            }

            public void SetLastPrice(double price)
            {
                LastPrice = price;
            }

            public void Clear()
            {
                Size = 0;
                Value = 0;
                Precision = 0;
                TickSize = 0;
                LastPrice = 0;
            }
        }

        public Trading()
        {
            FontSize = 14;
            BaseType = Type.Percent;
            StopValue = 0.5;
            Profits = 3;
            ProfitsMultiplier = 10;

            ShowRisk = true;
            RiskBalance = 10_000;
            RiskPercentValue = 1;

            ShowPositionBox = true;
            PositionBoxHorizontalMargin = 15;
            PositionBoxVerticalMargin = 30;
            PositionBoxHorizontalPadding = 8;
            PositionBoxVerticalPadding = 8;
            PositionBoxBackgroundColor = new XColor(255, 20, 20, 20);
            PositionBoxBorderColor = new XColor(255, 40, 40, 40);
            PositionBoxBorderThickness = 1;
            PositionBoxBorderStyle = XDashStyle.Solid;
            PositionBoxNeutralTextColor = Colors.Silver;
            PositionBoxPositiveTextColor = Colors.SeaGreen;
            PositionBoxNegativeTextColor = Colors.Firebrick;
            PositionBoxQtyValueTextColor = Colors.DimGray;

            PositionLinesMargin = 60;
            ShowPositionLinesEntry = true;
            ShowPositionLinesStop = true;
            ShowPositionLinesProfits = true;
            ShowPositionLinesEntryMarker = true;
            ShowPositionLinesStopMarker = true;
            ShowPositionLinesProfitsMarkers = true;
            PositionLinesEntryBorderColor = Colors.RoyalBlue;
            PositionLinesEntryBorderThickness = 1;
            PositionLinesEntryBorderStyle = XDashStyle.Solid;
            PositionLinesStopBorderColor = Colors.Firebrick;
            PositionLinesStopBorderThickness = 1;
            PositionLinesStopBorderStyle = XDashStyle.Solid;
            PositionLinesProfitsBorderColor = Colors.SeaGreen;
            PositionLinesProfitsBorderThickness = 1;
            PositionLinesProfitsBorderStyle = XDashStyle.Solid;
            PositionLinesTextBackgroundColor = Colors.Black;

            CursorLinesActivationKey = CustomKey.S;
            CursorLinesOppositeKey = CustomModifierKeys.Shift;
            ShowCursorLinesStop = true;
            ShowCursorLinesProfits = true;
            CursorLinesStopBorderColor = Colors.Tomato;
            CursorLinesStopBorderThickness = 1;
            CursorLinesStopBorderStyle = XDashStyle.Dash;
            CursorLinesProfitsBorderColor = Colors.CornflowerBlue;
            CursorLinesProfitsBorderThickness = 1;
            CursorLinesProfitsBorderStyle = XDashStyle.Solid;
        }

        private void Clear()
        {
            _lastIndex = 0;
            _position?.Clear();
        }

        protected override void Execute()
        {
            if (ClearData)
            {
                Clear();
            }

            if (_position == null)
                _position = new Position();

            if (!_position.AreConstantsSet())
            {
                _position.SetPrecision((double)DataProvider.Symbol.GetSize(1));
                _position.SetTickSize(DataProvider.Step / DataProvider.Scale);
            }

            if (DataProvider.Count > 0)
            {
                _position.SetLastPrice(
                    (double)DataProvider.GetCluster(DataProvider.Count - 1).Close
                        * DataProvider.Scale
                );
            }

            RefreshPosition();

            if (_position.IsOpen())
            {
                bool isLong = _position.IsLong();
                double positionPrice = _position.GetAvgPrice();

                double stopPrice = GetStopPrice(isLong, positionPrice);
                double[] profitPrices = GetProfitPrices(isLong, positionPrice);

                int count = 0;
                if (ShowPositionLinesEntryMarker)
                    count++;
                if (ShowPositionLinesStopMarker)
                    count++;
                if (ShowPositionLinesProfitsMarkers)
                    count += Profits;
                if (count == 0)
                    return;

                IndicatorSeriesData[] serieses = new IndicatorSeriesData[count];
                int index = 0;

                if (ShowPositionLinesEntryMarker)
                {
                    serieses[index] = new IndicatorSeriesData(new double[] { positionPrice })
                    {
                        Style =
                        {
                            DisableSelect = true,
                            DisableMinMax = true,
                            DisableValue = true,
                            Color = PositionLinesEntryBorderColor,
                        },
                    };
                    index++;
                }

                if (ShowPositionLinesStopMarker)
                {
                    serieses[index] = new IndicatorSeriesData(new double[] { stopPrice })
                    {
                        Style =
                        {
                            DisableSelect = true,
                            DisableMinMax = true,
                            DisableValue = true,
                            Color = PositionLinesStopBorderColor,
                        },
                    };
                    index++;
                }

                if (ShowPositionLinesProfitsMarkers)
                {
                    for (int i = 0; i < Profits; i++)
                    {
                        serieses[index] = new IndicatorSeriesData(new double[] { profitPrices[i] })
                        {
                            Style =
                            {
                                DisableSelect = true,
                                DisableMinMax = true,
                                DisableValue = true,
                                Color = PositionLinesProfitsBorderColor,
                            },
                        };
                        index++;
                    }
                }

                Series.Add(serieses);
            }
        }

        private void RefreshPosition()
        {
            List<IChartExecution> executions = DataProvider.GetExecutions();
            if (_lastIndex == executions.Count)
                return;

            List<IChartDeal> deals = DataProvider.GetDeals();
            IChartDeal lastDeal = deals.Count > 0 ? deals[deals.Count - 1] : null;

            IChartSymbol symbol = DataProvider.Symbol;
            for (int i = _lastIndex; i < executions.Count; i++)
            {
                IChartExecution execution = executions[i];
                if (_lastIndex == 0 && lastDeal != null)
                {
                    if (execution.Time < lastDeal.CloseTime)
                        continue;
                    else if (
                        execution.Time == lastDeal.CloseTime
                        && lastDeal.IsBuy != execution.IsBuy
                    )
                        continue;
                }

                if (execution.IsBuy)
                {
                    _position.Buy((double)symbol.GetSize(execution.Quantity), execution.Price);
                }
                else
                {
                    _position.Sell((double)symbol.GetSize(execution.Quantity), execution.Price);
                }
            }

            _lastIndex = executions.Count;
        }

        private double GetStopPrice(bool isLong, double price)
        {
            if (isLong)
            {
                if (BaseType == Type.Percent)
                {
                    // Percent
                    return price - (price * StopValue / 100);
                }
                else
                {
                    // Ticks
                    return price - StopValue * DataProvider.Step / DataProvider.Scale;
                }
            }
            else
            {
                if (BaseType == Type.Percent)
                {
                    // Percent
                    return price + (price * StopValue / 100);
                }
                else
                {
                    // Ticks
                    return price + StopValue * DataProvider.Step / DataProvider.Scale;
                }
            }
        }

        private double[] GetProfitPrices(bool isLong, double price)
        {
            double[] prices = new double[Profits];
            double multiplier = StopValue * ProfitsMultiplier;
            for (int i = 0; i < Profits; i++)
            {
                if (isLong)
                {
                    if (BaseType == Type.Percent)
                    {
                        // Percent
                        prices[i] = price + (price * multiplier * (i + 1) / 100);
                    }
                    else
                    {
                        // Ticks
                        prices[i] =
                            price + multiplier * (i + 1) * DataProvider.Step / DataProvider.Scale;
                    }
                }
                else
                {
                    if (BaseType == Type.Percent)
                    {
                        // Percent
                        prices[i] = price - (price * multiplier * (i + 1) / 100);
                    }
                    else
                    {
                        // Ticks
                        prices[i] =
                            price - multiplier * (i + 1) * DataProvider.Step / DataProvider.Scale;
                    }
                }
            }
            return prices;
        }

        private double GetStopPointsFromPercent(double stopPercent, double currentPrice)
        {
            return currentPrice * stopPercent / 100;
        }

        private double GetStopPointsFromTicks(double stopTicks, double ticksize)
        {
            return stopTicks * ticksize;
        }

        private (
            double tradeSize,
            double tradeValue,
            double tradeLeverage,
            double stopValue
        ) GetRisk(double balance, double riskPercent, double stopPoints, double currentPrice)
        {
            double riskValue = balance * riskPercent / 100;
            double tradeSize = riskValue / stopPoints;
            double tradeValue = tradeSize * currentPrice;

            return (tradeSize, tradeValue, tradeValue / balance, riskValue);
        }

        public override void Render(DxVisualQueue visual)
        {
            base.Render(visual);

            // NOTE: This is a workaround to avoid waiting for a price/tick update to get new position info.
            if (_position != null && DataProvider != null)
                RefreshPosition();

            RenderPositionGrid(visual);
            RenderPositionSummary(visual);
        }

        private void RenderPositionGrid(DxVisualQueue visual)
        {
            if (!_position.IsOpen())
                return;

            bool isLong = _position.IsLong();
            double positionPrice = _position.GetAvgPrice();
            double stopPrice = GetStopPrice(isLong, positionPrice);
            double[] profitPrices = GetProfitPrices(isLong, positionPrice);

            double startX =
                Canvas.Count > 0
                    ? GetX(Canvas.GetIndex(0)) + PositionLinesMargin
                    : PositionLinesMargin;
            double endX = Canvas.Rect.Right;

            XFont font = new XFont(Canvas.ChartFont.Name, FontSize);
            if (ShowPositionLinesEntry)
                RenderLine(
                    visual,
                    startX,
                    endX,
                    positionPrice,
                    "E",
                    true,
                    0,
                    4,
                    font,
                    _positionLinesEntryBorderBrush,
                    _positionLinesEntryBorderPen
                );
            if (ShowPositionLinesStop)
                RenderLine(
                    visual,
                    startX,
                    endX,
                    stopPrice,
                    "-1",
                    true,
                    0,
                    4,
                    font,
                    _positionLinesStopBorderBrush,
                    _positionLinesStopBorderPen
                );
            if (ShowPositionLinesProfits)
            {
                for (int i = 0; i < profitPrices.Length; i++)
                {
                    double multiplier = ProfitsMultiplier * (i + 1);
                    RenderLine(
                        visual,
                        startX,
                        endX,
                        profitPrices[i],
                        $"{multiplier}",
                        true,
                        0,
                        4,
                        font,
                        _positionLinesProfitsBorderBrush,
                        _positionLinesProfitsBorderPen
                    );
                }
            }
        }

        private void RenderPositionSummary(DxVisualQueue visual)
        {
            if (!ShowPositionBox && !ShowRisk)
                return;
            if (!_position.IsOpen() && !ShowRisk)
                return;

            BetterDraw betterDraw = new BetterDraw(visual);

            bool isLong = _position.IsLong();
            double positionSize = _position.GetSize();
            double positionValue = _position.GetValue();
            long positionTicks = _position.GetTicks();
            double positionPnL = _position.GetPnL();
            double positionPnLPercent = _position.GetPnLPercent();
            double positionRiskReward =
                BaseType == Type.Percent
                    ? _position.GetRiskRewardPercent(StopValue)
                    : _position.GetRiskReward(StopValue);
            (double riskSize, double riskValue, double riskLeverage, double stopValue) = GetRisk(
                RiskBalance,
                RiskPercentValue,
                BaseType == Type.Percent
                    ? GetStopPointsFromPercent(StopValue, _position.GetLastPrice())
                    : GetStopPointsFromTicks(StopValue, _position.GetTickSize()),
                _position.GetLastPrice()
            );
            riskSize = Math.Round(riskSize / _position.GetPrecision()) * _position.GetPrecision();
            bool isPositionRisky = positionSize > riskSize;

            double cornerRadius = 5;
            double labelSpacing = 3;
            double textSpacing = 8;
            double textGap1 = 10;
            double textGap2 = 15;
            double textGap3 = 6;
            double textGap4 = 16;
            double textGap5 = 10;
            double textGap6 = 10;
            double textGap7 = 10;

            XFont font = new XFont(Canvas.ChartFont.Name, FontSize);
            string textSide = isLong ? "Long ↑" : "Short ↓";
            string textSizeLabel = $"Qty";
            string textSize = $"{Math.Abs(positionSize)}";
            string textValueLabel = $"Value";
            string textValue = $"$ {Math.Abs(positionValue):#,##0.##}";
            string textPnL = positionPnL >= 0 ? $"$ +{positionPnL:N2}" : $"$ {positionPnL:N2}";
            string textPnLPercent =
                positionPnLPercent >= 0
                    ? $"+{positionPnLPercent:N2}%"
                    : $"{positionPnLPercent:N2}%";
            string textR =
                positionRiskReward >= 0
                    ? $"+{positionRiskReward:N2}r"
                    : $"{positionRiskReward:N2}r";
            string textTicks =
                positionTicks >= 0 ? $"T +{positionTicks:N0}" : $"T {positionTicks:N0}";

            string textRiskSizeLabel = $"Trade Qty";
            string textRiskSize = $"{riskSize}";
            string textRiskValueLabel = $"Trade Value";
            string textRiskValue = $"$ {riskValue:#,##0.00}";
            string textRiskLeverageLabel = $"Leverage";
            string textRiskLeverage = $"{riskLeverage:N2}x";
            string textRiskStopValueLabel = $"Stop Value";
            string textRiskStopValue = $"$ {stopValue:#,##0.##}";

            double widthText1 =
                font.GetSize(textSizeLabel).Width
                + labelSpacing
                + font.GetSize(textSize).Width
                + textSpacing
                + font.GetSize(textSide).Width;
            double widthText2 =
                font.GetSize(textValueLabel).Width + labelSpacing + font.GetSize(textValue).Width;
            double widthText3 =
                font.GetSize(textPnL).Width + textSpacing + font.GetSize(textPnLPercent).Width;
            double widthText4 =
                font.GetSize(textTicks).Width + textSpacing + font.GetSize(textR).Width;
            double widthText5 =
                font.GetSize(textRiskSizeLabel).Width
                + labelSpacing
                + font.GetSize(textRiskSize).Width;
            double widthText6 =
                font.GetSize(textRiskValueLabel).Width
                + labelSpacing
                + font.GetSize(textRiskValue).Width;
            double widthText7 =
                font.GetSize(textRiskLeverageLabel).Width
                + labelSpacing
                + font.GetSize(textRiskLeverage).Width;
            double widthText8 =
                font.GetSize(textRiskStopValueLabel).Width
                + labelSpacing
                + font.GetSize(textRiskStopValue).Width;

            int rows = 0;
            double heightGaps = 0.0;
            double maxWidth = 0.0;

            if (ShowPositionBox && _position.IsOpen())
            {
                maxWidth = Math.Max(
                    maxWidth,
                    Math.Max(widthText1, Math.Max(widthText2, Math.Max(widthText3, widthText4)))
                );
                rows += 4;
                heightGaps += textGap1 + textGap2 + textGap3;
            }

            if (ShowRisk)
            {
                maxWidth = Math.Max(
                    maxWidth,
                    Math.Max(widthText5, Math.Max(widthText6, widthText7))
                );
                rows += 4;
                if (rows == 4)
                    heightGaps += textGap5 + textGap6 + textGap7;
                else
                    heightGaps += textGap4 + textGap5 + textGap6 + textGap7;
            }

            // Skip empty box
            if (rows == 0)
                return;

            Rect boxRect = new Rect(
                PositionBoxHorizontalMargin,
                PositionBoxVerticalMargin,
                maxWidth + PositionBoxHorizontalPadding * 2 - 1,
                font.GetHeight() * rows + PositionBoxVerticalPadding * 2 + heightGaps
            );
            visual.FillRoundedRectangle(
                _positionBoxBackgroundBrush,
                boxRect,
                new Point(cornerRadius, cornerRadius)
            );
            visual.DrawRoundedRectangle(
                _positionBoxBorderPen,
                boxRect,
                new Point(cornerRadius, cornerRadius)
            );

            double x = boxRect.X + PositionBoxHorizontalPadding;
            double y = boxRect.Y + PositionBoxVerticalPadding;

            if (ShowPositionBox && _position.IsOpen())
            {
                // x += (maxWidth - widthText1) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textSizeLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (
                            textSize,
                            isPositionRisky
                                ? _positionBoxNegativeTextBrush
                                : _positionBoxNeutralTextBrush,
                            0,
                            0
                        ),
                    },
                    x,
                    y
                );
                betterDraw.DrawString(
                    textSide,
                    font,
                    isLong ? _positionBoxPositiveTextBrush : _positionBoxNegativeTextBrush,
                    boxRect.Right - PositionBoxHorizontalPadding - font.GetSize(textSide).Width,
                    y
                );

                y += font.GetHeight() + textGap1;
                x = boxRect.X + PositionBoxHorizontalPadding;
                // x += (maxWidth - widthText2) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textValueLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (
                            textValue,
                            isPositionRisky
                                ? _positionBoxNegativeTextBrush
                                : _positionBoxNeutralTextBrush,
                            0,
                            0
                        ),
                    },
                    x,
                    y
                );

                y += font.GetHeight() + textGap2;
                x = boxRect.X + PositionBoxHorizontalPadding;
                x += (maxWidth - widthText3) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground)[]
                    {
                        (
                            textPnL,
                            positionPnL > 0 ? _positionBoxPositiveTextBrush
                            : positionPnL < 0 ? _positionBoxNegativeTextBrush
                            : _positionBoxNeutralTextBrush
                        ),
                        (
                            textPnLPercent,
                            positionPnLPercent > 0 ? _positionBoxPositiveTextBrush
                            : positionPnLPercent < 0 ? _positionBoxNegativeTextBrush
                            : _positionBoxNeutralTextBrush
                        ),
                    },
                    x,
                    y,
                    textSpacing
                );

                y += font.GetHeight() + textGap3;
                x = boxRect.X + PositionBoxHorizontalPadding;
                x += (maxWidth - widthText4) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground)[]
                    {
                        (
                            textTicks,
                            positionTicks > 0 ? _positionBoxPositiveTextBrush
                            : positionTicks < 0 ? _positionBoxNegativeTextBrush
                            : _positionBoxNeutralTextBrush
                        ),
                        (
                            textR,
                            positionRiskReward > 0 ? _positionBoxPositiveTextBrush
                            : positionRiskReward < 0 ? _positionBoxNegativeTextBrush
                            : _positionBoxNeutralTextBrush
                        ),
                    },
                    x,
                    y,
                    textSpacing
                );
            }

            if (ShowRisk)
            {
                if (rows != 4)
                    visual.DrawLine(
                        _positionBoxBorderPen,
                        new Point(boxRect.X, y + font.GetHeight() + textGap4 / 2),
                        new Point(boxRect.Right, y + font.GetHeight() + textGap4 / 2)
                    );

                if (rows != 4)
                    y += font.GetHeight() + textGap4;
                x = boxRect.X + PositionBoxHorizontalPadding;
                // x += (maxWidth - widthText5) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textRiskSizeLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (textRiskSize, _positionBoxNeutralTextBrush, 0, 0),
                    },
                    x,
                    y
                );

                y += font.GetHeight() + textGap5;
                x = boxRect.X + PositionBoxHorizontalPadding;
                // x += (maxWidth - widthText6) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textRiskValueLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (textRiskValue, _positionBoxNeutralTextBrush, 0, 0),
                    },
                    x,
                    y
                );

                y += font.GetHeight() + textGap6;
                x = boxRect.X + PositionBoxHorizontalPadding;
                // x += (maxWidth - widthText7) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textRiskLeverageLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (textRiskLeverage, _positionBoxNeutralTextBrush, 0, 0),
                    },
                    x,
                    y
                );

                y += font.GetHeight() + textGap7;
                x = boxRect.X + PositionBoxHorizontalPadding;
                // x += (maxWidth - widthText8) / 2;
                betterDraw.DrawString(
                    font,
                    new (string text, XBrush foreground, double marginLeft, double marginRight)[]
                    {
                        (textRiskStopValueLabel, _positionBoxQtyValueTextBrush, 0, labelSpacing),
                        (textRiskStopValue, _positionBoxNeutralTextBrush, 0, 0),
                    },
                    x,
                    y
                );
            }
        }

        private void RenderLine(
            DxVisualQueue visual,
            double x1,
            double x2,
            double price,
            string text,
            bool alignLeft,
            double textMargin,
            double paddingX,
            XFont font,
            XBrush foreground,
            XPen pen
        )
        {
            double y = GetY(price);
            visual.DrawLine(pen, new Point(x1, y), new Point(x2, y));

            Size size = font.GetSize(text);
            Rect rect = new Rect(
                alignLeft ? x1 - paddingX - textMargin : x2 - size.Width - paddingX - textMargin,
                y - size.Height / 2,
                size.Width + paddingX * 2,
                size.Height
            );
            visual.FillRectangle(_positionLinesTextBackgroundBrush, rect);
            visual.DrawString(text, font, foreground, rect, XTextAlignment.Center);
        }

        private bool _isHolding = false;
        private bool _isCursorActive = false;

        public override void RenderCursor(
            DxVisualQueue visual,
            int cursorPos,
            Point cursorCenter,
            ref int topPos
        )
        {
            if (CursorLinesActivationKey == CustomKey.None)
                return;

            if (Keyboard.IsKeyDown((Key)CursorLinesActivationKey))
            {
                if (!_isHolding)
                {
                    _isCursorActive = !_isCursorActive;
                    _isHolding = true;
                }
            }
            else
                _isHolding = false;

            if (!_isCursorActive)
                return;

            double lastPrice = _position.GetLastPrice();
            double cursorPrice = Canvas.GetValue(cursorCenter.Y);
            bool isLong = cursorPrice < lastPrice;

            if (CursorLinesOppositeKey != CustomModifierKeys.None)
            {
                if (Keyboard.Modifiers.HasFlag((ModifierKeys)CursorLinesOppositeKey))
                    isLong = !isLong;
            }

            XFont font = new XFont(Canvas.ChartFont.Name, FontSize);

            double stopPrice = GetStopPrice(isLong, cursorPrice);
            double stopY = GetY(stopPrice);
            // if (ShowCursorLinesStop) visual.DrawLine(_cursorLinesStopBorderPen, new Point(0, stopY), new Point(Canvas.Rect.Right, stopY));
            if (ShowCursorLinesStop)
                RenderLine(
                    visual,
                    0,
                    Canvas.Rect.Right,
                    stopPrice,
                    "-1",
                    false,
                    12,
                    4,
                    font,
                    _cursorLinesStopBorderBrush,
                    _cursorLinesStopBorderPen
                );

            if (ShowCursorLinesProfits)
            {
                double[] profitPrices = GetProfitPrices(isLong, cursorPrice);
                for (int i = 0; i < profitPrices.Length; i++)
                {
                    double profitPrice = profitPrices[i];
                    // double profitY = GetY(profitPrice);
                    // visual.DrawLine(_cursorLinesProfitsBorderPen, new Point(0, profitY), new Point(Canvas.Rect.Right, profitY));
                    RenderLine(
                        visual,
                        0,
                        Canvas.Rect.Right,
                        profitPrice,
                        $"{ProfitsMultiplier * (i + 1)}",
                        false,
                        12,
                        4,
                        font,
                        _cursorLinesProfitsBorderBrush,
                        _cursorLinesProfitsBorderPen
                    );
                }
            }
        }

        public override IndicatorTitleInfo GetTitle()
        {
            return new IndicatorTitleInfo(Title, _positionBoxNeutralTextBrush);
        }

        public override void ApplyColors(IChartTheme palette)
        {
            base.ApplyColors(palette);

            PositionLinesTextBackgroundColor = palette.ChartBackColor;
        }

        public override void CopyTemplate(IndicatorBase indicator, bool style)
        {
            Trading i = (Trading)indicator;

            Alert.Copy(i.Alert, !style);
            OnPropertyChanged(nameof(Alert));

            FontSize = i.FontSize;
            BaseType = i.BaseType;
            StopValue = i.StopValue;
            Profits = i.Profits;
            ProfitsMultiplier = i.ProfitsMultiplier;

            ShowRisk = i.ShowRisk;
            RiskBalance = i.RiskBalance;
            RiskPercentValue = i.RiskPercentValue;

            ShowPositionBox = i.ShowPositionBox;
            PositionBoxHorizontalMargin = i.PositionBoxHorizontalMargin;
            PositionBoxVerticalMargin = i.PositionBoxVerticalMargin;
            PositionBoxHorizontalPadding = i.PositionBoxHorizontalPadding;
            PositionBoxVerticalPadding = i.PositionBoxVerticalPadding;
            PositionBoxBackgroundColor = i.PositionBoxBackgroundColor;
            PositionBoxBorderColor = i.PositionBoxBorderColor;
            PositionBoxBorderThickness = i.PositionBoxBorderThickness;
            PositionBoxBorderStyle = i.PositionBoxBorderStyle;
            PositionBoxNeutralTextColor = i.PositionBoxNeutralTextColor;
            PositionBoxPositiveTextColor = i.PositionBoxPositiveTextColor;
            PositionBoxNegativeTextColor = i.PositionBoxNegativeTextColor;
            PositionBoxQtyValueTextColor = i.PositionBoxQtyValueTextColor;

            PositionLinesMargin = i.PositionLinesMargin;
            ShowPositionLinesEntry = i.ShowPositionLinesEntry;
            ShowPositionLinesStop = i.ShowPositionLinesStop;
            ShowPositionLinesProfits = i.ShowPositionLinesProfits;
            ShowPositionLinesEntryMarker = i.ShowPositionLinesEntryMarker;
            ShowPositionLinesStopMarker = i.ShowPositionLinesStopMarker;
            ShowPositionLinesProfitsMarkers = i.ShowPositionLinesProfitsMarkers;
            PositionLinesEntryBorderColor = i.PositionLinesEntryBorderColor;
            PositionLinesEntryBorderThickness = i.PositionLinesEntryBorderThickness;
            PositionLinesEntryBorderStyle = i.PositionLinesEntryBorderStyle;
            PositionLinesStopBorderColor = i.PositionLinesStopBorderColor;
            PositionLinesStopBorderThickness = i.PositionLinesStopBorderThickness;
            PositionLinesStopBorderStyle = i.PositionLinesStopBorderStyle;
            PositionLinesProfitsBorderColor = i.PositionLinesProfitsBorderColor;
            PositionLinesProfitsBorderThickness = i.PositionLinesProfitsBorderThickness;
            PositionLinesProfitsBorderStyle = i.PositionLinesProfitsBorderStyle;
            PositionLinesTextBackgroundColor = i.PositionLinesTextBackgroundColor;

            CursorLinesActivationKey = i.CursorLinesActivationKey;
            CursorLinesOppositeKey = i.CursorLinesOppositeKey;
            ShowCursorLinesStop = i.ShowCursorLinesStop;
            ShowCursorLinesProfits = i.ShowCursorLinesProfits;
            CursorLinesStopBorderColor = i.CursorLinesStopBorderColor;
            CursorLinesStopBorderThickness = i.CursorLinesStopBorderThickness;
            CursorLinesStopBorderStyle = i.CursorLinesStopBorderStyle;
            CursorLinesProfitsBorderColor = i.CursorLinesProfitsBorderColor;
            CursorLinesProfitsBorderThickness = i.CursorLinesProfitsBorderThickness;
            CursorLinesProfitsBorderStyle = i.CursorLinesProfitsBorderStyle;

            base.CopyTemplate(indicator, style);
        }

        public override string ToString()
        {
            return $"*Trading++";
        }
    }
}
