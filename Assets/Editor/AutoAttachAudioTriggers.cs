#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class AutoAttachAudioTriggers : EditorWindow
{
    [MenuItem("Tools/Audio/Attach Audio Trigger to all buttons")]
    public static void AttachTriggersToAllButtons()
    {
        int modifiedCount = 0;

        Button[] sceneButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in sceneButtons)
        {
            if (!EditorUtility.IsPersistent(btn.transform.root.gameObject))
            {
                if (btn.GetComponent<UIAudioTrigger>() == null)
                {
                    btn.gameObject.AddComponent<UIAudioTrigger>();
                    EditorUtility.SetDirty(btn.gameObject);
                    modifiedCount++;
                }
            }
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                Button[] buttonsInPrefab = prefab.GetComponentsInChildren<Button>(true);
                bool prefabModified = false;

                foreach (Button btn in buttonsInPrefab)
                {
                    if (btn.GetComponent<UIAudioTrigger>() == null)
                    {
                        btn.gameObject.AddComponent<UIAudioTrigger>();
                        prefabModified = true;
                        modifiedCount++;
                    }
                }

                if (prefabModified)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }
    }
}
#endif