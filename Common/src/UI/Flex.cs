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
using TigerTrade.Dx;

namespace CustomCommon.UI
{
    public class Flex : Base
    {
        private List<Base> components;
        private Direction direction = UI.Direction.Vertical;
        private Horizontal horizontalAlign = Horizontal.Left;
        private Vertical verticalAlign = Vertical.Top;
        private double gap = 0;

        public int Count
        {
            get => components.Count;
        }

        public Flex()
        {
            this.components = new List<Base>();
        }

        public Flex(params Base[] components)
        {
            this.components = new List<Base>(components);
        }

        public override double GetBaseWidth(double parentWidth)
        {
            double childWidth = GetPixelWidth(parentWidth);

            double baseWidth = 0;

            int count = components.Count;

            if (direction == UI.Direction.Horizontal)
            {
                if (count > 1)
                    childWidth -= gap * (count - 1);

                for (int i = 0; i < count; i++)
                {
                    baseWidth += components[i].GetOuterWidth(childWidth);
                }

                if (count > 1)
                    baseWidth += gap * (count - 1);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    double componentWidth = components[i].GetOuterWidth(childWidth);

                    if (componentWidth > baseWidth)
                        baseWidth = componentWidth;
                }
            }

            if (minWidthUnit != Unit.None)
                baseWidth = Math.Max(GetPixelMinWidth(parentWidth), baseWidth);
            if (maxWidthUnit != Unit.None)
                baseWidth = Math.Min(GetPixelMaxWidth(parentWidth), baseWidth);

            double realWidth = Math.Max(childWidth, baseWidth);

            if (widthUnit == Unit.Percent)
                realWidth -= marginLeft + marginRight;

            return realWidth;
        }

        public override double GetBaseHeight(double parentHeight)
        {
            double childHeight = GetPixelHeight(parentHeight);

            double baseHeight = 0;

            int count = components.Count;

            if (direction == UI.Direction.Horizontal)
            {
                for (int i = 0; i < count; i++)
                {
                    double componentHeight = components[i].GetOuterHeight(childHeight);

                    if (componentHeight > baseHeight)
                        baseHeight = componentHeight;
                }
            }
            else
            {
                if (count > 1)
                    childHeight -= gap * (count - 1);

                for (int i = 0; i < count; i++)
                {
                    baseHeight += components[i].GetOuterHeight(childHeight);
                }

                if (count > 1)
                    baseHeight += gap * (count - 1);
            }

            if (minHeightUnit != Unit.None)
                baseHeight = Math.Max(GetPixelMinHeight(parentHeight), baseHeight);
            if (maxHeightUnit != Unit.None)
                baseHeight = Math.Min(GetPixelMaxHeight(parentHeight), baseHeight);

            double realHeight = Math.Max(childHeight, baseHeight);

            if (heightUnit == Unit.Percent)
                realHeight -= marginTop + marginBottom;

            return realHeight;
        }

        public override void Render(
            DxVisualQueue visual,
            double x,
            double y,
            double parentWidth,
            double parentHeight
        )
        {
            RenderBase(visual, x, y, parentWidth, parentHeight);

            double baseWidth = GetBaseWidth(parentWidth);
            double baseHeight = GetBaseHeight(parentHeight);

            double innerX = GetBaseX(x);
            double innerY = GetBaseY(y);

            if (direction == UI.Direction.Horizontal)
            {
                int count = components.Count;

                if (count > 1)
                    baseWidth -= gap * (count - 1);

                for (int i = 0; i < count; i++)
                {
                    Base component = components[i];

                    if (verticalAlign == Vertical.Top)
                    {
                        component.Render(visual, innerX, innerY, baseWidth, baseHeight);
                    }
                    else if (verticalAlign == Vertical.Center)
                    {
                        component.Render(
                            visual,
                            innerX,
                            innerY + ((baseHeight - component.GetOuterHeight(baseHeight)) / 2),
                            baseWidth,
                            baseHeight
                        );
                    }
                    else if (verticalAlign == Vertical.Bottom)
                    {
                        component.Render(
                            visual,
                            innerX,
                            innerY + (baseHeight - component.GetOuterHeight(baseHeight)),
                            baseWidth,
                            baseHeight
                        );
                    }

                    innerX += component.GetOuterWidth(baseWidth) + gap;
                }
            }
            else
            {
                int count = components.Count;

                if (count > 1)
                    baseHeight -= gap * (count - 1);

                for (int i = 0; i < count; i++)
                {
                    Base component = components[i];

                    if (horizontalAlign == Horizontal.Left)
                    {
                        component.Render(visual, innerX, innerY, baseWidth, baseHeight);
                    }
                    else if (horizontalAlign == Horizontal.Center)
                    {
                        component.Render(
                            visual,
                            innerX + ((baseWidth - component.GetOuterWidth(baseWidth)) / 2),
                            innerY,
                            baseWidth,
                            baseHeight
                        );
                    }
                    else if (horizontalAlign == Horizontal.Right)
                    {
                        component.Render(
                            visual,
                            innerX + (baseWidth - component.GetOuterWidth(baseWidth)),
                            innerY,
                            baseWidth,
                            baseHeight
                        );
                    }

                    innerY += component.GetOuterHeight(baseHeight) + gap;
                }
            }
        }

        public List<Base> Get()
        {
            return components;
        }

        public Flex Set(List<Base> components)
        {
            this.components = components;
            return this;
        }

        public Flex Clear()
        {
            components.Clear();
            return this;
        }

        public Flex Insert(int index, Base component)
        {
            components.Insert(index, component);
            return this;
        }

        public Flex Add(Base component)
        {
            components.Add(component);
            return this;
        }

        public Flex Remove(Base component)
        {
            components.Remove(component);
            return this;
        }

        public Flex RemoveAt(int index)
        {
            components.RemoveAt(index);
            return this;
        }

        public Direction GetDirection()
        {
            return this.direction;
        }

        public Flex Direction(Direction direction)
        {
            this.direction = direction;
            return this;
        }

        public Horizontal GetHorizontalAlign()
        {
            return this.horizontalAlign;
        }

        public Vertical GetVerticalAlign()
        {
            return this.verticalAlign;
        }

        public Flex Align(Horizontal align)
        {
            horizontalAlign = align;
            return this;
        }

        public Flex Align(Vertical align)
        {
            verticalAlign = align;
            return this;
        }

        public Flex Align(Horizontal hAlign, Vertical vAlign)
        {
            horizontalAlign = hAlign;
            verticalAlign = vAlign;
            return this;
        }

        public double GetGap()
        {
            return this.gap;
        }

        public Flex Gap(double gap)
        {
            this.gap = gap;
            return this;
        }
    }
}
