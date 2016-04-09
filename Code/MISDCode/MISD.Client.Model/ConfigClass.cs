using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISD.Client.Model
{
    public class ConfigClass
    {
        private static bool? isPowerwall = null;
        public static bool IsPowerwall
        {
            get
            {
                if (!isPowerwall.HasValue)
                {
                    ReadAttributesFromCommandLine();
                }
                if (isPowerwall.HasValue)
                    return isPowerwall.Value;
                else
                    return Client.Properties.Settings.Default.IsPowerwall;

            }
            set
            {
                isPowerwall = value;
            }
        }

        private static bool ReadAttributesFromCommandLine()
        {
            if (Environment.GetCommandLineArgs().Contains("powerwall"))
            {
                string l = Environment.GetCommandLineArgs()[1].ToLower().Trim();
                isPowerwall = l.Equals("powerwall");
                isOperator = !isPowerwall;
                if (Environment.GetCommandLineArgs().Length > 2)
                {
                    string ip = Environment.GetCommandLineArgs()[2];
                    Console.WriteLine("Read IP Adress: " + ip);
                    OperatorIP = ip;
                }
                if (Environment.GetCommandLineArgs().Length > 3)
                {
                    string offset = Environment.GetCommandLineArgs()[3];
                    Console.WriteLine("Read Offset: " + offset);
                    try
                    {
                        PowerwallOffset = Convert.ToDouble(Environment.GetCommandLineArgs()[3]);
                    }catch{
                    }
                }
                return true;
            }
            else if (Environment.GetCommandLineArgs().Contains("operator"))
            {
                string l = Environment.GetCommandLineArgs()[1].ToLower().Trim();
                isOperator = l.Equals("operator");
                isPowerwall = !isOperator;
                
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool? isOperator = null;
        public static bool IsOperator
        {
            get
            {
                if (!isOperator.HasValue)
                {
                    ReadAttributesFromCommandLine();
                }
                if (isOperator.HasValue)
                    return isOperator.Value;
                else
                    return Client.Properties.Settings.Default.IsOperator;
            }
            set
            {
                isOperator = value;
            }
        }
        private static double? powerwallHeight = null;
        public static double PowerwallHeight
        {
            get
            {
                if (!powerwallHeight.HasValue)
                {
                    // TODO Parse attribute from CommandLine
                    powerwallHeight = Client.Properties.Settings.Default.PowerwallResY;
                }
                return powerwallHeight.Value;
            }
            set
            {
                
            }
        }

        private static double? powerwallOffset = null;
        public static double PowerwallOffset
        {
            get
            {
                if (!powerwallOffset.HasValue)
                {
                    ReadAttributesFromCommandLine();
                }
                if (!powerwallOffset.HasValue)
                {
                    powerwallOffset = Client.Properties.Settings.Default.PowerwallOffsetX;
                }
                return powerwallOffset.Value;
            }
            private set {
                powerwallOffset = value;
            }
        }


        private static string operatorIP = null;
        public static string OperatorIP
        {
            get
            {
                if (operatorIP == null)
                {
                    ReadAttributesFromCommandLine();
                }
                if (operatorIP == null)
                {
                    OperatorIP = Client.Properties.Settings.Default.OperatorIP;
                } 
                return 
                    operatorIP;
            }
            private set { operatorIP = value; }
        }


        private static double? powerwallWidth = null;
        public static double PowerwallWidth
        {
            get
            {
                if (!powerwallWidth.HasValue)
                {
                    // TODO Parse attribute from CommandLine
                    powerwallWidth = Client.Properties.Settings.Default.PowerwallResX;
                }
                return powerwallWidth.Value;
            }
            private set
            {
                
            }
        }
    }
}
