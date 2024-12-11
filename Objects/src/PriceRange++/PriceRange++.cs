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

namespace ObjectsPlusPlus.PriceRange
{
    /// <summary>
    /// ------- CP0 ------
    /// |                |
    /// |       EP0      |
    /// |                |
    /// ------- CP1 ------
    /// </summary>
    internal class PriceRangeInfo
    {
        /// <summary>
        /// Top, Center
        /// </summary>
        public Point ControlPoint0;

        /// <summary>
        /// Bottom, Center
        /// </summary>
        public Point ControlPoint1;

        /// <summary>
        /// Center, Center
        /// </summary>
        public Point ExtraPoint0;

        public Rect PriceRange;
        public Rect Text;
        public Rect Alert;

        public bool PriceRangeIntersectsWithCanvas;
        public bool TextIntersectsWithCanvas;
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
        Name = "PriceRangePlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("PriceRangePlusPlusObject", "PriceRange++", 2, Type = typeof(PriceRange))]
    internal sealed class PriceRange : ObjectBase
    {
        private static int MinLineHitbox = 4;
        private static int MiddlePointHitbox = 12;
        private static int MiddlePointDoubleHitbox = MiddlePointHitbox * 2;
        private static double TextMarginX = 3;
        private static double TextMarginY = 5;
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private int? controlPointEditingIndex = null;
        private PriceRangeInfo? info;

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

        protected override int PenWidth => LineWidth;

        public PriceRange()
        {
            Color color = Colors.Lime;

            Visibility = TimeframeVisibility.Disabled;

            DrawBorder = true;
            LineColor = color;
            LineWidth = 1;
            LineStyle = XDashStyle.Solid;

            DrawBack = false;
            BackColor = Color.FromArgb(50, color.R, color.G, color.B);

            ShowText = false;
            Text = "Text";
            TextHorizontalAlign = TextHorizontalAlign.Left;
            TextVerticalAlign = TextVerticalAlign.Top;
            TextOptions = new TextOptions();

            ShowTopPriceLabel = false;
            ShowBottomPriceLabel = false;

            Alert = new ChartAlertSettings();
            AlertDistance = 0;
            AlertUnit = AlertDistanceUnit.Tick;
            AlertFrequency = AlertFrequency.EveryTime;
            AlertOptions = new AlertOptions();
        }

        protected override void Prepare()
        {
            if (InSetup)
            {
                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);
            }

            double canvasWidth = Canvas.Rect.Width;
            double mx = canvasWidth / 2;
            Point p0 = new Point(mx, Canvas.GetY(ControlPoints[0].Y));
            Point p1 = new Point(mx, Canvas.GetY(ControlPoints[1].Y));
            Point e0 = new Point(mx, (p0.Y + p1.Y) / 2);

            Rect priceRange;
            if (p0.Y <= p1.Y)
                priceRange = new Rect(0, p0.Y, canvasWidth, p1.Y - p0.Y);
            else
                priceRange = new Rect(0, p1.Y, canvasWidth, p0.Y - p1.Y);

            double halfBorderWidth = 0;
            if (DrawBorder)
                halfBorderWidth = Math.Ceiling(((double)_linePen.Width) / 2.0);

            XFont? font = null;

            Rect text = Rect.Empty;
            if (ShowText)
            {
                if (Text != "")
                {
                    font = new XFont(Canvas.ChartFont.Name, TextOptions.FontSize);
                    double textWidth = font.GetWidth(Text);
                    double textHeight = font.GetHeight();

                    double x;
                    double y;

                    if (TextHorizontalAlign == TextHorizontalAlign.Center)
                        x = priceRange.X + (priceRange.Width / 2) - (textWidth / 2);
                    else if (TextHorizontalAlign == TextHorizontalAlign.Right)
                        x = priceRange.Right - textWidth - TextMarginX;
                    else
                        x = priceRange.X + TextMarginX;

                    if (TextVerticalAlign == TextVerticalAlign.Center)
                        y = priceRange.Y + (priceRange.Height / 2) - (textHeight / 2);
                    else if (TextVerticalAlign == TextVerticalAlign.Bottom)
                        y = priceRange.Bottom + TextMarginY + halfBorderWidth;
                    else
                        y = priceRange.Y - textHeight - TextMarginY - halfBorderWidth;

                    text = new Rect(x, y, textWidth, textHeight);
                }
            }

            Rect alert = Rect.Empty;
            if (Alert.IsActive)
            {
                if (AlertOptions.ShowBell)
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
                        Canvas.Rect.Right - DrawH.AlertBellWidth - AlertOptions.BellOffset,
                        Canvas.GetY(price) - DrawH.AlertBellHalfHeight,
                        DrawH.AlertBellWidth,
                        DrawH.AlertBellHeight
                    );
                }
            }

            info = new PriceRangeInfo
            {
                ControlPoint0 = p0,
                ControlPoint1 = p1,
                ExtraPoint0 = e0,

                PriceRange = priceRange,
                Text = text,
                Alert = alert,

                PriceRangeIntersectsWithCanvas = Canvas.Rect.IntersectsWith(priceRange),
                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                AlertIntersectsWithCanvas = Canvas.Rect.IntersectsWith(alert),

                Font = font,
            };

