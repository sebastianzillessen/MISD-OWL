using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Model
{
    public interface ITileCustomUI
    {
        void SelectIndicatorValues();

        void Expand();

        void Collapse();
    }
}
