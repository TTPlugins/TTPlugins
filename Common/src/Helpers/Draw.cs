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
using System.Windows;
using System.Windows.Media;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Data;
using TigerTrade.Dx;
using TigerTrade.Dx.Fonts;

namespace CustomCommon.Helpers
{
    public static class Draw
    {
        public static XBrush ControlPointBrush = new XBrush(new XColor(255, 0, 0, 0));
        public static XPen ControlPointPen = new XPen(new XBrush(new XColor(255, 255, 115, 29)), 1);
        public static double ControlPointCornerSize = 11.0;
        public static double ControlPointEdgeSize = 10.0;
        public static double ControlPointLockedCornerSize = 5.5;
        public static double ControlPointLockedEdgeSize = 5.0;
        public static double ControlPointEdgeRadius = 2.0;
        public static double ControlPointCornerHalfSize = ControlPointCornerSize / 2;
        public static double ControlPointEdgeHalfSize = ControlPointEdgeSize / 2;
        public static double ControlPointLockedCornerHalfSize = ControlPointLockedCornerSize / 2;
        public static double ControlPointLockedEdgeHalfSize = ControlPointLockedEdgeSize / 2;
        public static Point ControlPointEdgeRadiusPoint = new Point(
            ControlPointEdgeRadius,
            ControlPointEdgeRadius
        );
        public static double ControlPointHitbox = Math.Max(
            ControlPointCornerHalfSize,
            ControlPointEdgeHalfSize
        );

        public static XFont AlertBellFont = new XFont("FontAwesome", 10);
        public static string AlertBellText = char.ConvertFromUtf32(
            (int)FontAwesomeIcon.BellOutline
        );
        public static double AlertBellWidth = AlertBellFont.GetWidth(AlertBellText);
        public static double AlertBellHeight = AlertBellFont.GetHeight();
        public static double AlertBellHalfWidth = AlertBellWidth / 2;
        public static double AlertBellHalfHeight = AlertBellHeight / 2;
        public static XBrush AlertBellBrush = new XBrush(Colors.Gold);

        /// <summary>
        /// Get the bounds of the canvas
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double left, double top, double right, double bottom) GetCanvasBounds(
            IChartCanvas canvas
        )
        {
            Rect r = canvas.Rect;

            return (r.Left, r.Top, r.Right, r.Bottom);
        }

        /// <summary>
        /// Get a point on the canvas based on the given index and price
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point GetPoint(IChartCanvas canvas, int index, double price)
        {
            return new Point(canvas.GetX(index), canvas.GetY(price));
        }

