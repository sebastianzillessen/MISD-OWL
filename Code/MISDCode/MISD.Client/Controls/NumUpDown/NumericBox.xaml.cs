using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MISD.Client.Controls.NumUpDown
{
    [TemplatePart(Name = "PART_NumericTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(Button))]
    /// <summary>
    /// WPF User control - NumericBox
    /// </summary>
    public partial class NumericBox : UserControl
    {
        #region Variables

        private int value;           // value
        private int increment;       // increment
        private int minimum;         // minimum value
        private int maximum;         // maximum value

        private string valueFormat;     // string format of the value

        private DispatcherTimer timer;  // timer for Increaseing/Decreasing value with certain time interval

        #endregion

        public NumericBox()
        {
            InitializeComponent();

            // Set timer properties
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(100.0);
        }

        #region Properties

        #region Overrided properties
        /*
         * Override the properties for PART_NumericTextBox
         */
        new public Brush Foreground
        {
            get { return PART_NumericTextBox.Foreground; }
            set { PART_NumericTextBox.Foreground = value; }
        }

        #endregion

        #region Value format
        /*
         * --- Property name - ValueFormat ---
         * This property is necessary to show the value in a specific format
         */
        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(NumericBox), new PropertyMetadata("0.00", OnValueFormatChanged));

        private static void OnValueFormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericBox numericBoxControl = new NumericBox();
            numericBoxControl.valueFormat = (string)args.NewValue;
        }
        //===========================================================
        /// <summary>
        /// Set or get value format
        /// </summary>
        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value);}
        }

        #endregion

        #region Min\Max value property
        /*
         * --- Property name - Minimum ---
         * This property sets a minimum bound of the value
         */
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericBox), new PropertyMetadata(int.MinValue, OnMinimumChanged));

        private static void OnMinimumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericBox numericBoxControl = new NumericBox();
            numericBoxControl.minimum = (int)args.NewValue;
        }
        //===========================================================
        /// <summary>
        /// Set or get minimum
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        //---------------------------------------------------------------------------------------------------
        /*
         * --- Property name - Maximum ---
         * This property sets a maximum bound of the value
         */
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericBox), new PropertyMetadata(int.MaxValue, OnMaximumChanged));

        private static void OnMaximumChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericBox numericBoxControl = new NumericBox();
            numericBoxControl.maximum = (int)args.NewValue;
        }
        //===========================================================
        /// <summary>
        /// Set or get maximum
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        #endregion

        #region Increment property
        /*
         * --- Property name - Increment ---
         * This property defines an increment value
         */
        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(int), typeof(NumericBox), new PropertyMetadata((int)1, OnIncrementChanged));

        private static void OnIncrementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericBox numericBoxControl = new NumericBox();
            numericBoxControl.increment = (int)args.NewValue;
        }
        //===========================================================
        /// <summary>
        /// Set or get increment
        /// </summary>
        public int Increment
        {
            get { return (int)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        #endregion

        #region Value property
        /*
         * --- Property name - Value ---
         * This property defines a current value
         */
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericBox), new PropertyMetadata(new int(), OnValueChanged));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericBox numericBoxControl = (NumericBox)sender;
            numericBoxControl.value = (int)args.NewValue;
            numericBoxControl.PART_NumericTextBox.Text = numericBoxControl.value.ToString(numericBoxControl.ValueFormat);
            numericBoxControl.OnValueChanged((int)args.OldValue, (int)args.NewValue);
        }
        //===========================================================
        /// <summary>
        /// Set or get value
        /// </summary>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion

        #endregion

        #region Custom events

        #region ValueEvent
        /*
         * Registering new event - ValueChanged. It is invoked when the ValueProperty changes 
         */
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<int>), typeof(NumericBox));

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        private void OnValueChanged(int oldValue, int newValue)
        {
            Value = newValue;
            RoutedPropertyChangedEventArgs<int> args = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue);
            args.RoutedEvent = NumericBox.ValueChangedEvent;
            RaiseEvent(args);
        }
        #endregion

        #endregion

        #region Events

        #region Buttons Event

        private void increaseBtn_Click(object sender, RoutedEventArgs e)
        {
            IncreaseValue();
        }

        private void decreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            DecreaseValue();
        }

        #endregion

        #region Text input event
        private void numericBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int tempValue = int.Parse(PART_NumericTextBox.Text);
                if (!(tempValue < Minimum || tempValue > Maximum)) Value = tempValue;
            }
            catch (FormatException)
            {
            }

        }
        #endregion

        #region Mouse wheel event

        private void numericBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) IncreaseValue();
            else if (e.Delta < 0) DecreaseValue();
        }

        #endregion

        #region Change value when Increase\Decrease buttons are pressed

        private void Increase_Timer_Tick(object sender, EventArgs e)
        {
            IncreaseValue();
        }

        private void Deccrease_Timer_Tick(object sender, EventArgs e)
        {
            DecreaseValue();
        }

        private void increaseBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.timer.Tick += Increase_Timer_Tick;
            timer.Start();
        }

        private void increaseBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.timer.Tick -= Increase_Timer_Tick;
            timer.Stop();
        }

        private void decreaseBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.timer.Tick += Deccrease_Timer_Tick;
            timer.Start();
        }

        private void decreaseBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.timer.Tick -= Deccrease_Timer_Tick;
            timer.Stop();
        }

        #endregion

        #endregion

        #region Private Methods
        //=============================================================
        /// <summary>
        /// Set increment
        /// </summary>
        private void SetIncrement()
        {
            Increment = 1;
        }
        //=============================================================
        /// <summary>
        /// Increase value
        /// </summary>
        private void IncreaseValue()
        {
            Value += Increment;
            if (Value < Minimum || Value > Maximum) Value -= Increment;
        }
        //=============================================================
        /// <summary>
        /// Decrease value
        /// </summary>
        private void DecreaseValue()
        {
            Value -= Increment;
            if (Value < Minimum || Value > Maximum) Value += Increment;
        }
        #endregion

        #region Overrided Methods
        //=============================================================
        /// <summary>
        /// Apply new templates after setting new style
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button btn = GetTemplateChild("PART_IncreaseButton") as Button;
            if (btn != null)
            {
                btn.Click += increaseBtn_Click;
                btn.PreviewMouseLeftButtonDown += increaseBtn_PreviewMouseLeftButtonDown;
                btn.PreviewMouseLeftButtonUp += increaseBtn_PreviewMouseLeftButtonUp;
            }

            btn = GetTemplateChild("PART_DecreaseButton") as Button;
            if (btn != null)
            {
                btn.Click += decreaseBtn_Click;
                btn.PreviewMouseLeftButtonDown += decreaseBtn_PreviewMouseLeftButtonDown;
                btn.PreviewMouseLeftButtonUp += decreaseBtn_PreviewMouseLeftButtonUp;
            }

            TextBox tb = GetTemplateChild("PART_NumericTextBox") as TextBox;
            if (tb != null)
            {
                PART_NumericTextBox = tb;
                PART_NumericTextBox.Text = Value.ToString(ValueFormat);
                PART_NumericTextBox.PreviewTextInput += numericBox_TextInput;
                PART_NumericTextBox.MouseWheel += numericBox_MouseWheel;
            }

            btn = null;
            tb = null;

        }
        #endregion
    }
}
