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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using TigerTrade.Core.UI.Converters;

namespace CustomCommon.Helpers
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "AlertFrequency",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum AlertFrequency
    {
        [EnumMember(Value = "Once"), Description("Once")]
        Once,

        [EnumMember(Value = "OncePerBar"), Description("Once per Bar")]
        OncePerBar,

        [EnumMember(Value = "EveryTime"), Description("Every Time")]
        EveryTime,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "AlertDistanceUnit",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum AlertDistanceUnit
    {
        [EnumMember(Value = "Tick"), Description("Tick")]
        Tick,

        [EnumMember(Value = "Price"), Description("Price")]
        Price,

        [EnumMember(Value = "Percent"), Description("Percent")]
        Percent,
    }

    public static class AlertManager
    {
        private static Mutex mutex = new Mutex(false, "AlertManagerMutex");
        private static Dictionary<string, long> lastTimestamps = new Dictionary<string, long>();

        public static bool Check(string symbolPrice, int throttle)
        {
            if (!mutex.WaitOne(100))
                return true;

            bool trigger = false;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (lastTimestamps.ContainsKey(symbolPrice))
            {
                if (now - lastTimestamps[symbolPrice] >= throttle)
                {
                    trigger = true;
                    lastTimestamps[symbolPrice] = now;
                }
            }
            else
            {
                trigger = true;
                lastTimestamps.Add(symbolPrice, now);
            }

            mutex.ReleaseMutex();
            return trigger;
        }
    }

    public sealed class AlertLevel
    {
        private double ticksize;
        private double? lastPrice = null;
        private int? lastIndex = null;
        private bool isTriggered = false;

        public double minPrice;
        public double maxPrice;

        private AlertFrequency frequency;
        public AlertFrequency Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                PropertiesChanged();
            }
        }

        private double price;
        public double Price
        {
            get => price;
            set
            {
                price = value;
                PropertiesChanged();
            }
        }

        private AlertDistanceUnit distanceUnit;
        public AlertDistanceUnit DistanceUnit
        {
            get => distanceUnit;
            set
            {
                distanceUnit = value;
                PropertiesChanged();
            }
        }

        private double distance;
        public double Distance
        {
            get => distance;
            set
            {
                distance = value;
                PropertiesChanged();
            }
        }

        public AlertLevel(double ticksize, double price)
        {
            this.ticksize = ticksize;
            this.frequency = AlertFrequency.EveryTime;
            this.price = price;
            this.distanceUnit = AlertDistanceUnit.Price;
            this.distance = 0;

            PropertiesChanged();
        }

        public AlertLevel(
            double ticksize,
            double price,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            this.ticksize = ticksize;
            this.frequency = AlertFrequency.EveryTime;
            this.price = price;
            this.distanceUnit = distanceUnit;
            this.distance = distance;

            PropertiesChanged();
        }

        public AlertLevel(
            double ticksize,
            AlertFrequency frequency,
            double price,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            this.ticksize = ticksize;
            this.frequency = frequency;
            this.price = price;
            this.distanceUnit = distanceUnit;
            this.distance = distance;

            PropertiesChanged();
        }

        public void UpdatePropertiesIfNeeded(
            AlertFrequency frequency,
            double price,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            if (
                frequency == this.frequency
                && price == this.price
                && distanceUnit == this.distanceUnit
                && distance == this.distance
            )
                return;

            this.frequency = frequency;
            this.price = price;
            this.distanceUnit = distanceUnit;
            this.distance = distance;

            PropertiesChanged();
        }

        public bool Check(double currentPrice, int currentIndex)
        {
            if (this.lastPrice == null)
                this.lastPrice = currentPrice;

            double topPrice;
            double bottomPrice;
            if (currentPrice >= this.lastPrice)
            {
                topPrice = currentPrice;
                bottomPrice = this.lastPrice.Value;
            }
            else
            {
                topPrice = this.lastPrice.Value;
                bottomPrice = currentPrice;
            }

            this.lastPrice = currentPrice;

            if (!(this.minPrice > topPrice || this.maxPrice < bottomPrice))
            {
                if (this.isTriggered)
                {
                    if (currentPrice < this.minPrice || currentPrice > this.maxPrice)
                        this.isTriggered = false;
                    return false;
                }
                else
                {
                    bool ignore =
                        this.frequency == AlertFrequency.OncePerBar
                        && currentIndex == this.lastIndex;
                    this.lastIndex = currentIndex;

                    this.isTriggered = true;
                    return !ignore;
                }
            }

            this.isTriggered = false;
            return false;
        }

        private void PropertiesChanged()
        {
            (double minPrice, double maxPrice) = GetPriceRange(
                this.ticksize,
                this.price,
                this.distanceUnit,
                this.distance
            );

            this.minPrice = minPrice;
            this.maxPrice = maxPrice;
            this.lastIndex = null;
            this.isTriggered = false;
        }

        /// <summary>
        /// Get the price range to use for the alert based on the provided
        /// target price and distance.
        /// </summary>
        /// <param name="price">The target price</param>
        /// <param name="distanceUnit">The unit to use for the distance</param>
        /// <param name="distance">The distance from the given price</param>
        /// <param name="ticksize">The ticksize to use for tick calculations</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double minPrice, double maxPrice) GetPriceRange(
            double ticksize,
            double price,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            if (distance <= 0)
                return (price, price);

            double priceDistance = distance;
            if (distanceUnit == AlertDistanceUnit.Tick)
                priceDistance *= ticksize;
            else if (distanceUnit == AlertDistanceUnit.Percent)
                priceDistance *= price / 100;

            return (
                minPrice: Decimals.RoundBy(price - priceDistance, ticksize),
                maxPrice: Decimals.RoundBy(price + priceDistance, ticksize)
            );
        }
    }

    public sealed class AlertLine
    {
        private double ticksize;
        private double? lastPrice = null;
        private int? lastIndex = null;
        private bool isTriggered = false;
        private double? lastTime = null;

        public double minPrice;
        public double maxPrice;

        private AlertFrequency frequency;
        public AlertFrequency Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                PropertiesChanged();
            }
        }

        private LineData line;
        public LineData Line
        {
            get => line;
            set
            {
                line = value;
                PropertiesChanged();
            }
        }

        private AlertDistanceUnit distanceUnit;
        public AlertDistanceUnit DistanceUnit
        {
            get => distanceUnit;
            set
            {
                distanceUnit = value;
                PropertiesChanged();
            }
        }

        private double distance;
        public double Distance
        {
            get => distance;
            set
            {
                distance = value;
                PropertiesChanged();
            }
        }

        public AlertLine(double ticksize, LineData line)
        {
            this.ticksize = ticksize;
            this.frequency = AlertFrequency.EveryTime;
            this.line = line;
            this.distanceUnit = AlertDistanceUnit.Price;
            this.distance = 0;

            PropertiesChanged();
        }

        public AlertLine(
            double ticksize,
            LineData line,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            this.ticksize = ticksize;
            this.frequency = AlertFrequency.EveryTime;
            this.line = line;
            this.distanceUnit = distanceUnit;
            this.distance = distance;

            PropertiesChanged();
        }

        public AlertLine(
            double ticksize,
            AlertFrequency frequency,
            LineData line,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            this.ticksize = ticksize;
            this.frequency = frequency;
            this.line = line;
            this.distanceUnit = distanceUnit;
            this.distance = distance;

            PropertiesChanged();
        }

        public void UpdatePropertiesIfNeeded(
            AlertFrequency frequency,
            LineData line,
            AlertDistanceUnit distanceUnit,
            double distance,
            double currentTime
        )
        {
            if (
                frequency == this.frequency
                && line == this.line
                && distanceUnit == this.distanceUnit
                && distance == this.distance
            )
            {
                if (currentTime == this.lastTime)
                    return;

                this.lastTime = currentTime;
                PropertiesChanged(false);
                return;
            }

            this.frequency = frequency;
            this.line = line;
            this.distanceUnit = distanceUnit;
            this.distance = distance;
            this.lastTime = currentTime;

            PropertiesChanged();
        }

        public bool Check(double currentPrice, int currentIndex)
        {
            if (this.lastPrice == null)
                this.lastPrice = currentPrice;

            double topPrice;
            double bottomPrice;
            if (currentPrice >= this.lastPrice)
            {
                topPrice = currentPrice;
                bottomPrice = this.lastPrice.Value;
            }
            else
            {
                topPrice = this.lastPrice.Value;
                bottomPrice = currentPrice;
            }

            this.lastPrice = currentPrice;

            if (double.IsNaN(this.minPrice) || double.IsNaN(this.maxPrice))
            {
                this.isTriggered = false;
                return false;
            }

            if (!(this.minPrice > topPrice || this.maxPrice < bottomPrice))
            {
                if (this.isTriggered)
                {
                    if (currentPrice < this.minPrice || currentPrice > this.maxPrice)
                        this.isTriggered = false;
                    return false;
                }
                else
                {
                    bool ignore =
                        this.frequency == AlertFrequency.OncePerBar
                        && currentIndex == this.lastIndex;
                    this.lastIndex = currentIndex;

                    this.isTriggered = true;
                    return !ignore;
                }
            }

            this.isTriggered = false;
            return false;
        }

        private void PropertiesChanged(bool reset = true)
        {
            if (this.lastTime == null)
            {
                this.minPrice = double.NaN;
                this.maxPrice = double.NaN;
            }
            else
            {
                double price = this.line.GetY(this.lastTime.Value);

                if (double.IsNaN(price))
                {
                    this.minPrice = double.NaN;
                    this.maxPrice = double.NaN;
                }
                else
                {
                    (double minPrice, double maxPrice) = GetPriceRange(
                        this.ticksize,
                        price,
                        this.distanceUnit,
                        this.distance
                    );

                    this.minPrice = minPrice;
                    this.maxPrice = maxPrice;
                }
            }

            if (reset)
            {
                this.lastIndex = null;
                this.isTriggered = false;
            }
        }

        /// <summary>
        /// Get the price range to use for the alert based on the provided
        /// target price and distance.
        /// </summary>
        /// <param name="price">The target price</param>
        /// <param name="distanceUnit">The unit to use for the distance</param>
        /// <param name="distance">The distance from the given price</param>
        /// <param name="ticksize">The ticksize to use for tick calculations</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double minPrice, double maxPrice) GetPriceRange(
            double ticksize,
            double price,
            AlertDistanceUnit distanceUnit,
            double distance
        )
        {
            if (distance <= 0)
                return (price, price);

            double priceDistance = distance;
            if (distanceUnit == AlertDistanceUnit.Tick)
                priceDistance *= ticksize;
            else if (distanceUnit == AlertDistanceUnit.Percent)
                priceDistance *= price / 100;

            return (
                minPrice: Decimals.RoundBy(price - priceDistance, ticksize),
                maxPrice: Decimals.RoundBy(price + priceDistance, ticksize)
            );
        }
    }
}
