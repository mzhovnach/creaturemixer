using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Enemies))]
public class EnemiesEditor : Editor
{
	public override void OnInspectorGUI()
	{
        DrawDefaultInspector();

        if (GUILayout.Button("Scan Enemies"))
        {
            ScanEnemies();
        }
    }

    private void ScanEnemies()
    {
        Enemies myTarget = (Enemies)target;
        // find all Enemies
        string path = "Assets/Prefabs/Enemies";
        var guids2 = AssetDatabase.FindAssets("t:gameobject", new string[] { path });
        myTarget.EnemiesDefaultParams = new List<EnemyParams>();
        foreach (var guid in guids2)
        {
            GameObject enemyObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            string enemyName = enemyObject.name;
            //enemyObject = CreateObjectTemporarly(enemyObject);
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            EnemyParams enemyParams = new EnemyParams();
            enemyParams.Init(enemyName, enemy);
            myTarget.EnemiesDefaultParams.Add(enemyParams);
        }
        EditorUtility.SetDirty(myTarget);
    }

    //private GameObject CreateObjectTemporarly(GameObject prefab)
    //{
    //    return ((GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity));
    //}

    //private GameObject CreateObjectTemporarly(string objName)
    //{
    //    Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Objects/" + objName + ".prefab", typeof(GameObject));
    //    return ((GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity));
    //}

    //GameObject.DestroyImmediate(enemyObject);
}
