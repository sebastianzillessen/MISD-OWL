using MISD.Client.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MISD.Client.ViewModel;
using System.Windows;

namespace MISD.Client.Controls.Panels
{
    public static class PointExtensions
    {
        public static decimal LineLength(this DecimalPoint p, DecimalPoint q)
        {
            return (decimal)Math.Sqrt(Math.Pow((double)p.X - (double)q.X, 2) + Math.Pow((double)p.Y + (double)q.Y, 2));
        }

        public static decimal LineHeight(this DecimalPoint p, DecimalPoint q)
        {
            return Math.Abs(p.Y - q.Y);
        }

        public static DecimalPoint AddPoint(this DecimalPoint p, DecimalPoint q)
        {
            return new DecimalPoint(p.X + q.X, p.Y + q.Y);
        }
    }

    public class PanelHelper
    {
        double tileWidth
        {
            get
            {
                return MainWindowViewModel.Instance.TileWidth;
            }
        }
        double tileMargin = 8;

        decimal Height
        {
            get;
            set;
        }
        Dictionary<DecimalPoint, DecimalPoint> Points
        {
            get;
            set;
        }

        public PanelHelper(double height)
        {
            if (double.IsInfinity(height))
            {
                this.Height = decimal.MaxValue;
            }
            else
            {
                this.Height = (decimal)height;
            }
            this.Points = new Dictionary<DecimalPoint, DecimalPoint>();
            this.Points.Add(new DecimalPoint(0, 0), new DecimalPoint(0, this.Height));
        }

        public System.Windows.Size DesiredSize
        {
            get
            {
                this.OptimizePoints();

                decimal x = 0;
                decimal y = 0;
                foreach (var entry in this.Points)
                {
                    if (entry.Value.Y > y && entry.Value.X > 0)
                    {
                        y = entry.Value.Y;
                    }
                    if (entry.Value.X > x)
                    {
                        x = entry.Value.X;
                    }
                }

                // TODO: Replace this.Height with y
                return new Size((double)x, (double)y + 1);
            }
        }

        public void OptimizePoints()
        {
            foreach (var element in (from p in this.Points
                                     from q in this.Points
                                     where p.Value.X == q.Key.X && p.Value.Y == q.Key.Y
                                     select new { p, q }).ToArray())
            {
                this.Points.Remove(element.p.Key);
                this.Points.Remove(element.q.Key);
                this.Points.Add(element.p.Key, element.q.Value);
            }

            foreach (var element in this.Points.ToArray())
            {
                if (element.Key.X == element.Value.X && element.Key.Y == element.Value.Y)
                {
                    this.Points.Remove(element.Key);
                }
            }
        }

        public Rect PlaceControl(UIElement child)
        {
            var finalSize = child.DesiredSize;

            if (finalSize.Width == 0 || finalSize.Height == 0) return new Rect(0, 0, 0, 0);

            this.OptimizePoints();

            var sortedPointsLeftFirst = from p in this.Points
                                        orderby p.Key.X, p.Key.Y ascending
                                        select p;
            var sortedPointsTopFirst = from p in this.Points
                                       orderby p.Key.Y, p.Key.X ascending
                                       select p;

            Rect finalRect = new Rect();

            for (int i = 0; i < sortedPointsLeftFirst.Count(); i++)
            {
                var entry = sortedPointsLeftFirst.ElementAt(i);

                var upperPoint = entry.Key;
                var lowerPoint = entry.Value;

                if ((decimal)finalSize.Height <= upperPoint.LineHeight(lowerPoint))
                {
                    // Case 1: The tile is shorter or equal to the current line
                    finalRect = new Rect(upperPoint.ToPoint(), finalSize);

                    // Split line
                    var newUpperPoint1 = new DecimalPoint(upperPoint.X + (decimal)finalSize.Width, upperPoint.Y);
                    var newLowerPoint1 = new DecimalPoint(upperPoint.X + (decimal)finalSize.Width, upperPoint.Y + (decimal)finalSize.Height);

                    this.Points.Remove(upperPoint);

                    this.Points.Add(newUpperPoint1, newLowerPoint1);

                    if ((decimal)finalSize.Height < upperPoint.LineHeight(lowerPoint))
                    {
                        var newUpperPoint2 = new DecimalPoint(lowerPoint.X, upperPoint.Y + (decimal)finalSize.Height);
                        var newLowerPoint2 = lowerPoint;
                        this.Points.Add(newUpperPoint2, newLowerPoint2);
                    }

                    break;
                }
                else
                {
                    // Case 2:The tile is longer than the current line
                    var lowerNeighbors = new Dictionary<DecimalPoint, DecimalPoint>();
                    decimal sum = upperPoint.LineHeight(lowerPoint);
                    foreach (var neighbor in from p in sortedPointsTopFirst
                                             where p.Key.Y >= lowerPoint.Y
                                             select p)
                    {
                        lowerNeighbors.Add(neighbor.Key, neighbor.Value);
                        sum += neighbor.Key.LineHeight(neighbor.Value);
                        if (sum >= (decimal)finalSize.Height)
                        {
                            break;
                        }
                    }

                    if (sum < (decimal)finalSize.Height)
                    {
                        // We would leak outside the view at the bottom
                        continue;
                    }

                    // Now we do have all lower neighbors
                    if (lowerNeighbors.Count > 0)
                    {
                        // Do we have place or not?
                        bool cont = false;
                        foreach (var neighbor in lowerNeighbors)
                        {
                            if (neighbor.Key.X > upperPoint.X)
                            {
                                // We have no space!
                                cont = true;
                                break;
                            }
                        }
                        if (cont) continue;
                    }
                    else
                    {
                        continue;
                    }

                    // We have space
                    finalRect = new Rect(new Point((double)upperPoint.X, (double)upperPoint.Y), finalSize);

                    var lowestNeighbor = lowerNeighbors.Last();

                    // Remove all lower neighbor points
                    for (int j = 0; j < lowerNeighbors.Count; j++)
                    {
                        this.Points.Remove(lowerNeighbors.ElementAt(j).Key);
                    }

                    this.Points.Remove(upperPoint);

                    // Add the necessary points
                    var newUpperPoint1 = new DecimalPoint(upperPoint.X + (decimal)finalSize.Width, upperPoint.Y);
                    var newLowerPoint1 = new DecimalPoint(upperPoint.X + (decimal)finalSize.Width, upperPoint.Y + (decimal)finalSize.Height);

                    var newUpperPoint2 = new DecimalPoint(lowestNeighbor.Key.X, upperPoint.Y + (decimal)finalSize.Height);
                    var newLowerPoint2 = lowestNeighbor.Value;

                    this.Points.Add(newUpperPoint1, newLowerPoint1);
                    this.Points.Add(newUpperPoint2, newLowerPoint2);

                    return finalRect;

                }

            }

            if (finalRect == new Rect())
            {
                var right = this.Points.Max(p => p.Key.X);

                var topPoint = (from p in this.Points
                                where p.Key.Y == 0
                                select p).First();

                this.Points.Remove(topPoint.Key);
                this.Points.Add(new DecimalPoint(right, topPoint.Key.Y), new DecimalPoint(right, topPoint.Value.Y));

                this.OptimizePoints();
                return this.PlaceControl(child);
            }

            return finalRect;
        }
    }
}
