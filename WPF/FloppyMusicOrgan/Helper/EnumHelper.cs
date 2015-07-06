using System;
using System.Collections.Generic;
using System.Linq;
using FloppyMusicOrgan.Properties;

namespace FloppyMusicOrgan.Helper
{
    public static class EnumHelper
    {
        public static Dictionary<Enum, string> GetTranslatedEnumsAsDictionary(Type type, string format = null)
        {
            return Enum
                .GetValues(type)
                .OfType<Enum>()
                .ToDictionary(
                    enumValue => enumValue,
                    enumValue => enumValue.TranslateEnumValue(format));
        }

        private static string TranslateEnumValue(this Enum value, string format = null)
        {
            var result = string.Empty;

            try
            {
                var enumType = value.GetType();
                var enumTypeName = enumType.Name;
                var enumValue = value.ToString();
                result = enumTypeName + "." + enumValue;

                var searchText = string.Format("Enum_{0}_{1}", enumTypeName, enumValue);
                var foundText = Resources.ResourceManager.GetString(searchText);

                if (format == null)
                {
                    if (foundText != null)
                        result = foundText;
                }
                else
                {
                    result = string.Format(format, foundText ?? string.Empty, result, enumValue);
                }

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }
    }
}
