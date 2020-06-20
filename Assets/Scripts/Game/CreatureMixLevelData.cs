using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "cmlevel_", menuName = "Creature Mix Level", order = 0)]
public class CreatureMixLevelData : ScriptableObject 
{
	public int                   		Id;
    public ELevelType                   Type;
    public List<int>                    Colors; // possible colors of chips
    public List<Vector2Int>             Aims; // x - type, y - level of pipe
	[HideInInspector]
	public List<SSlotData>			    NeededStates;
    public bool                         AddNewPipes = true;
    [Range(0, 2)] public int            CreatureId;
    [Range(0, 50)] public int           Moves = 0;
    public int                          CollectAim = 0;

    // battle part
    [HideInInspector]
    public List<QueueElement>           Queue;
}