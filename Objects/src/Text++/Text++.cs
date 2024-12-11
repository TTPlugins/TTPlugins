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
using CustomCommon.Helpers;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Objects.Common;
using TigerTrade.Core.UI.Converters;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;
using DrawH = CustomCommon.Helpers.Draw;

namespace ObjectsPlusPlus.Text
{
    /// <summary>
    /// CP0 ------
    /// </summary>
    internal class TextInfo
    {
        public Point ControlPoint0;

        public string TextValue = "Text...";

        public Rect Text;
        public Rect MirrorText;

        public bool TextIntersectsWithCanvas;
        public bool MirrorTextIntersectsWithCanvas;

        public XFont? Font;
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "TextHorizontalAlign",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum TextHorizontalAlign
    {
        [EnumMember(Value = "Left"), Description("Left")]
        Left,

        [EnumMember(Value = "Center"), Description("Center")]
        Center,

        [EnumMember(Value = "Right"), Description("Right")]
        Right,
    }

    [DataContract(
        Name = "TextPlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("TextPlusPlusObject", "Text++", 1, Type = typeof(Text))]
    internal sealed class Text : ObjectBase
    {
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private TextInfo? info;

        private ObjectPoint[] lastControlPoints;
        private FrozenPoint[] lastFrozenPoints;
        private bool fixedCoordinateSystemOnSetup = false;

        private long lastDrawTimestamp = 0;

        private TimeframeVisibility _visibility;

        [DataMember(Name = "Visibility")]
        [Category("General"), DisplayName("Timeframe Visibility")]
        public TimeframeVisibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility)
                    return;

                _visibility = value;

                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);

                OnPropertyChanged(nameof(Visibility));
            }
        }

        private CoordinateSystem _system;
        private bool _frozenTime = false;
        private bool _frozenPrice = false;

        [DataMember(Name = "System")]
        [Category("General"), DisplayName("Coordinate System")]
        public CoordinateSystem System
        {
            get => _system;
            set
            {
                if (value == _system)
                    return;

                _system = value;

                CoordinateSystemChanged();

                OnPropertyChanged(nameof(System));
            }
        }

        [DataMember(Name = "Price")]
        [Category("General"), DisplayName("Price")]
        public double Price
        {
            get => ControlPoints[0].Y;
            set
            {
                if (value == ControlPoints[0].Y)
                    return;

                ControlPoints[0].Y = value;

                OnPropertyChanged(nameof(ControlPoints));
            }
        }

        private string _text;

        [DataMember(Name = "TextValue")]
        [Category("Text"), DisplayName("Text")]
        public string TextValue
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value;

