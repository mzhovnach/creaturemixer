using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "cmlevel_", menuName = "Creature Mix Level", order = 0)]
public class CreatureMixLevelData : ScriptableObject 
{
	public int                   		Id;
    public List<Vector2Int>             Aims; // x - type, y - level of pipe
	[HideInInspector]
	public List<SSlotData>			    NeededStates;
}