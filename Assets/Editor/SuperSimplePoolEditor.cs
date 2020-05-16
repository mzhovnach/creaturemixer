using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SuperSimplePool))]
public class SuperSimplePoolEditor : Editor
{
	public override void OnInspectorGUI()
	{
        DrawDefaultInspector();

        //if (GUILayout.Button("Scan Sprites"))
        //{
        //    ScanSprites();
        //}
        if (GUILayout.Button("Scan Prefabs"))
        {
            ScanPrefabs();
        }
    }

    private void ScanSprites()
    {
        //YubiCapScene myTarget = (YubiCapScene)target;
        //// Find all Texture2Ds that have 'co' in their filename, that are labelled with 'concrete' or 'architecture' and are placed in 'MyAwesomeProps' folder
        //var guids2 = AssetDatabase.FindAssets("t:texture2D",new string[] {"Assets/Art/Atlas"});
        //myTarget.Sprites = new Sprite[guids2.Length];
        //int index = 0;
        //foreach (var guid in guids2)
        //{
        //    myTarget.Sprites[index] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
        //    index++;
        //}
        //EditorUtility.SetDirty(myTarget);
    }

    private void ScanPrefabs()
    {
        SuperSimplePool myTarget = (SuperSimplePool)target;
        // find all GameObjects
        string path = myTarget.FolderToScan;
        if (path != ""){
            path = "/" + path;
        }
        var guids2 = AssetDatabase.FindAssets("t:gameobject", new string[] { "Assets/Prefabs" + path });
        myTarget.Prefabs = new List<GameObject>();
        int index = 0;
        foreach (var guid in guids2)
        {
            myTarget.Prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));
            index++;
        }
        EditorUtility.SetDirty(myTarget);
    }
}
