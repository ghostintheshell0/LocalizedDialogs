using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace LocalizedDialogs.Editor
{
    public static class LocalizedStringExtensions
    {
        public static string GetLocalizedEditorString(this LocalizedString localizedString)
        {
            if(localizedString.IsEmpty) return default;

            var collection = LocalizationEditorSettings.GetStringTableCollection(localizedString.TableReference);
            var locales = LocalizationEditorSettings.GetLocales();
            if (locales.Count > 0 )
            {
                StringTable table = (StringTable)collection.GetTable(locales[0].Identifier);
                if (table != null)
                    return table.GetEntryFromReference(localizedString.TableEntryReference).LocalizedValue;
            }

            return default;
        }

        public static string GetKey(this LocalizedString localizedString)
        {
            if(localizedString.IsEmpty) return string.Empty;
            var collection = LocalizationEditorSettings.GetStringTableCollection(localizedString.TableReference);
            return collection.SharedData.GetEntryFromReference(localizedString.TableEntryReference).Key;
        }
    }
}