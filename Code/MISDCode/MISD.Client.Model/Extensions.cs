using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Model
{
    public static class Extensions
    {
        public static Version GetVersion(this IPluginVisualization plugin)
        {
            return System.Reflection.Assembly.GetAssembly(plugin.GetType()).GetName().Version;
        }

        public static IEnumerable<MonitoredSystem> GetMonitoredSystems(this IEnumerable<TileableElement> elements)
        {
            var result = new List<MonitoredSystem>();
            elements.ToList().ForEach(p => result.AddRange(p.GetMonitoredSystems()));
            return result;
        }

        public static IEnumerable<OrganizationalUnit> GetOrganizationalUnits(this IEnumerable<TileableElement> elements)
        {
            var result = new List<OrganizationalUnit>();
            elements.ToList().ForEach(p => result.AddRange(p.GetOrganizationalUnits()));
            return result;
        }

        public static IEnumerable<MonitoredSystem> GetMonitoredSystems(this TileableElement element)
        {
            var result = new List<MonitoredSystem>();

            if (element is MonitoredSystem)
            {
                result.Add(element as MonitoredSystem);
            }
            else
            {
                var value = element as OrganizationalUnit;

                // Add own monitoredsystems
                result.AddRange(from p in value.Elements
                                where (p as MonitoredSystem) != null
                                select p as MonitoredSystem);

                // Add monitoredsystems of child OUs
                (from p in value.Elements
                 where (p as OrganizationalUnit) != null
                 select p as OrganizationalUnit).ToList().ForEach(p => result.AddRange(p.GetMonitoredSystems()));
            }

            return result;
        }

        public static IEnumerable<OrganizationalUnit> GetOrganizationalUnits(this TileableElement element)
        {
            var result = new List<OrganizationalUnit>();

            if (element is OrganizationalUnit)
            {
                var value = element as OrganizationalUnit;

                // Add the element itself
                result.Add(value);

                // Add monitoredsystems of child OUs
                (from p in value.Elements
                 where (p as OrganizationalUnit) != null
                 select p as OrganizationalUnit).ToList().ForEach(p => result.AddRange(p.GetOrganizationalUnits()));
            }

            return result;
        }
    }
}
