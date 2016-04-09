using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MISD.RegExUtil;

namespace MISD.Client.Controls.RegExGenList
{
    public class RegExGenListItemData
    {
        private Operation regExOperation;
        private string regExPart;

        public Operation RegExOperation
        {
            get { return regExOperation; }
            set { regExOperation = value; }
        }

        public string RegExPart
        {
            get { return regExPart; }
            set { regExPart = value; }
        }
    }

    public class RegExGenListItem : ListBoxItem
    {
        public RegExGenListItem()
        {

        }

        #region Properties
        public RegExGenList RegExGenListBackLink { get; set; }

        public string OperationContent
        {
            /*get
            {
                if (Content is Tuple<Operation, string>)
                {
                    var part = Content as Tuple<Operation, string>;
                    return RegExOperationConvert(part.Item1);
                }
                else
                {
                    return RegExOperationConvert(RegExUtil.Operation.Contain);
                }
            }*/
            set
            {
                if (Content is Tuple<Operation, string>)
                {
                    var part = Content as Tuple<Operation, string>;
                    var newpart = new Tuple<Operation, string>(RegExOperationConvertBack(value), part.Item2);
                    Content = newpart;
                }
            }
        }

        public string RegExToGenerateContent
        {
            /*get
            {
                if (Content is Tuple<Operation, string>)
                {
                    var part = Content as Tuple<Operation, string>;
                    return part.Item2;
                }
                else
                {
                    return "";
                }
            }*/
            set
            {
                if (Content is Tuple<Operation, string>)
                {
                    var part = Content as Tuple<Operation, string>;
                    var newpart = new Tuple<Operation, string>(part.Item1, value);
                    Content = newpart;
                }
            }
        }

        public string Operation
        {
            get
            {
                return (string) this.GetValue(OperationProperty);
            }
            set
            {
                this.SetValue(OperationProperty, value);
            }
        }

        public string RegExToGenerate
        {
            get
            {
                return (string)this.GetValue(RegExToGenerateProperty);
            }
            set
            {
                this.SetValue(RegExToGenerateProperty, value);
            }
        }

        

        #endregion

        #region Dependecy Properties

        public static readonly DependencyProperty OperationProperty =
            DependencyProperty.Register("Operation", typeof(string), typeof(RegExGenListItem));

        public static readonly DependencyProperty RegExToGenerateProperty =
            DependencyProperty.Register("RegExToGenerate", typeof(string), typeof(RegExGenListItem));

        #endregion

        private void UpdateContent()
        {
            this.OperationContent = Operation;
            this.RegExToGenerateContent = RegExToGenerate;
        }
      
        #region event handlers
        private void RegExToGenerateAddPart_Click(object sender, RoutedEventArgs e)
        {
            RegExGenListBackLink.Items.Add("");
        }

        private void RegExToGenerateDeletePart_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region Converters

        private string RegExOperationConvert(RegExUtil.Operation value)
        {
            switch (value)
            {
                case RegExUtil.Operation.Contain:
                    return "&#8712;";
                case RegExUtil.Operation.Equal:
                    return "=";
                case RegExUtil.Operation.Less:
                    return "&lt;";
                case RegExUtil.Operation.Major:
                    return "&gt;";
                case RegExUtil.Operation.NotContain:
                    return "&#8713;";
                default:
                    return "&#8712;";
            }

        }

        private RegExUtil.Operation RegExOperationConvertBack(string value)
        {

            switch (value)
            {
                case "&#8712;":
                    return RegExUtil.Operation.Contain;
                case "=":
                    return RegExUtil.Operation.Equal;
                case "&lt;":
                    return RegExUtil.Operation.Less;
                case "&gt;":
                    return RegExUtil.Operation.Major;
                case "&#8713;":
                    return RegExUtil.Operation.NotContain;
                default:
                    return RegExUtil.Operation.Contain;
            }
        }

        #endregion
    }
}
