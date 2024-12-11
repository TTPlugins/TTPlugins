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

namespace CustomCommon.Helpers
{
    public static class Decimals
    {
        /// <summary>
        /// Rounds a number to a given multiplier
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RoundBy(double n, double multiplier)
        {
            return Math.Round(Math.Round(n / multiplier) * multiplier, Count(multiplier));
        }

        /// <summary>
        /// Counts the decimals in a number
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count(double n)
        {
            if (double.IsNaN(n) || double.IsInfinity(n))
                return 0;

            string[] parts = n.ToString("F17").Split('.');
            if (parts.Length < 2)
                return 0;

            return parts[1].TrimEnd('0').Length;
        }
    }
}
