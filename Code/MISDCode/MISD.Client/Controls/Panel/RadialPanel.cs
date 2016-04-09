using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace MISD.Client.Controls.Panels
{
    /// <summary>
    /// Panel to display items in a radial/arc.
    /// </summary>
    public class RadialPanel : Panel
    {
        #region Dependency Properties

        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register("OuterRadius", typeof(double), typeof(RadialPanel),
            new UIPropertyMetadata(0.0, OuterRadiusChanged));

        private static void OuterRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var arcPanel = obj as RadialPanel;
            if (arcPanel != null)
            {
                arcPanel.Width = (double)e.NewValue * 2;
                arcPanel.Height = (double)e.NewValue * 2;
            }
        }

        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(RadialPanel), new UIPropertyMetadata(0.0));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(RadialPanel), new UIPropertyMetadata(360.0));

        public bool IsClockwise
        {
            get { return (bool)GetValue(IsClockwiseProperty); }
            set { SetValue(IsClockwiseProperty, value); }
        }

        public static readonly DependencyProperty IsClockwiseProperty =
            DependencyProperty.Register("IsClockwise", typeof(bool), typeof(RadialPanel), new UIPropertyMetadata(true));

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(RadialPanel), new UIPropertyMetadata(0.0));

        public bool ShowBorder
        {
            get { return (bool)GetValue(ShowBorderProperty); }
            set { SetValue(ShowBorderProperty, value); }
        }

        public static readonly DependencyProperty ShowBorderProperty =
            DependencyProperty.Register("ShowBorder", typeof(bool), typeof(RadialPanel), new UIPropertyMetadata(true));

        public bool ShowPieLines
        {
            get { return (bool)GetValue(ShowPieLinesProperty); }
            set { SetValue(ShowPieLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowPieLinesProperty =
            DependencyProperty.Register("ShowPieLines", typeof(bool), typeof(RadialPanel), new UIPropertyMetadata(false));

        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(Brush), typeof(RadialPanel),
                                        new UIPropertyMetadata(Brushes.Gray));

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(RadialPanel),
                                        new UIPropertyMetadata(Colors.LightGray));

        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register("BackgroundOpacity", typeof(double), typeof(RadialPanel),
                                        new UIPropertyMetadata(0.4));

        #endregion

        private double _angleEach;

        protected override Size MeasureOverride(Size availableSize)
        {
            CalculateAnglePerSection();

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            CalculateAnglePerSection();

            var startAngle = AngleToRadian(StartAngle);
            var startPointX = OuterRadius + (IsClockwise ? 1 : -1) * Math.Sin(startAngle) * (OuterRadius + InnerRadius) / 2;
            var startPointY = (OuterRadius - InnerRadius) / 2 + (1 - Math.Cos(startAngle)) * (OuterRadius + InnerRadius) / 2;
            var currentPosition = new Point(startPointX, startPointY);

            int childCount = Children.Count;
            double perAngle = AngleToRadian(Angle) / childCount;

            for (int i = 0; i < childCount; i++)
            {
                UIElement child = Children[i];

                var angle = (i + 1) * perAngle + startAngle;
                var offsetX = Math.Sin(angle) * (OuterRadius + InnerRadius) / 2;
                var offsetY = (1 - Math.Cos(angle)) * (OuterRadius + InnerRadius) / 2;

                var childRect = new Rect(new Point(currentPosition.X - child.DesiredSize.Width / 2,
                                                currentPosition.Y - child.DesiredSize.Height / 2),
                                        new Point(currentPosition.X + child.DesiredSize.Width / 2,
                                                currentPosition.Y + child.DesiredSize.Height / 2));
                child.Arrange(childRect);
                currentPosition.X = (IsClockwise ? 1 : -1) * offsetX + OuterRadius;
                currentPosition.Y = offsetY + (OuterRadius - InnerRadius) / 2;
            }

            return new Size(2 * OuterRadius, 2 * OuterRadius);
        }

        private void CalculateAnglePerSection()
        {
            _angleEach = Angle / InternalChildren.Count;
        }

        private static double AngleToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var centerPoint = new Point(RenderSize.Width / 2.0, RenderSize.Height / 2.0);
            var pen = new Pen(BorderColor, 1.0) { DashStyle = DashStyles.Solid };

            // Display inner & outer circles.
            if (ShowBorder)
            {
                var renderBrush = new SolidColorBrush(BackgroundColor) { Opacity = BackgroundOpacity };

                var geometryGroup = new GeometryGroup { FillRule = FillRule.EvenOdd };
                var outerBorder = OuterRadius - 1;
                geometryGroup.Children.Add(new EllipseGeometry { RadiusX = outerBorder, RadiusY = outerBorder, Center = centerPoint });
                geometryGroup.Children.Add(new EllipseGeometry { RadiusX = InnerRadius, RadiusY = InnerRadius, Center = centerPoint });

                dc.DrawGeometry(renderBrush, pen, geometryGroup);
            }

            if (ShowPieLines)
            {
                if (InternalChildren.Count == 1)
                    return;

                // Initialize angle.
                var angleChild = -(_angleEach / 2.0) - 90.0;

                //Take into account the requested start angle
                angleChild += StartAngle;

                // Loop through each child to draw radial lines from center.
#pragma warning disable 168
                foreach (var child in InternalChildren)
#pragma warning restore 168
                {
                    var angleChildInRadian = 2.0 * Math.PI * angleChild / 360;
                    var innerPoint = new Point(centerPoint.X + (InnerRadius * Math.Cos(angleChildInRadian)),
                                               centerPoint.Y + (InnerRadius * Math.Sin(angleChildInRadian)));
                    var outerPoint = new Point(centerPoint.X + (OuterRadius * Math.Cos(angleChildInRadian)),
                                               centerPoint.Y + (OuterRadius * Math.Sin(angleChildInRadian)));
                    dc.DrawLine(pen, innerPoint, outerPoint);
                    angleChild += _angleEach;
                }
            }
        }
    }
}

