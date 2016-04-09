using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MISD.Client.Managers;
using MISD.Client.Model;
using System.Windows.Media.Animation;

namespace MISD.Client.Controls.Panels
{
    public class SpaceFillingPanel : System.Windows.Controls.Panel
    {
        public SpaceFillingPanel()
        {
            // allow dropping inside this panel
            //this.AllowDrop = true;
            //this.Drop += DropHandler;
        }

        /// <summary>
        /// Enables the dropping of OUs or MonitoredSystems inside the hierarchy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void DropHandler(object sender, DragEventArgs e)
        //{
        //    try
        //    {
        //        // Tuple: bool isOU, int ID/ouID, int ouID/parentID
        //        Tuple<bool, int, int> oldValues = (Tuple<bool, int, int>)e.Data.GetData(typeof(Tuple<bool, int, int>));

        //        // dropping a OU
        //        if (oldValues.Item1 == true)
        //        {
        //            // find out which element was hit
        //            var result = VisualTreeHelper.HitTest(this, e.GetPosition(sender as UIElement));

        //            DependencyObject parent = result.VisualHit;
        //            while (parent != null && !(parent.GetType().Equals(typeof(ExtendedTreeViewItem))))
        //            {
        //                parent = VisualTreeHelper.GetParent(parent);
        //            }

        //            // find the corresponding monitored system
        //            if (parent != null)
        //            {
        //                var monitoredSystem = (parent as ExtendedTreeViewItem).DataContext as MonitoredSystem;

        //                // move the monitored System
        //                if (monitoredSystem != null)
        //                {
        //                    // check if the old and new OU are different
        //                    if (!oldValues.Item3.Equals(monitoredSystem.OuID))
        //                    {
        //                        foreach (OrganizationalUnit current in DataModel.Instance.GetOrganizationalUnits(DataModel.Instance.Elements))
        //                        {
        //                            if (current.ID.Equals(oldValues.Item3))
        //                            {
        //                                current.ParentID = monitoredSystem.OuID;
        //                            }
        //                        }
        //                    }
        //                    LayoutManager.Instance.MoveBefore(oldValues.Item3, monitoredSystem.ID, true);
        //                    e.Handled = true;
        //                }
        //                else
        //                {
        //                    // find out the OUID of this panel
        //                    int OUID = -1;
        //                    foreach (UIElement child in Children)
        //                    {
        //                        if (child.GetType().Equals(typeof(Tile)))
        //                        {
        //                            OUID = (child as Tile).MonitoredSystem.OuID;
        //                            break;
        //                        }
        //                    }

        //                    // check if the old and new OU are different
        //                    if (OUID != -1 && !oldValues.Item3.Equals(OUID))
        //                    {
        //                        foreach (OrganizationalUnit current in DataModel.Instance.GetOrganizationalUnits(DataModel.Instance.Elements))
        //                        {
        //                            if (current.ID.Equals(oldValues.Item3))
        //                            {
        //                                current.ParentID = OUID;
        //                                e.Handled = true;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        // dropping an MonitoredSystem
        //        else
        //        {
        //            // find out which element was hit
        //            var result = VisualTreeHelper.HitTest(this, e.GetPosition(sender as UIElement));

        //            DependencyObject parent = result.VisualHit;
        //            while (parent != null && !(parent.GetType().Equals(typeof(ExtendedTreeViewItem))))
        //            {
        //                parent = VisualTreeHelper.GetParent(parent);
        //            }

        //            // find the corresponding monitored system
        //            if (parent != null)
        //            {
        //                var monitoredSystem = (parent as TreeViewItem).DataContext as MonitoredSystem;

        //                // check if the OU has changed as well
        //                if (monitoredSystem != null && !oldValues.Item3.Equals(monitoredSystem.OuID))
        //                {
        //                    foreach (MonitoredSystem current in DataModel.Instance.GetMonitoredSystems(DataModel.Instance.Elements))
        //                    {
        //                        if (current.ID.Equals(oldValues.Item2))
        //                        {
        //                            current.OuID = monitoredSystem.OuID;
        //                        }
        //                    }
        //                }

