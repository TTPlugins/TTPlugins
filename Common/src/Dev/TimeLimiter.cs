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

namespace CustomCommon.Debug
{
    public sealed class TimeLimiter
    {
        private long rate;
        private long last;

        public TimeLimiter()
        {
            this.rate = 0;
            this.last = 0;
        }

        public TimeLimiter(long rate)
        {
            this.rate = rate;
            this.last = 0;
        }

        /// <summary>
        /// Get the current unix timestamp in milliseconds
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Now()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Check if the time elapsed since the last update has elapsed the rate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check()
        {
            long now = Now();

            if (now - this.last < this.rate)
                return false;

            this.last = now;
            return true;
        }

        /// <summary>
        /// Check if the time elapsed since the last update has elapsed the given rate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(long rate)
        {
            long now = Now();

            if (now - this.last < rate)
                return false;

            this.last = now;
            return true;
        }
    }
}
