﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MSEGui
{
    [TypeConverter(typeof(MultiKeyGestureConverter))]
    public class MultiKeyGesture : KeyGesture
    {
        private readonly IList<Key> _keys;
        private readonly ReadOnlyCollection<Key> _readOnlyKeys;
        private int _currentKeyIndex;
        private DateTime _lastKeyPress;
        private static readonly TimeSpan _maximumDelayBetweenKeyPresses = TimeSpan.FromSeconds(1);

        public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers)
            : this(keys, modifiers, string.Empty)
        {
        }

        public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers, string displayString)
            : base(Key.None, modifiers, displayString)
        {
            _keys = new List<Key>(keys);
            _readOnlyKeys = new ReadOnlyCollection<Key>(_keys);

            if (_keys.Count == 0)
            {
                throw new ArgumentException("At least one key must be specified.", "keys");
            }
        }

        public ICollection<Key> Keys
        {
            get { return _readOnlyKeys; }
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var args = inputEventArgs as KeyEventArgs;

            if ((args == null) || !IsDefinedKey(args.Key))
            {
                return false;
            }

            if (_currentKeyIndex != 0 && ((DateTime.Now - _lastKeyPress) > _maximumDelayBetweenKeyPresses))
            {
                //took too long to press next key so reset
                _currentKeyIndex = 0;
                return false;
            }

            //the modifier only needs to be held down for the first keystroke, but you could also require that the modifier be held down for every keystroke
            if (_currentKeyIndex == 0 && Modifiers != Keyboard.Modifiers)
            {
                //wrong modifiers
                _currentKeyIndex = 0;
                return false;
            }

            if (_keys[_currentKeyIndex] != args.Key)
            {
                //wrong key
                _currentKeyIndex = 0;
                return false;
            }

            ++_currentKeyIndex;

            if (_currentKeyIndex != _keys.Count)
            {
                //still matching
                _lastKeyPress = DateTime.Now;
                inputEventArgs.Handled = true;
                return false;
            }

            //match complete
            _currentKeyIndex = 0;
            return true;
        }

        private static bool IsDefinedKey(Key key)
        {
            return ((key >= Key.None) && (key <= Key.OemClear));
        }
    }
}
