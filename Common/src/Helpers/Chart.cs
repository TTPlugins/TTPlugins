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
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Data;
using TigerTrade.Chart.Objects.Common;
using TigerTrade.Core.UI.Converters;

namespace CustomCommon.Helpers
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "CoordinateSystem",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum CoordinateSystem
    {
        [EnumMember(Value = "Default"), Description("Default")]
        Default,

        [EnumMember(Value = "FrozenTime"), Description("Frozen Time")]
        FrozenTime,

        [EnumMember(Value = "FrozenPrice"), Description("Frozen Price")]
        FrozenPrice,

        [EnumMember(Value = "Frozen"), Description("Frozen")]
        Frozen,
    }

    public static class Chart
    {
        /// <summary>
        /// Get the uncompressed ticksize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetTicksize(double step, int scale)
        {
            return step / scale;
        }

        /// <summary>
        /// Get the uncompressed ticksize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetTicksize(IChartDataProvider dp)
        {
            return dp.Step / dp.Scale;
        }

        /// <summary>
        /// Get the precision multiplier useful for rounding the size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetPrecision(IChartDataProvider dp)
        {
            return (double)dp.Symbol.GetSize(1);
        }

        /// <summary>
        /// Get the precision multiplier useful for rounding the size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetPrecision(IChartSymbol symbol)
        {
            return (double)symbol.GetSize(1);
        }

        /// <summary>
        /// Checks if a given range of indices is valid
        /// </summary>
        /// <returns>
        /// True if the indices are positive numbers, false otherwise
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidRange(int startIndex, int endIndex)
        {
            return startIndex >= 0 && endIndex >= 0;
        }

        /// <summary>
        /// Checks if a given index is valid
        /// </summary>
        /// <returns>
        /// True if the index is a positive number, false otherwise
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex(int index)
        {
            return index >= 0;
        }

        /// <summary>
        /// Get the start index (inclusive) and end index (exclusive) of the entire historical data loaded on the chart
        /// </summary>
        /// <returns>
        /// Either start and end index or (-1, -1) if chart has no data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int startIndex, int endIndex) GetHistoryIndices(IChartDataProvider dp)
        {
            int count = dp.Count;

            if (count == 0)
                return (-1, -1);

            return (0, count);
        }

        /// <summary>
        /// Get the start index (inclusive) and end index (exclusive) of the entire historical data loaded on the chart
        /// </summary>
        /// <returns>
        /// Either start and end index or (-1, -1) if chart has no data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int startIndex, int endIndex) GetHistoryIndices(int count)
        {
            if (count == 0)
                return (-1, -1);

            return (0, count);
        }

        /// <summary>
        /// Get the start index (inclusive) and end index (exclusive) of the visible range
        /// </summary>
        /// <returns>
        /// Either start and end index or (-1, -1) if chart has no data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int startIndex, int endIndex) GetVisibleRangeIndices(
            IChartDataProvider dp,
            IChartCanvas canvas
        )
        {
            int count = dp.Count;

            if (count == 0)
                return (-1, -1);

            int end = count - canvas.Start;
            int start = end - canvas.Count + 1;

            return (start, end);
        }

        /// <summary>
        /// Get the start index (inclusive) and end index (exclusive) of the visible range
        /// </summary>
        /// <returns>
        /// Either start and end index or (-1, -1) if chart has no data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int startIndex, int endIndex) GetVisibleRangeIndices(
            int count,
            int canvasStart,
            int canvasCount
        )
        {
            if (count == 0)
                return (-1, -1);

            int end = count - canvasStart;
            int start = end - canvasCount + 1;

            return (start, end);
        }

        /// <summary>
        /// Get the latest valid index (aka realtime index)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRealtimeIndex(IChartDataProvider dp)
        {
            return dp.Count - 1;
        }

        /// <summary>
        /// Get the latest valid index (aka realtime index)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRealtimeIndex(int count)
        {
            return count - 1;
        }

        /// <summary>
        /// Get the latest price
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetRealtimePrice(IChartDataProvider dp)
        {
            return dp.GetRawCluster(dp.Count - 1).Close * dp.Step;
        }

        /// <summary>
        /// Get the latest price
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetRealtimePrice(IChartDataProvider dp, int step, int count)
        {
            return dp.GetRawCluster(count - 1).Close * step;
        }

        /// <summary>
        /// Get the latest price
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetRealtimePrice(IRawCluster cluster, int step)
        {
            return cluster.Close * step;
        }

        /// <summary>
        /// Get the latest price safely
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double? GetRealtimePriceSafe(IChartDataProvider dp)
        {
            if (dp == null)
                return null;

            int index = dp.Count - 1;
            if (index < 0)
                return null;

            return dp.GetRawCluster(index)?.Close * dp.Step;
        }

        /// <summary>
        /// Get the latest price safely
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double? GetRealtimePriceSafe(IChartDataProvider dp, int step, int count)
        {
            if (dp == null)
                return null;

            int index = count - 1;
            if (index < 0)
                return null;

            return dp.GetRawCluster(index)?.Close * step;
        }

        /// <summary>
        /// Get the latest price safely
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double? GetRealtimePriceSafe(IRawCluster cluster, int step)
        {
            return cluster?.Close * step;
        }

        /// <summary>
        /// Get the min and max time values for the visible range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double? minTime, double? maxTime) GetVisibleRangeTimes(
            IChartDataProvider dp,
            IChartCanvas canvas
        )
        {
            if (dp == null || canvas == null)
                return (null, null);

            int count = dp.Count;
            if (count == 0)
                return (null, null);

            int endIndex = count - canvas.Start - 1;
            int startIndex = endIndex - canvas.Count;

            if (startIndex < 0 || endIndex < 0)
                return (null, null);

            int datesCount = dp.Dates.Count;

            double timeStep = 0;
            if (datesCount >= 2)
                timeStep = dp.Dates[1] - dp.Dates[0];

            double? minTime = null;
            if (startIndex < datesCount)
                minTime = dp.Dates[startIndex] - timeStep;

            double? maxTime = null;
            if (endIndex < datesCount)
                maxTime = dp.Dates[endIndex] + ((canvas.AfterBars + 1) * timeStep);

            return (minTime, maxTime);
        }

        /// <summary>
        /// Get the min and max price values for the visible range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double? minPrice, double? maxPrice) GetVisibleRangePrices(
            IChartCanvas canvas
        )
        {
            if (canvas == null)
                return (null, null);

            if (canvas.Count == 0)
                return (null, null);

            double minPrice = canvas.GetValue(canvas.Rect.Bottom);
            double maxPrice = canvas.GetValue(0);

            return (minPrice, maxPrice);
        }

        /// <summary>
        /// Get the date based on a given x coordinate.
        /// </summary>
        /// <param name="dp">The DataProvider</param>
        /// <param name="canvas">The Canvas</param>
        /// <param name="x">The x coordinate</param>
        public static double? GetDate(IChartDataProvider dp, IChartCanvas canvas, double x)
        {
            if (dp == null || canvas == null)
                return null;

            int datesCount = dp.Dates.Count;
            if (datesCount <= 0)
                return null;

            int startIndex = 0;
            int endIndex = datesCount - 1;

            double startX = canvas.GetX(startIndex);
            double endX = canvas.GetX(endIndex);

            if (x <= startX)
            {
                int offset = (int)Math.Round((startX - x) / canvas.ColumnWidth);

                if (offset <= 0)
                    return dp.Dates[startIndex];
                else if (datesCount >= 2)
                {
                    double step = dp.Dates[1] - dp.Dates[0];
                    return dp.Dates[startIndex] - (offset * step);
                }
            }
            else if (x >= endX)
            {
                int offset = (int)Math.Round((x - endX) / canvas.ColumnWidth);

                if (offset <= 0)
                    return dp.Dates[endIndex];
                else if (datesCount >= 2)
                {
                    double step = dp.Dates[1] - dp.Dates[0];
                    return dp.Dates[endIndex] + (offset * step);
                }
            }

            // PERF: The following code performs a binary search to find the
            // date at the closest x coordinate to the given one.
            // This is not the ideal solution because this could be calculated
            // using some formulas, but with the limited knowledge of how
            // TigerTrade's charting library works under the hood we will keep
            // this solution because it's the easiest to implement instead of
            // trying to reverse engineer the correct formulas.
            double startDistance = canvas.GetX(startIndex) - x;
            double endDistance = canvas.GetX(endIndex) - x;

            while (endIndex - startIndex > 1)
            {
                int index = (startIndex + endIndex) / 2;
                double distance = canvas.GetX(index) - x;

                if (distance < 0)
                {
                    startIndex = index;
                    startDistance = distance;
                }
                else if (distance > 0)
                {
                    endIndex = index;
                    endDistance = distance;
                }
                else
                {
                    startIndex = index;
                    startDistance = distance;

                    endIndex = index;
                    endDistance = distance;
                    break;
                }
            }

            if (startDistance < endDistance)
                return dp.Dates[startIndex];
            else
                return dp.Dates[endIndex];
        }

        /// <summary>
        /// Search for the closest date to a given x coordinate.
        /// </summary>
        /// <param name="dp">The DataProvider</param>
        /// <param name="canvas">The Canvas</param>
        /// <param name="x">The x coordinate</param>
        public static double? SearchClosestDate(
            IChartDataProvider dp,
            IChartCanvas canvas,
            double x
        )
        {
            if (dp == null || canvas == null)
                return null;

            int datesCount = dp.Dates.Count;
            if (datesCount <= 0)
                return null;

            int startIndex = 0;
            double startDistance = canvas.GetX(startIndex) - x;

            int endIndex = datesCount - 1;
            double endDistance = canvas.GetX(endIndex) - x;

            while (endIndex - startIndex > 1)
            {
                int index = (startIndex + endIndex) / 2;
                double distance = canvas.GetX(index) - x;

                if (distance < 0)
                {
                    startIndex = index;
                    startDistance = distance;
                }
                else if (distance > 0)
                {
                    endIndex = index;
                    endDistance = distance;
                }
                else
                {
                    startIndex = index;
                    startDistance = distance;

                    endIndex = index;
                    endDistance = distance;
                    break;
                }
            }

            if (startDistance < endDistance)
                return dp.Dates[startIndex];
            else
                return dp.Dates[endIndex];
        }
    }

    public struct LineData
    {
        public ObjectPoint p0;
        public ObjectPoint p1;
        public double thickness;

        public LineData(ObjectPoint p0, ObjectPoint p1, double thickness = 1)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.thickness = thickness;
        }

        public static bool operator ==(LineData l1, LineData l2)
        {
            if (ReferenceEquals(l1, l2))
                return true;

            return l1.p0.X == l2.p0.X
                && l1.p0.Y == l2.p0.Y
                && l1.p1.X == l2.p1.X
                && l1.p1.Y == l2.p1.Y
                && l1.thickness == l2.thickness;
        }

        public static bool operator !=(LineData l1, LineData l2)
        {
            return !(l1 == l2);
        }

        public override bool Equals(object obj)
        {
            if (obj is LineData line)
                return this == line;

            return false;
        }

        public override int GetHashCode()
        {
            return (p0.X, p0.Y, p1.X, p1.Y, thickness).GetHashCode();
        }

        public double GetX(double y)
        {
            if (p0.Y == p1.Y)
                return double.NaN;

            double slope = (p1.Y - p0.Y) / (p1.X - p0.X);
            return p0.X + (y - p0.Y) / slope;
        }

        public double GetY(double x)
        {
            if (p0.X == p1.X)
                return double.NaN;

            double slope = (p1.Y - p0.Y) / (p1.X - p0.X);
            return p0.Y + slope * (x - p0.X);
        }

        public double DistanceFrom(double x, double y)
        {
            return DistanceFromPointToLineSegment(x, y, p0.X, p0.Y, p1.X, p1.Y);
        }

        public bool Intersects(double distance)
        {
            return distance <= thickness / 2.0;
        }

        public bool IntersectsWith(double x, double y)
        {
            double distance = DistanceFromPointToLineSegment(x, y, p0.X, p0.Y, p1.X, p1.Y);
            return distance <= thickness / 2.0;
        }

        public bool IntersectsWith(ObjectPoint point)
        {
            double distance = DistanceFromPointToLineSegment(
                point.X,
                point.Y,
                p0.X,
                p0.Y,
                p1.X,
                p1.Y
            );
            return distance <= thickness / 2.0;
        }

        public bool IntersectsWith(LineData line)
        {
            ObjectPoint? intersection = GetIntersectionPoint(this.p0, this.p1, line.p0, line.p1);

            if (intersection == null)
                return false;

            return IntersectsWith(intersection.Value) || line.IntersectsWith(intersection.Value);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static double DistanceFromPointToLineSegment(
            double x,
            double y,
            double lx1,
            double ly1,
            double lx2,
            double ly2
        )
        {
            double dx = lx2 - lx1;
            double dy = ly2 - ly1;

            if ((dx == 0) && (dy == 0))
                return Math.Sqrt((x - lx1) * (x - lx1) + (y - ly1) * (y - ly1));

            double t = ((x - lx1) * dx + (y - ly1) * dy) / (dx * dx + dy * dy);

            if (t < 0)
                return Math.Sqrt((x - lx1) * (x - lx1) + (y - ly1) * (y - ly1));
            else if (t > 1)
                return Math.Sqrt((x - lx2) * (x - lx2) + (y - ly2) * (y - ly2));

            double px = lx1 + t * dx;
            double py = ly1 + t * dy;

            return Math.Sqrt((x - px) * (x - px) + (y - py) * (y - py));
        }

        public static bool IsPointOnSegment(
            double x,
            double y,
            double lx1,
            double ly1,
            double lx2,
            double ly2
        )
        {
            return Math.Min(lx1, lx2) <= x
                && x <= Math.Max(lx1, lx2)
                && Math.Min(ly1, ly2) <= y
                && y <= Math.Max(ly1, ly2);
        }

        // FIX: Check if is correct
        public static ObjectPoint? GetIntersectionPoint(
            ObjectPoint a0,
            ObjectPoint a1,
            ObjectPoint b0,
            ObjectPoint b1
        )
        {
            double A1 = a1.Y - a0.Y;
            double B1 = a0.X - a1.X;
            double C1 = A1 * a0.X + B1 * a0.Y;

            double A2 = b1.Y - b0.Y;
            double B2 = b0.X - b1.X;
            double C2 = A2 * b0.X + B2 * b0.Y;

            double determinant = A1 * B2 - A2 * B1;

            // Parallel Lines
            if (determinant == 0)
                return null;

            double x = (B2 * C1 - B1 * C2) / determinant;
            double y = (A1 * C2 - A2 * C1) / determinant;

            if (
                IsPointOnSegment(x, y, a0.X, a0.Y, a1.X, a1.Y)
                && IsPointOnSegment(x, y, b0.X, b0.Y, b1.X, b1.Y)
            )
                return new ObjectPoint(x, y);

            return null;
        }
    }
}
