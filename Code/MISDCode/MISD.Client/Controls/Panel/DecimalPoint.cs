using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Controls.Panels
{
    public struct DecimalPoint
    {
        public decimal X;
        public decimal Y;

        public DecimalPoint(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public Point ToPoint()
        {
            return new Point((double)X, (double)Y);
        }
    }
}
