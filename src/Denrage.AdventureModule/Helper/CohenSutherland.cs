using Denrage.AdventureModule.Libs.Messages.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Helper
{
    public static class CohenSutherland
    {
        [Flags]
        private enum OutCode
        {
            Inside = 0,
            Left = 1,
            Right = 2,
            Bottom = 4,
            Top = 8,
        }
        
        private static OutCode ComputeOutCode(double x, double y, ref Rectangle rectangle)
        {
            var result = OutCode.Inside;

            if (x < rectangle.X)
            {
                result |= OutCode.Left;
            }
            else if(x > rectangle.X + rectangle.Width)
            {
                result |= OutCode.Right;
            }

            if (y < rectangle.Y)
            {
                result |= OutCode.Top;
            }
            else if (y > rectangle.Y + rectangle.Height)
            {
                result |= OutCode.Bottom;
            }

            return result;
        }

        public static bool IsIntersecting(ref Line line, ref Rectangle rectangle)
        {
            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            var pointOne = ComputeOutCode(line.Start.X, line.Start.Y, ref rectangle);
            var pointTwo = ComputeOutCode(line.End.X, line.End.Y, ref rectangle);
            var currentStart = new Point((int)line.Start.X, (int)line.Start.Y);
            var currentEnd = new Point((int)line.End.X, (int)line.End.Y);
            bool accept = false;

            while (true)
            {
                if (pointOne == OutCode.Inside && pointTwo == OutCode.Inside)
                {
                    // bitwise OR is 0: both points inside window; trivially accept and exit loop
                    accept = true;
                    break;
                }
                else if (((int)pointOne & (int)pointTwo) != 0)
                {
                    // bitwise AND is not 0: both points share an outside zone (LEFT, RIGHT, TOP,
                    // or BOTTOM), so both must be outside window; exit loop (accept is false)
                    break;
                }
                else
                {
                    // failed both tests, so calculate the line segment to clip
                    // from an outside point to an intersection with clip edge
                    int x = 0, y = 0;

                    // At least one endpoint is outside the clip rectangle; pick it.
                    var outcodeOut = pointTwo > pointOne ? pointTwo : pointOne;

                    // Now find the intersection point;
                    // use formulas:
                    //   slope = (y1 - y0) / (x1 - x0)
                    //   x = x0 + (1 / slope) * (ym - y0), where ym is ymin or ymax
                    //   y = y0 + slope * (xm - x0), where xm is xmin or xmax
                    // No need to worry about divide-by-zero because, in each case, the
                    // outcode bit being tested guarantees the denominator is non-zero
                    if (outcodeOut.HasFlag(OutCode.Bottom))
                    {           // point is above the clip window
                        x = currentStart.X + (currentEnd.X - currentStart.X) * (rectangle.Y + rectangle.Height - currentStart.Y) / (currentEnd.Y - currentStart.Y);
                        y = rectangle.Y + rectangle.Height;
                    }
                    else if (outcodeOut.HasFlag(OutCode.Top))
                    { // point is below the clip window
                        x = currentStart.X + (currentEnd.X - currentStart.X) * (rectangle.Y - currentStart.Y) / (currentEnd.Y - currentStart.Y);
                        y = rectangle.Y;
                    }
                    else if (outcodeOut.HasFlag(OutCode.Right))
                    {  // point is to the right of clip window
                        y = currentStart.Y + (currentEnd.Y - currentStart.Y) * (rectangle.X + rectangle.Width - currentStart.X) / (currentEnd.X - currentStart.X);
                        x = rectangle.X + rectangle.Width;
                    }
                    else if (outcodeOut.HasFlag(OutCode.Left))
                    {   // point is to the left of clip window
                        y = currentStart.Y + (currentEnd.Y - currentStart.Y) * (rectangle.X - currentStart.X) / (currentEnd.X - currentStart.X);
                        x = rectangle.X;
                    }

                    // Now we move outside point to intersection point to clip
                    // and get ready for next pass.
                    if (outcodeOut == pointOne)
                    {
                        currentStart.X = x;
                        currentStart.Y = y;
                        pointOne = ComputeOutCode(currentStart.X, currentStart.Y, ref rectangle);
                    }
                    else
                    {
                        currentEnd.X = x;
                        currentEnd.Y = y;
                        pointTwo = ComputeOutCode(currentEnd.X, currentEnd.Y, ref rectangle);
                    }
                }
            }

            return accept;
        }
    }
}
