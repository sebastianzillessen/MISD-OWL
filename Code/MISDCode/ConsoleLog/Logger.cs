using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog
{
    public class Logger
    {
        public static void WriteLine(String s)
        {
            try
            {
                MainWindow.Instance.WriteLine(s);
            }
            catch { 
                Console.WriteLine(s);
            }
        }


    }
}
