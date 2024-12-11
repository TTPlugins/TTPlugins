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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using TigerTrade.Chart.Base.Enums;
using TigerTrade.Chart.Data;
using TigerTrade.Chart.Objects.Common;
using TigerTrade.Core.UI.Converters;

namespace CustomCommon.Helpers
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "TimeframeVisibility",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum TimeframeVisibility
    {
        [EnumMember(Value = "Disabled"), Description("Disabled")]
        Disabled,

        [EnumMember(Value = "All"), Description("All")]
        All,

        [EnumMember(Value = "Lower"), Description("Lower")]
        Lower,

        [EnumMember(Value = "Current"), Description("Current")]
        Current,

        [EnumMember(Value = "Higher"), Description("Higher")]
        Higher,
    }

    public static class Time
    {
        /// <summary>
        /// Get the chart period type (e.g. Tick/Second/Minute/etc.)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ChartPeriodType GetTimeframeType(IChartDataProvider dp)
        {
            return dp.Period.Type;
        }

        /// <summary>
        /// Get the chart period interval (e.g. 1/5/15/etc.)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTimeframeInterval(IChartDataProvider dp)
        {
            return dp.Period.Interval;
        }

        /// <summary>
        /// Get the timeframe string (e.g. 1m/1h/1D/etc.)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTimeframe(IChartDataProvider dp)
        {
            ChartPeriodType type = dp.Period.Type;
            int interval = dp.Period.Interval;

            if (type == ChartPeriodType.Tick)
                return $"{interval}T";
            else if (type == ChartPeriodType.Second)
                return $"{interval}s";
            else if (type == ChartPeriodType.Minute)
                return $"{interval}m";
            else if (type == ChartPeriodType.Hour)
                return $"{interval}h";
            else if (type == ChartPeriodType.Day)
                return $"{interval}D";
            else if (type == ChartPeriodType.Week)
                return $"{interval}W";
            else if (type == ChartPeriodType.Month)
                return $"{interval}M";
            else if (type == ChartPeriodType.Year)
                return $"{interval}Y";

            return $"{interval}{type}";
        }

        /// <summary>
        /// Get the timeframe string (e.g. 1m/1h/1D/etc.)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTimeframe(ChartPeriodType type, int interval)
        {
            if (type == ChartPeriodType.Tick)
                return $"{interval}T";
            else if (type == ChartPeriodType.Second)
                return $"{interval}s";
            else if (type == ChartPeriodType.Minute)
                return $"{interval}m";
            else if (type == ChartPeriodType.Hour)
                return $"{interval}h";
            else if (type == ChartPeriodType.Day)
                return $"{interval}D";
            else if (type == ChartPeriodType.Week)
                return $"{interval}W";
            else if (type == ChartPeriodType.Month)
                return $"{interval}M";
            else if (type == ChartPeriodType.Year)
                return $"{interval}Y";

            return $"{interval}{type}";
        }

        /// <summary>
        /// Get the countdown till the next timeframe interval
        /// </summary>
        public static string GetTimeframeCountdown(IChartDataProvider dp)
        {
            ChartPeriodType type = dp.Period.Type;
            int interval = dp.Period.Interval;

            if (type == ChartPeriodType.Tick)
                return "";
            else if (type == ChartPeriodType.Second)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % interval) + interval - 1;

                if (seconds != 0)
                    return $"{seconds}s";
                else
                    return "";
            }
            else if (type == ChartPeriodType.Minute)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (minutes != 0)
                    countdown.Append($"{minutes}m ");
                if (seconds != 0)
                    countdown.Append($"{seconds}s ");

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Hour)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (hours == 0)
                {
                    if (minutes != 0)
                        countdown.Append($"{minutes}m ");
                    if (seconds != 0)
                        countdown.Append($"{seconds}s ");
                }
                else
                {
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                    if (minutes != 0)
                        countdown.Append($"{minutes}m ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Day)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = (-now.Day % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Week)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;

                int days;
                DayOfWeek day = now.DayOfWeek;
                if (day == DayOfWeek.Monday)
                    days = 6;
                else if (day == DayOfWeek.Tuesday)
                    days = 5;
                else if (day == DayOfWeek.Wednesday)
                    days = 4;
                else if (day == DayOfWeek.Thursday)
                    days = 3;
                else if (day == DayOfWeek.Friday)
                    days = 2;
                else if (day == DayOfWeek.Saturday)
                    days = 1;
                else if (day == DayOfWeek.Sunday)
                    days = 0;
                else
                    return "";

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Month)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = DateTime.DaysInMonth(now.Year, now.Month) - now.Day;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Year)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = (DateTime.IsLeapYear(now.Year) ? 366 : 365) - now.DayOfYear;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }

            return "";
        }

        /// <summary>
        /// Get the countdown till the next timeframe interval
        /// </summary>
        public static string GetTimeframeCountdown(ChartPeriodType type, int interval)
        {
            if (type == ChartPeriodType.Tick)
                return "";
            else if (type == ChartPeriodType.Second)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % interval) + interval - 1;

                if (seconds != 0)
                    return $"{seconds}s";
                else
                    return "";
            }
            else if (type == ChartPeriodType.Minute)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (minutes != 0)
                    countdown.Append($"{minutes}m ");
                if (seconds != 0)
                    countdown.Append($"{seconds}s ");

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Hour)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (hours == 0)
                {
                    if (minutes != 0)
                        countdown.Append($"{minutes}m ");
                    if (seconds != 0)
                        countdown.Append($"{seconds}s ");
                }
                else
                {
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                    if (minutes != 0)
                        countdown.Append($"{minutes}m ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Day)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = (-now.Day % interval) + interval - 1;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Week)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;

                int days;
                DayOfWeek day = now.DayOfWeek;
                if (day == DayOfWeek.Monday)
                    days = 6;
                else if (day == DayOfWeek.Tuesday)
                    days = 5;
                else if (day == DayOfWeek.Wednesday)
                    days = 4;
                else if (day == DayOfWeek.Thursday)
                    days = 3;
                else if (day == DayOfWeek.Friday)
                    days = 2;
                else if (day == DayOfWeek.Saturday)
                    days = 1;
                else if (day == DayOfWeek.Sunday)
                    days = 0;
                else
                    return "";

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Month)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = DateTime.DaysInMonth(now.Year, now.Month) - now.Day;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }
            else if (type == ChartPeriodType.Year)
            {
                if (interval != 1)
                    return "";

                DateTimeOffset now = DateTimeOffset.UtcNow;
                int seconds = (-now.Second % 60) + 59;
                int minutes = (-now.Minute % 60) + 59;
                int hours = (-now.Hour % 24) + 23;
                int days = (DateTime.IsLeapYear(now.Year) ? 366 : 365) - now.DayOfYear;

                StringBuilder countdown = new StringBuilder();

                if (days == 0)
                {
                    if (hours == 0)
                    {
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                        if (seconds != 0)
                            countdown.Append($"{seconds}s ");
                    }
                    else
                    {
                        if (hours != 0)
                            countdown.Append($"{hours}h ");
                        if (minutes != 0)
                            countdown.Append($"{minutes}m ");
                    }
                }
                else
                {
                    if (days != 0)
                        countdown.Append($"{days}D ");
                    if (hours != 0)
                        countdown.Append($"{hours}h ");
                }

                return countdown.ToString().Trim();
            }

            return "";
        }

        /// <summary>
        /// Changes the timeframe visibility options to match the given visibility
        /// </summary>
        public static void SetTimeframeVisibility(
            IChartDataProvider dataProvider,
            ObjectPeriods objectPeriods,
            TimeframeVisibility visibility
        )
        {
            if (visibility == TimeframeVisibility.Disabled)
                return;

            if (objectPeriods == null)
                return;

            (ChartPeriodType type, int max)[] periods = new (ChartPeriodType type, int max)[]
            {
                (ChartPeriodType.Tick, 0),
                (ChartPeriodType.Second, 59),
                (ChartPeriodType.Minute, 59),
                (ChartPeriodType.Hour, 24),
                (ChartPeriodType.Day, 366),
                (ChartPeriodType.Week, 0),
                (ChartPeriodType.Month, 0),
                (ChartPeriodType.Year, 0),
            };
            int length = periods.Length;

            if (visibility == TimeframeVisibility.All)
            {
                for (int i = 0; i < length; i++)
                {
                    (ChartPeriodType type, int max) period = periods[i];

                    objectPeriods.Update(period.type.ToString(), false, false, 0, period.max);
                }
                return;
            }

            if (dataProvider?.Period == null)
                return;

            ChartPeriodType periodType = dataProvider.Period.Type;
            int periodInterval = dataProvider.Period.Interval;

            if (visibility == TimeframeVisibility.Lower)
            {
                bool found = false;

                for (int i = 0; i < length; i++)
                {
                    (ChartPeriodType type, int max) period = periods[i];

                    if (period.type == periodType)
                    {
                        objectPeriods.Update(period.type.ToString(), true, true, 0, periodInterval);

                        found = true;
                        continue;
                    }

                    if (found)
                        objectPeriods.Update(period.type.ToString(), false, false, 0, period.max);
                    else
                        objectPeriods.Update(period.type.ToString(), true, true, 0, period.max);
                }
            }
            else if (visibility == TimeframeVisibility.Current)
            {
                for (int i = 0; i < length; i++)
                {
                    (ChartPeriodType type, int max) period = periods[i];

                    if (period.type == periodType)
                        objectPeriods.Update(
                            period.type.ToString(),
                            true,
                            true,
                            periodInterval,
                            periodInterval
                        );
                    else
                        objectPeriods.Update(period.type.ToString(), false, false, 0, period.max);
                }
            }
            else if (visibility == TimeframeVisibility.Higher)
            {
                bool found = false;

                for (int i = 0; i < length; i++)
                {
                    (ChartPeriodType type, int max) period = periods[i];

                    if (period.type == periodType)
                    {
                        objectPeriods.Update(
                            period.type.ToString(),
                            true,
                            true,
                            periodInterval,
                            period.max
                        );

                        found = true;
                        continue;
                    }

                    if (found)
                        objectPeriods.Update(period.type.ToString(), true, true, 0, period.max);
                    else
                        objectPeriods.Update(period.type.ToString(), false, false, 0, period.max);
                }
            }
        }
    }
}
