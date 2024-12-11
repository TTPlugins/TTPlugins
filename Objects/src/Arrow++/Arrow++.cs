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
using LineP = CustomCommon.Helpers.Line;

namespace ObjectsPlusPlus.Arrow
{
    /// <summary>
    /// CP0 ------ CP1
    /// </summary>
    internal class ArrowInfo
    {
        public Point ControlPoint0;
        public Point ControlPoint1;

        public LineP Line;
        public LineP Tip0;
        public LineP Tip1;
        public Rect Text;
        public LineP? Mirror;
        public LineP? MirrorTip0;
        public LineP? MirrorTip1;
        public Rect MirrorText;

        public bool LineIntersectsWithCanvas;
        public bool Tip0IntersectsWithCanvas;
        public bool Tip1IntersectsWithCanvas;
        public bool TextIntersectsWithCanvas;
        public bool MirrorIntersectsWithCanvas;
        public bool MirrorTip0IntersectsWithCanvas;
        public bool MirrorTip1IntersectsWithCanvas;
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "TextVerticalAlign",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum TextVerticalAlign
    {
        [EnumMember(Value = "Top"), Description("Top")]
        Top,

        [EnumMember(Value = "Center"), Description("Center")]
        Center,

        [EnumMember(Value = "Bottom"), Description("Bottom")]
        Bottom,
    }

