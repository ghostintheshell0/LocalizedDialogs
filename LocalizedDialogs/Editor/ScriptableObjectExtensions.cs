using UnityEditor;
using UnityEngine;

namespace LocalizedDialogs.Editor
{
    public static class ScriptableObjectExtensions
    {
        public static T LoadSO<T>() where T : ScriptableObject
        {
            if(TryLoadSO(out T result))
            {
                return result;
            }

            if(result == default)
            {
                var allGUIDs = AssetDatabase.FindAssets($"t:{nameof(T)}");
                for(var i = 0; i < allGUIDs.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(allGUIDs[i]);
                    result = AssetDatabase.LoadAssetAtPath<T>(path);
                    if(result != default)
                    {
                        return result;
                    }
                }
            }

            return default;
        }

        public static bool TryLoadSO<T>(out T result)
        {
            var selectedAssets = Selection.GetFiltered<T>(SelectionMode.Unfiltered);

            if(selectedAssets.Length > 0)
            {
                result = (T)selectedAssets[0];
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryLoadSO<T>(string guid, out T so) where T : Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            so = AssetDatabase.LoadAssetAtPath<T>(path);
            return so != default;
        }
    }
}