            isObjectInArea =
                info.PriceRangeIntersectsWithCanvas
                || info.TextIntersectsWithCanvas
                || info.AlertIntersectsWithCanvas;
        }

        protected override void Draw(DxVisualQueue visual, ref List<ObjectLabelInfo> labels)
        {
            if (info == null || !isObjectInArea)
                return;

            if (areControlPointsVisible && !Lock)
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

            if (DrawBack && !_backColor.IsTransparent && info.PriceRangeIntersectsWithCanvas)
                visual.FillRectangle(_backBrush, info.PriceRange);

            if (DrawBorder && !_lineColor.IsTransparent)
            {
                double left = info.PriceRange.Left;
                double right = info.PriceRange.Right;
                double top = info.PriceRange.Top;
                double bottom = info.PriceRange.Bottom;

                visual.DrawLine(_linePen, left, top, right, top);
                visual.DrawLine(_linePen, left, bottom, right, bottom);
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

            if (!info.Alert.IsEmpty && info.AlertIntersectsWithCanvas)
                DrawH.AlertBell(visual, info.Alert.X, info.Alert.Y);

            if (areControlPointsVisible)
            {
                int length = ControlPoints.Length;
                for (int i = 0; i < length; i++)
                {
                    labels.Add(new ObjectLabelInfo(ControlPoints[i].Y, LineColor));
                }

                areControlPointsVisible = false;
            }
            else
            {
                if (ShowTopPriceLabel)
                {
                    double priceTop = Math.Max(ControlPoints[0].Y, ControlPoints[1].Y);
                    labels.Add(new ObjectLabelInfo(priceTop, LineColor));
                }

                if (ShowBottomPriceLabel)
                {
                    double priceBottom = Math.Min(ControlPoints[0].Y, ControlPoints[1].Y);
                    labels.Add(new ObjectLabelInfo(priceBottom, LineColor));
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
                DrawH.ControlPointLockedCorner(visual, info.ExtraPoint0);
                DrawH.ControlPointLockedEdge(visual, info.ControlPoint0);
                DrawH.ControlPointLockedEdge(visual, info.ControlPoint1);
            }
            else if (InMove)
            {
                if (controlPointEditingIndex != null)
                {
                    DrawH.ControlPointCorner(visual, info.ExtraPoint0);

                    if (controlPointEditingIndex != 0)
                        DrawH.ControlPointEdge(visual, info.ControlPoint0);
                    if (controlPointEditingIndex != 1)
                        DrawH.ControlPointEdge(visual, info.ControlPoint1);
                }

                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                DrawH.ControlPointCorner(visual, info.ExtraPoint0);
                DrawH.ControlPointEdge(visual, info.ControlPoint0);
                DrawH.ControlPointEdge(visual, info.ControlPoint1);

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

            return true;
        }

        protected override bool IsObjectInArea()
        {
            return isObjectInArea;
        }

        protected override bool InObject(int x, int y)
        {
            if (info == null)
                return false;

            if (
                y >= info.ControlPoint0.Y - DrawH.ControlPointHitbox
                && y <= info.ControlPoint0.Y + DrawH.ControlPointHitbox
            )
                return true;

            if (
                y >= info.ControlPoint1.Y - DrawH.ControlPointHitbox
                && y <= info.ControlPoint1.Y + DrawH.ControlPointHitbox
            )
                return true;

            Rect middlePoint = new Rect(
                info.ExtraPoint0.X - MiddlePointHitbox,
                info.ExtraPoint0.Y - MiddlePointHitbox,
                MiddlePointDoubleHitbox,
                MiddlePointDoubleHitbox
            );

            if (middlePoint.Contains(x, y))
                return true;

            return info.Text.Contains(x, y) || info.Alert.Contains(x, y);
        }

        public override int GetControlPoint(int x, int y)
        {
            if (info == null)
                return -1;

            if (Math.Abs(info.ControlPoint0.Y - y) < DrawH.ControlPointHitbox)
                return 0;

            if (Math.Abs(info.ControlPoint1.Y - y) < DrawH.ControlPointHitbox)
                return 1;

            return -1;
        }

        public override int GetExtraPoint(int x, int y)
        {
            return -1;
        }

        protected override int GetMinDist(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetMinDistance(x, y, info.PriceRange, info.Text, info.Alert);
        }

        public override void ControlPointEditing(int index)
        {
            base.ControlPointEditing(index);

            controlPointEditingIndex = index;
        }

        public override void ControlPointsChanged()
        {
            base.ControlPointsChanged();

            controlPointEditingIndex = null;
        }

        public override void CheckAlert(List<IndicatorBase> indicators)
        {
            if (!Alert.IsActive || DataProvider == null)
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
                    AddAlert(Alert, $"[PriceRange++] Top at {priceTop}");
            }
            else if (isAlertBottomTriggered)
            {
                if (AlertManager.Check(DataProvider.Symbol.Code, AlertOptions.Throttle))
                    AddAlert(Alert, $"[PriceRange++] Bottom at {priceBottom}");
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
            if (objectBase is PriceRange o)
            {
                Visibility = o.Visibility;

                DrawBorder = o.DrawBorder;
                LineColor = o.LineColor;
                LineWidth = o.LineWidth;
                LineStyle = o.LineStyle;

                DrawBack = o.DrawBack;
                BackColor = o.BackColor;

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
            }

            base.CopyTemplate(objectBase, style);
        }
    }
}
