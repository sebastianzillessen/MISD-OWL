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
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MISD.Server.Email.WarningMailParser
{
    class MailParser
    {
        private Hashtable templateTags = new Hashtable();
        private string matchPattern = @"(\[%\w+%\])";

        #region Tag Methods

        public void AddTag(TemplateTag templateTag)
        {
            templateTags[templateTag.Tag] = templateTag;
        }

        public void AddTag(string Tag, string Value)
        {
            AddTag(new TemplateTag(Tag, Value));
        }

        public void RemoveTag(string Tag)
        {
            templateTags.Remove(Tag);
        }

        public void ClearTags()
        {
            templateTags.Clear();
        }

        #endregion

        #region Parser Methods

        private string _replaceTagHandler(Match token)
        {
            if (templateTags.Contains(token.Value))
                return ((TemplateTag)templateTags[token.Value]).Value;
            else
                return string.Empty;
        }

        public string ParseTemplateString(string Template)
        {
            MatchEvaluator replaceCallback = new MatchEvaluator(_replaceTagHandler);
            string newString = Regex.Replace(Template, matchPattern, replaceCallback);

            return newString;
        }

        #endregion
    }
}
