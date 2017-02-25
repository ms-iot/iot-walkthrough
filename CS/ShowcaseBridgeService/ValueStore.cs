using System;
using Windows.Foundation.Collections;

namespace ShowcaseBridgeService
{
    class ValueChangedEventArgs : EventArgs
    {
        private ValueSet _changedValues;

        public ValueChangedEventArgs(ValueSet changedValues)
        {
            _changedValues = changedValues;
        }

        public ValueSet ChangedValues { get { return _changedValues; } }
    }

    class ValueStore
    {
        private ValueSet _configs = new ValueSet();

        public void SetSettings(ValueSet settings)
        {
            foreach (var element in settings)
            {
                _configs[element.Key] = element.Value;
            }
            ValueChangedEventArgs args = new ValueChangedEventArgs(settings);
            ValueChanged?.Invoke(this, args);
        }

        public object GetSetting(String key)
        {
            object value;
            _configs.TryGetValue(key, out value);
            return value;
        }

        public EventHandler ValueChanged;
    }
}