                OnPropertyChanged(nameof(TextValue));
            }
        }

        private TextHorizontalAlign _textHorizontalAlign;

        [DataMember(Name = "TextHorizontalAlign")]
        [Category("Text"), DisplayName("Horizontal Align")]
        public TextHorizontalAlign TextHorizontalAlign
        {
            get => _textHorizontalAlign;
            set
            {
                if (value == _textHorizontalAlign)
                    return;

                _textHorizontalAlign = value;

                OnPropertyChanged(nameof(TextHorizontalAlign));
            }
        }

        private double _fontSize;

        [DataMember(Name = "FontSize")]
        [Category("Text"), DisplayName("Font Size")]
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
        [Category("Text"), DisplayName("Text Color")]
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
        [Category("Text"), DisplayName("Background")]
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
        [Category("Text"), DisplayName("Border")]
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
        [Category("Text"), DisplayName("Border Thickness")]
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
        [Category("Text"), DisplayName("Border Style")]
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

        private bool _showMirrorText;

        [DataMember(Name = "ShowMirrorText")]
        [Category("Mirror"), DisplayName("Show Mirror")]
        public bool ShowMirrorText
        {
            get => _showMirrorText;
            set
            {
                if (value == _showMirrorText)
                    return;

                _showMirrorText = value;

                OnPropertyChanged(nameof(ShowMirrorText));
            }
        }

        private double _mirrorOffset;

        [DataMember(Name = "MirrorOffset")]
        [Category("Mirror"), DisplayName("Mirror Offset")]
        public double MirrorOffset
        {
            get => _mirrorOffset;
            set
            {
                if (value == _mirrorOffset)
                    return;

                _mirrorOffset = value;

                OnPropertyChanged(nameof(MirrorOffset));
            }
        }

        private FrozenPoint[] _frozenPoints;

        [DataMember(Name = "FrozenPoints")]
        [Category("Object"), DisplayName("Frozen Points")]
        public FrozenPoint[] FrozenPoints
        {
            get =>
                _frozenPoints
                ?? (
                    _frozenPoints = new FrozenPoint[1]
                    {
                        new FrozenPoint { X = 300, Y = 100 },
                    }
                );
            set
            {
                if (Equals(value, _frozenPoints))
                    return;

                _frozenPoints = value;

                OnPropertyChanged(nameof(FrozenPoints));
            }
        }

        protected override int PenWidth => BorderColor.IsTransparent ? 0 : BorderThickness;

        public Text()
        {
            Visibility = TimeframeVisibility.Disabled;
            System = CoordinateSystem.Default;

            TextValue = "Text";
            TextHorizontalAlign = TextHorizontalAlign.Right;
            FontSize = 12;
            ForegroundColor = Colors.Red;
            BackgroundColor = Colors.Transparent;
            BorderColor = Colors.Transparent;
            BorderThickness = 1;
            BorderStyle = XDashStyle.Solid;

            ShowMirrorText = false;
            MirrorOffset = 3;

            FrozenPoints = new FrozenPoint[1]
            {
                new FrozenPoint { X = 300, Y = 100 },
            };
        }

        private void CoordinateSystemChanged()
        {
            bool freezeTime;
            bool freezePrice;

            if (_system == CoordinateSystem.Frozen)
            {
                freezeTime = true;
                freezePrice = true;
            }
            else
            {
                freezeTime = _system == CoordinateSystem.FrozenTime;
                freezePrice = _system == CoordinateSystem.FrozenPrice;
            }

            bool timeChanged = _frozenTime != freezeTime;
            bool priceChanged = _frozenPrice != freezePrice;

            _frozenTime = freezeTime;
            _frozenPrice = freezePrice;

            if (DataProvider == null || Canvas == null || ControlPoints == null)
                return;

            int length = FrozenPoints.Length;
            if (length != ControlPoints.Length)
                return;

            if (timeChanged)
            {
                if (_frozenTime)
                {
                    double canvasWidth = Canvas.Rect.Width;
                    for (int i = 0; i < length; i++)
                    {
                        FrozenPoints[i].x = Math.Max(
                            0,
                            Math.Min(canvasWidth, canvasWidth - ToPoint(ControlPoints[i]).X)
                        );
                    }
                }
                else
                {
                    double canvasWidth = Canvas.Rect.Width;
                    for (int i = 0; i < length; i++)
                    {
                        double? date = Chart.GetDate(
                            DataProvider,
                            Canvas,
                            canvasWidth - FrozenPoints[i].x
                        );

                        if (date == null)
                            continue;

                        ControlPoints[i].X = date.Value;
                    }
                }
            }

            if (priceChanged)
            {
                if (_frozenPrice)
                {
                    double max = Canvas.Rect.Height;
                    for (int i = 0; i < length; i++)
                    {
                        FrozenPoints[i].y = Math.Max(
                            0,
                            Math.Min(max, Canvas.GetY(ControlPoints[i].Y))
                        );
                    }
                }
                else
                {
                    double h = Canvas.StepHeight / 2.0;
                    for (int i = 0; i < length; i++)
                    {
                        ControlPoints[i].Y = Canvas.GetValue(FrozenPoints[i].y - h);
                    }
                }
            }
        }

        private void FixCoordinateSystemOnSetup()
        {
            if (InSetup)
            {
                _frozenTime = false;
                _frozenPrice = false;

                fixedCoordinateSystemOnSetup = true;
                return;
            }

            if (!fixedCoordinateSystemOnSetup)
                return;
            fixedCoordinateSystemOnSetup = false;

            if (_system == CoordinateSystem.Frozen)
            {
                _frozenTime = true;
                _frozenPrice = true;
            }
            else
            {
                _frozenTime = _system == CoordinateSystem.FrozenTime;
                _frozenPrice = _system == CoordinateSystem.FrozenPrice;
            }

            int length = FrozenPoints.Length;
            if (length != ControlPoints.Length)
                return;

            if (_frozenTime)
            {
                double canvasWidth = Canvas.Rect.Width;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].x = Math.Max(
                        0,
                        Math.Min(canvasWidth, canvasWidth - ToPoint(ControlPoints[i]).X)
                    );
                }
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].y = Math.Max(0, Math.Min(max, Canvas.GetY(ControlPoints[i].Y)));
                }
            }
        }

        protected override void Prepare()
        {
            if (InSetup)
            {
                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);
            }

            Point p0 = ToPoint(ControlPoints[0]);

            FixCoordinateSystemOnSetup();

            if (_frozenTime)
                p0.X = Canvas.Rect.Width - FrozenPoints[0].x;

            if (_frozenPrice)
                p0.Y = FrozenPoints[0].y;

            double halfBorderWidth = 0;
            if (!BorderColor.IsTransparent)
                halfBorderWidth = Math.Ceiling(((double)BorderThickness) / 2.0);

            string textValue = TextValue == "" ? "Text..." : TextValue;
            XFont font = new XFont(Canvas.ChartFont.Name, FontSize);
            double textWidth = font.GetWidth(textValue);
            double textHeight = font.GetHeight();

            double textX;
            if (TextHorizontalAlign == TextHorizontalAlign.Center)
                textX = p0.X - (textWidth / 2);
            else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                textX = p0.X;
            else
                textX = p0.X - textWidth;

            double textY = p0.Y - (textHeight / 2);

            Rect text = new Rect(textX, textY, textWidth, textHeight);

            Rect mirrorText = Rect.Empty;
            if (ShowMirrorText)
            {
                mirrorText = new Rect(
                    Canvas.Rect.Right - textWidth - halfBorderWidth - MirrorOffset,
                    text.Y,
                    textWidth,
                    textHeight
                );

                if (mirrorText.X < p0.X)
                    mirrorText = Rect.Empty;
            }

            info = new TextInfo
            {
                ControlPoint0 = p0,

                TextValue = textValue,

                Text = text,
                MirrorText = mirrorText,

                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                MirrorTextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(mirrorText),

                Font = font,
            };

            isObjectInArea = info.TextIntersectsWithCanvas || info.MirrorTextIntersectsWithCanvas;
        }

        protected override void Draw(DxVisualQueue visual, ref List<ObjectLabelInfo> labels)
        {
            if (info == null || !isObjectInArea)
                return;

            if (areControlPointsVisible && !Lock && !_frozenPrice)
            {
                if (Keyboard.IsKeyDown(Key.Up))
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (now - lastDrawTimestamp >= AdjustCoordinatesRefreshRate)
                    {
                        lastDrawTimestamp = now;

                        ControlPoints[0].Y += DataProvider.Step;

                        ControlPointsChanged();
                        OnPropertyChanged(nameof(ControlPoints));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Down))
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (now - lastDrawTimestamp >= AdjustCoordinatesRefreshRate)
                    {
                        lastDrawTimestamp = now;

                        ControlPoints[0].Y -= DataProvider.Step;

                        ControlPointsChanged();
                        OnPropertyChanged(nameof(ControlPoints));
                    }
                }
            }

            if (!info.Text.IsEmpty && info.TextIntersectsWithCanvas && info.Font != null)
            {
                if (!BackgroundColor.IsTransparent)
                    visual.FillRectangle(BackgroundBrush, info.Text);
                if (!BorderColor.IsTransparent)
                    visual.DrawRectangle(BorderPen, info.Text);
                if (!ForegroundColor.IsTransparent)
                    visual.DrawString(info.TextValue, info.Font, ForegroundBrush, info.Text);
            }

            if (
                !info.MirrorText.IsEmpty
                && info.MirrorTextIntersectsWithCanvas
                && info.Font != null
            )
            {
                if (!BackgroundColor.IsTransparent)
                    visual.FillRectangle(BackgroundBrush, info.MirrorText);
                if (!BorderColor.IsTransparent)
                    visual.DrawRectangle(BorderPen, info.MirrorText);
                if (!ForegroundColor.IsTransparent)
                    visual.DrawString(info.TextValue, info.Font, ForegroundBrush, info.MirrorText);
            }

            if (areControlPointsVisible)
            {
                double h = Canvas.StepHeight / 2.0;

                double price;
                if (_frozenPrice)
                    price = Canvas.GetValue(FrozenPoints[0].y - h);
                else
                    price = ControlPoints[0].Y;

                labels.Add(new ObjectLabelInfo(price, ForegroundColor));

                areControlPointsVisible = false;
            }
        }

        public override void DrawControlPoints(DxVisualQueue visual)
        {
            if (info == null)
                return;

            areControlPointsVisible = true;

            if (Lock)
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint0);
            else if (InMove)
            {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                DrawH.ControlPointCorner(visual, info.ControlPoint0);

                if (Mouse.OverrideCursor != null)
                    Mouse.OverrideCursor = null;
            }
        }

        protected override bool IsObjectOnChart()
        {
            if (DataProvider == null)
                return false;

            int datesCount = DataProvider.Dates.Count;
            if (datesCount <= 0)
                return false;

            if (_frozenTime || ShowMirrorText)
                return true;

            return ControlPoints[0].X >= DataProvider.Dates[0];
        }

        protected override bool IsObjectInArea()
        {
            return isObjectInArea;
        }

        protected override bool InObject(int x, int y)
        {
            if (info == null)
                return false;

            return info.Text.Contains(x, y) || info.MirrorText.Contains(x, y);
        }

        public override int GetControlPoint(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetControlPointIndex(x, y, info.ControlPoint0);
        }

        public override int GetExtraPoint(int x, int y)
        {
            return -1;
        }

        protected override int GetMinDist(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetMinDistance(x, y, info.Text, info.MirrorText);
        }

        public override void BeginDrag()
        {
            base.BeginDrag();

            lastControlPoints = new ObjectPoint[1]
            {
                new ObjectPoint { X = ControlPoints[0].X, Y = ControlPoints[0].Y },
            };

            int length = FrozenPoints.Length;
            lastFrozenPoints = new FrozenPoint[length];
            for (int i = 0; i < length; i++)
            {
                lastFrozenPoints[i] = new FrozenPoint
                {
                    X = FrozenPoints[i].x,
                    Y = FrozenPoints[i].y,
                };
            }
        }

        public override void DragObject(double dx, double dy)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (Math.Abs(dx) >= Math.Abs(dy))
                    dy = 0;
                else
                    dx = 0;
            }

            base.DragObject(dx, dy);

            if (dx == 0)
                ControlPoints[0].X = lastControlPoints[0].X;
            if (dy == 0)
                ControlPoints[0].Y = lastControlPoints[0].Y;

            if (_frozenTime)
            {
                double max = Canvas.Rect.Width;

                int length = FrozenPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].x = Math.Max(0, Math.Min(max, lastFrozenPoints[i].x - dx));
                }
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height;

                int length = FrozenPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].y = Math.Max(0, Math.Min(max, lastFrozenPoints[i].y + dy));
                }
            }
        }

        public override void ControlPointEditing(int index)
        {
            base.ControlPointEditing(index);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point lp = ToPoint(lastControlPoints[index]);
                Point cp = ToPoint(ControlPoints[index]);

                if (Math.Abs(lp.X - cp.X) >= Math.Abs(lp.Y - cp.Y))
                    ControlPoints[index].Y = lastControlPoints[index].Y;
                else
                    ControlPoints[index].X = lastControlPoints[index].X;
            }

            if (_frozenTime)
            {
                double canvasWidth = Canvas.Rect.Width;
                FrozenPoints[index].x = Math.Max(
                    0,
                    Math.Min(canvasWidth, canvasWidth - ToPoint(ControlPoints[index]).X)
                );
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height;
                FrozenPoints[index].y = Math.Max(
                    0,
                    Math.Min(max, Canvas.GetY(ControlPoints[index].Y))
                );
            }
        }

        public override void ControlPointsChanged()
        {
            base.ControlPointsChanged();

            if (_frozenTime || _frozenPrice)
            {
                int length = FrozenPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].FrozenPointChanged();
                }

                OnPropertyChanged(nameof(FrozenPoints));
            }
        }

        public override void CheckAlert(List<IndicatorBase> indicators)
        {
            return;
        }

        public override void ApplyTheme(IChartTheme theme)
        {
            base.ApplyTheme(theme);
        }

        public override void CopyTemplate(ObjectBase objectBase, bool style)
        {
            if (objectBase is Text o)
            {
                Visibility = o.Visibility;
                System = o.System;

                TextValue = o.TextValue;
                TextHorizontalAlign = o.TextHorizontalAlign;
                FontSize = o.FontSize;
                ForegroundColor = o.ForegroundColor;
                BackgroundColor = o.BackgroundColor;
                BorderColor = o.BorderColor;
                BorderThickness = o.BorderThickness;
                BorderStyle = o.BorderStyle;

                ShowMirrorText = o.ShowMirrorText;
                MirrorOffset = o.MirrorOffset;

                if (o.FrozenPoints?.Length == 1)
                {
                    FrozenPoints = new FrozenPoint[1]
                    {
                        new FrozenPoint { X = o.FrozenPoints[0].X, Y = o.FrozenPoints[0].Y },
                    };
                }
            }

            base.CopyTemplate(objectBase, style);
        }
    }
}