        /// <summary>
        /// Get the X and Y coordinates on the canvas based on the given index and price
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double x, double y) GetXY(IChartCanvas canvas, int index, double price)
        {
            return (canvas.GetX(index), canvas.GetY(price));
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(IChartCanvas canvas, Point point)
        {
            Rect r = canvas.Rect;
            double x = point.X;
            double y = point.Y;

            return x < r.Left || x > r.Right || y < r.Top || y > r.Bottom;
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(
            (double left, double top, double right, double bottom) canvasBounds,
            Point point
        )
        {
            double x = point.X;
            double y = point.Y;

            return x < canvasBounds.left
                || x > canvasBounds.right
                || y < canvasBounds.top
                || y > canvasBounds.bottom;
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(
            double canvasLeft,
            double canvasTop,
            double canvasRight,
            double canvasBottom,
            Point point
        )
        {
            double x = point.X;
            double y = point.Y;

            return x < canvasLeft || x > canvasRight || y < canvasTop || y > canvasBottom;
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(IChartCanvas canvas, double x, double y)
        {
            Rect r = canvas.Rect;

            return x < r.Left || x > r.Right || y < r.Top || y > r.Bottom;
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(
            (double left, double top, double right, double bottom) canvasBounds,
            double x,
            double y
        )
        {
            return x < canvasBounds.left
                || x > canvasBounds.right
                || y < canvasBounds.top
                || y > canvasBounds.bottom;
        }

        /// <summary>
        /// Check if a point on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointInvisible(
            double canvasLeft,
            double canvasTop,
            double canvasRight,
            double canvasBottom,
            double x,
            double y
        )
        {
            return x < canvasLeft || x > canvasRight || y < canvasTop || y > canvasBottom;
        }

        /// <summary>
        /// Snap a point to the nearest angle using a given point as the origin.
        /// </summary>
        /// <param name="origin">The point to use as the origin</param>
        /// <param name="point">The point to snap to the nearest angle</param>
        public static void SnapToNearestAngle(ref Point origin, ref Point point)
        {
            double angle = (double)(
                Math.Atan2(point.Y - origin.Y, point.X - origin.X) * (180 / Math.PI)
            );
            if (angle < 0)
                angle += 360;

            if (angle >= 337.5 || angle < 22.5)
            {
                point.Y = origin.Y;
                return;
            }
            else if (angle >= 22.5 && angle < 67.5)
                angle = 45;
            else if (angle >= 67.5 && angle < 112.5)
            {
                point.X = origin.X;
                return;
            }
            else if (angle >= 112.5 && angle < 157.5)
                angle = 135;
            else if (angle >= 157.5 && angle < 202.5)
            {
                point.Y = origin.Y;
                return;
            }
            else if (angle >= 202.5 && angle < 247.5)
                angle = 225;
            else if (angle >= 247.5 && angle < 292.5)
            {
                point.X = origin.X;
                return;
            }
            else if (angle >= 292.5 && angle < 337.5)
                angle = 315;

            double length = Math.Sqrt(
                Math.Pow(point.X - origin.X, 2) + Math.Pow(point.Y - origin.Y, 2)
            );
            double m = angle * (Math.PI / 180);

            point.X = origin.X + length * Math.Cos(m);
            point.Y = origin.Y + length * Math.Sin(m);
        }

        /// <summary>
        /// Check if a X coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsXInvisible(IChartCanvas canvas, double x)
        {
            Rect r = canvas.Rect;

            return x < r.Left || x > r.Right;
        }

        /// <summary>
        /// Check if a X coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsXInvisible(
            (double left, double top, double right, double bottom) canvasBounds,
            double x
        )
        {
            return x < canvasBounds.left || x > canvasBounds.right;
        }

        /// <summary>
        /// Check if a X coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsXInvisible(double canvasLeft, double canvasRight, double x)
        {
            return x < canvasLeft || x > canvasRight;
        }

        /// <summary>
        /// Check if a Y coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsYInvisible(IChartCanvas canvas, double y)
        {
            Rect r = canvas.Rect;

            return y < r.Top || y > r.Bottom;
        }

        /// <summary>
        /// Check if a Y coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsYInvisible(
            (double left, double top, double right, double bottom) canvasBounds,
            double y
        )
        {
            return y < canvasBounds.top || y > canvasBounds.bottom;
        }

        /// <summary>
        /// Check if a Y coordinate on the canvas would be invisible to the end user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsYInvisible(double canvasTop, double canvasBottom, double y)
        {
            return y < canvasTop || y > canvasBottom;
        }

        /// <summary>
        /// Change the opacity of an existing color
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XColor ChangeOpacity(XColor color, int opacity)
        {
            return new XColor((byte)opacity, color);
        }

        /// <summary>
        /// Change the opacity of an existing brush
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XBrush ChangeOpacity(XBrush brush, int opacity)
        {
            return new XBrush(new XColor((byte)opacity, brush.Color));
        }

        /// <summary>
        /// Change the opacity of an existing pen
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPen ChangeOpacity(XPen pen, int opacity)
        {
            return new XPen(
                new XBrush(new XColor((byte)opacity, pen.Brush.Color)),
                pen.Width,
                pen.Style
            );
        }

        /// <summary>
        /// Get the zoom decrement ratio to apply to the size of a drawing
        /// </summary>
        /// <param name="columnWidth">The Canvas.ColumnWidth property indicating the zoom level</param>
        /// <param name="minColumnWidth">The max zoom level after which the ratio stops decrementing</param>
        /// <param name="reduceRatio">The speed at which the ratio decrements</param>
        /// <param name="minRatio">The min speed at which the ratio decrement should default to instead of going to 0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetZoomDecrementRatio(
            double columnWidth,
            double minColumnWidth,
            double reduceRatio,
            double minRatio
        )
        {
            if (columnWidth >= minColumnWidth)
                return 1.0;

            return Math.Max(minRatio, 1.0 - ((minColumnWidth - columnWidth) / reduceRatio));
        }

        /// <summary>
        /// Get the index of the hovered control point or -1 otherwise.
        /// </summary>
        /// <param name="x">The mouse x</param>
        /// <param name="y">The mouse y</param>
        /// <param name="points">The control points to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetControlPointIndex(int x, int y, params Point[] points)
        {
            int length = points.Length;

            for (int i = 0; i < length; i++)
            {
                double dx = Math.Abs(points[i].X - x);
                double dy = Math.Abs(points[i].Y - y);

                if (dx < ControlPointHitbox && dy < ControlPointHitbox)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Get the min distance of a given hovered rectangle or -1 otherwise.
        /// </summary>
        /// <param name="x">The mouse x</param>
        /// <param name="y">The mouse y</param>
        /// <param name="rects">The rectangles to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinDistance(int x, int y, params Rect[] rects)
        {
            int length = rects.Length;

            for (int i = 0; i < length; i++)
            {
                if (rects[i].IsEmpty)
                    continue;

                double distance = Math.Min(
                    Math.Min(rects[i].Right - x, x - rects[i].X),
                    Math.Min(rects[i].Bottom - y, y - rects[i].Y)
                );

                if (distance >= 0)
                    return (int)distance;
            }

            return -1;
        }

        /// <summary>
        /// Get the min distance of a given hovered line or -1 otherwise.
        /// </summary>
        /// <param name="x">The mouse x</param>
        /// <param name="y">The mouse y</param>
        /// <param name="lines">The lines to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinDistance(int x, int y, params Line[] lines)
        {
            int length = lines.Length;

            for (int i = 0; i < length; i++)
            {
                double distance = lines[i].DistanceFrom(x, y);
                if (lines[i].Intersects(distance))
                    return (int)distance;
            }

            return -1;
        }

        /// <summary>
        /// Get the middle control point between two control points
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (double mx, double my) GetMiddleControlPoint(
            IChartDataProvider dp,
            IChartCanvas canvas,
            double x0,
            double y0,
            double x1,
            double y1
        )
        {
            (double? minTime, double? maxTime) = Chart.GetVisibleRangeTimes(dp, canvas);
            (double? minPrice, double? maxPrice) = Chart.GetVisibleRangePrices(canvas);

            if (x0 < x1)
            {
                if (minTime != null && minTime > x0)
                    x0 = minTime.Value;
                if (maxTime != null && maxTime < x1)
                    x1 = maxTime.Value;
            }
            else
            {
                if (minTime != null && minTime > x1)
                    x1 = minTime.Value;
                if (maxTime != null && maxTime < x0)
                    x0 = maxTime.Value;
            }

            if (y0 < y1)
            {
                if (minPrice != null && minPrice > y0)
                    y0 = minPrice.Value;
                if (maxPrice != null && maxPrice < y1)
                    y1 = maxPrice.Value;
            }
            else
            {
                if (minPrice != null && minPrice > y1)
                    y1 = minPrice.Value;
                if (maxPrice != null && maxPrice < y0)
                    y0 = maxPrice.Value;
            }

            return (mx: (x0 + x1) / 2, my: (y0 + y1) / 2);
        }

        /// <summary>
        /// Draw a control point with the `Corner` style.
        /// </summary>
        /// <param name="visual">The visual queue to use</param>
        /// <param name="point">The control point</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ControlPointCorner(DxVisualQueue visual, Point point)
        {
            visual.FillEllipse(
                ControlPointBrush,
                point,
                ControlPointCornerHalfSize,
                ControlPointCornerHalfSize
            );
            visual.DrawEllipse(
                ControlPointPen,
                point,
                ControlPointCornerHalfSize,
                ControlPointCornerHalfSize
            );
        }

        /// <summary>
        /// Draw a control point with the `Edge` style.
        /// </summary>
        /// <param name="visual">The visual queue to use</param>
        /// <param name="point">The control point</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ControlPointEdge(DxVisualQueue visual, Point point)
        {
            Rect rect = new Rect(
                point.X - ControlPointEdgeHalfSize,
                point.Y - ControlPointEdgeHalfSize,
                ControlPointEdgeSize,
                ControlPointEdgeSize
            );

            visual.FillRoundedRectangle(ControlPointBrush, rect, ControlPointEdgeRadiusPoint);
            visual.DrawRoundedRectangle(ControlPointPen, rect, ControlPointEdgeRadiusPoint);
        }

        /// <summary>
        /// Draw a control point with the `LockedCorner` style.
        /// </summary>
        /// <param name="visual">The visual queue to use</param>
        /// <param name="point">The control point</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ControlPointLockedCorner(DxVisualQueue visual, Point point)
        {
            visual.FillEllipse(
                ControlPointBrush,
                point,
                ControlPointLockedCornerHalfSize,
                ControlPointLockedCornerHalfSize
            );
            visual.DrawEllipse(
                ControlPointPen,
                point,
                ControlPointLockedCornerHalfSize,
                ControlPointLockedCornerHalfSize
            );
        }

        /// <summary>
        /// Draw a control point with the `LockedEdge` style.
        /// </summary>
        /// <param name="visual">The visual queue to use</param>
        /// <param name="point">The control point</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ControlPointLockedEdge(DxVisualQueue visual, Point point)
        {
            Rect rect = new Rect(
                point.X - ControlPointLockedEdgeHalfSize,
                point.Y - ControlPointLockedEdgeHalfSize,
                ControlPointLockedEdgeSize,
                ControlPointLockedEdgeSize
            );

            visual.FillRoundedRectangle(ControlPointBrush, rect, ControlPointEdgeRadiusPoint);
            visual.DrawRoundedRectangle(ControlPointPen, rect, ControlPointEdgeRadiusPoint);
        }

        /// <summary>
        /// Draw an alert bell at the given coordinates
        /// </summary>
        /// <param name="visual">The visual queue to use</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlertBell(DxVisualQueue visual, double x, double y)
        {
            visual.DrawString(AlertBellText, AlertBellFont, AlertBellBrush, x, y);
        }
    }

    public struct Line
    {
        public Point p0;
        public Point p1;
        public double thickness;

        public Line(Point p0, Point p1, double thickness = 1.0)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.thickness = thickness;
        }

        public static bool operator ==(Line l1, Line l2)
        {
            if (ReferenceEquals(l1, l2))
                return true;

            return l1.p0 == l2.p0 && l1.p1 == l2.p1 && l1.thickness == l2.thickness;
        }

        public static bool operator !=(Line l1, Line l2)
        {
            return !(l1 == l2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Line line)
                return this == line;

            return false;
        }

        public override int GetHashCode()
        {
            return (p0, p1, thickness).GetHashCode();
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

        public double GetAngle()
        {
            double angle = (double)(Math.Atan2(p1.Y - p0.Y, p1.X - p0.X) * (180 / Math.PI));
            if (angle < 0)
                angle += 360;

            return angle;
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

        public bool IntersectsWith(Point point)
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

        public bool IntersectsWith(Line line)
        {
            Point? intersection = GetIntersectionPoint(this.p0, this.p1, line.p0, line.p1);

            if (intersection == null)
                return false;

            return IntersectsWith(intersection.Value) || line.IntersectsWith(intersection.Value);
        }

        public bool IntersectsWith(Rect rect)
        {
            if (rect.Contains(p0) || rect.Contains(p1))
                return true;

            if (
                IntersectsWith(new Line(rect.TopLeft, rect.BottomRight))
                || IntersectsWith(new Line(rect.TopRight, rect.BottomLeft))
            )
                return true;

            return false;
        }

        public bool IsContainedBy(Rect rect)
        {
            return rect.Contains(p0) && rect.Contains(p1);
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

        public static Point? GetIntersectionPoint(Point a0, Point a1, Point b0, Point b1)
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
                return new Point(x, y);

            return null;
        }
    }
}
