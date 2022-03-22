using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Pools))]
public class PoolsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Scan Prefabs"))
        {
            ScanPrefabs();
        }

        if (GUILayout.Button("Scan Sprites"))
        {
            ScanSprites();
        }
    }

    private void ScanPrefabs()
    {
        Pools myTarget = (Pools)target;
        // find all GameObjects
        myTarget.Prefabs = new List<GameObject>();
        for (int i = 0; i < myTarget.PrefabsFolder.Count; ++i)
        {
            string path = myTarget.PrefabsFolder[i];
            if (path != "")
            {
                path = "/" + path;
            }
            var guids2 = AssetDatabase.FindAssets("t:gameobject", new string[] { "Assets/Prefabs" + path });

            int index = 0;
            foreach (var guid in guids2)
            {
                myTarget.Prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
                index++;
            }
        }
        EditorUtility.SetDirty(myTarget);
    }

    private void ScanSprites()
    {
        Pools myTarget = (Pools)target;
        // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'concrete' or 'architecture' and are placed in 'MyAwesomeProps' folder
        myTarget.Sprites = new List<Sprite>();
        for (int i = 0; i < myTarget.SpritesFolder.Count; ++i)
        {
            string path = myTarget.SpritesFolder[i];
            if (path != "")
            {
                path = "/" + path;
            }
            var guids2 = AssetDatabase.FindAssets("t:texture2D", new string[] { "Assets/Art/" + path });

            int index = 0;
            foreach (var guid in guids2)
            {
                myTarget.Sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
                index++;
            }
        }
        EditorUtility.SetDirty(myTarget);
    }
}
