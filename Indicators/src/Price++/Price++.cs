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
using TigerTrade.Chart.Base.Enums;
using TigerTrade.Chart.Data;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Indicators.Enums;
using TigerTrade.Core.UI.Converters;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace IndicatorsPlusPlus.Price
{
    [DataContract(
        Name = "PricePlusPlusIndicator",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom"
    )]
    [Indicator("PricePlusPlusIndicator", "*Price++", true, Type = typeof(Price))]
    internal sealed class Price : IndicatorBase
    {
        private bool _showCountdown;

        [DataMember(Name = "ShowCountdown")]
        [Category("General"), DisplayName("Show Countdown")]
        public bool ShowCountdown
        {
            get => _showCountdown;
            set
            {
                if (value == _showCountdown)
                    return;

                _showCountdown = value;

                OnPropertyChanged(nameof(ShowCountdown));
            }
        }

        private double _widthMultiplier;

        [DataMember(Name = "WidthMultiplier")]
        [Category("General"), DisplayName("Width Multiplier")]
        public double WidthMultiplier
        {
            get => _widthMultiplier;
            set
            {
                value = Math.Max(0, value);

                if (value == _widthMultiplier)
                    return;

                _widthMultiplier = value;

                OnPropertyChanged(nameof(WidthMultiplier));
            }
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        [DataContract(
            Name = "Visibility",
            Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
        )]
        public enum Visibility
        {
            [EnumMember(Value = "Hidden"), Description("Hidden")]
            Hidden,

            [EnumMember(Value = "Right"), Description("Right")]
            Right,

            [EnumMember(Value = "Full"), Description("Full")]
            Full,
        }

        private Visibility _priceLineVisibility;

        [DataMember(Name = "PriceLineVisibility")]
        [Category("Price Line"), DisplayName("Visibility")]
        public Visibility PriceLineVisibility
        {
            get => _priceLineVisibility;
            set
            {
                if (value == _priceLineVisibility)
                    return;

                _priceLineVisibility = value;

                OnPropertyChanged(nameof(PriceLineVisibility));
            }
        }

        private bool _priceLineCustomColor;

        [DataMember(Name = "PriceLineCustomColor")]
        [Category("Price Line"), DisplayName("Custom Color")]
        public bool PriceLineCustomColor
        {
            get => _priceLineCustomColor;
            set
            {
                if (value == _priceLineCustomColor)
                    return;

                _priceLineCustomColor = value;

                OnPropertyChanged(nameof(PriceLineCustomColor));
            }
        }

        [Browsable(false)]
        public XPen PriceLinePen;

        [Browsable(false)]
        public XBrush PriceLineBrush;

        private XColor _priceLineColor;

        [DataMember(Name = "PriceLineColor")]
        [Category("Price Line"), DisplayName("Line Color")]
        public XColor PriceLineColor
        {
            get => _priceLineColor;
            set
            {
                if (value == _priceLineColor)
                    return;

                _priceLineColor = value;

                PriceLineBrush = new XBrush(_priceLineColor);
                PriceLinePen = new XPen(PriceLineBrush, PriceLineThickness, PriceLineStyle);

                OnPropertyChanged(nameof(PriceLineColor));
            }
        }

        private int _priceLineThickness;

        [DataMember(Name = "PriceLineThickness")]
        [Category("Price Line"), DisplayName("Line Thickness")]
        public int PriceLineThickness
        {
            get => _priceLineThickness;
            set
            {
                if (value == _priceLineThickness)
                    return;

                _priceLineThickness = value;

                PriceLinePen = new XPen(PriceLineBrush, _priceLineThickness, PriceLineStyle);

                OnPropertyChanged(nameof(PriceLineThickness));
            }
        }

        private XDashStyle _priceLineStyle;

        [DataMember(Name = "PriceLineStyle")]
        [Category("Price Line"), DisplayName("Line Style")]
        public XDashStyle PriceLineStyle
        {
            get => _priceLineStyle;
            set
            {
                if (value == _priceLineStyle)
                    return;

                _priceLineStyle = value;

                PriceLinePen = new XPen(PriceLineBrush, PriceLineThickness, _priceLineStyle);

                OnPropertyChanged(nameof(PriceLineStyle));
            }
        }

        private bool _showExchangeName;

        [DataMember(Name = "ShowExchangeName")]
        [Category("Title"), DisplayName("Show Exchange Name")]
        public bool ShowExchangeName
        {
            get => _showExchangeName;
            set
            {
                if (value == _showExchangeName)
                    return;

                _showExchangeName = value;

                OnPropertyChanged(nameof(ShowExchangeName));
            }
        }

        private bool _showExchangeType;

        [DataMember(Name = "ShowExchangeType")]
        [Category("Title"), DisplayName("Show Exchange Type")]
        public bool ShowExchangeType
        {
            get => _showExchangeType;
            set
            {
                if (value == _showExchangeType)
                    return;

                _showExchangeType = value;

                OnPropertyChanged(nameof(ShowExchangeType));
            }
        }

        [Browsable(false)]
        public XBrush ExchangeNameBrush;

        private XColor _exchangeNameColor;

        [DataMember(Name = "ExchangeNameColor")]
        [Category("Title"), DisplayName("Name Color")]
        public XColor ExchangeNameColor
        {
            get => _exchangeNameColor;
            set
            {
                if (value == _exchangeNameColor)
                    return;

                _exchangeNameColor = value;

                ExchangeNameBrush = new XBrush(_exchangeNameColor);

                OnPropertyChanged(nameof(ExchangeNameColor));
            }
        }

        [Browsable(false)]
        public XBrush ExchangeSpotBrush;

        private XColor _exchangeSpotColor;

        [DataMember(Name = "ExchangeSpotColor")]
        [Category("Title"), DisplayName("Spot Color")]
        public XColor ExchangeSpotColor
        {
            get => _exchangeSpotColor;
            set
            {
                if (value == _exchangeSpotColor)
                    return;

                _exchangeSpotColor = value;

                ExchangeSpotBrush = new XBrush(_exchangeSpotColor);

                OnPropertyChanged(nameof(ExchangeSpotColor));
            }
        }

        [Browsable(false)]
        public XBrush ExchangeFuturesBrush;

        private XColor _exchangeFuturesColor;

        [DataMember(Name = "ExchangeFutures")]
        [Category("Title"), DisplayName("Futures ")]
        public XColor ExchangeFuturesColor
        {
            get => _exchangeFuturesColor;
            set
            {
                if (value == _exchangeFuturesColor)
                    return;

                _exchangeFuturesColor = value;

                ExchangeFuturesBrush = new XBrush(_exchangeFuturesColor);

                OnPropertyChanged(nameof(ExchangeFuturesColor));
            }
        }

        [Browsable(false)]
        public override bool ShowIndicatorTitle => false;

        [Browsable(false)]
        public override bool ShowIndicatorValues => false;

        [Browsable(false)]
        public override bool ShowIndicatorLabels => true;

        [Browsable(false)]
        public override bool IsStock => true;

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnBarClose;

        [Browsable(false)]
        public override IndicatorCalculation DefaultCalculation => IndicatorCalculation.OnBarClose;

        public Price()
        {
            ShowCountdown = true;
            WidthMultiplier = 0.5;

            PriceLineVisibility = Visibility.Right;
            PriceLineCustomColor = false;
            PriceLineColor = Colors.Teal;
            PriceLineThickness = 1;
            PriceLineStyle = XDashStyle.DashDotDot;

            ShowExchangeName = true;
            ShowExchangeType = true;
            ExchangeNameColor = new XColor(255, 40, 40, 40);
            ExchangeSpotColor = Colors.Goldenrod;
            ExchangeFuturesColor = Colors.CornflowerBlue;
        }

        protected override void Execute() { }

        public override void Render(DxVisualQueue visual)
        {
            base.Render(visual);

            double step = DataProvider.Step;

            int canvasStart = Canvas.Start;
            int endIndex = DataProvider.Count - canvasStart;
            int startIndex = endIndex - Canvas.Count;
            endIndex--;

            double canvasTop = Canvas.Rect.Top;
            double canvasBottom = Canvas.Rect.Bottom;

            double width = Math.Max(1, WidthMultiplier * Canvas.ColumnWidth);
            double halfWidth = width / 2;

            XColor positiveColor;
            XColor negativeColor;

            if (Canvas.StockType == ChartStockType.Bars)
            {
                positiveColor = Canvas.Theme.BarUpBarColor;
                negativeColor = Canvas.Theme.BarDownBarColor;

                int thickness = Math.Max(1, (int)width);

                if (positiveColor == negativeColor)
                {
                    XPen pen = new XPen(new XBrush(positiveColor), thickness);

                    for (int i = endIndex; i >= startIndex; i--)
                    {
                        IRawCluster cluster = DataProvider.GetRawCluster(i);
                        double highY = Canvas.GetY(cluster.High * step);
                        double lowY = Canvas.GetY(cluster.Low * step);

                        if (Math.Abs(highY - lowY) < 1.0)
                            highY--;
                        if (highY > canvasBottom || lowY < canvasTop)
                            continue;

                        double x = Canvas.GetX(i);

                        visual.DrawLine(pen, new Point(x, highY), new Point(x, lowY));
                    }
                }
                else
                {
                    XPen positivePen = new XPen(new XBrush(positiveColor), thickness);
                    XPen negativePen = new XPen(new XBrush(negativeColor), thickness);

                    for (int i = endIndex; i >= startIndex; i--)
                    {
                        IRawCluster cluster = DataProvider.GetRawCluster(i);
                        double highY = Canvas.GetY(cluster.High * step);
                        double lowY = Canvas.GetY(cluster.Low * step);

                        if (Math.Abs(highY - lowY) < 1.0)
                            highY--;
                        if (highY > canvasBottom || lowY < canvasTop)
                            continue;

                        double x = Canvas.GetX(i);

                        if (cluster.Close >= cluster.Open)
                            visual.DrawLine(positivePen, new Point(x, highY), new Point(x, lowY));
                        else
                            visual.DrawLine(negativePen, new Point(x, highY), new Point(x, lowY));
                    }
                }
            }
            else
            {
                XColor positiveBackground = Canvas.Theme.CandleUpBackColor;
                XColor positiveBorder = Canvas.Theme.CandleUpBorderColor;
                XColor positiveWick = Canvas.Theme.CandleUpWickColor;
                XColor negativeBackground = Canvas.Theme.CandleDownBackColor;
                XColor negativeBorder = Canvas.Theme.CandleDownBorderColor;
                XColor negativeWick = Canvas.Theme.CandleDownWickColor;

                bool drawPositiveBackground = positiveBackground.Alpha != 0;
                bool drawPositiveBorder = positiveBorder.Alpha != 0;
                bool drawPositiveWick = positiveWick.Alpha != 0;
                bool drawNegativeBackground = negativeBackground.Alpha != 0;
                bool drawNegativeBorder = negativeBorder.Alpha != 0;
                bool drawNegativeWick = negativeWick.Alpha != 0;

                if (drawPositiveBorder || drawNegativeBorder)
                {
                    positiveColor = positiveBorder;
                    negativeColor = negativeBorder;
                }
                else if (drawPositiveWick || drawNegativeWick)
                {
                    positiveColor = positiveWick;
                    negativeColor = negativeWick;
                }
                else
                {
                    positiveColor = positiveBackground;
                    negativeColor = negativeBackground;
                }

                if (width <= 1)
                {
                    if (positiveBorder == positiveWick && negativeBorder == negativeWick)
                    {
                        if (positiveBorder == negativeBorder)
                        {
                            XPen pen = new XPen(new XBrush(positiveBorder), 1);

                            for (int i = endIndex; i >= startIndex; i--)
                            {
                                IRawCluster cluster = DataProvider.GetRawCluster(i);
                                double highY = Canvas.GetY(cluster.High * step);
                                double lowY = Canvas.GetY(cluster.Low * step);

                                if (Math.Abs(highY - lowY) < 1.0)
                                    highY--;
                                if (highY > canvasBottom || lowY < canvasTop)
                                    continue;

                                double x = Canvas.GetX(i);

                                visual.DrawLine(pen, new Point(x, highY), new Point(x, lowY));
                            }
                        }
                        else
                        {
                            XPen positivePen = new XPen(new XBrush(positiveBorder), 1);
                            XPen negativePen = new XPen(new XBrush(negativeBorder), 1);

                            for (int i = endIndex; i >= startIndex; i--)
                            {
                                IRawCluster cluster = DataProvider.GetRawCluster(i);
                                double highY = Canvas.GetY(cluster.High * step);
                                double lowY = Canvas.GetY(cluster.Low * step);

                                if (Math.Abs(highY - lowY) < 1.0)
                                    highY--;
                                if (highY > canvasBottom || lowY < canvasTop)
                                    continue;

                                double x = Canvas.GetX(i);

                                if (cluster.Close >= cluster.Open)
                                    visual.DrawLine(
                                        positivePen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                                else
                                    visual.DrawLine(
                                        negativePen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                            }
                        }
                    }
                    else
                    {
                        XPen positiveBorderPen = new XPen(new XBrush(positiveBorder), 1);
                        XPen negativeBorderPen = new XPen(new XBrush(negativeBorder), 1);
                        XPen positiveWickPen = new XPen(new XBrush(positiveWick), 1);
                        XPen negativeWickPen = new XPen(new XBrush(negativeWick), 1);

                        for (int i = endIndex; i >= startIndex; i--)
                        {
                            IRawCluster cluster = DataProvider.GetRawCluster(i);
                            double highY = Canvas.GetY(cluster.High * step);
                            double lowY = Canvas.GetY(cluster.Low * step);

                            if (Math.Abs(highY - lowY) < 1.0)
                                highY--;
                            if (highY > canvasBottom || lowY < canvasTop)
                                continue;

                            long open = cluster.Open;
                            long close = cluster.Close;
                            double openY = Canvas.GetY(open * step);
                            double closeY = Canvas.GetY(close * step);

                            double x = Canvas.GetX(i);

                            if (close >= open)
                            {
                                visual.DrawLine(
                                    positiveWickPen,
                                    new Point(x, highY),
                                    new Point(x, lowY)
                                );
                                visual.DrawLine(
                                    positiveBorderPen,
                                    new Point(x, openY),
                                    new Point(x, closeY)
                                );
                            }
                            else
                            {
                                visual.DrawLine(
                                    negativeWickPen,
                                    new Point(x, highY),
                                    new Point(x, lowY)
                                );
                                visual.DrawLine(
                                    negativeBorderPen,
                                    new Point(x, openY),
                                    new Point(x, closeY)
                                );
                            }
                        }
                    }
                }
                else
                {
                    if (width <= 2)
                    {
                        drawPositiveBackground = false;
                        drawNegativeBackground = false;
                    }

                    if (drawPositiveBackground || drawNegativeBackground)
                    {
                        if (
                            positiveBackground == positiveBorder
                            && negativeBackground == negativeBorder
                        )
                        {
                            XBrush positiveBackgroundBrush = new XBrush(positiveBackground);
                            XBrush negativeBackgroundBrush = new XBrush(negativeBackground);
                            XPen positiveWickPen = new XPen(new XBrush(positiveWick), 1);
                            XPen negativeWickPen = new XPen(new XBrush(negativeWick), 1);

                            for (int i = endIndex; i >= startIndex; i--)
                            {
                                IRawCluster cluster = DataProvider.GetRawCluster(i);
                                double highY = Canvas.GetY(cluster.High * step);
                                double lowY = Canvas.GetY(cluster.Low * step);

                                if (Math.Abs(highY - lowY) < 1.0)
                                    highY--;
                                if (highY > canvasBottom || lowY < canvasTop)
                                    continue;

                                long open = cluster.Open;
                                long close = cluster.Close;
                                double openY = Canvas.GetY(open * step);
                                double closeY = Canvas.GetY(close * step);

                                double x = Canvas.GetX(i);
                                double xLeft = x - halfWidth;
                                double xRight = x + halfWidth;

                                if (close >= open)
                                {
                                    if (closeY - openY < 1.0)
                                        closeY--;

                                    visual.DrawLine(
                                        positiveWickPen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                                    visual.FillRectangle(
                                        positiveBackgroundBrush,
                                        new Rect(new Point(xLeft, openY), new Point(xRight, closeY))
                                    );
                                }
                                else
                                {
                                    if (openY - closeY < 1.0)
                                        openY--;

                                    visual.DrawLine(
                                        negativeWickPen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                                    visual.FillRectangle(
                                        negativeBackgroundBrush,
                                        new Rect(new Point(xLeft, openY), new Point(xRight, closeY))
                                    );
                                }
                            }
                        }
                        else
                        {
                            XBrush positiveBackgroundBrush = new XBrush(positiveBackground);
                            XBrush negativeBackgroundBrush = new XBrush(negativeBackground);
                            XPen positiveBorderPen = new XPen(new XBrush(positiveBorder), 1);
                            XPen negativeBorderPen = new XPen(new XBrush(negativeBorder), 1);
                            XPen positiveWickPen = new XPen(new XBrush(positiveWick), 1);
                            XPen negativeWickPen = new XPen(new XBrush(negativeWick), 1);

                            for (int i = endIndex; i >= startIndex; i--)
                            {
                                IRawCluster cluster = DataProvider.GetRawCluster(i);
                                double highY = Canvas.GetY(cluster.High * step);
                                double lowY = Canvas.GetY(cluster.Low * step);

                                if (Math.Abs(highY - lowY) < 1.0)
                                    highY--;
                                if (highY > canvasBottom || lowY < canvasTop)
                                    continue;

                                long open = cluster.Open;
                                long close = cluster.Close;
                                double openY = Canvas.GetY(open * step);
                                double closeY = Canvas.GetY(close * step);

                                double x = Canvas.GetX(i);
                                double xLeft = x - halfWidth;
                                double xRight = x + halfWidth;

                                Rect body = new Rect(
                                    new Point(xLeft, openY),
                                    new Point(xRight, closeY)
                                );

                                if (close >= open)
                                {
                                    if (closeY - openY < 1.0)
                                        closeY--;

                                    visual.DrawLine(
                                        positiveWickPen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                                    visual.FillRectangle(positiveBackgroundBrush, body);
                                    visual.DrawRectangle(positiveBorderPen, body);
                                }
                                else
                                {
                                    if (openY - closeY < 1.0)
                                        openY--;

                                    visual.DrawLine(
                                        negativeWickPen,
                                        new Point(x, highY),
                                        new Point(x, lowY)
                                    );
                                    visual.FillRectangle(negativeBackgroundBrush, body);
                                    visual.DrawRectangle(negativeBorderPen, body);
                                }
                            }
                        }
                    }
                    else
                    {
                        XBrush positiveBorderBrush = new XBrush(positiveBorder);
                        XBrush negativeBorderBrush = new XBrush(negativeBorder);
                        XPen positiveWickPen = new XPen(new XBrush(positiveWick), 1);
                        XPen negativeWickPen = new XPen(new XBrush(negativeWick), 1);

                        for (int i = endIndex; i >= startIndex; i--)
                        {
                            IRawCluster cluster = DataProvider.GetRawCluster(i);
                            double highY = Canvas.GetY(cluster.High * step);
                            double lowY = Canvas.GetY(cluster.Low * step);

                            if (Math.Abs(highY - lowY) < 1.0)
                                highY--;
                            if (highY > canvasBottom || lowY < canvasTop)
                                continue;

                            long open = cluster.Open;
                            long close = cluster.Close;
                            double openY = Canvas.GetY(open * step);
                            double closeY = Canvas.GetY(close * step);

                            double x = Canvas.GetX(i);
                            double xLeft = x - halfWidth;
                            double xRight = x + halfWidth;

                            if (close >= open)
                            {
                                if (closeY - openY < 1.0)
                                    closeY--;

                                visual.DrawLine(
                                    positiveWickPen,
                                    new Point(x, highY),
                                    new Point(x, closeY)
                                );
                                visual.DrawLine(
                                    positiveWickPen,
                                    new Point(x, lowY),
                                    new Point(x, openY)
                                );
                                visual.FillRectangle(
                                    positiveBorderBrush,
                                    new Rect(new Point(xLeft, openY), new Point(xRight, closeY))
                                );
                            }
                            else
                            {
                                if (openY - closeY < 1.0)
                                    openY--;

                                visual.DrawLine(
                                    negativeWickPen,
                                    new Point(x, highY),
                                    new Point(x, openY)
                                );
                                visual.DrawLine(
                                    negativeWickPen,
                                    new Point(x, lowY),
                                    new Point(x, closeY)
                                );
                                visual.FillRectangle(
                                    negativeBorderBrush,
                                    new Rect(new Point(xLeft, openY), new Point(xRight, closeY))
                                );
                            }
                        }
                    }
                }
            }

            if (canvasStart == 0)
            {
                if (PriceLineVisibility != Visibility.Hidden)
                {
                    double canvasRight = Canvas.Rect.Right;

                    IRawCluster cluster = DataProvider.GetRawCluster(endIndex);
                    long close = cluster.Close;
                    bool isPositive = close >= cluster.Open;
                    double closeY = Canvas.GetY(close * step);

                    double x;
                    if (PriceLineVisibility == Visibility.Right)
                        x = Canvas.GetX(endIndex) + halfWidth;
                    else
                        x = Canvas.Rect.Left;

                    XPen priceLinePen;
                    if (PriceLineCustomColor)
                        priceLinePen = PriceLinePen;
                    else if (isPositive)
                        priceLinePen = new XPen(
                            new XBrush(positiveColor),
                            PriceLineThickness,
                            PriceLineStyle
                        );
                    else
                        priceLinePen = new XPen(
                            new XBrush(negativeColor),
                            PriceLineThickness,
                            PriceLineStyle
                        );

                    visual.DrawLine(
                        priceLinePen,
                        new Point(x, closeY),
                        new Point(canvasRight, closeY)
                    );

                    if (ShowCountdown)
                    {
                        string countdown = Time.GetTimeframeCountdown(
                            DataProvider.Period.Type,
                            DataProvider.Period.Interval
                        );

                        if (countdown != "")
                        {
                            XFont font = Canvas.ChartFont;

                            double textWidth = Math.Max(
                                font.GetWidth(countdown),
                                font.GetWidth(new string('#', countdown.Length))
                            );
                            double textHeight = font.GetHeight();
                            double textHalfHeight = textHeight / 2;
                            double paddingX = 8;
                            double paddingY = 4;

                            Color c = (Color)priceLinePen.Brush.Color;
                            double lum = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                            XColor foreground = lum > 128 ? Colors.Black : Colors.White;

                            canvasRight -= textWidth;
                            closeY -= textHalfHeight - 1;

                            visual.FillRectangle(
                                priceLinePen.Brush,
                                new Rect(
                                    canvasRight - paddingX,
                                    closeY - (paddingY / 2),
                                    textWidth + paddingX,
                                    textHeight + paddingY
                                )
                            );
                            visual.DrawString(
                                countdown,
                                font,
                                new XBrush(foreground),
                                new Rect(
                                    canvasRight - (paddingX / 2),
                                    closeY,
                                    textWidth,
                                    textHeight
                                )
                            );
                        }
                    }
                }
                else if (ShowCountdown)
                {
                    string countdown = Time.GetTimeframeCountdown(
                        DataProvider.Period.Type,
                        DataProvider.Period.Interval
                    );

                    if (countdown != "")
                    {
                        double canvasRight = Canvas.Rect.Right;

                        IRawCluster cluster = DataProvider.GetRawCluster(endIndex);
                        long close = cluster.Close;
                        bool isPositive = close >= cluster.Open;
                        double closeY = Canvas.GetY(close * step);

                        XPen priceLinePen;
                        if (PriceLineCustomColor)
                            priceLinePen = PriceLinePen;
                        else if (isPositive)
                            priceLinePen = new XPen(
                                new XBrush(positiveColor),
                                PriceLineThickness,
                                PriceLineStyle
                            );
                        else
                            priceLinePen = new XPen(
                                new XBrush(negativeColor),
                                PriceLineThickness,
                                PriceLineStyle
                            );

                        XFont font = Canvas.ChartFont;

                        double textWidth = Math.Max(
                            font.GetWidth(countdown),
                            font.GetWidth(new string('#', countdown.Length))
                        );
                        double textHeight = font.GetHeight();
                        double textHalfHeight = textHeight / 2;
                        double paddingX = 8;
                        double paddingY = 4;

                        Color c = (Color)priceLinePen.Brush.Color;
                        double lum = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                        XColor foreground = lum > 128 ? Colors.Black : Colors.White;

                        canvasRight -= textWidth;
                        closeY -= textHalfHeight - 1;

                        visual.FillRectangle(
                            priceLinePen.Brush,
                            new Rect(
                                canvasRight - paddingX,
                                closeY - (paddingY / 2),
                                textWidth + paddingX,
                                textHeight + paddingY
                            )
                        );
                        visual.DrawString(
                            countdown,
                            font,
                            new XBrush(foreground),
                            new Rect(canvasRight - (paddingX / 2), closeY, textWidth, textHeight)
                        );
                    }
                }
            }

            if (ShowExchangeName || ShowExchangeType)
            {
                XFont font = Canvas.ChartFont;
                double textHeight = font.GetHeight();

                double x = 10;
                double y = 10;

                if (ShowExchangeName)
                {
                    string text = DataProvider.Symbol.Exchange.ToUpper().Replace("-FUT", "").Trim();
                    double textWidth = font.GetWidth(text);

                    visual.DrawString(
                        text,
                        font,
                        ExchangeNameBrush,
                        new Rect(x, y, textWidth, textHeight)
                    );
                    x += textWidth;
                }

                if (ShowExchangeType)
                {
                    string text;
                    XBrush brush;

                    if (DataProvider.Symbol.Name.Contains("/"))
                    {
                        text = "SPOT";
                        brush = ExchangeSpotBrush;
                    }
                    else
                    {
                        text = "FUTURES";
                        brush = ExchangeFuturesBrush;
                    }

                    visual.DrawString(
                        text,
                        font,
                        brush,
                        new Rect(x, y, font.GetWidth(text), textHeight)
                    );
                }
            }
        }

        public override void RenderCursor(
            DxVisualQueue visual,
            int cursorPos,
            Point cursorCenter,
            ref int topPos
        )
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                return;
            else if (cursorPos < 0)
                return;

            double step = DataProvider.Step;
            double precision = (double)DataProvider.Symbol.GetSize(1);
            string priceFormat = Format.Price(step);

            IRawCluster cluster = DataProvider.GetRawCluster(cursorPos);
            double high = cluster.High * step;
            double low = cluster.Low * step;
            double volume = cluster.Volume * precision;

            long rawOpen = cluster.Open;
            long rawClose = cluster.Close;
            double open = rawOpen * step;
            double close = rawClose * step;

            double marginX = 5;
            double marginY = 15;
            double paddingX = 6;
            double paddingY = 6;
            double horizontalGap = 4;
            double smallVerticalGap = 4;
            double bigVerticalGap = 10;

            string highText;
            string lowText;
            if (rawClose >= rawOpen)
            {
                highText =
                    $"{high.ToString(priceFormat)} (+{(((high / low) - 1.0) * 100.0).ToString("#,##0.00")}%)";
                lowText = $"{low.ToString(priceFormat)}";
            }
            else
            {
                highText = $"{high.ToString(priceFormat)}";
                lowText =
                    $"{low.ToString(priceFormat)} ({(((low / high) - 1.0) * 100.0).ToString("#,##0.00")}%)";
            }

            string openText = $"{open.ToString(priceFormat)}";
            string closeText = $"{close.ToString(priceFormat)}";
            string volumeText = $"{Format.CompactNumber(volume, 2)}";

            XFont font = Canvas.ChartFont;
            double textHeight = font.GetHeight();
            double prefixWidth = font.GetWidth("W");
            double highTextWidth = font.GetWidth(highText);
            double lowTextWidth = font.GetWidth(lowText);
            double openTextWidth = font.GetWidth(openText);
            double closeTextWidth = font.GetWidth(closeText);
            double volumeTextWidth = font.GetWidth(volumeText);

            double maxWidth =
                Math.Max(
                    highTextWidth,
                    Math.Max(
                        lowTextWidth,
                        Math.Max(openTextWidth, Math.Max(closeTextWidth, volumeTextWidth))
                    )
                )
                + prefixWidth
                + horizontalGap;
            double width = maxWidth + paddingX;
            double height =
                (5 * textHeight) + (4 * smallVerticalGap) + bigVerticalGap + (2 * paddingY);

            double x = cursorCenter.X + marginX;
            double y = topPos + marginY;
            if (x + width >= Canvas.Rect.Right)
                x -= width + marginX * 2;
            if (y + height >= Canvas.Rect.Bottom)
                y -= height + marginY * 2;

            visual.FillRectangle(Canvas.Theme.ChartBackBrush, new Rect(x, y, width, height));

            x += paddingX;
            y += paddingY;

            double textX = x + prefixWidth + horizontalGap;
            double smallGapY = textHeight + smallVerticalGap;
            double bigGapY = textHeight + bigVerticalGap;

            XBrush gray = new XBrush(new XColor(255, 100, 100, 100));
            XBrush darkGray = new XBrush(new XColor(140, 100, 100, 100));

            visual.DrawString(
                "H",
                font,
                new XBrush(new XColor(140, 0, 255, 0)),
                new Rect(x, y, prefixWidth, textHeight)
            );
            visual.DrawString(
                highText,
                font,
                new XBrush(new XColor(255, 0, 255, 0)),
                new Rect(textX, y, highTextWidth, textHeight)
            );
            y += smallGapY;
            visual.DrawString(
                "L",
                font,
                new XBrush(new XColor(140, 255, 0, 0)),
                new Rect(x, y, prefixWidth, textHeight)
            );
            visual.DrawString(
                lowText,
                font,
                new XBrush(new XColor(255, 255, 0, 0)),
                new Rect(textX, y, lowTextWidth, textHeight)
            );
            y += smallGapY;
            visual.DrawString("O", font, darkGray, new Rect(x, y, prefixWidth, textHeight));
            visual.DrawString(openText, font, gray, new Rect(textX, y, openTextWidth, textHeight));
            y += smallGapY;
            visual.DrawString("C", font, darkGray, new Rect(x, y, prefixWidth, textHeight));
            visual.DrawString(
                closeText,
                font,
                gray,
                new Rect(textX, y, closeTextWidth, textHeight)
            );
            y += bigGapY;
            visual.DrawString("V", font, darkGray, new Rect(x, y, prefixWidth, textHeight));
            visual.DrawString(
                volumeText,
                font,
                gray,
                new Rect(textX, y, volumeTextWidth, textHeight)
            );
        }

        public override List<IndicatorValueInfo> GetValues(int cursorPos)
        {
            return new List<IndicatorValueInfo>();
        }

        public override void GetLabels(ref List<IndicatorLabelInfo> labels)
        {
            int count = DataProvider.Count;
            int start = Canvas.Start;

            if (count <= start)
                return;

            double step = DataProvider.Step;

            IRawCluster cluster = DataProvider.GetRawCluster(count - 1 - start);
            long close = cluster.Close;

            XColor color;
            if (PriceLineCustomColor)
                color = PriceLineColor;
            else
            {
                bool isPositive = close >= cluster.Open;

                if (Canvas.StockType == ChartStockType.Bars)
                {
                    if (isPositive)
                        color = Canvas.Theme.BarUpBarColor;
                    else
                        color = Canvas.Theme.BarDownBarColor;
                }
                else
                {
                    XColor positiveBorder = Canvas.Theme.CandleUpBorderColor;
                    XColor negativeBorder = Canvas.Theme.CandleDownBorderColor;

                    if (positiveBorder.Alpha != 0 || negativeBorder.Alpha != 0)
                    {
                        if (isPositive)
                            color = positiveBorder;
                        else
                            color = negativeBorder;
                    }
                    else
                    {
                        XColor positiveWick = Canvas.Theme.CandleUpWickColor;
                        XColor negativeWick = Canvas.Theme.CandleDownWickColor;

                        if (positiveWick.Alpha != 0 || negativeWick.Alpha != 0)
                        {
                            if (isPositive)
                                color = positiveWick;
                            else
                                color = negativeWick;
                        }
                        else
                        {
                            if (isPositive)
                                color = Canvas.Theme.CandleUpBackColor;
                            else
                                color = Canvas.Theme.CandleDownBackColor;
                        }
                    }
                }
            }

            labels.Add(new IndicatorLabelInfo(close * step, color));
        }

        public override IndicatorTitleInfo GetTitle()
        {
            return new IndicatorTitleInfo(Title, XBrush.White);
        }

        private int? _minmaxStartIndex = null;
        private int? _minmaxEndIndex = null;
        private double? _minmaxHigh = null;
        private double? _minmaxLow = null;

        public override bool GetMinMax(out double min, out double max)
        {
            int canvasCount = Canvas.Count;

            if (canvasCount == 0)
            {
                max = 0;
                min = 0;
                return false;
            }

            double step = DataProvider.Step;

            int endIndex = DataProvider.Count - Canvas.Start;
            int startIndex = endIndex - canvasCount;

            if (
                startIndex == _minmaxStartIndex
                && endIndex == _minmaxEndIndex
                && _minmaxHigh != null
                && _minmaxLow != null
            )
            {
                IRawCluster cluster = DataProvider.GetRawCluster(endIndex - 1);
                double currentHigh = cluster.High * step;
                double currentLow = cluster.Low * step;

                if (currentHigh > _minmaxHigh.Value)
                    _minmaxHigh = currentHigh;
                if (currentLow < _minmaxLow.Value)
                    _minmaxLow = currentLow;

                max = _minmaxHigh.Value;
                min = _minmaxLow.Value;

                return true;
            }

            long? high = null;
            long? low = null;

            for (int i = startIndex; i < endIndex; i++)
            {
                IRawCluster cluster = DataProvider.GetRawCluster(i);
                long currentHigh = cluster.High;
                long currentLow = cluster.Low;

                if (high == null || currentHigh > high)
                    high = currentHigh;
                if (low == null || currentLow < low)
                    low = currentLow;
            }

            if (high == null || low == null)
            {
                max = 0;
                min = 0;
                return false;
            }

            _minmaxHigh = high.Value * step;
            _minmaxLow = low.Value * step;

            max = _minmaxHigh.Value;
            min = _minmaxLow.Value;

            return true;
        }

        public override void CopyTemplate(IndicatorBase indicator, bool style)
        {
            Price i = (Price)indicator;

            ShowCountdown = i.ShowCountdown;
            WidthMultiplier = i.WidthMultiplier;

            PriceLineVisibility = i.PriceLineVisibility;
            PriceLineCustomColor = i.PriceLineCustomColor;
            PriceLineColor = i.PriceLineColor;
            PriceLineThickness = i.PriceLineThickness;
            PriceLineStyle = i.PriceLineStyle;

            ShowExchangeName = i.ShowExchangeName;
            ShowExchangeType = i.ShowExchangeType;
            ExchangeNameColor = i.ExchangeNameColor;
            ExchangeSpotColor = i.ExchangeSpotColor;
            ExchangeFuturesColor = i.ExchangeFuturesColor;

            base.CopyTemplate(indicator, style);
        }

        public override string ToString()
        {
            return $"*Price++";
        }
    }
}
