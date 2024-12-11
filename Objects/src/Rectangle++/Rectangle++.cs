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
using TigerTrade.Chart.Alerts;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Objects.Common;
using TigerTrade.Core.UI.Converters;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;
using DrawH = CustomCommon.Helpers.Draw;

namespace ObjectsPlusPlus.Rectangle
{
    /// <summary>
    /// CP0 --- EP2 --- EP0
    ///  |               |
    /// EP5             EP3
    ///  |               |
    /// EP1 --- EP4 --- CP1
    /// </summary>
    internal class RectangleInfo
    {
        /// <summary>
        /// Top, Left
        /// </summary>
        public Point ControlPoint0;

        /// <summary>
        /// Bottom, Right
        /// </summary>
        public Point ControlPoint1;

        /// <summary>
        /// Top, Right
        /// </summary>
        public Point ExtraPoint0;

        /// <summary>
        /// Bottom, Left
        /// </summary>
        public Point ExtraPoint1;

        /// <summary>
        /// Top, Middle
        /// </summary>
        public Point ExtraPoint2;

        /// <summary>
        /// Middle, Right
        /// </summary>
        public Point ExtraPoint3;

        /// <summary>
        /// Bottom, Middle
        /// </summary>
        public Point ExtraPoint4;

        /// <summary>
        /// Middle, Left
        /// </summary>
        public Point ExtraPoint5;

        public Rect Rectangle;
        public Rect Text;
        public Rect Mirror;
        public Rect MirrorText;
        public Rect Alert;

        public bool RectangleIntersectsWithCanvas;
        public bool TextIntersectsWithCanvas;
        public bool MirrorIntersectsWithCanvas;
        public bool MirrorTextIntersectsWithCanvas;
        public bool AlertIntersectsWithCanvas;

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
        Name = "RectanglePlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("RectanglePlusPlusObject", "Rectangle++", 2, Type = typeof(Rectangle))]
    internal sealed class Rectangle : ObjectBase
    {
        private static double TextMarginX = 5;
        private static double TextMarginY = 5;
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private int? controlPointEditingIndex = null;
        private int? extraPointEditingIndex = null;
        private RectangleInfo? info;

        private ObjectPoint[] lastControlPoints;
        private FrozenPoint[] lastFrozenPoints;
        private bool fixedCoordinateSystemOnSetup = false;

        private long lastDrawTimestamp = 0;

        private AlertLevel? alertTop;
        private AlertLevel? alertBottom;

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

        private bool _drawBorder;

        [DataMember(Name = "DrawBorder")]
        [Category("Border"), DisplayName("Border")]
        public bool DrawBorder
        {
            get => _drawBorder;
            set
            {
                if (value == _drawBorder)
                    return;

                _drawBorder = value;

                OnPropertyChanged(nameof(DrawBorder));
            }
        }

        private XBrush _lineBrush;

        private XPen _linePen;

        private XColor _lineColor;

        [DataMember(Name = "LineColor")]
        [Category("Border"), DisplayName("Line Color")]
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

        private int _lineWidth;

        [DataMember(Name = "LineWidth")]
        [Category("Border"), DisplayName("Line Width")]
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

                OnPropertyChanged(nameof(LineWidth));
            }
        }

        private XDashStyle _lineStyle;

        [DataMember(Name = "LineStyle")]
        [Category("Border"), DisplayName("Line Style")]
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

        private bool _drawBack;

        [DataMember(Name = "Background")]
        [Category("Background"), DisplayName("Background")]
        public bool DrawBack
        {
            get => _drawBack;
            set
            {
                if (value == _drawBack)
                    return;

                _drawBack = value;

                OnPropertyChanged(nameof(DrawBack));
            }
        }

        private XBrush _backBrush;

        private XColor _backColor;

