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

using System.Diagnostics;

namespace CustomCommon.Debug
{
    public sealed class DebugTime
    {
        public static string SmallStopwatch(Stopwatch sw)
        {
            long ticks = sw.ElapsedTicks;
            long frequency = Stopwatch.Frequency;

            ticks *= 1000;
            long millis = ticks / frequency;

            ticks *= 1000;
            long micros = (ticks / frequency) % 1000;

            return $"{millis}ms {micros}µs";
        }
    }
}
