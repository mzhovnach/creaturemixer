using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct UpgradeInfo
{
	public string Id;
	public int LevelNum;
	public List<string> ToEnable;
	public List<string> ToDisable;
}

public class ScriptableUpgradesData : ScriptableObject
{
	public List<UpgradeInfo> Upgrades;

    public UpgradeInfo GetUpgrade(int levelId)
    {
		return Upgrades.Find(u => u.LevelNum == levelId);
    }
}