    [DataContract(
        Name = "ArrowPlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("ArrowPlusPlusObject", "Arrow++", 2, Type = typeof(Arrow))]
    internal sealed class Arrow : ObjectBase
    {
        private static double TextMarginX = 5;
        private static double TextMarginY = 5;
        private static int MinLineHitbox = 8;
        private static double ArrowTipLength = 14;
        private static double ArrowTipLengthMultiplier = 2;
        private static double ArrowTipAngle = 25;
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private int? controlPointEditingIndex = null;
        private ArrowInfo? info;

        private ObjectPoint[] lastControlPoints;
        private FrozenPoint[] lastFrozenPoints;
        private bool fixedCoordinateSystemOnSetup = false;
        private bool overrideFreezeOnlyPrice2 = false;

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

        private bool _freezeOnlyPrice2;

        [DataMember(Name = "FreezeOnlyPrice2")]
        [Category("General"), DisplayName("Freeze only Price 2")]
        public bool FreezeOnlyPrice2
        {
            get => _freezeOnlyPrice2;
            set
            {
                if (value == _freezeOnlyPrice2)
                    return;

                _freezeOnlyPrice2 = value;

                OnPropertyChanged(nameof(FreezeOnlyPrice2));
            }
        }

        [DataMember(Name = "Price1")]
        [Category("General"), DisplayName("Price 1")]
        public double Price1
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

        [DataMember(Name = "Price2")]
        [Category("General"), DisplayName("Price 2")]
        public double Price2
        {
            get => ControlPoints[1].Y;
            set
            {
                if (value == ControlPoints[1].Y)
                    return;

                ControlPoints[1].Y = value;

                OnPropertyChanged(nameof(ControlPoints));
            }
        }

        private XBrush _lineBrush;

        private XPen _linePen;

        private XColor _lineColor;

        [DataMember(Name = "LineColor")]
        [Category("Arrow"), DisplayName("Arrow Color")]
        public XColor LineColor
        {
            get => _lineColor;
            set
            {
                if (value == _lineColor)
                    return;

                _lineColor = value;

                _lineBrush = new XBrush(_lineColor);
                _linePen = new XPen(_lineBrush, LineWidth, LineStyle);

                OnPropertyChanged(nameof(LineColor));
            }
        }

        private int lineHitbox;
        private int _lineWidth;

        [DataMember(Name = "LineWidth")]
        [Category("Arrow"), DisplayName("Arrow Width")]
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                value = Math.Max(1, Math.Min(100, value));

                if (value == _lineWidth)
                    return;

                _lineWidth = value;

                _linePen = new XPen(_lineBrush, _lineWidth, LineStyle);
                lineHitbox = Math.Max(MinLineHitbox, _lineWidth);

                OnPropertyChanged(nameof(LineWidth));
            }
        }

        private XDashStyle _lineStyle;

        [DataMember(Name = "LineStyle")]
        [Category("Arrow"), DisplayName("Arrow Style")]
        public XDashStyle LineStyle
        {
            get => _lineStyle;
            set
            {
                if (value == _lineStyle)
                    return;

                _lineStyle = value;

                _linePen = new XPen(_lineBrush, LineWidth, _lineStyle);

                OnPropertyChanged(nameof(LineStyle));
            }
        }

        private bool _showMirror;

        [DataMember(Name = "ShowMirror")]
        [Category("Mirror"), DisplayName("Show Mirror")]
        public bool ShowMirror
        {
            get => _showMirror;
            set
            {
                if (value == _showMirror)
                    return;

                _showMirror = value;

                OnPropertyChanged(nameof(ShowMirror));
            }
        }

        private bool _showMirrorText;

        [DataMember(Name = "ShowMirrorText")]
        [Category("Mirror"), DisplayName("Show Mirror Text")]
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

        private double _mirrorWidth;

        [DataMember(Name = "MirrorWidth")]
        [Category("Mirror"), DisplayName("Mirror Width")]
        public double MirrorWidth
        {
            get => _mirrorWidth;
            set
            {
                value = Math.Max(0, value);

                if (value == _mirrorWidth)
                    return;

                _mirrorWidth = value;

                OnPropertyChanged(nameof(MirrorWidth));
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

        private bool _showText;

        [DataMember(Name = "ShowText")]
        [Category("Text"), DisplayName("Show")]
        public bool ShowText
        {
            get => _showText;
            set
            {
                if (value == _showText)
                    return;

                _showText = value;

                OnPropertyChanged(nameof(ShowText));
            }
        }

        private string _text;

        [DataMember(Name = "Text")]
        [Category("Text"), DisplayName("Text")]
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value;

                OnPropertyChanged(nameof(Text));
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

        private TextVerticalAlign _textVerticalAlign;

        [DataMember(Name = "TextVerticalAlign")]
        [Category("Text"), DisplayName("Vertical Align")]
        public TextVerticalAlign TextVerticalAlign
        {
            get => _textVerticalAlign;
            set
            {
                if (value == _textVerticalAlign)
                    return;

                _textVerticalAlign = value;

                OnPropertyChanged(nameof(TextVerticalAlign));
            }
        }

        private TextOptions _textOptions;

        [DataMember(Name = "TextOptions")]
        [Category("Text"), DisplayName("Options")]
        public TextOptions TextOptions
        {
            get => _textOptions ?? (_textOptions = new TextOptions());
            set
            {
                if (Equals(value, _textOptions))
                    return;

                _textOptions = value;

                OnPropertyChanged(nameof(TextOptions));
            }
        }

        private bool _showTopPriceLabel;

        [DataMember(Name = "ShowTopPriceLabel")]
        [Category("Extra"), DisplayName("Show Top Price Label")]
        public bool ShowTopPriceLabel
        {
            get => _showTopPriceLabel;
            set
            {
                if (value == _showTopPriceLabel)
                    return;

                _showTopPriceLabel = value;

                OnPropertyChanged(nameof(ShowTopPriceLabel));
            }
        }

        private bool _showBottomPriceLabel;

        [DataMember(Name = "ShowBottomPriceLabel")]
        [Category("Extra"), DisplayName("Show Bottom Price Label")]
        public bool ShowBottomPriceLabel
        {
            get => _showBottomPriceLabel;
            set
            {
                if (value == _showBottomPriceLabel)
                    return;

                _showBottomPriceLabel = value;

                OnPropertyChanged(nameof(ShowBottomPriceLabel));
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
                    _frozenPoints = new FrozenPoint[2]
                    {
                        new FrozenPoint { X = 100, Y = 100 },
                        new FrozenPoint { X = 300, Y = 300 },
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

        protected override int PenWidth => LineWidth;

        public Arrow()
        {
            Color color = Colors.Lime;

            Visibility = TimeframeVisibility.Disabled;
            System = CoordinateSystem.Default;
            FreezeOnlyPrice2 = true;

            LineColor = color;
            LineWidth = 1;
            LineStyle = XDashStyle.Solid;

            ShowMirror = false;
            ShowMirrorText = false;
            MirrorWidth = 100;
            MirrorOffset = 0;

            ShowText = false;
            Text = "Text";
            TextHorizontalAlign = TextHorizontalAlign.Center;
            TextVerticalAlign = TextVerticalAlign.Center;
            TextOptions = new TextOptions();

            ShowTopPriceLabel = false;
            ShowBottomPriceLabel = false;

            FrozenPoints = new FrozenPoint[2]
            {
                new FrozenPoint { X = 100, Y = 100 },
                new FrozenPoint { X = 300, Y = 300 },
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
                    double max = canvasWidth - LineWidth;
                    for (int i = 0; i < length; i++)
                    {
                        FrozenPoints[i].x = Math.Max(
                            0,
                            Math.Min(max, canvasWidth - ToPoint(ControlPoints[i]).X)
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
                    double max = Canvas.Rect.Height - LineWidth;
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
                double max = canvasWidth - LineWidth;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].x = Math.Max(
                        0,
                        Math.Min(max, canvasWidth - ToPoint(ControlPoints[i]).X)
                    );
                }
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height - LineWidth;
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

            if (DataProvider == null)
                return;

            int datesCount = DataProvider.Dates.Count;
            if (datesCount <= 0)
                return;

            LineData lineData = new LineData(ControlPoints[0], ControlPoints[1]);

            double minDate = DataProvider.Dates[0];
            double maxDate = Math.Max(lineData.p0.X, lineData.p1.X);
            if (datesCount >= 2)
                maxDate = Math.Min(maxDate, DataProvider.Dates[datesCount - 1]);

            if (lineData.p0.X < lineData.p1.X)
            {
                if (lineData.p0.X < minDate)
                {
                    double y0 = lineData.GetY(minDate);
                    double y1 = lineData.GetY(maxDate);

                    lineData.p0.X = minDate;
                    lineData.p0.Y = y0;
                    lineData.p1.X = maxDate;
                    lineData.p1.Y = y1;
                }
            }
            else if (lineData.p0.X > lineData.p1.X)
            {
                if (lineData.p1.X < minDate)
                {
                    double y0 = lineData.GetY(maxDate);
                    double y1 = lineData.GetY(minDate);

                    lineData.p0.X = maxDate;
                    lineData.p0.Y = y0;
                    lineData.p1.X = minDate;
                    lineData.p1.Y = y1;
                }
            }

            Point p0 = ToPoint(lineData.p0);
            Point p1 = ToPoint(lineData.p1);

            FixCoordinateSystemOnSetup();

            if (_frozenTime)
            {
                double canvasWidth = Canvas.Rect.Width;
                p0.X = canvasWidth - FrozenPoints[0].x;
                p1.X = canvasWidth - FrozenPoints[1].x;
            }

            if (_frozenPrice)
            {
                if (_freezeOnlyPrice2)
                {
                    p1.Y = p0.Y + (FrozenPoints[1].y - FrozenPoints[0].y);
                }
                else
                {
                    p0.Y = FrozenPoints[0].y;
                    p1.Y = FrozenPoints[1].y;
                }
            }

            LineP line = new LineP(p0, p1, lineHitbox);

            LineP tip0;
            LineP tip1;
            {
                double tipX = line.p1.X;
                double tipY = line.p1.Y;

                double tipExtraLength = ArrowTipLengthMultiplier * (_lineWidth - 1);
                double tipLength = ArrowTipLength + tipExtraLength;
                double tipAngle = ArrowTipAngle;

                double angle = line.GetAngle();
                double pi180 = Math.PI / 180;
                double m0 = (angle + tipAngle) * pi180;
                double m1 = (angle - tipAngle) * pi180;

                tip0 = new LineP(
                    new Point(tipX - (tipLength * Math.Cos(m0)), tipY - (tipLength * Math.Sin(m0))),
                    line.p1,
                    lineHitbox
                );
                tip1 = new LineP(
                    new Point(tipX - (tipLength * Math.Cos(m1)), tipY - (tipLength * Math.Sin(m1))),
                    line.p1,
                    lineHitbox
                );
            }

            LineP? mirror = null;
            LineP? mirrorTip0 = null;
            LineP? mirrorTip1 = null;
            if (ShowMirror)
            {
                double width = Math.Max(0, MirrorWidth);

                double x0;
                double x1;

                if (p0.X <= p1.X)
                {
                    x1 = Canvas.Rect.Right - MirrorOffset;
                    x0 = x1 - MirrorWidth;
                }
                else
                {
                    x0 = Canvas.Rect.Right - MirrorOffset;
                    x1 = x0 - MirrorWidth;
                }

                mirror = new LineP(new Point(x0, p0.Y), new Point(x1, p1.Y), lineHitbox);

                if (Math.Min(mirror.Value.p0.X, mirror.Value.p1.X) < Math.Min(line.p0.X, line.p1.X))
                    mirror = null;

                if (mirror != null)
                {
                    double tipX = mirror.Value.p1.X;
                    double tipY = mirror.Value.p1.Y;

                    double tipExtraLength = ArrowTipLengthMultiplier * (_lineWidth - 1);
                    double tipLength = ArrowTipLength + tipExtraLength;
                    double tipAngle = ArrowTipAngle;

                    double angle = mirror.Value.GetAngle();
                    double pi180 = Math.PI / 180;
                    double m0 = (angle + tipAngle) * pi180;
                    double m1 = (angle - tipAngle) * pi180;

                    mirrorTip0 = new LineP(
                        new Point(
                            tipX - (tipLength * Math.Cos(m0)),
                            tipY - (tipLength * Math.Sin(m0))
                        ),
                        mirror.Value.p1,
                        lineHitbox
                    );
                    mirrorTip1 = new LineP(
                        new Point(
                            tipX - (tipLength * Math.Cos(m1)),
                            tipY - (tipLength * Math.Sin(m1))
                        ),
                        mirror.Value.p1,
                        lineHitbox
                    );
                }
            }

            XFont? font = null;

            Rect text = Rect.Empty;
            if (ShowText && Text != "")
            {
                font = new XFont(Canvas.ChartFont.Name, TextOptions.FontSize);
                double textWidth = font.GetWidth(Text);
                double textHeight = font.GetHeight();

                double x;
                double y;

                if (TextHorizontalAlign == TextHorizontalAlign.Center)
                    x =
                        Math.Min(line.p0.X, line.p1.X)
                        + (Math.Abs(line.p0.X - line.p1.X) / 2)
                        - (textWidth / 2);
                else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                    x = Math.Max(line.p0.X, line.p1.X) - textWidth;
                else
                    x = Math.Min(line.p0.X, line.p1.X);

                if (TextVerticalAlign == TextVerticalAlign.Center)
                {
                    y =
                        Math.Min(line.p0.Y, line.p1.Y)
                        + (Math.Abs(line.p0.Y - line.p1.Y) / 2)
                        - (textHeight / 2);

                    if (TextHorizontalAlign == TextHorizontalAlign.Left)
                        x += TextMarginX;
                    else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                        x -= TextMarginX;
                }
                else if (TextVerticalAlign == TextVerticalAlign.Bottom)
                    y = Math.Max(line.p0.Y, line.p1.Y) + TextMarginY;
                else
                    y = Math.Min(line.p0.Y, line.p1.Y) - textHeight - TextMarginY;

                text = new Rect(x, y, textWidth, textHeight);
            }

            Rect mirrorText = Rect.Empty;
            if (ShowMirrorText && Text != "" && mirror != null)
            {
                if (font == null)
                    font = new XFont(Canvas.ChartFont.Name, TextOptions.FontSize);
                double textWidth = font.GetWidth(Text);
                double textHeight = font.GetHeight();

                double x;
                double y;

                if (TextHorizontalAlign == TextHorizontalAlign.Center)
                    x =
                        Math.Min(mirror.Value.p0.X, mirror.Value.p1.X)
                        + (Math.Abs(mirror.Value.p0.X - mirror.Value.p1.X) / 2)
                        - (textWidth / 2);
                else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                    x = Math.Max(mirror.Value.p0.X, mirror.Value.p1.X) - textWidth;
                else
                    x = Math.Min(mirror.Value.p0.X, mirror.Value.p1.X);

                if (TextVerticalAlign == TextVerticalAlign.Center)
                {
                    y =
                        Math.Min(mirror.Value.p0.Y, mirror.Value.p1.Y)
                        + (Math.Abs(mirror.Value.p0.Y - mirror.Value.p1.Y) / 2)
                        - (textHeight / 2);

                    if (TextHorizontalAlign == TextHorizontalAlign.Left)
                        x += TextMarginX;
                    else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                        x -= TextMarginX;
                }
                else if (TextVerticalAlign == TextVerticalAlign.Bottom)
                    y = Math.Max(mirror.Value.p0.Y, mirror.Value.p1.Y) + TextMarginY;
                else
                    y = Math.Min(mirror.Value.p0.Y, mirror.Value.p1.Y) - textHeight - TextMarginY;

                mirrorText = new Rect(x, y, textWidth, textHeight);
            }

            info = new ArrowInfo
            {
                ControlPoint0 = p0,
                ControlPoint1 = p1,

                Line = line,
                Tip0 = tip0,
                Tip1 = tip1,
                Text = text,
                Mirror = mirror,
                MirrorTip0 = mirrorTip0,
                MirrorTip1 = mirrorTip1,
                MirrorText = mirrorText,

                LineIntersectsWithCanvas = line.IntersectsWith(Canvas.Rect),
                Tip0IntersectsWithCanvas = tip0.IntersectsWith(Canvas.Rect),
                Tip1IntersectsWithCanvas = tip1.IntersectsWith(Canvas.Rect),
                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                MirrorIntersectsWithCanvas = mirror?.IntersectsWith(Canvas.Rect) ?? false,
                MirrorTip0IntersectsWithCanvas = mirrorTip0?.IntersectsWith(Canvas.Rect) ?? false,
                MirrorTip1IntersectsWithCanvas = mirrorTip1?.IntersectsWith(Canvas.Rect) ?? false,
                MirrorTextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(mirrorText),

                Font = font,
            };

            isObjectInArea =
                info.LineIntersectsWithCanvas
                || info.Tip0IntersectsWithCanvas
                || info.Tip1IntersectsWithCanvas
                || info.TextIntersectsWithCanvas
                || info.MirrorIntersectsWithCanvas
                || info.MirrorTip0IntersectsWithCanvas
                || info.MirrorTip1IntersectsWithCanvas
                || info.MirrorTextIntersectsWithCanvas;
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

                        double ticksize = DataProvider.Step;

                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            if (ControlPoints[0].Y >= ControlPoints[1].Y)
                                ControlPoints[0].Y += ticksize;
                            else
                                ControlPoints[1].Y += ticksize;
                        }
                        else if (
                            Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                        )
                        {
                            if (ControlPoints[0].Y >= ControlPoints[1].Y)
                                ControlPoints[1].Y += ticksize;
                            else
                                ControlPoints[0].Y += ticksize;
                        }
                        else
                        {
                            ControlPoints[0].Y += ticksize;
                            ControlPoints[1].Y += ticksize;
                        }

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

                        double ticksize = DataProvider.Step;

                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            if (ControlPoints[0].Y >= ControlPoints[1].Y)
                                ControlPoints[0].Y -= ticksize;
                            else
                                ControlPoints[1].Y -= ticksize;
                        }
                        else if (
                            Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                        )
                        {
                            if (ControlPoints[0].Y >= ControlPoints[1].Y)
                                ControlPoints[1].Y -= ticksize;
                            else
                                ControlPoints[0].Y -= ticksize;
                        }
                        else
                        {
                            ControlPoints[0].Y -= ticksize;
                            ControlPoints[1].Y -= ticksize;
                        }

                        ControlPointsChanged();
                        OnPropertyChanged(nameof(ControlPoints));
                    }
                }
            }

            if (!_lineColor.IsTransparent)
            {
                if (info.LineIntersectsWithCanvas)
                    visual.DrawLine(_linePen, info.Line.p0, info.Line.p1);
                if (info.Tip0IntersectsWithCanvas || info.Tip1IntersectsWithCanvas)
                    visual.DrawLines(_linePen, [info.Tip0.p0, info.Line.p1, info.Tip1.p0]);

                if (info.Mirror != null && info.MirrorTip0 != null && info.MirrorTip1 != null)
                {
                    if (info.MirrorIntersectsWithCanvas)
                        visual.DrawLine(_linePen, info.Mirror.Value.p0, info.Mirror.Value.p1);
                    if (info.MirrorTip0IntersectsWithCanvas || info.MirrorTip1IntersectsWithCanvas)
                        visual.DrawLines(
                            _linePen,
                            [
                                info.MirrorTip0.Value.p0,
                                info.Mirror.Value.p1,
                                info.MirrorTip1.Value.p0,
                            ]
                        );
                }
            }

            if (!info.Text.IsEmpty && info.TextIntersectsWithCanvas && info.Font != null)
            {
                if (!TextOptions.BackgroundColor.IsTransparent)
                    visual.FillRectangle(TextOptions.BackgroundBrush, info.Text);
                if (!TextOptions.BorderColor.IsTransparent)
                    visual.DrawRectangle(TextOptions.BorderPen, info.Text);
                if (!TextOptions.ForegroundColor.IsTransparent)
                    visual.DrawString(Text, info.Font, TextOptions.ForegroundBrush, info.Text);
            }

            if (
                !info.MirrorText.IsEmpty
                && info.MirrorTextIntersectsWithCanvas
                && info.Font != null
            )
            {
                if (!TextOptions.BackgroundColor.IsTransparent)
                    visual.FillRectangle(TextOptions.BackgroundBrush, info.MirrorText);
                if (!TextOptions.BorderColor.IsTransparent)
                    visual.DrawRectangle(TextOptions.BorderPen, info.MirrorText);
                if (!TextOptions.ForegroundColor.IsTransparent)
                    visual.DrawString(
                        Text,
                        info.Font,
                        TextOptions.ForegroundBrush,
                        info.MirrorText
                    );
            }

            if (areControlPointsVisible)
            {
                if (_frozenPrice)
                {
                    double h = Canvas.StepHeight / 2.0;

                    if (_freezeOnlyPrice2)
                    {
                        labels.Add(new ObjectLabelInfo(ControlPoints[0].Y, LineColor));
                        labels.Add(
                            new ObjectLabelInfo(
                                Canvas.GetValue(info.ControlPoint1.Y - h),
                                LineColor
                            )
                        );
                    }
                    else
                    {
                        labels.Add(
                            new ObjectLabelInfo(Canvas.GetValue(FrozenPoints[0].y - h), LineColor)
                        );
                        labels.Add(
                            new ObjectLabelInfo(Canvas.GetValue(FrozenPoints[1].y - h), LineColor)
                        );
                    }
                }
                else
                {
                    labels.Add(new ObjectLabelInfo(ControlPoints[0].Y, LineColor));
                    labels.Add(new ObjectLabelInfo(ControlPoints[1].Y, LineColor));
                }

                areControlPointsVisible = false;
            }
            else
            {
                if (ShowTopPriceLabel)
                {
                    double price;

                    if (_frozenPrice)
                    {
                        double h = Canvas.StepHeight / 2.0;

                        if (_freezeOnlyPrice2)
                        {
                            if (info.ControlPoint1.Y <= info.ControlPoint0.Y)
                                price = Canvas.GetValue(info.ControlPoint1.Y - h);
                            else
                                price = ControlPoints[0].Y;
                        }
                        else
                            price = Canvas.GetValue(
                                Math.Min(FrozenPoints[0].y, FrozenPoints[1].y) - h
                            );
                    }
                    else
                        price = Math.Max(ControlPoints[0].Y, ControlPoints[1].Y);

                    labels.Add(new ObjectLabelInfo(price, LineColor));
                }

                if (ShowBottomPriceLabel)
                {
                    double price;

                    if (_frozenPrice)
                    {
                        double h = Canvas.StepHeight / 2.0;

                        if (_freezeOnlyPrice2)
                        {
                            if (info.ControlPoint1.Y <= info.ControlPoint0.Y)
                                price = ControlPoints[0].Y;
                            else
                                price = Canvas.GetValue(info.ControlPoint1.Y - h);
                        }
                        else
                            price = Canvas.GetValue(
                                Math.Max(FrozenPoints[0].y, FrozenPoints[1].y) - h
                            );
                    }
                    else
                        price = Math.Min(ControlPoints[0].Y, ControlPoints[1].Y);

                    labels.Add(new ObjectLabelInfo(price, LineColor));
                }
            }
        }

        public override void DrawControlPoints(DxVisualQueue visual)
        {
            if (info == null)
                return;

            areControlPointsVisible = true;

            if (Lock)
            {
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint0);
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint1);
            }
            else if (InMove)
            {
                if (controlPointEditingIndex != null)
                {
                    if (controlPointEditingIndex != 0)
                        DrawH.ControlPointCorner(visual, info.ControlPoint0);
                    if (controlPointEditingIndex != 1)
                        DrawH.ControlPointCorner(visual, info.ControlPoint1);
                }

                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                DrawH.ControlPointCorner(visual, info.ControlPoint0);
                DrawH.ControlPointCorner(visual, info.ControlPoint1);

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

            if (_frozenTime || ShowMirror)
                return true;

            double date0 = ControlPoints[0].X;
            double date1 = ControlPoints[1].X;

            double maxDate;
            if (date0 <= date1)
                maxDate = date1;
            else
                maxDate = date0;

            return maxDate >= DataProvider.Dates[0];
        }

        protected override bool IsObjectInArea()
        {
            return isObjectInArea;
        }

        protected override bool InObject(int x, int y)
        {
            if (info == null)
                return false;

            return info.Line.IntersectsWith(x, y)
                || info.Tip0.IntersectsWith(x, y)
                || info.Tip1.IntersectsWith(x, y)
                || info.Text.Contains(x, y)
                || (info.Mirror?.IntersectsWith(x, y) ?? false)
                || (info.MirrorTip0?.IntersectsWith(x, y) ?? false)
                || (info.MirrorTip1?.IntersectsWith(x, y) ?? false)
                || info.MirrorText.Contains(x, y);
        }

        public override int GetControlPoint(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetControlPointIndex(x, y, info.ControlPoint0, info.ControlPoint1);
        }

        public override int GetExtraPoint(int x, int y)
        {
            return -1;
        }

        protected override int GetMinDist(int x, int y)
        {
            if (info == null)
                return -1;

            {
                double distance = info.Line.DistanceFrom(x, y);
                if (info.Line.Intersects(distance))
                    return (int)distance;
            }

            {
                double distance = info.Tip0.DistanceFrom(x, y);
                if (info.Tip0.Intersects(distance))
                    return (int)distance;
            }

            {
                double distance = info.Tip1.DistanceFrom(x, y);
                if (info.Tip1.Intersects(distance))
                    return (int)distance;
            }

            if (info.Mirror != null)
            {
                double distance = info.Mirror.Value.DistanceFrom(x, y);
                if (info.Mirror.Value.Intersects(distance))
                    return (int)distance;
            }

            if (info.MirrorTip0 != null)
            {
                double distance = info.MirrorTip0.Value.DistanceFrom(x, y);
                if (info.MirrorTip0.Value.Intersects(distance))
                    return (int)distance;
            }

            if (info.MirrorTip1 != null)
            {
                double distance = info.MirrorTip1.Value.DistanceFrom(x, y);
                if (info.MirrorTip1.Value.Intersects(distance))
                    return (int)distance;
            }

            return DrawH.GetMinDistance(x, y, info.Text, info.MirrorText);
        }

        public override void BeginDrag()
        {
            if (_frozenPrice && _freezeOnlyPrice2)
            {
                overrideFreezeOnlyPrice2 = true;
                _freezeOnlyPrice2 = false;

                double max = Canvas.Rect.Height - LineWidth;
                double distance = FrozenPoints[1].y - FrozenPoints[0].y;

                FrozenPoints[0].y = Math.Max(0, Math.Min(max, Canvas.GetY(ControlPoints[0].Y)));
                FrozenPoints[1].y = FrozenPoints[0].y + distance;
            }

            base.BeginDrag();

            lastControlPoints = new ObjectPoint[2]
            {
                new ObjectPoint { X = ControlPoints[0].X, Y = ControlPoints[0].Y },
                new ObjectPoint { X = ControlPoints[1].X, Y = ControlPoints[1].Y },
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
            {
                ControlPoints[0].X = lastControlPoints[0].X;
                ControlPoints[1].X = lastControlPoints[1].X;
            }
            if (dy == 0)
            {
                ControlPoints[0].Y = lastControlPoints[0].Y;
                ControlPoints[1].Y = lastControlPoints[1].Y;
            }

            if (_frozenTime)
            {
                double max = Canvas.Rect.Width - LineWidth;

                int length = FrozenPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].x = Math.Max(0, Math.Min(max, lastFrozenPoints[i].x - dx));
                }
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height - LineWidth;

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

            controlPointEditingIndex = index;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                int other = index == 0 ? 1 : 0;
                Point o = ToPoint(ControlPoints[other]);

                if (_frozenTime)
                    o.X = Canvas.Rect.Width - FrozenPoints[other].x;
                if (_frozenPrice)
                    o.Y = FrozenPoints[other].y;

                Point p = ToPoint(ControlPoints[index]);
                DrawH.SnapToNearestAngle(ref o, ref p);

                if (_frozenTime)
                {
                    double canvasWidth = Canvas.Rect.Width;
                    double max = canvasWidth - LineWidth;
                    FrozenPoints[index].x = Math.Max(0, Math.Min(max, canvasWidth - p.X));
                }
                else
                {
                    double? date = Chart.GetDate(DataProvider, Canvas, p.X);
                    if (date != null)
                        ControlPoints[index].X = date.Value;
                }

                if (_frozenPrice)
                {
                    double max = Canvas.Rect.Height - LineWidth;
                    FrozenPoints[index].y = Math.Max(0, Math.Min(max, p.Y));
                }
                else
                {
                    double price = Canvas.GetValue(p.Y - (Canvas.StepHeight / 2.0));
                    ControlPoints[index].Y = price;
                }
            }
            else
            {
                if (_frozenTime)
                {
                    double canvasWidth = Canvas.Rect.Width;
                    double max = canvasWidth - LineWidth;
                    FrozenPoints[index].x = Math.Max(
                        0,
                        Math.Min(max, canvasWidth - ToPoint(ControlPoints[index]).X)
                    );
                }

                if (_frozenPrice)
                {
                    double max = Canvas.Rect.Height - LineWidth;
                    FrozenPoints[index].y = Math.Max(
                        0,
                        Math.Min(max, Canvas.GetY(ControlPoints[index].Y))
                    );
                }
            }
        }