        //                // move the monitored System
        //                if (monitoredSystem != null)
        //                {
        //                    LayoutManager.Instance.MoveBefore(oldValues.Item2, monitoredSystem.ID, false);
        //                    e.Handled = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("EXCEPTION: SpaceFillingPanel_DropHandler: " + ex.ToString());
        //    }
        //}

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            PanelHelper helper = new PanelHelper(finalSize.Height);

            foreach (UIElement child in Children)
            {
                var offsetBefore = VisualTreeHelper.GetOffset(child);

                child.Arrange(helper.PlaceControl(child));

                var offsetAfter = VisualTreeHelper.GetOffset(child);

                TranslateTransform transform = new TranslateTransform();
                child.RenderTransform = transform;

                DoubleAnimation animation = new DoubleAnimation(offsetBefore.X - offsetAfter.X, 0, new Duration(new TimeSpan(0, 0, 0, 0, 800)));
                animation.EasingFunction = new ExponentialEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                animation.AccelerationRatio = 0;
                animation.DecelerationRatio = 1;
                transform.BeginAnimation(TranslateTransform.XProperty, animation);

                DoubleAnimation animationY = new DoubleAnimation(offsetBefore.Y - offsetAfter.Y, 0, new Duration(new TimeSpan(0, 0, 0, 0, 800)));
                animationY.EasingFunction = new ExponentialEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                animationY.AccelerationRatio = 0;
                animationY.DecelerationRatio = 1;
                transform.BeginAnimation(TranslateTransform.YProperty, animationY);
            }

            return finalSize;
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            PanelHelper helper = new PanelHelper(availableSize.Height);

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                helper.PlaceControl(child);
            }

            return helper.DesiredSize;
        }
    }
}

/// OLD LAYOUT STUFF

//protected override Size ArrangeOverride(System.Windows.Size finalSize)
//{
//    foreach (UIElement child in Children)
//    {
//        child.IsHitTestVisible = false;
//    }

//    double x = 0;
//    double y = 0;

//    // Iterate over all elements
//    foreach (UIElement child in Children)
//    {
//        var yLow = FindPosition(x, y, child, false);

//        if (yLow + child.DesiredSize.Height >= finalSize.Height)
//        {
//            var xLow = FindPosition(x, 0, child, true);

//            bool foundLowerPosition = false;
//            double lowerY = 0;
//            double lowerX = 0;

//            for (double i = x + 216; i < xLow; i = i + 216)
//            {
//                lowerY = FindPosition(i, 0, child, false);

//                if (lowerY > 0 && lowerY + child.DesiredSize.Height < finalSize.Height)
//                {
//                    foundLowerPosition = true;
//                    lowerX = i;
//                    break;
//                }
//            }

//            if (foundLowerPosition)
//            {
//                x = lowerX;
//                y = lowerY;

//            }
//            else
//            {
//                x = xLow;
//                y = 0;
//            }
//        }
//        else
//        {
//            y = yLow;
//        }

//        // Arranges the child element at the position that was figured out
//        child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
//        child.IsHitTestVisible = true;
//    }

//    return finalSize;
//}

//private static HitTestResult HitTest(Visual visual, Point point)
//{
//    HitTestResult result = null;

//    VisualTreeHelper.HitTest(visual,
//                 (target) =>
//                 {
//                     var uiElement = target as UIElement;
//                     if (uiElement != null && !uiElement.IsHitTestVisible)
//                         return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
//                     else
//                         return HitTestFilterBehavior.Continue;
//                 },
//                 (target) =>
//                 {
//                     result = target;
//                     return HitTestResultBehavior.Stop;
//                 },
//                 new PointHitTestParameters(point));

//    return result;
//}

