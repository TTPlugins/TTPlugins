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
using TigerTrade.Core.UI.Converters;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace IndicatorsPlusPlus.Delta
{
    [ReadOnly(true)]
    [DataContract(Name = "Palette")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class Palette : INotifyPropertyChanged, DeepCopy<Palette>
    {
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        [DataContract(
            Name = "PaletteTextAlignment",
            Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
        )]
        public enum PaletteTextAlignment
        {
            [EnumMember(Value = "Left"), Description("Left")]
            Left,

            [EnumMember(Value = "Right"), Description("Right")]
            Right,
        }

        private string _name;

        [DataMember(Name = "Name")]
        [DisplayName("Name")]
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(Name));
            }
        }

        [Browsable(false)]
        public XBrush AskBackgroundBrush;

        private XColor _askBackgroundColor;

        [DataMember(Name = "AskBackgroundColor")]
        [DisplayName("Ask Background")]
        public XColor AskBackgroundColor
        {
            get => _askBackgroundColor;
            set
            {
                if (value == _askBackgroundColor)
                {
                    return;
                }

                _askBackgroundColor = value;
                AskBackgroundBrush = new XBrush(_askBackgroundColor);

                RecalculateTiers();
                OnPropertyChanged(nameof(AskBackgroundColor));
            }
        }

        [Browsable(false)]
        public XPen AskBorderPen;

        [Browsable(false)]
        public XBrush AskBorderBrush;

        private XColor _askBorderColor;

        [DataMember(Name = "AskBorderColor")]
        [DisplayName("Ask Border")]
        public XColor AskBorderColor
        {
            get => _askBorderColor;
            set
            {
                if (value == _askBorderColor)
                {
                    return;
                }

                _askBorderColor = value;
                AskBorderBrush = new XBrush(_askBorderColor);
                AskBorderPen = new XPen(AskBorderBrush, BorderThickness, BorderStyle);

                RecalculateTiers();
                OnPropertyChanged(nameof(AskBorderColor));
            }
        }

        [Browsable(false)]
        public XBrush BidBackgroundBrush;

        private XColor _bidBackgroundColor;

        [DataMember(Name = "BidBackgroundColor")]
        [DisplayName("Bid Background")]
        public XColor BidBackgroundColor
        {
            get => _bidBackgroundColor;
            set
            {
                if (value == _bidBackgroundColor)
                {
                    return;
                }

                _bidBackgroundColor = value;
                BidBackgroundBrush = new XBrush(_bidBackgroundColor);

                RecalculateTiers();
                OnPropertyChanged(nameof(BidBackgroundColor));
            }
        }

        [Browsable(false)]
        public XPen BidBorderPen;

        [Browsable(false)]
        public XBrush BidBorderBrush;

        private XColor _bidBorderColor;

        [DataMember(Name = "BidBorderColor")]
        [DisplayName("Bid Border")]
        public XColor BidBorderColor
        {
            get => _bidBorderColor;
            set
            {
                if (value == _bidBorderColor)
                {
                    return;
                }

                _bidBorderColor = value;
                BidBorderBrush = new XBrush(_bidBorderColor);
                BidBorderPen = new XPen(BidBorderBrush, BorderThickness, BorderStyle);

                RecalculateTiers();
                OnPropertyChanged(nameof(BidBorderColor));
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
                if (value == _borderThickness)
                {
                    return;
                }

                _borderThickness = value;
                AskBorderPen = new XPen(AskBorderBrush, _borderThickness, BorderStyle);
                BidBorderPen = new XPen(BidBorderBrush, _borderThickness, BorderStyle);

                RecalculateTiers();
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
                AskBorderPen = new XPen(AskBorderBrush, BorderThickness, _borderStyle);
                BidBorderPen = new XPen(BidBorderBrush, BorderThickness, _borderStyle);

                RecalculateTiers();
                OnPropertyChanged(nameof(BorderStyle));
            }
        }

        private PaletteTextAlignment _askTextAlignment;

        [DataMember(Name = "AskTextAlignment")]
        [DisplayName("Ask Text Alignment")]
        public PaletteTextAlignment AskTextAlignment
        {
            get => _askTextAlignment;
            set
            {
                if (value == _askTextAlignment)
                {
                    return;
                }

                _askTextAlignment = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(AskTextAlignment));
            }
        }

        private PaletteTextAlignment _bidTextAlignment;

        [DataMember(Name = "BidTextAlignment")]
        [DisplayName("Bid Text Alignment")]
        public PaletteTextAlignment BidTextAlignment
        {
            get => _bidTextAlignment;
            set
            {
                if (value == _bidTextAlignment)
                {
                    return;
                }

                _bidTextAlignment = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(BidTextAlignment));
            }
        }

        private double _fontSize;

        [DataMember(Name = "FontSize")]
        [DisplayName("Font Size")]
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize)
                {
                    return;
                }

                _fontSize = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(FontSize));
            }
        }

        private double _textLineWidth;

        [DataMember(Name = "TextLineWidth")]
        [DisplayName("Text Line Width")]
        public double TextLineWidth
        {
            get => _textLineWidth;
            set
            {
                if (value == _textLineWidth)
                {
                    return;
                }

                _textLineWidth = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(TextLineWidth));
            }
        }

        [Browsable(false)]
        public XPen AskTextLinePen;

        [Browsable(false)]
        public XPen BidTextLinePen;

        private int _textLineThickness;

        [DataMember(Name = "TextLineThickness")]
        [DisplayName("Text Line Thickness")]
        public int TextLineThickness
        {
            get => _textLineThickness;
            set
            {
                if (value == _textLineThickness)
                {
                    return;
                }

                _textLineThickness = value;
                AskTextLinePen = new XPen(
                    AskBorderBrush.Color.Alpha > 0 ? AskBorderBrush : AskBackgroundBrush,
                    _textLineThickness,
                    TextLineStyle
                );
                BidTextLinePen = new XPen(
                    BidBorderBrush.Color.Alpha > 0 ? BidBorderBrush : BidBackgroundBrush,
                    _textLineThickness,
                    TextLineStyle
                );

                RecalculateTiers();
                OnPropertyChanged(nameof(TextLineThickness));
            }
        }

        private XDashStyle _textLineStyle;

        [DataMember(Name = "TextLineStyle")]
        [DisplayName("Text Line Style")]
        public XDashStyle TextLineStyle
        {
            get => _textLineStyle;
            set
            {
                if (value == _textLineStyle)
                {
                    return;
                }

                _textLineStyle = value;
                AskTextLinePen = new XPen(
                    AskBorderBrush.Color.Alpha > 0 ? AskBorderBrush : AskBackgroundBrush,
                    TextLineThickness,
                    _textLineStyle
                );
                BidTextLinePen = new XPen(
                    BidBorderBrush.Color.Alpha > 0 ? BidBorderBrush : BidBackgroundBrush,
                    TextLineThickness,
                    _textLineStyle
                );

                RecalculateTiers();
                OnPropertyChanged(nameof(TextLineStyle));
            }
        }

        public Palette()
        {
            Name = "Default";
            AskBackgroundColor = new XColor(0, Colors.SeaGreen);
            AskBorderColor = new XColor(255, Colors.SeaGreen);
            BidBackgroundColor = new XColor(0, Colors.Firebrick);
            BidBorderColor = new XColor(255, Colors.Firebrick);
            BorderThickness = 1;
            BorderStyle = XDashStyle.Solid;
            AskTextAlignment = PaletteTextAlignment.Left;
            BidTextAlignment = PaletteTextAlignment.Right;
            FontSize = 14;
            TextLineWidth = 15;
            TextLineThickness = 1;
            TextLineStyle = XDashStyle.Solid;
        }

        public Palette DeepCopy()
        {
            return new Palette
            {
                Name = Name,
                AskBackgroundColor = AskBackgroundColor,
                AskBorderColor = AskBorderColor,
                BidBackgroundColor = BidBackgroundColor,
                BidBorderColor = BidBorderColor,
                BorderThickness = BorderThickness,
                BorderStyle = BorderStyle,
                AskTextAlignment = AskTextAlignment,
                BidTextAlignment = BidTextAlignment,
                FontSize = FontSize,
                TextLineWidth = TextLineWidth,
                TextLineThickness = TextLineThickness,
                TextLineStyle = TextLineStyle,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Action OnRecalculateTiers;

        private void RecalculateTiers()
        {
            OnRecalculateTiers?.Invoke();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
