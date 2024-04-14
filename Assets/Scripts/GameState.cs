using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PartType {
    Body,
    Head,
    Arm
}

public class GameState
{
    public bool Paused = true;
    
    public Vector2Int PlayerPosition = new Vector2Int(-1, -4);
    public Vector2Int PlayerNextPosition;
    public Vector2Int BossPosition = new Vector2Int(-1, -1);
    public Vector2Int PlayerDirection;
    public Vector2Int BossDirection;
    public Vector2Int BossNextPosition;

    public int PlayerLives = 3;
    public int BossLives = 5;
    public int RequiredStrength = 3;
    public int LevelIndex;
    public bool BossOutOfSpace = false;
    public bool PlayerDidAttack;
    public bool BossDidAttack;
    public List<Vector2Int> BossAttackTiles;
    public BossPart BossAttackPart;
    public float BossDaze = 0f;
    public bool SeenStuck;
    public int SeenLevelSequence = - 1;

    public List<Vector2Int> PlayerMoveQueue;
    public List<BossPart> UnlockedParts = new List<BossPart>();

    public bool LockedBody;
    public bool LockedArm;
    public bool LockedHead;

    public BossPart SelectedBody;
    public BossPart SelectedArm;
    public BossPart SelectedHead;

    public bool[,] ObstacleMap;

    public Tilemap BaseTilemap = new Tilemap();
    public Tilemap WarningTilemap = new Tilemap();
    public List<TileBase> ObstacleTiles = new List<TileBase>();

    public bool IsObstacle(Vector2Int position)
    {
        if (ObstacleTiles.Contains(BaseTilemap.GetTile(new Vector3Int(position.x, position.y, 0))))
        {
            return true;
        }

        return false;
    }

    public bool IsBoss(Vector2Int position)
    {
        if (position == BossPosition) return true;
        if (SelectedArm != null && SelectedArm.Collides &&
            position == BossPosition + LeftArmOffset(BossDirection)) return true;
        if (SelectedArm != null && SelectedArm.Collides &&
            position == BossPosition + RightArmOffset(BossDirection)) return true;

        return false;
    }

    Vector2Int LeftArmOffset(Vector2Int direction)
    {
        return new Vector2Int(direction.y, -direction.x);
    }

    Vector2Int RightArmOffset(Vector2Int direction)
    {
        return new Vector2Int(-direction.y, direction.x);
    }

    public void SetTileState(List<Vector2Int> pattern, TileBase tile, int layer)
    {
        for (int i = 0; i < pattern.Count; i++)
        {
            WarningTilemap.SetTile(new Vector3Int(pattern[i].x, pattern[i].y, layer), tile);
        }
    }

    public void ClearTileStates(int layer)
    {
        WarningTilemap.ClearAllTiles();
        //WarningTilemap.BoxFill(new Vector3Int(0,0,layer),null,-20,-20,40, 50);
    }
}