        public override void ControlPointsChanged()
        {
            base.ControlPointsChanged();

            controlPointEditingIndex = null;

            if (overrideFreezeOnlyPrice2)
            {
                overrideFreezeOnlyPrice2 = false;
                _freezeOnlyPrice2 = true;
            }

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

            LineColor = theme.ChartObjectLineColor;
        }

        public override void CopyTemplate(ObjectBase objectBase, bool style)
        {
            if (objectBase is Arrow o)
            {
                Visibility = o.Visibility;
                System = o.System;
                FreezeOnlyPrice2 = o.FreezeOnlyPrice2;

                LineColor = o.LineColor;
                LineWidth = o.LineWidth;
                LineStyle = o.LineStyle;

                ShowMirror = o.ShowMirror;
                ShowMirrorText = o.ShowMirrorText;
                MirrorWidth = o.MirrorWidth;
                MirrorOffset = o.MirrorOffset;

                ShowText = o.ShowText;
                Text = o.Text;
                TextHorizontalAlign = o.TextHorizontalAlign;
                TextVerticalAlign = o.TextVerticalAlign;
                TextOptions = o.TextOptions.DeepCopy();

                ShowTopPriceLabel = o.ShowTopPriceLabel;
                ShowBottomPriceLabel = o.ShowBottomPriceLabel;

                if (o.FrozenPoints?.Length == 2)
                {
                    FrozenPoints = new FrozenPoint[2]
                    {
                        new FrozenPoint { X = o.FrozenPoints[0].X, Y = o.FrozenPoints[0].Y },
                        new FrozenPoint { X = o.FrozenPoints[1].X, Y = o.FrozenPoints[1].Y },
                    };
                }
            }

            base.CopyTemplate(objectBase, style);
        }
    }
}
