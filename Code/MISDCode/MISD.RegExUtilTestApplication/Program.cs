/*
 * Copyright 2012 Paul Brombosch, Ehssan Doust, David Krauss,
 * Fabian Müller, Yannic Noller, Hanna Schäfer, Jonas Scheurich,
 * Arno Schneider, Sebastian Zillessen
 *
 * This file is part of MISD-OWL, a project of the
 * University of Stuttgart (Institution VISUS, Studienprojekt Spring 2012).
 *
 * MISD-OWL is published under GNU Lesser General Public License Version 3.
 * MISD-OWL is free software, you are allowed to redistribute and/or
 * modify it under the terms of the GNU Lesser General Public License
 * Version 3 or any later version. For details see here:
 * http://www.gnu.org/licenses/lgpl.html
 *
 * MISD-OWL is distributed without any warranty, witmlhout even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MISD.RegExUtil;

namespace MISD.RegExUtilTestApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("++++++++++++++++++++++++++++++++");
                Console.WriteLine("+++ RegExUtilTestApplication +++");
                Console.WriteLine("++++++++++++++++++++++++++++++++");
                Console.WriteLine();
                Console.WriteLine("(1) >");
                Console.WriteLine("(2) <");
                Console.WriteLine("(3) =");
                Console.WriteLine("(4) contain");
                Console.WriteLine("(5) not_contain");
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Choose one Operation: ");
                var key = Console.ReadKey();

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Enter value: ");
                var value = Console.ReadLine();

                string result = "";
                switch (((ConsoleKeyInfo)key).KeyChar)
                {
                    case '1':
                        result = RegExUtility.GenerateRegEx(value, Operation.Major);
                        break;
                    case '2':
                        result = RegExUtility.GenerateRegEx(value, Operation.Less);
                        break;
                    case '3':
                        result = RegExUtility.GenerateRegEx(value, Operation.Equal);
                        break;
                    case '4':
                        result = RegExUtility.GenerateRegEx(value, Operation.Contain);
                        break;
                    case '5':
                        result = RegExUtility.GenerateRegEx(value, Operation.NotContain);
                        break;
                    default:
                        break;
                }

                if (result != "")
                {
                    Console.WriteLine();
                    Console.WriteLine("Generated RegEx: ");
                    Console.WriteLine(result);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("++++++++++++++++++++++++++++++++");
                    Console.WriteLine("+++      Test the RegEx      +++");
                    Console.WriteLine("++++++++++++++++++++++++++++++++");

                    while (true)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Enter value: ");
                        var testValue = Console.ReadLine();
                        bool matched = Regex.Match(testValue, result).Success;

                        Console.WriteLine("Matched = " + matched);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Operation not implemented...");
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(">>> Program unexcepted closed");
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
    }
}
