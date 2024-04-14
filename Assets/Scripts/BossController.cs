using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BossController : MonoBehaviour
{
    [SerializeField] private float _moveCycleTime;
    [SerializeField] private int _targetDistance = 1;
    [SerializeField] private TileBase _warningTile;
    private GameState _gameState;
    private bool _lastMoveHorizontal;
    private float _attackCooldownTimer;
    private float _moveTimer;
    
    void Awake()
    {
        _gameState = FindObjectOfType<GameController>().GameState;
    }

    private IEnumerator Behavour()
    {
        while (true)
        {
            if (_gameState.Paused)
            {
                yield return null;
                continue;
            }
            
            _attackCooldownTimer -= Time.deltaTime;
            _moveTimer -= Time.deltaTime;

            bool prioritizeEvade = false;
            Vector2Int diff = _gameState.PlayerPosition - _gameState.BossPosition;

            var newPosition = GetMovement(out bool moveHorizontal);
            
            int targetDistanceOffset = Mathf.Abs(diff.x) + Mathf.Abs(diff.y) -
                                       (_gameState.SelectedBody != null ? _gameState.SelectedBody.TargetDistance : 1);
            if (targetDistanceOffset < 0 && newPosition != _gameState.BossPosition) prioritizeEvade = true;
            
            if (_attackCooldownTimer <= 0 && !prioritizeEvade)
            {
                if (TryAttackPattern(_gameState.SelectedArm))
                {
                    yield return PerformAttack(_gameState.SelectedArm);
                    
                } else if (TryAttackPattern(_gameState.SelectedBody))
                {
                    yield return PerformAttack(_gameState.SelectedBody);
                } else if (TryAttackPattern(_gameState.SelectedHead))
                {
                    yield return PerformAttack(_gameState.SelectedHead);
                }
            }

            if (_moveTimer <= 0)
            {
                if (!RotateTowardsPlayer())
                {
                    if (newPosition != _gameState.BossPosition)
                    {
                        _gameState.BossPosition = newPosition;
                    }
                    _lastMoveHorizontal = moveHorizontal;
                }
                _moveTimer = _moveCycleTime;
            }
            
            yield return null;
        }
    }

    private IEnumerator PerformAttack(BossPart bossPart)
    {
        _gameState.SetTileState(_gameState.BossAttackTiles, _warningTile, 2);
        yield return new WaitForSeconds(bossPart.AttackDelay);
        _gameState.BossDidAttack = true;
        _gameState.ClearTileStates(2);

        if (_gameState.Paused) yield break;
        
        foreach (var pos in _gameState.BossAttackTiles)
        {
            if (pos == _gameState.PlayerPosition)
            {
                _gameState.PlayerLives -= 1;
            }
        }
        
        yield return GetComponent<CharacterView>().DazedEffect(bossPart.AttackDaze);
        _attackCooldownTimer = _gameState.SelectedArm.AttackCooldown;
        _moveTimer = _moveCycleTime;
    }

    private void OnEnable()
    {
        _attackCooldownTimer = 2f;
        _moveTimer = _moveCycleTime;
        StartCoroutine(Behavour());
    }

    public Vector2Int GetMovement(out bool moveHorizontal)
    {
        Vector2Int diff = _gameState.PlayerPosition - _gameState.BossPosition;

        moveHorizontal = false;
        
        int targetDistanceOffset = Mathf.Abs(diff.x) + Mathf.Abs(diff.y) -
                                   (_gameState.SelectedBody != null ? _gameState.SelectedBody.TargetDistance : 1);
        if (targetDistanceOffset == 0) return _gameState.BossPosition;
        targetDistanceOffset = targetDistanceOffset / Mathf.Abs(targetDistanceOffset);

        diff = new Vector2Int(Mathf.Clamp(diff.x, -1, 1), Mathf.Clamp(diff.y, -1, 1));
        
        if (diff.x != 0 && diff.y != 0) moveHorizontal = !_lastMoveHorizontal;
        else moveHorizontal = diff.x != 0;

        Vector2Int newPosition = _gameState.BossPosition;
        if (moveHorizontal) newPosition += targetDistanceOffset * new Vector2Int(diff.x,0);
        else newPosition += targetDistanceOffset * new Vector2Int(0,diff.y);

        if (!IsSpaceFree(newPosition, _gameState.BossDirection)) return _gameState.BossPosition;
        
        return newPosition;
    }

    //Return true if boss moved to reach rotation
    public bool RotateTowardsPlayer()
    {
        Vector2Int diff = _gameState.PlayerPosition - _gameState.BossPosition;

        if (diff == Vector2Int.zero) return false;
        
        Vector2Int newDirection;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            newDirection = new Vector2Int(diff.x / Mathf.Abs(diff.x), 0);
            if (newDirection == -_gameState.BossDirection)
            {
                int diffY = diff.y == 0 ? 1 : diff.y;
                newDirection = new Vector2Int(0,diffY / Mathf.Abs(diffY));
            }
        }
        else
        {
            newDirection = new Vector2Int(0,diff.y / Mathf.Abs(diff.y));
            if (newDirection == -_gameState.BossDirection)
            {
                int diffX = diff.x == 0 ? 1 : diff.x;
                newDirection = new Vector2Int(diffX / Mathf.Abs(diffX), 0);
            }
        }
        
        if (!AllowedRotation(_gameState.BossDirection, newDirection)) return false;
        
        if (IsSpaceFree(_gameState.BossPosition, newDirection)) _gameState.BossDirection = newDirection;
        else if (IsSpaceFree(_gameState.BossPosition + _gameState.BossDirection, newDirection))
        {
            _gameState.BossPosition += _gameState.BossDirection;
            _gameState.BossDirection = newDirection;
            return true;
        } /*else if (IsSpaceFree(_gameState.BossPosition -_gameState.BossDirection, newDirection))
        {
            _gameState.BossPosition -= _gameState.BossDirection;
            _gameState.BossDirection = newDirection;
            return true;
        }*/
        else
        {
            _gameState.BossOutOfSpace = true;
        }

        return false;
    }

    bool AllowedRotation(Vector2Int fromDirectrion, Vector2Int toDirection)
    {
        return fromDirectrion != toDirection && fromDirectrion != -toDirection;
    }
    
    Vector2Int LeftArmOffset(Vector2Int direction)
    {
        return new Vector2Int(direction.y,-direction.x);
    }
    
    Vector2Int RightArmOffset(Vector2Int direction)
    {
        return new Vector2Int(-direction.y,direction.x);
    }

    public bool IsSpaceFree(Vector2Int position, Vector2Int direction)
    {
        if (_gameState.IsObstacle(position)) return false;
        if (_gameState.SelectedArm != null && _gameState.SelectedArm.Collides && _gameState.IsObstacle(position + LeftArmOffset(direction))) return false;
        if (_gameState.SelectedArm != null && _gameState.SelectedArm.Collides && _gameState.IsObstacle(position + RightArmOffset(direction))) return false;
        
        return true;
    }

    private bool TryAttackPattern(BossPart bodyPart)
    {
        if (bodyPart == null || bodyPart.AttackPattern.Count == 0) return false;

        var attackPattern = TransformedAttackPattern(bodyPart.AttackPattern);

        foreach (var position in attackPattern)
        {
            if (position == _gameState.PlayerPosition)
            {
                _gameState.BossAttackTiles = attackPattern;
                _gameState.BossAttackPart = bodyPart;
                return true;
            }
        }

        return false;
    }

    private List<Vector2Int> TransformedAttackPattern(List<Vector2Int> pattern)
    {
        List<Vector2Int> newPositions = new List<Vector2Int>();
        
        int direction = 0;
        if (_gameState.BossDirection.y < 0) direction = 0;
        else if (_gameState.BossDirection.x < 0) direction = 1;
        else if (_gameState.BossDirection.y > 0) direction = 2;
        else direction = 3;
        
        for (int i = 0; i < pattern.Count; i++)
        {
            Vector2Int fromPos = pattern[i];
            Vector2Int newPos;
            switch (direction)
            {
                case 0:
                    newPos = fromPos;
                    break;
                case 1:
                    newPos = new Vector2Int(fromPos.y,-fromPos.x);
                    break;
                case 2:
                    newPos = -fromPos;
                    break;
                default:
                    newPos = new Vector2Int(-fromPos.y,fromPos.x);
                    break;
            }
            
            newPositions.Add(_gameState.BossPosition + newPos);
        }

        return newPositions;
    }
}
