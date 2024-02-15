using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MSEGui
{
    public class MultiKeyBinding : InputBinding
    {
        [TypeConverter(typeof(MultiKeyGestureConverter))]
        public override InputGesture Gesture
        {
            get { return base.Gesture as MultiKeyGesture; }
            set
            {
                if (!(value is MultiKeyGesture))
                {
                    throw new ArgumentException();
                }

                base.Gesture = value;
            }
        }
    }
}
