using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create BodyPart", fileName = "BodyPart", order = 0)]
[Serializable]
public class BossPart : ScriptableObject
{
    public bool CanBeHit;
    public bool CanBeHitBehind;
    public bool Collides;
    public PartType PartType;
    public int Strength;
    public List<Vector2Int> AttackPattern;
    public float AttackCooldown;
    public float AttackDaze;
    public float AttackDelay;
    public int TargetDistance = 1;
    public int EffectIndex = -1;
    public float EffectOffset;
    public AudioClip AttackAudioClip;
    
    public List<Sprite> IdleSprites;
    public List<Sprite> AttackSprites;
    public List<Sprite> PaperdollSprites;
    public string PaperdollName;
    public int _frames = 1;

    public Sprite GetSprite(int direction, int animation)
    {
        int spriteOffset = 0;

        switch (direction)
        {
            case 0:
                spriteOffset = 0;
                break;
            case 1:
                spriteOffset = 1;
                break;
            case 2:
                spriteOffset = 2;
                break;
            case 3:
                spriteOffset = 1;
                break;
        }

        if (_frames > 1)
        {
            spriteOffset += animation * 3;
        }
        else if ((direction == 1 || direction == 3) && IdleSprites.Count > 3 && animation == 1) spriteOffset = 3;
        
        return IdleSprites[Mathf.Clamp(spriteOffset, 0, IdleSprites.Count)];
    }
}