//private double FindPosition(double x, double y, UIElement child, bool inX)
//{
//    var xLow = x;
//    var xHigh = xLow + child.DesiredSize.Width;
//    var yLow = y;
//    var yHigh = yLow + child.DesiredSize.Height;
//    var hitLow = HitTest(this, new Point(xLow + 8, yLow + 4));
//    var hitLowHigh = HitTest(this, new Point(xLow + 8, yHigh - 4));
//    var hitHigh = HitTest(this, new Point(xHigh - 8, yHigh - 4));
//    var hitHighLow = HitTest(this, new Point(xHigh - 8, yLow + 4));
//    var hitLow2 = HitTest(this, new Point(xLow, yLow));
//    var hitLowHigh2 = HitTest(this, new Point(xLow, yHigh));
//    var hitHigh2 = HitTest(this, new Point(xHigh, yHigh));
//    var hitHighLow2 = HitTest(this, new Point(xHigh, yLow));

//    if (inX)
//    {
//        // move in x-direction until no hit
//        while (hitLow != null || hitHigh != null || hitLowHigh != null || hitHighLow != null || hitLow2 != null || hitHigh2 != null || hitLowHigh2 != null || hitHighLow2 != null)
//        {
//            xLow += 24;
//            xHigh = xLow + child.DesiredSize.Width;
//            hitLow = HitTest(this, new Point(xLow + 8, yLow + 4));
//            hitLowHigh = HitTest(this, new Point(xLow + 8, yHigh - 4));
//            hitHigh = HitTest(this, new Point(xHigh - 8, yHigh - 4));
//            hitHighLow = HitTest(this, new Point(xHigh - 8, yLow + 4));
//            hitLow2 = HitTest(this, new Point(xLow, yLow));
//            hitLowHigh2 = HitTest(this, new Point(xLow, yHigh));
//            hitHigh2 = HitTest(this, new Point(xHigh, yHigh));
//            hitHighLow2 = HitTest(this, new Point(xHigh, yLow));
//        }

//        return xLow;
//    }
//    else
//    {
//        // move in y-direction until no hit
//        while (hitLow != null || hitHigh != null || hitLowHigh != null || hitHighLow != null || hitLow2 != null || hitHigh2 != null || hitLowHigh2 != null || hitHighLow2 != null)
//        {
//            yLow += 12;
//            yHigh = yLow + child.DesiredSize.Height;
//            hitLow = HitTest(this, new Point(xLow + 8, yLow + 4));
//            hitLowHigh = HitTest(this, new Point(xLow + 8, yHigh - 4));
//            hitHigh = HitTest(this, new Point(xHigh - 8, yHigh - 4));
//            hitHighLow = HitTest(this, new Point(xHigh - 8, yLow + 4));
//            hitLow2 = HitTest(this, new Point(xLow, yLow));
//            hitLowHigh2 = HitTest(this, new Point(xLow, yHigh));
//            hitHigh2 = HitTest(this, new Point(xHigh, yHigh));
//            hitHighLow2 = HitTest(this, new Point(xHigh, yLow));
//        }

//        return yLow;
//    }
//}

//protected override Size MeasureOverride(Size availableSize)
//{
//    double x = 0;
//    double y = 0;
//    double lastHeight = 0;
//    double maxWidth = 0;
//    double totalWidth = 0;
//    double totalHeight = 0;

//    foreach (UIElement child in Children)
//    {
//        child.Measure(availableSize);

//        // Row if full, so a new column is needed
//        if (y + lastHeight + child.DesiredSize.Height > availableSize.Height)
//        {
//            totalWidth += maxWidth;
//            // New column
//            x += maxWidth;
//            y = 0;
//            maxWidth = 0;
//            lastHeight = child.DesiredSize.Height;
//        }
//        // No new column needed
//        else
//        {
//            totalHeight += lastHeight;
//            // New row
//            y += lastHeight;
//            lastHeight = child.DesiredSize.Height;
//        }

//        // Save maximum width in current row
//        maxWidth = Math.Max(child.DesiredSize.Width, maxWidth);
//    }
//    totalWidth += maxWidth;
//    totalHeight += lastHeight;

//    var resultSize = new Size();

//    resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? totalWidth : availableSize.Width;
//    resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? totalHeight : availableSize.Height;

//    return resultSize;
//}