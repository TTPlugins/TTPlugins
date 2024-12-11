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
using System.Runtime.CompilerServices;
using TigerTrade.Chart.Data;

namespace CustomCommon.Helpers
{
    public static class Format
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Price(IChartDataProvider dp)
        {
            return $"#,##0.{new string('0', Decimals.Count(dp.Step))}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Price(double step)
        {
            return $"#,##0.{new string('0', Decimals.Count(step))}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Price(int decimals)
        {
            return $"#,##0.{new string('0', decimals)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PriceCompact(IChartDataProvider dp)
        {
            return $"#,##0.{new string('#', Decimals.Count(dp.Step))}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PriceCompact(double step)
        {
            return $"#,##0.{new string('#', Decimals.Count(step))}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PriceCompact(int decimals)
        {
            return $"#,##0.{new string('#', decimals)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CompactNumber(double value, double decimals)
        {
            double v = Math.Abs(value);
            string format = $"F{decimals}";

            if (v < 1_000)
                return value.ToString(format);
            else if (v < 1_000_000)
                return (value / 1_000).ToString(format) + "K";
            else if (v < 1_000_000_000)
                return (value / 1_000_000).ToString(format) + "M";
            else
                return (value / 1_000_000_000).ToString(format) + "B";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CompactNumberNoPostfix(double value, double decimals)
        {
            double v = Math.Abs(value);
            string format = $"0.{new string('#', (int)decimals)}";

            if (v < 1_000)
                return value.ToString(format);
            else if (v < 1_000_000)
                return (value / 1_000).ToString(format);
            else if (v < 1_000_000_000)
                return (value / 1_000_000).ToString(format);
            else
                return (value / 1_000_000_000).ToString(format);
        }
    }
}
