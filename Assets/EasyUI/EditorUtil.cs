#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyUI
{
    public static class EditorUtil
    {
        public static IEnumerable<T> FindAssets<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t: {typeof(T).Name}");
            if (guids == null || guids.Length == 0)
            {
                yield break;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                yield return AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }
    }
}
#endif