using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MISD.Client.Model;

namespace MISD.Client.Model
{
    /// <summary>
    /// This interface defines the necessary values for plugin visualizations.
    /// </summary>
    public interface IPluginVisualization
    {
        /// <summary>
        /// Gets or sets the unique plugin name of this visualization.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a value that specifies how many rows the custom UI of this plugin visualization needs inside a tile.
        /// </summary>
        int Rows { get; }

        /// <summary>
        /// Gets or sets a value that specifies how many columns the custom UI of this plugin visualization needs inside a tile.
        /// </summary>
        int Columns { get; }

        /// <summary>
        /// Calculates the main value of this plugin depending on the given indicator values.
        /// </summary>
        string CalculateMainValue(IEnumerable<Indicator> indicatorValues);
    }
}
