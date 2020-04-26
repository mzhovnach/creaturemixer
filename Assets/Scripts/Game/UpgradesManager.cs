using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UpgradesManager : MonoBehaviour
{
    [SerializeField] private ScriptableUpgradesData UpgradesData;
    [SerializeField] private List<Transform> Transforms;

    public void Reset()
    {
        SetLevel(0, true);
    }

    public void SetLevel(int levelNum, bool force)
    {
        UpgradeInfo upgrade = UpgradesData.GetUpgrade(levelNum);
        ApplyUpgrade(upgrade, force);
    }

    void ApplyUpgrade(UpgradeInfo upgrade, bool force)
    {
        if (string.IsNullOrEmpty(upgrade.Id)) return;

        if (force)
        {
            for (int i = 0; i <= upgrade.LevelNum; i++)
            {
                UpgradeInfo info = UpgradesData.GetUpgrade(i);
                if ( ! string.IsNullOrEmpty(info.Id))
                {
                    SetElementsState(info.ToDisable, false);
                    SetElementsState(info.ToEnable, true);
                }
            }
        }
        else
        {
            //TODO: Зробити анімовану появу елеменів
            // а поки що просто вкл/викл ноди як і в forced режимі
            for (int i = 0; i <= upgrade.LevelNum; i++)
            {
                UpgradeInfo info = UpgradesData.GetUpgrade(i);
                if (!string.IsNullOrEmpty(info.Id))
                {
                    SetElementsState(info.ToDisable, false);
                    SetElementsState(info.ToEnable, true);
                }
            }
        }
    }

    void SetElementsState(List<string> items, bool state)
    {
        foreach (var item in items)
        {
            EnableUpgradeElement(item, state);
        }
    }

    void EnableUpgradeElement(string elementName, bool state)
    {
        foreach (var item in Transforms)
        {
            if (elementName == item.name)
            {
                item.gameObject.SetActive(state);
            }
        }
    }

#if UNITY_EDITOR
    public void ReloadChildren()
    {
        UpgradeInfo upgrade = UpgradesData.GetUpgrade(0);
        Transforms.Clear();
        upgrade.ToEnable.Clear();
        upgrade.ToDisable.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                upgrade.ToEnable.Add(child.name);
            }
            else
            {
                upgrade.ToDisable.Add(child.name);
            }
            Transforms.Add(child);
        }
    }

    [CustomEditor(typeof(UpgradesManager))]
    public class UpgradesManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Reload Children Init State"))
            {
                UpgradesManager manager = (UpgradesManager)target;
                manager.ReloadChildren();
            }
        }
    }
#endif
}
