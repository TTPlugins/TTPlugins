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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;
using CustomCommon.Helpers;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace ObjectsPlusPlus.Rectangle
{
    [ReadOnly(true)]
    [DataContract(Name = "TextOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class TextOptions : INotifyPropertyChanged, DeepCopy<TextOptions>
    {
        private double _fontSize;

        [DataMember(Name = "FontSize")]
        [DisplayName("Font Size")]
        public double FontSize
        {
            get => _fontSize;
            set
            {
                value = Math.Max(1, value);

                if (value == _fontSize)
                    return;

                _fontSize = value;

                OnPropertyChanged(nameof(FontSize));
            }
        }

        [Browsable(false)]
        public XBrush ForegroundBrush;

        private XColor _foregroundColor;

        [DataMember(Name = "ForegroundColor")]
        [DisplayName("Text Color")]
        public XColor ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                if (value == _foregroundColor)
                    return;

                _foregroundColor = value;
                ForegroundBrush = new XBrush(_foregroundColor);

                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        [Browsable(false)]
        public XBrush BackgroundBrush;

        private XColor _backgroundColor;

        [DataMember(Name = "BackgroundColor")]
        [DisplayName("Background")]
        public XColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (value == _backgroundColor)
                    return;

                _backgroundColor = value;
                BackgroundBrush = new XBrush(_backgroundColor);

                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        [Browsable(false)]
        public XPen BorderPen;

        [Browsable(false)]
        public XBrush BorderBrush;

        private XColor _borderColor;

        [DataMember(Name = "BorderColor")]
        [DisplayName("Border")]
        public XColor BorderColor
        {
            get => _borderColor;
            set
            {
                if (value == _borderColor)
                    return;

                _borderColor = value;
                BorderBrush = new XBrush(_borderColor);
                BorderPen = new XPen(BorderBrush, BorderThickness, BorderStyle);

                OnPropertyChanged(nameof(BorderColor));
            }
        }

        private int _borderThickness;

        [DataMember(Name = "BorderThickness")]
        [DisplayName("Border Thickness")]
        public int BorderThickness
        {
            get => _borderThickness;
            set
            {
                value = Math.Max(0, value);

                if (value == _borderThickness)
                    return;

                _borderThickness = value;
                BorderPen = new XPen(BorderBrush, _borderThickness, BorderStyle);

                OnPropertyChanged(nameof(BorderThickness));
            }
        }

        private XDashStyle _borderStyle;

        [DataMember(Name = "BorderStyle")]
        [DisplayName("Border Style")]
        public XDashStyle BorderStyle
        {
            get => _borderStyle;
            set
            {
                if (value == _borderStyle)
                {
                    return;
                }

                _borderStyle = value;
                BorderPen = new XPen(BorderBrush, BorderThickness, _borderStyle);

                OnPropertyChanged(nameof(BorderStyle));
            }
        }

        public TextOptions()
        {
            FontSize = 12;
            ForegroundColor = Colors.Red;
            BackgroundColor = Colors.Transparent;
            BorderColor = Colors.Transparent;
            BorderThickness = 1;
            BorderStyle = XDashStyle.Solid;
        }

        public TextOptions DeepCopy()
        {
            return new TextOptions
            {
                FontSize = FontSize,
                ForegroundColor = ForegroundColor,
                BackgroundColor = BackgroundColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                BorderStyle = BorderStyle,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return "";
        }
    }
}
