using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveCooldown;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private float _hurtCooldown;
    private GameState _gameState;

    private float _moveTimer;
    private float _attackTimer;
    private float _hurtTimer;

    void Awake()
    {
        _gameState = FindObjectOfType<GameController>().GameState;
    }
    
    void Update()
    {
        if (_gameState.Paused) return;
        
        _moveTimer += Time.deltaTime;
        _attackTimer += Time.deltaTime;
        
        Vector2Int lastPos = _gameState.PlayerPosition;
        Vector2Int newDirection = _gameState.PlayerNextPosition;
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            newDirection = Vector2Int.down;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            newDirection = Vector2Int.up;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            newDirection = Vector2Int.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            newDirection = Vector2Int.right;
        }
        if (Input.GetKeyDown(KeyCode.Space) && _attackTimer > _attackCooldown)
        {
            _gameState.PlayerDidAttack = true;
            _attackTimer = 0f;
            if (_gameState.BossPosition == _gameState.PlayerPosition + _gameState.PlayerDirection)
            {
                if (_gameState.SelectedBody.CanBeHit) _gameState.BossLives--;
                else if (_gameState.SelectedBody.CanBeHitBehind &&
                         _gameState.PlayerDirection == _gameState.BossDirection) _gameState.BossLives--;
            }
        }
        
        if (_moveTimer > _moveCooldown*.25f && _gameState.IsBoss(_gameState.PlayerPosition))
        {
            Vector2Int[] tryPositions = new Vector2Int[4];
            tryPositions[0] = _gameState.PlayerPosition - _gameState.PlayerDirection;
            tryPositions[1] = _gameState.PlayerPosition + _gameState.BossDirection;
            tryPositions[2] = _gameState.PlayerPosition + _gameState.PlayerDirection;
            tryPositions[3] = _gameState.PlayerPosition - _gameState.BossDirection;

            for (int i = 0; i < 4; i++)
            {
                if (!_gameState.IsObstacle(tryPositions[i]) &&
                    !_gameState.IsBoss(tryPositions[i]))
                {
                    _gameState.PlayerPosition = tryPositions[i];
                    _gameState.PlayerNextPosition = Vector2Int.zero;
                    break;
                }
            }
            

            _gameState.PlayerLives--;
            _moveTimer = 0;
        }

        if (newDirection != Vector2Int.zero && !_gameState.IsObstacle(_gameState.PlayerPosition + newDirection))
        {
            if (_moveTimer > _moveCooldown)
            {
                _gameState.PlayerPosition = _gameState.PlayerPosition + newDirection;
                _gameState.PlayerDirection = _gameState.PlayerPosition - lastPos;
                _gameState.PlayerNextPosition = Vector2Int.zero;
                
                _moveTimer = 0;
            }
            else
            {
                _gameState.PlayerNextPosition = newDirection;
            }
        }
    }
}
