using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField] private bool _isPlayer;
    [SerializeField] private List<GameObject> _hearts;
    private GameState _gameState;

    private float _animationTime;

    void Awake()
    {
        _gameState = FindObjectOfType<GameController>().GameState;
    }

    void Update()
    {
        int compare = _isPlayer ? _gameState.PlayerLives : _gameState.BossLives;
        for (int i = 0; i < _hearts.Count; i++)
        {
            _hearts[i].SetActive(!_gameState.Paused && compare > i);
        }
    }
}