        [DataMember(Name = "BackColor")]
        [Category("Background"), DisplayName("Back Color")]
        public XColor BackColor
        {
            get => _backColor;
            set
            {
                if (value == _backColor)
                    return;

                _backColor = value;

                _backBrush = new XBrush(_backColor);

                OnPropertyChanged(nameof(BackColor));
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

        private ChartAlertSettings _alert;

        [DataMember(Name = "Alert")]
        [Category("Alert"), DisplayName("Alert")]
        public ChartAlertSettings Alert
        {
            get => _alert ?? (_alert = new ChartAlertSettings());
            set
            {
                if (Equals(value, _alert))
                    return;

                _alert = value;

                OnPropertyChanged(nameof(Alert));
            }
        }

        private double _alertDistance;

        [DataMember(Name = "AlertDistance")]
        [Category("Alert"), DisplayName("Distance")]
        public double AlertDistance
        {
            get => _alertDistance;
            set
            {
                value = Math.Max(0, value);

                if (value == _alertDistance)
                    return;

                _alertDistance = value;

                OnPropertyChanged(nameof(AlertDistance));
            }
        }

        private AlertDistanceUnit _alertUnit;

        [DataMember(Name = "AlertUnit")]
        [Category("Alert"), DisplayName("Distance Unit")]
        public AlertDistanceUnit AlertUnit
        {
            get => _alertUnit;
            set
            {
                if (value == _alertUnit)
                    return;

                _alertUnit = value;

                OnPropertyChanged(nameof(AlertUnit));
            }
        }

        private AlertFrequency _alertFrequency;

        [DataMember(Name = "AlertFrequency")]
        [Category("Alert"), DisplayName("Frequency")]
        public AlertFrequency AlertFrequency
        {
            get => _alertFrequency;
            set
            {
                if (value == _alertFrequency)
                    return;

                _alertFrequency = value;

                Alert.Active = true;
                if (_alertFrequency == AlertFrequency.Once)
                    Alert.Execution = ChartAlertExecution.OnlyOnce;
                else if (_alertFrequency == AlertFrequency.OncePerBar)
                    Alert.Execution = ChartAlertExecution.EveryTime;
                else if (_alertFrequency == AlertFrequency.EveryTime)
                    Alert.Execution = ChartAlertExecution.EveryTime;

                OnPropertyChanged(nameof(AlertFrequency));
            }
        }

        private AlertOptions _alertOptions;

        [DataMember(Name = "AlertOptions")]
        [Category("Alert"), DisplayName("Alert Options")]
        public AlertOptions AlertOptions
        {
            get => _alertOptions ?? (_alertOptions = new AlertOptions());
            set
            {
                if (Equals(value, _alertOptions))
                    return;

                _alertOptions = value;

                OnPropertyChanged(nameof(AlertOptions));
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

        public override ObjectPoint[] ExtraPoints
        {
            get
            {
                ref ObjectPoint p0 = ref ControlPoints[0];
                ref ObjectPoint p1 = ref ControlPoints[1];

                double x0 = p0.X;
                double y0 = p0.Y;
                double x1 = p1.X;
                double y1 = p1.Y;

                (double mx, double my) = DrawH.GetMiddleControlPoint(
                    DataProvider,
                    Canvas,
                    x0,
                    y0,
                    x1,
                    y1
                );

                return new ObjectPoint[6]
                {
                    new ObjectPoint(x1, y0),
                    new ObjectPoint(x0, y1),
                    new ObjectPoint(mx, y0),
                    new ObjectPoint(x1, my),
                    new ObjectPoint(mx, y1),
                    new ObjectPoint(x0, my),
                };
            }
        }

        public Rectangle()
        {
            Color color = Colors.Lime;

            Visibility = TimeframeVisibility.Disabled;
            System = CoordinateSystem.Default;

            DrawBorder = true;
            LineColor = color;
            LineWidth = 1;
            LineStyle = XDashStyle.Solid;

            DrawBack = false;
            BackColor = Color.FromArgb(50, color.R, color.G, color.B);

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

            Alert = new ChartAlertSettings();
            AlertDistance = 0;
            AlertUnit = AlertDistanceUnit.Tick;
            AlertFrequency = AlertFrequency.EveryTime;
            AlertOptions = new AlertOptions();

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
                    double max = canvasWidth - (DrawBorder ? LineWidth : 0);
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
                    double max = Canvas.Rect.Height - (DrawBorder ? LineWidth : 0);
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
                double max = canvasWidth - (DrawBorder ? LineWidth : 0);
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
                double max = Canvas.Rect.Height - (DrawBorder ? LineWidth : 0);
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
            Point p1 = ToPoint(ControlPoints[1]);
            Point e0 = ToPoint(ExtraPoints[0]);
            Point e1 = ToPoint(ExtraPoints[1]);
            Point e2 = ToPoint(ExtraPoints[2]);
            Point e3 = ToPoint(ExtraPoints[3]);
            Point e4 = ToPoint(ExtraPoints[4]);
            Point e5 = ToPoint(ExtraPoints[5]);

            FixCoordinateSystemOnSetup();

            if (_frozenTime)
            {
                double canvasWidth = Canvas.Rect.Width;
                p0.X = canvasWidth - FrozenPoints[0].x;
                e5.X = canvasWidth - FrozenPoints[0].x;
                e1.X = canvasWidth - FrozenPoints[0].x;

                p1.X = canvasWidth - FrozenPoints[1].x;
                e3.X = canvasWidth - FrozenPoints[1].x;
                e0.X = canvasWidth - FrozenPoints[1].x;

                double midX = canvasWidth - ((FrozenPoints[0].x + FrozenPoints[1].x) / 2);
                e2.X = midX;
                e4.X = midX;
            }

            if (_frozenPrice)
            {
                p0.Y = FrozenPoints[0].y;
                e2.Y = FrozenPoints[0].y;
                e0.Y = FrozenPoints[0].y;

                p1.Y = FrozenPoints[1].y;
                e4.Y = FrozenPoints[1].y;
                e1.Y = FrozenPoints[1].y;

                double midY = (FrozenPoints[0].y + FrozenPoints[1].y) / 2;
                e5.Y = midY;
                e3.Y = midY;
            }

            Rect rectangle = new Rect(p0, p1);

            double halfBorderWidth = 0;
            if (DrawBorder)
                halfBorderWidth = Math.Ceiling(((double)_linePen.Width) / 2.0);

            Rect mirror = Rect.Empty;
            if (ShowMirror)
            {
                double width = Math.Max(0, MirrorWidth);

                mirror = new Rect(
                    Canvas.Rect.Right - width - MirrorOffset - halfBorderWidth,
                    rectangle.Y,
                    width,
                    rectangle.Height
                );

                if (mirror.X < rectangle.X)
                    mirror = Rect.Empty;
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
                    x = rectangle.X + (rectangle.Width / 2) - (textWidth / 2);
                else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                    x = rectangle.Right - textWidth;
                else
                    x = rectangle.X;

                if (TextVerticalAlign == TextVerticalAlign.Center)
                {
                    y = rectangle.Y + (rectangle.Height / 2) - (textHeight / 2);

                    if (TextHorizontalAlign == TextHorizontalAlign.Left)
                        x += TextMarginX + halfBorderWidth;
                    else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                        x -= TextMarginX + halfBorderWidth;
                }
                else if (TextVerticalAlign == TextVerticalAlign.Bottom)
                    y = rectangle.Bottom + TextMarginY + halfBorderWidth;
                else
                    y = rectangle.Y - textHeight - TextMarginY - halfBorderWidth;

                text = new Rect(x, y, textWidth, textHeight);
            }

            Rect mirrorText = Rect.Empty;
            if (ShowMirrorText && Text != "" && !mirror.IsEmpty)
            {
                if (font == null)
                    font = new XFont(Canvas.ChartFont.Name, TextOptions.FontSize);
                double textWidth = font.GetWidth(Text);
                double textHeight = font.GetHeight();

                double x;
                double y;

                if (TextHorizontalAlign == TextHorizontalAlign.Center)
                    x = mirror.X + (mirror.Width / 2) - (textWidth / 2);
                else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                    x = mirror.Right - textWidth;
                else
                    x = mirror.X;

                if (TextVerticalAlign == TextVerticalAlign.Center)
                {
                    y = mirror.Y + (mirror.Height / 2) - (textHeight / 2);

                    if (TextHorizontalAlign == TextHorizontalAlign.Left)
                        x += TextMarginX + halfBorderWidth;
                    else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                        x -= TextMarginX + halfBorderWidth;
                }
                else if (TextVerticalAlign == TextVerticalAlign.Bottom)
                    y = mirror.Bottom + TextMarginY + halfBorderWidth;
                else
                    y = mirror.Y - textHeight - TextMarginY - halfBorderWidth;

                mirrorText = new Rect(x, y, textWidth, textHeight);
            }

            Rect alert = Rect.Empty;
            if (Alert.IsActive && AlertOptions.ShowBell && !this._frozenPrice)
            {
                double priceTop = Math.Max(ControlPoints[0].Y, ControlPoints[1].Y);
                double priceBottom = Math.Min(ControlPoints[0].Y, ControlPoints[1].Y);

                if (this.alertTop == null)
                    this.alertTop = new AlertLevel(
                        DataProvider.Step,
                        AlertFrequency,
                        priceTop,
                        AlertUnit,
                        AlertDistance
                    );
                else
                    this.alertTop.UpdatePropertiesIfNeeded(
                        AlertFrequency,
                        priceTop,
                        AlertUnit,
                        AlertDistance
                    );

                if (this.alertBottom == null)
                    this.alertBottom = new AlertLevel(
                        DataProvider.Step,
                        AlertFrequency,
                        priceBottom,
                        AlertUnit,
                        AlertDistance
                    );
                else
                    this.alertBottom.UpdatePropertiesIfNeeded(
                        AlertFrequency,
                        priceBottom,
                        AlertUnit,
                        AlertDistance
                    );

                double currentPrice = Chart.GetRealtimePriceSafe(DataProvider) ?? 0;
                double deltaMinTop = Math.Abs(currentPrice - alertTop.minPrice);
                double deltaMaxTop = Math.Abs(currentPrice - alertTop.maxPrice);
                double deltaMinBottom = Math.Abs(currentPrice - alertBottom.minPrice);
                double deltaMaxBottom = Math.Abs(currentPrice - alertBottom.maxPrice);

                double price;
                if (
                    deltaMaxTop <= deltaMinTop
                    && deltaMaxTop <= deltaMinBottom
                    && deltaMaxTop <= deltaMaxBottom
                )
                    price = alertTop.maxPrice;
                else if (
                    deltaMinBottom <= deltaMinTop
                    && deltaMinBottom <= deltaMaxTop
                    && deltaMinBottom <= deltaMaxBottom
                )
                    price = alertBottom.minPrice;
                else if (
                    deltaMinTop <= deltaMaxTop
                    && deltaMinTop <= deltaMinBottom
                    && deltaMinTop <= deltaMaxBottom
                )
                    price = alertTop.minPrice;
                else if (
                    deltaMaxBottom <= deltaMinTop
                    && deltaMaxBottom <= deltaMaxTop
                    && deltaMaxBottom <= deltaMinBottom
                )
                    price = alertBottom.maxPrice;
                else
                    price = (alertTop.maxPrice + alertBottom.minPrice) / 2;

                alert = new Rect(
                    (mirror.IsEmpty ? Canvas.Rect.Right : mirror.X)
                        - DrawH.AlertBellWidth
                        - AlertOptions.BellOffset,
                    Canvas.GetY(price) - DrawH.AlertBellHalfHeight,
                    DrawH.AlertBellWidth,
                    DrawH.AlertBellHeight
                );

                if (alert.X < rectangle.X)
                    alert = Rect.Empty;
            }

            info = new RectangleInfo
            {
                ControlPoint0 = p0,
                ControlPoint1 = p1,
                ExtraPoint0 = e0,
                ExtraPoint1 = e1,
                ExtraPoint2 = e2,
                ExtraPoint3 = e3,
                ExtraPoint4 = e4,
                ExtraPoint5 = e5,

                Rectangle = rectangle,
                Text = text,
                Mirror = mirror,
                MirrorText = mirrorText,
                Alert = alert,

                RectangleIntersectsWithCanvas = Canvas.Rect.IntersectsWith(rectangle),
                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                MirrorIntersectsWithCanvas = Canvas.Rect.IntersectsWith(mirror),
                MirrorTextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(mirrorText),
                AlertIntersectsWithCanvas = Canvas.Rect.IntersectsWith(alert),

                Font = font,
            };

            isObjectInArea =
                info.RectangleIntersectsWithCanvas
                || info.TextIntersectsWithCanvas
                || info.MirrorIntersectsWithCanvas
                || info.MirrorTextIntersectsWithCanvas
                || info.AlertIntersectsWithCanvas;
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

            if (DrawBack && !_backColor.IsTransparent)
            {
                if (info.RectangleIntersectsWithCanvas)
                    visual.FillRectangle(_backBrush, info.Rectangle);

                if (!info.Mirror.IsEmpty && info.MirrorIntersectsWithCanvas)
                    visual.FillRectangle(_backBrush, info.Mirror);
            }

            if (DrawBorder && !_lineColor.IsTransparent)
            {
                if (info.RectangleIntersectsWithCanvas)
                    visual.DrawRectangle(_linePen, info.Rectangle);

                if (!info.Mirror.IsEmpty && info.MirrorIntersectsWithCanvas)
                    visual.DrawRectangle(_linePen, info.Mirror);
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

            if (!info.Alert.IsEmpty && info.AlertIntersectsWithCanvas)
                DrawH.AlertBell(visual, info.Alert.X, info.Alert.Y);

            if (areControlPointsVisible)
            {
                double h = Canvas.StepHeight / 2.0;

                int length = ControlPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    double price;
                    if (_frozenPrice)
                        price = Canvas.GetValue(FrozenPoints[i].y - h);
                    else
                        price = ControlPoints[i].Y;

                    labels.Add(new ObjectLabelInfo(price, LineColor));
                }

                areControlPointsVisible = false;
            }
            else
            {
                if (ShowTopPriceLabel)
                {
                    double h = Canvas.StepHeight / 2.0;

                    double price;
                    if (_frozenPrice)
                        price = Canvas.GetValue(Math.Min(FrozenPoints[0].y, FrozenPoints[1].y) - h);
                    else
                        price = Math.Max(ControlPoints[0].Y, ControlPoints[1].Y);

                    labels.Add(new ObjectLabelInfo(price, LineColor));
                }

                if (ShowBottomPriceLabel)
                {
                    double h = Canvas.StepHeight / 2.0;

                    double price;
                    if (_frozenPrice)
                        price = Canvas.GetValue(Math.Max(FrozenPoints[0].y, FrozenPoints[1].y) - h);
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
                double mx = info.ExtraPoint2.X;
                if (mx > info.Rectangle.Left && mx < info.Rectangle.Right)
                {
                    DrawH.ControlPointLockedEdge(visual, info.ExtraPoint2);
                    DrawH.ControlPointLockedEdge(visual, info.ExtraPoint4);
                }

                double my = info.ExtraPoint3.Y;
                if (my > info.Rectangle.Top && my < info.Rectangle.Bottom)
                {
                    DrawH.ControlPointLockedEdge(visual, info.ExtraPoint3);
                    DrawH.ControlPointLockedEdge(visual, info.ExtraPoint5);
                }

                DrawH.ControlPointLockedCorner(visual, info.ExtraPoint0);
                DrawH.ControlPointLockedCorner(visual, info.ExtraPoint1);
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint0);
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint1);
            }
            else if (InMove)
            {
                if (controlPointEditingIndex != null || extraPointEditingIndex != null)
                {
                    double mx = info.ExtraPoint2.X;
                    if (mx > info.Rectangle.Left && mx < info.Rectangle.Right)
                    {
                        if (extraPointEditingIndex != 2)
                            DrawH.ControlPointEdge(visual, info.ExtraPoint2);
                        if (extraPointEditingIndex != 4)
                            DrawH.ControlPointEdge(visual, info.ExtraPoint4);
                    }

                    double my = info.ExtraPoint3.Y;
                    if (my > info.Rectangle.Top && my < info.Rectangle.Bottom)
                    {
                        if (extraPointEditingIndex != 3)
                            DrawH.ControlPointEdge(visual, info.ExtraPoint3);
                        if (extraPointEditingIndex != 5)
                            DrawH.ControlPointEdge(visual, info.ExtraPoint5);
                    }

                    if (extraPointEditingIndex != 0)
                        DrawH.ControlPointCorner(visual, info.ExtraPoint0);
                    if (extraPointEditingIndex != 1)
                        DrawH.ControlPointCorner(visual, info.ExtraPoint1);
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
                double mx = info.ExtraPoint2.X;
                if (mx > info.Rectangle.Left && mx < info.Rectangle.Right)
                {
                    DrawH.ControlPointEdge(visual, info.ExtraPoint2);
                    DrawH.ControlPointEdge(visual, info.ExtraPoint4);
                }

                double my = info.ExtraPoint3.Y;
                if (my > info.Rectangle.Top && my < info.Rectangle.Bottom)
                {
                    DrawH.ControlPointEdge(visual, info.ExtraPoint3);
                    DrawH.ControlPointEdge(visual, info.ExtraPoint5);
                }

                DrawH.ControlPointCorner(visual, info.ExtraPoint0);
                DrawH.ControlPointCorner(visual, info.ExtraPoint1);
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

            if (_frozenTime || ShowMirror || Alert.IsActive)
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

            return info.Rectangle.Contains(x, y)
                || info.Text.Contains(x, y)
                || info.Mirror.Contains(x, y)
                || info.MirrorText.Contains(x, y)
                || info.Alert.Contains(x, y);
        }

        public override int GetControlPoint(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetControlPointIndex(x, y, info.ControlPoint0, info.ControlPoint1);
        }

        public override int GetExtraPoint(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetControlPointIndex(
                x,
                y,
                info.ExtraPoint0,
                info.ExtraPoint1,
                info.ExtraPoint2,
                info.ExtraPoint3,
                info.ExtraPoint4,
                info.ExtraPoint5
            );
        }

        protected override int GetMinDist(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetMinDistance(
                x,
                y,
                info.Rectangle,
                info.Text,
                info.Mirror,
                info.MirrorText,
                info.Alert
            );
        }

        public override void BeginDrag()
        {
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
                double max = Canvas.Rect.Width - (DrawBorder ? LineWidth : 0);

                int length = FrozenPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    FrozenPoints[i].x = Math.Max(0, Math.Min(max, lastFrozenPoints[i].x - dx));
                }
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height - (DrawBorder ? LineWidth : 0);

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

            if (_frozenTime)
            {
                double canvasWidth = Canvas.Rect.Width;
                double max = canvasWidth - (DrawBorder ? LineWidth : 0);
                FrozenPoints[index].x = Math.Max(
                    0,
                    Math.Min(max, canvasWidth - ToPoint(ControlPoints[index]).X)
                );
            }

            if (_frozenPrice)
            {
                double max = Canvas.Rect.Height - (DrawBorder ? LineWidth : 0);
                FrozenPoints[index].y = Math.Max(
                    0,
                    Math.Min(max, Canvas.GetY(ControlPoints[index].Y))
                );
            }
        }

        public override void ExtraPointChanged(int index, ObjectPoint op)
        {
            extraPointEditingIndex = index;

            int? xIndex = null;
            int? yIndex = null;

            if (index == 0)
            {
                xIndex = 1;
                yIndex = 0;
            }
            else if (index == 1)
            {
                xIndex = 0;
                yIndex = 1;
            }
            else if (index == 2)
                yIndex = 0;
            else if (index == 3)
                xIndex = 1;
            else if (index == 4)
                yIndex = 1;
            else if (index == 5)
                xIndex = 0;

            if (xIndex != null)
            {
                int i = xIndex.Value;
                ControlPoints[i].X = op.X;

                if (_frozenTime)
                {
                    double canvasWidth = Canvas.Rect.Width;
                    double max = canvasWidth - (DrawBorder ? LineWidth : 0);
                    FrozenPoints[i].x = Math.Max(
                        0,
                        Math.Min(max, canvasWidth - ToPoint(ControlPoints[i]).X)
                    );
                }
            }

            if (yIndex != null)
            {
                int i = yIndex.Value;
                ControlPoints[i].Y = op.Y;

                if (_frozenPrice)
                {
                    double max = Canvas.Rect.Height - (DrawBorder ? LineWidth : 0);
                    FrozenPoints[i].y = Math.Max(0, Math.Min(max, Canvas.GetY(ControlPoints[i].Y)));
                }
            }
        }

        public override void ControlPointsChanged()
        {
            base.ControlPointsChanged();

            controlPointEditingIndex = null;
            extraPointEditingIndex = null;

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
            if (!Alert.IsActive || DataProvider == null || this._frozenPrice)
            {
                alertTop = null;
                alertBottom = null;
                return;
            }

            int index = DataProvider.Count - 1;
            if (index < 0)
            {
                alertTop = null;
                alertBottom = null;
                return;
            }

            double step = DataProvider.Step;
            double price = DataProvider.GetRawCluster(index).Close * step;

            double priceTop = Math.Max(ControlPoints[0].Y, ControlPoints[1].Y);
            double priceBottom = Math.Min(ControlPoints[0].Y, ControlPoints[1].Y);

            if (this.alertTop == null)
                this.alertTop = new AlertLevel(
                    step,
                    AlertFrequency,
                    priceTop,
                    AlertUnit,
                    AlertDistance
                );
            else
                this.alertTop.UpdatePropertiesIfNeeded(
                    AlertFrequency,
                    priceTop,
                    AlertUnit,
                    AlertDistance
                );

            if (this.alertBottom == null)
                this.alertBottom = new AlertLevel(
                    step,
                    AlertFrequency,
                    priceBottom,
                    AlertUnit,
                    AlertDistance
                );
            else
                this.alertBottom.UpdatePropertiesIfNeeded(
                    AlertFrequency,
                    priceBottom,
                    AlertUnit,
                    AlertDistance
                );

            bool isAlertTopTriggered = this.alertTop.Check(price, index);
            bool isAlertBottomTriggered = this.alertBottom.Check(price, index);

            if (isAlertTopTriggered)
            {
                if (AlertManager.Check(DataProvider.Symbol.Code, AlertOptions.Throttle))
                    AddAlert(Alert, $"[Rectangle++] Top at {priceTop}");
            }
            else if (isAlertBottomTriggered)
            {
                if (AlertManager.Check(DataProvider.Symbol.Code, AlertOptions.Throttle))
                    AddAlert(Alert, $"[Rectangle++] Bottom at {priceBottom}");
            }
        }

        public override void ApplyTheme(IChartTheme theme)
        {
            base.ApplyTheme(theme);

            LineColor = theme.ChartObjectLineColor;
            BackColor = theme.ChartObjectFillColor;
        }

        public override void CopyTemplate(ObjectBase objectBase, bool style)
        {
            if (objectBase is Rectangle o)
            {
                Visibility = o.Visibility;
                System = o.System;

                DrawBorder = o.DrawBorder;
                LineColor = o.LineColor;
                LineWidth = o.LineWidth;
                LineStyle = o.LineStyle;

                DrawBack = o.DrawBack;
                BackColor = o.BackColor;

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

                Alert.Copy(o.Alert, !style);
                AlertDistance = o.AlertDistance;
                AlertUnit = o.AlertUnit;
                AlertFrequency = o.AlertFrequency;
                AlertOptions = o.AlertOptions.DeepCopy();
                OnPropertyChanged(nameof(Alert));

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
