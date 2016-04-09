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
 * MISD-OWL is distributed without any warranty, without even the
 * implied warranty of merchantability or fitness for a particular purpose.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MISD.RegExUtil
{
    /// <summary>
    /// This class provides useful methods for the working with regular expressions.
    /// Especially it creates regular expressions for mathematical operations.
    /// </summary>
    public class RegExUtility
    {
        #region Properties

        /// <summary>
        /// Seperator for RegEx List.
        /// </summary>
        public static string Separator
        {
            get
            {
                return "<~>";
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor. No object should be created.
        /// </summary>
        private RegExUtility() {}

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the RegEx for a given operation and value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="operation"></param>
        /// <returns>RegEx, NULL if an error occured</returns>
        public static string GenerateRegEx(string value, Operation operation)
        {
            string resultString = null;
            string floatString;

            switch (operation)
            {
                case Operation.Major:
                    floatString = ConvertInFloatString(value);
                    if (floatString != null)
                    {
                        resultString = GenerateRegEx_Major(floatString);
                    }
                    break;
                case Operation.Less:
                    floatString = ConvertInFloatString(value);
                    if (floatString != null)
                    {
                        resultString = GenerateRegEx_Less(floatString);
                    }
                    break;
                case Operation.Equal:
                    floatString = ConvertInFloatString(value);
                    if (floatString != null)
                    {
                        resultString = GenerateRegEx_Equal(floatString);
                    }
                    break;
                case Operation.Contain:
                    if (value != null)
                    {
                        resultString = GenerateRegEx_Contain(value);
                    }
                    break;
                case Operation.NotContain:
                    if (value != null)
                    {
                        resultString = GenerateRegEx_NotContain(value);
                    }
                    break;
                default:
                    break;
            }

            return resultString;
        }

        /// <summary>
        /// Merges a list of RegEx strings with the seperator RegExGen.Separator.
        /// </summary>
        /// <param name="RegExList">List of strings</param>
        /// <returns>merged RegEx, NULL if error occured</returns>
        public static string MergeRegEx(List<string> regExList)
        {
            if (regExList != null)
            {
                if (regExList.Count > 0)
                {
                    if (regExList.Count == 1)
                    {
                        return regExList.ElementAt(0);
                    }
                    else
                    {
                        StringBuilder returnValue = new StringBuilder("");
                        foreach (string regEx in regExList)
                        {
                            if (returnValue.ToString() != "")
                            {
                                returnValue.Append(RegExUtility.Separator);
                            }
                            returnValue.Append(regEx);
                        }
                        return returnValue.ToString();
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Breaks a merged list of RegEx.
        /// For matching, please use the Match-method!
        /// </summary>
        /// <param name="mergedRegex">string</param>
        /// <returns>List, empty if error occured<string></returns>
        public static List<string> BreakMergedRegEx(string mergedRegex)
        {
            List<string> resultValue = new List<string>();
            if (mergedRegex != null)
            {
                string[] splittedRegex = mergedRegex.Split(new string[]{RegExUtility.Separator}, StringSplitOptions.None);

                foreach (string regEx in splittedRegex)
                {
                    resultValue.Add(regEx);
                }
            }
            return resultValue;
        }

        /// <summary>
        /// Matchs a value with a RegEx pattern. The RegEx pattern can be a merged RegEx by RegExUtil.
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="regEx">string</param>
        /// <returns>bool</returns>
        public static bool Match(string value, string regEx)
        {
            if (value != null && regEx != null)
            {
                bool regExMatched = true;
                foreach (string pattern in RegExUtility.BreakMergedRegEx(regEx))
                {
                    if (!Regex.Match(value, pattern).Success)
                    {
                        regExMatched = false;
                        break;
                    }
                }
                return regExMatched;
            }
            else
            {
                return false;
            }
        }

        #endregion
        
        #region Private Methods

        private static string GenerateRegEx_Major(string value)
        {
            StringBuilder returnValue = new StringBuilder("");

            int prePointValue = -1;
            string afterPointValue = "";
            int temp = 0;

            StringBuilder prePointRegEx = new StringBuilder("");
            StringBuilder afterPointRegEx = new StringBuilder("");

            // Split the given number in two pieces.
            string[] splittedValue = value.Split(new char[] {'.'});
            prePointValue = Convert.ToInt32(splittedValue[0]);
            afterPointValue = splittedValue[1];

            // Build major prePointRegEx
            int counter = 1;
            temp = prePointValue - (prePointValue / 10) * 10;
            prePointValue = prePointValue / 10;
            if (temp < 9)
            {
                if (prePointValue > 0)
                {
                    prePointRegEx.Append("(" + prePointValue + "[" + (1 + temp) + "-9]");
                }
                else
                {
                    prePointRegEx.Append("([" + (1 + temp) + "-9]");
                }
            }
            while (prePointValue.ToString().Length > 0 && prePointValue > 0)
            {
                temp = prePointValue - (prePointValue / 10) * 10;
                prePointValue = prePointValue / 10;

                if (temp < 9)
                {
                    if (prePointRegEx.Length > 0)
                    {
                        prePointRegEx.Append("|[" + (1 + temp) + "-9][0-9]{" + counter + "}");
                    }
                    else
                    {
                        prePointRegEx.Append("([" + (1 + temp) + "-9][0-9]{" + counter + "}");
                    }
                }

                counter++;
            }
            prePointRegEx.Append("|[1-9][0-9]{" + counter + ",})");

            // Build major afterPointRegEx
            char tempChar;
            int tempInt = 0;
            afterPointRegEx.Append("(" + afterPointValue + "[0]*[" + (1 + tempInt) + "-9][0-9]*");

            tempChar = afterPointValue.ElementAt(afterPointValue.Length - 1);
            afterPointValue = afterPointValue.Remove(afterPointValue.Length - 1);

            while (afterPointValue.ToString().Length > 0)
            {
                tempInt = Convert.ToInt32(tempChar.ToString());
                if (tempInt < 9)
                {
                    afterPointRegEx.Append("|" + afterPointValue + "[" + (1 + tempInt) + "-9][0-9]*");
                }

                tempChar = afterPointValue.ElementAt(afterPointValue.Length - 1);
                afterPointValue = afterPointValue.Remove(afterPointValue.Length - 1);
            }
            tempInt = Convert.ToInt32(tempChar.ToString());
            if (tempInt < 9)
            {
                afterPointRegEx.Append("|" + "[" + (1 + tempInt) + "-9][0-9]*");
            }
            afterPointRegEx.Append(")");

            // Restore PrePointValue
            prePointValue = Convert.ToInt32(splittedValue[0]);

            // Build final RegEx
            returnValue.Append("^(");

            returnValue.Append("(");
            returnValue.Append(prePointRegEx);
            returnValue.Append("(,|.)");
            returnValue.Append("[0-9]{1,}");
            returnValue.Append(")");

            returnValue.Append("|");

            returnValue.Append("(");
            returnValue.Append(prePointValue);
            returnValue.Append("(,|.)");
            returnValue.Append(afterPointRegEx);
            returnValue.Append(")");

            returnValue.Append("|");

            returnValue.Append("(");
            returnValue.Append(prePointRegEx);
            returnValue.Append(")");

            returnValue.Append(")$");

            return returnValue.ToString();
        }

        private static string GenerateRegEx_Less(string value)
        {
            StringBuilder returnValue = new StringBuilder("");

            int prePointValue = -1;
            string afterPointValue = "";
            int temp = 0;

            StringBuilder prePointRegEx = new StringBuilder("");
            StringBuilder afterPointRegEx = new StringBuilder("");

            // Split the given number in two pieces.
            string[] splittedValue = value.Split(new char[] { '.' });
            prePointValue = Convert.ToInt32(splittedValue[0]);
            afterPointValue = splittedValue[1];

            if (prePointValue == 0 && Convert.ToInt32(afterPointValue) == 0)
            {
                returnValue.Append("$^");
            }
            else
            {
                // Build less prePointRegEx
                if (prePointValue > 0)
                {
                    int counter = 1;
                    temp = prePointValue - (prePointValue / 10) * 10;
                    prePointValue = prePointValue / 10;

                    if (temp > 0)
                    {
                        if (prePointValue > 0)
                        {
                            prePointRegEx.Append("(" + prePointValue + "[0-" + (temp - 1) + "]");
                        }
                        else
                        {
                            prePointRegEx.Append("([0-" + (temp - 1) + "]");
                        }
                        while (prePointValue.ToString().Length > 0 && prePointValue > 0)
                        {
                            temp = prePointValue - (prePointValue / 10) * 10;
                            prePointValue = prePointValue / 10;

                            if (prePointValue > 0)
                            {
                                prePointRegEx.Append("|" + prePointValue + "[0-" + (temp - 1) + "][0-9]{0," + counter + "}");
                            }
                            else
                            {
                                prePointRegEx.Append("|[0-" + (temp - 1) + "][0-9]{0," + counter + "}");
                            }

                            counter++;
                        }
                        prePointRegEx.Append(")");
                    }
                }

                Console.WriteLine("AfterPointValue = " + afterPointValue);

                // Build less afterPointRegEx
                char tempChar;
                int tempInt;
                if (Convert.ToInt32(afterPointValue) > 0)
                {
                    afterPointRegEx.Append("(");

                    tempChar = afterPointValue.ElementAt(afterPointValue.Length - 1);
                    afterPointValue = afterPointValue.Remove(afterPointValue.Length - 1);

                    while (afterPointValue.Length > 0)
                    {
                        tempInt = Convert.ToInt32(tempChar.ToString());
                        if (tempInt > 0)
                        {
                            afterPointRegEx.Append(afterPointValue + "[0-" + (tempInt - 1) + "][0-9]*|" + afterPointValue + "|");
                        }

                        tempChar = afterPointValue.ElementAt(afterPointValue.Length - 1);
                        afterPointValue = afterPointValue.Remove(afterPointValue.Length - 1);
                    }
                    tempInt = Convert.ToInt32(tempChar.ToString());
                    if (tempInt > 0)
                    {
                        afterPointRegEx.Append("[0-" + (tempInt - 1) + "][0-9]*");
                    }
                    else
                    {
                        if (afterPointRegEx.ToString().ElementAt(afterPointRegEx.Length - 1) == '|')
                        {
                            afterPointRegEx.Remove(afterPointRegEx.Length - 1, 1);
                        }
                    }

                    afterPointRegEx.Append(")");
                }

                // Restore PrePointValue
                prePointValue = Convert.ToInt32(splittedValue[0]);

                // Build final RegEx
                returnValue.Append("^(");

                if (prePointRegEx.ToString() != "")
                {
                    returnValue.Append("(");
                    returnValue.Append(prePointRegEx);
                    returnValue.Append("(,|.)");
                    returnValue.Append("[0-9]{1,}");
                    returnValue.Append(")");

                    returnValue.Append("|");

                    returnValue.Append("(");
                    returnValue.Append(prePointRegEx);
                    returnValue.Append(")");
                }

                if (afterPointRegEx.ToString() != "")
                {
                    if (prePointRegEx.ToString() != "")
                    {
                        returnValue.Append("|");
                    }
                    else
                    {
                        returnValue.Append("0|");
                    }

                    returnValue.Append("(");
                    returnValue.Append(prePointValue);
                    returnValue.Append("(,|.)");
                    returnValue.Append(afterPointRegEx);
                    returnValue.Append(")");
                }

                returnValue.Append(")$");
            }

            return returnValue.ToString();
        }

        private static string GenerateRegEx_Equal(string value)
        {
            StringBuilder returnValue = new StringBuilder("");

            int prePointValue = -1;
            string afterPointValue = "";

            // Split the given number in two pieces.
            string[] splittedValue = value.Split(new char[] { '.' });
            prePointValue = Convert.ToInt32(splittedValue[0]);
            afterPointValue = splittedValue[1];

            returnValue.Append("^(");

            returnValue.Append("(");
            returnValue.Append(prePointValue);
            returnValue.Append("(,|.)");
            returnValue.Append(afterPointValue);
            returnValue.Append(")");

            if (afterPointValue.ToString() == "0")
            {
                returnValue.Append("|(");
                returnValue.Append(prePointValue);
                returnValue.Append(")");
            }

            returnValue.Append(")$");

            return returnValue.ToString();
        }

        private static string GenerateRegEx_Contain(string value)
        {
            return "^(.*" + value + ").*$";
        }

        private static string GenerateRegEx_NotContain(string value)
        {
            return "^(?!.*" + value + ").*$";
        }

        /// <summary>
        /// Converts the given string in a double.
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>double or null</returns>
        private static string ConvertInFloatString(string value)
        {
            try
            {
                NumberFormatInfo provider = new NumberFormatInfo( );
                provider.NumberDecimalSeparator = ".";
                string returnValue = Convert.ToDouble(value, provider).ToString(provider);
                if (!returnValue.Contains('.'))
                {
                    returnValue += ".0";
                }
                return returnValue;
            }
            catch (FormatException e)
            {
                return null;
            }
            catch (OverflowException e)
            {
                return null;
            }
        }

        #endregion
    }
}
