using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create LevelConfig", fileName = "LevelConfig", order = 0)]
public class LevelConfig : ScriptableObject
{
    public List<BossPart> UnlockedParts;
    public int RequiredStrength;
    public BossPart LockedBody;
    public BossPart LockedArm;
    public BossPart LockedHead;
    public Sequence FirstTimeSequence;
    public Sequence LoseSequence;
    public Vector2Int BossSpawnPoint;
    public Vector2Int PlayerSpawnPoint;
    public int TilemapIndex;
}