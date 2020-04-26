using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "cmlevel_", menuName = "Creature Mix Level", order = 0)]
public class CreatureMixLevelData : ScriptableObject 
{
	public int                   		Id;
    public List<int>                    Colors; // possible colors of chips
    public List<Vector2Int>             Aims; // x - type, y - level of pipe
	[HideInInspector]
	public List<SSlotData>			    NeededStates;
    public bool                         AddNewPipes = false;
}