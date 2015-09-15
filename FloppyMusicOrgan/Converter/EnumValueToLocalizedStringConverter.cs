using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using FloppyMusicOrgan.Helper;

namespace FloppyMusicOrgan.Converter
{
    public class EnumValueToLocalizedStringConverter : IValueConverter
    {
        private bool _isInitialized;
        private Type _type;
        private IDictionary _displayValues;
        private IDictionary _reverseValues;

        public Type Type
        {
            get
            {
                return _type;
            }

            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("parameter is not an Enumermated type", "value");

                _type = value;
                _reverseValues = new Dictionary<string, Enum>();
            }
        }

        public ReadOnlyCollection<string> DisplayNames
        {
            get
            {
                CreateTranslationMappings();

                return new List<string>((IEnumerable<string>)_displayValues.Values).AsReadOnly();
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CreateTranslationMappings();

            return _displayValues[value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CreateTranslationMappings();

            return _reverseValues[value];
        }

        private void CreateTranslationMappings()
        {
            if (_isInitialized)
                return;

            _displayValues = EnumHelper.GetTranslatedEnumsAsDictionary(Type);

            foreach (var key in _displayValues.Keys)
            {
                _reverseValues.Add(_displayValues[key], key);
            }

            _isInitialized = true;
        }
    }
}
