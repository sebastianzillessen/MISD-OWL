using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MISD.Client.Controls.NumUpDown
{
    /// <summary>
    /// Interaktionslogik für TimeControl.xaml
    /// </summary>
    public partial class TimeControl : UserControl
    {
        public TimeControl()
        {
            InitializeComponent();
        }

        #region Fields
        private Boolean txtHoursIsFocused = false;
        private Boolean txtMinutesIsFocused = false;
        private Boolean txtSecondIsFocused = false;
        #endregion


        #region Value property

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeControl), new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            TimeControl timebox = (TimeControl)d;
            timebox.OnValueChanged((TimeSpan)args.OldValue, (TimeSpan)args.NewValue);
        }


       
        public TimeSpan Value
        {
            get { return (TimeSpan)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void OnValueChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            if (newValue.Days > 0)
            {
                int hour = newValue.Days * 24 + newValue.Hours;
                this.txtHours.Text = Convert.ToString(hour);
            }
            else
            {
                this.txtHours.Text = Convert.ToString(newValue.Hours);
            }
            this.txtMinutes.Text = Convert.ToString(newValue.Minutes);
            this.txtSecond.Text = Convert.ToString(newValue.Seconds);
            Value= (TimeSpan)newValue;
        }

        #endregion


        #region Event Subscriptions

        /// <summary>
        /// Handles the Click event of the btnDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {

            string controlId = this.GetControlWithFocus().Name;
            if ("txtHours".Equals(controlId))
            {
                this.ChangeHours(false);
            }
            else if ("txtMinutes".Equals(controlId))
            {

                this.ChangeMinutes(false);
            }
            else if ("txtSecond".Equals(controlId))
            {
                this.ChangeSecond(false);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            string controlId = this.GetControlWithFocus().Name;
            if ("txtHours".Equals(controlId))
            {
                this.ChangeHours(true);
            }
            else if ("txtMinutes".Equals(controlId))
            {
                this.ChangeMinutes(true);
            }
            else if ("txtSecond".Equals(controlId))
            {
                this.ChangeSecond(true);
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void txt_KeyUp(object sender, KeyEventArgs e)
        {
            // check for up and down keyboard presses
            if (Key.Up.Equals(e.Key))
            {
                btnUp_Click(this, null);
            }
            else if (Key.Down.Equals(e.Key))
            {
                btnDown_Click(this, null);
            }
        }

        /// <summary>
        /// Handles the MouseWheel event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> instance containing the event data.</param>
        private void txt_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                btnUp_Click(this, null);
            }
            else
            {
                btnDown_Click(this, null);
            }
        }

        /// <summary>
        /// Handles the PreviewKeyUp event of the txt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void txt_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            // make sure all characters are number
            bool allNumbers = textBox.Text.All(Char.IsNumber);
            if (!allNumbers)
            {
                e.Handled = true;
                return;
            }

            // make sure user did not enter values out of range
            int value;
            int.TryParse(textBox.Text, out value);
            if ("txtSecond".Equals(textBox.Name) && value > 59)
            {
                EnforceLimits(e, textBox);
            }
            else if ("txtMinutes".Equals(textBox.Name) && value > 59)
            {
                EnforceLimits(e, textBox);
            }
            TextBox textBoxh = (TextBox)this.txtHours;
            int hours;
            int.TryParse(this.txtHours.Text, out hours);
            TextBox textBoxm = (TextBox)this.txtMinutes;
            int minutes;
            int.TryParse(this.txtHours.Text, out minutes);
            TextBox textBoxs = (TextBox)this.txtSecond;
            int seconds;
            int.TryParse(textBoxs.Text, out seconds);
            var value2 = new TimeSpan(hours, minutes, seconds);
            Value = value2;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the hours.
        /// </summary>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void ChangeHours(bool isUp)
        {
            int value = Convert.ToInt32(this.txtHours.Text);
            if (isUp)
            {
                value += 1;
            }
            else
            {
                if (value > 0)
                {
                    value -= 1;
                }
            }
            this.txtHours.Text = Convert.ToString(value);
            var newValue = new TimeSpan(value,Value.Minutes,Value.Seconds);
            Value = newValue;

        }

        /// <summary>
        /// Changes the minutes.
        /// </summary>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void ChangeMinutes(bool isUp)
        {
            int value = Convert.ToInt32(this.txtMinutes.Text);
            if (isUp)
            {
                value += 1;
                if (value == 60)
                {
                    value = 0;
                }
            }
            else
            {
                value -= 1;
                if (value == -1)
                {
                    value = 59;
                }
            }

            string textValue = Convert.ToString(value);
            if (value < 10)
            {
                textValue = "0" + Convert.ToString(value);
            }
            this.txtMinutes.Text = textValue;
            var newValue = new TimeSpan(Value.Hours, value, Value.Seconds);
            Value = newValue;

        }

        /// <summary>
        /// Changes the Seconds.
        /// </summary>
        /// <param name="isUp">if set to <c>true</c> [is up].</param>
        private void ChangeSecond(bool isUp)
        {
            int value = Convert.ToInt32(this.txtSecond.Text);
            if (isUp)
            {
                value += 1;
                if (value == 60)
                {
                    value = 0;
                }
            }
            else
            {
                value -= 1;
                if (value == -1)
                {
                    value = 59;
                }
            }

            string textValue = Convert.ToString(value);
            if (value < 10)
            {
                textValue = "0" + Convert.ToString(value);
            }
            this.txtSecond.Text = textValue;
            var newValue = new TimeSpan(Value.Hours, Value.Minutes, value);
            Value = newValue;

        }

        /// <summary>
        /// Enforces the limits.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        /// <param name="textBox">The text box.</param>
        /// <param name="enteredValue">The entered value.</param>
        private static void EnforceLimits(KeyEventArgs e, TextBox textBox)
        {
            string enteredValue = GetEnteredValue(e.Key);
            string text = textBox.Text.Replace(enteredValue, "");
            if (string.IsNullOrEmpty(text))
            {
                text = enteredValue;
            }
            textBox.Text = text;
            e.Handled = true;
        }

        /// <summary>
        /// Gets the control with focus.
        /// </summary>
        /// <returns></returns>
        private TextBox GetControlWithFocus()
        {
            TextBox txt = new TextBox();
            if (this.txtHoursIsFocused)
            {
                txt = this.txtHours;
            }
            else if (this.txtMinutesIsFocused)
            {

                txt = this.txtMinutes;
            }
            else
            {

                txt = this.txtSecond;
            }
            return txt;
        }

        /// <summary>
        /// Gets the entered value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetEnteredValue(Key key)
        {
            string value = string.Empty;
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0:
                    value = "0";
                    break;
                case Key.D1:
                case Key.NumPad1:
                    value = "1";
                    break;
                case Key.D2:
                case Key.NumPad2:
                    value = "2";
                    break;
                case Key.D3:
                case Key.NumPad3:
                    value = "3";
                    break;
                case Key.D4:
                case Key.NumPad4:
                    value = "4";
                    break;
                case Key.D5:
                case Key.NumPad5:
                    value = "5";
                    break;
                case Key.D6:
                case Key.NumPad6:
                    value = "6";
                    break;
                case Key.D7:
                case Key.NumPad7:
                    value = "7";
                    break;
                case Key.D8:
                case Key.NumPad8:
                    value = "8";
                    break;
                case Key.D9:
                case Key.NumPad9:
                    value = "9";
                    break;
            }
            return value;
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Name == "txtHours")
            {
                this.txtHoursIsFocused = true;
                this.txtSecondIsFocused = false;
                this.txtMinutesIsFocused = false;
            }
            else if (((TextBox)sender).Name == "txtMinutes")
            {
                this.txtHoursIsFocused = false;
                this.txtSecondIsFocused = false;
                this.txtMinutesIsFocused = true;
            }
            else
            {
                this.txtHoursIsFocused = false;
                this.txtSecondIsFocused = true;
                this.txtMinutesIsFocused = false;
            }
        }

        #endregion
      
    }
}

