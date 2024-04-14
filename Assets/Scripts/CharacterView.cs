using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private Sprite[] _idleSprites;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _headRenderer;
    [SerializeField] private SpriteRenderer _leftArmRenderer;
    [SerializeField] private SpriteRenderer _rightArmRenderer;
    [SerializeField] private SpriteRenderer _attackRenderer;
    [SerializeField] private SpriteRenderer _shadowRenderer;
    [SerializeField] private SpriteRenderer _barRenderer;
    [SerializeField] private Color _hitEffectColor = Color.red;
    public ParticleSystem _dustParticle;
    public ParticleSystem _sparkParticle;
    [SerializeField] private bool _isPlayer;
    [SerializeField] private float _animationSpeed = .5f;
    [SerializeField] private float _attackAnimationSpeed = 6f;
    [SerializeField] public ParticleSystem[] _attackEffects;
    [SerializeField] public ParticleSystem[] _deathEffects;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _moveAudioClip;
    [SerializeField] private AudioClip _attackAudioClip;

    [SerializeField] private Vector2[] _leftArmOffsets;
    [SerializeField] private Vector2[] _rightArmOffsets;
    [SerializeField] private Vector3[] _leftArmScales;
    [SerializeField] private Vector3[] _rightArmScales;
    [SerializeField] private Vector3[] _attackScales;
    [SerializeField] private Vector3[] _attackOffsets;

    [SerializeField] private BossPart _bodyPart;
    [SerializeField] private BossPart _headPart;
    [SerializeField] private BossPart _armPart;
    [SerializeField] private BossPart _attackPart;

    private GameState _gameState;

    private float _animationTime;
    private float _attackAnimationTime = 3f;
    private int _prevLives;

    private Vector2Int _lastPosition;

    void Awake()
    {
        _gameState = FindObjectOfType<GameController>().GameState;
        _lastPosition = BoardPosition;
        transform.position = new Vector3(BoardPosition.x, BoardPosition.y,0);
        transform.position += new Vector3(.5f, .5f);
    }

    public Vector2Int BoardPosition => _isPlayer ? _gameState.PlayerPosition : _gameState.BossPosition;
    public Vector2Int Direction => _isPlayer ? _gameState.PlayerDirection : _gameState.BossDirection;

    public int Lives => _isPlayer ? _gameState.PlayerLives : _gameState.BossLives;
    void Update()
    {
        //_animationTime += Time.deltaTime * _animationSpeed;

        if (Lives != _prevLives)
        {
            if (Lives < _prevLives) StartCoroutine(HitEffect());
            _prevLives = Lives;
        }
        
        if (_isPlayer && _gameState.PlayerDidAttack)
        {
            _gameState.PlayerDidAttack = false;
            _attackAnimationTime = 0f;
        }
        
        if (!_isPlayer && _gameState.BossDidAttack)
        {
            _gameState.BossDidAttack = false;
            _attackAnimationTime = 0f;
        }
        
        int direction = 0;
        if (Direction.y < 0) direction = 0;
        else if (Direction.x < 0) direction = 1;
        else if (Direction.y > 0) direction = 2;
        else direction = 3;

        float xScale = -1;
        
        if (BoardPosition != _lastPosition)
        {
            _lastPosition = BoardPosition;
            
            Vector3 newPosition = new Vector3(BoardPosition.x, BoardPosition.y,0);
            newPosition += new Vector3(.5f, .5f);
            transform.DOKill(true);
            transform.DOJump(newPosition, .5f, 1, .2f);
            _shadowRenderer.transform.DOMove(newPosition + Vector3.down*.3f, .2f);
            _animationTime += 1;
            _dustParticle.transform.position = transform.position;
            _dustParticle.Emit(10);
            _gameState.BossOutOfSpace = false;
            _audioSource.PlayOneShot(_moveAudioClip);
        }

        int animation = (int)Mathf.Repeat(_animationTime, 2f);

        int baseSorting = 400 - BoardPosition.y * 10;
        
        switch (direction)
        {
            case 0:
                xScale = 1f;
                break;
            case 1:
                xScale = 1f;
                break;
            case 2:
                xScale = 1f;
                break;
            case 3:
                xScale = -1f;
                break;
        }

        if (direction != 1 && direction != 3) xScale = animation == 0 ? -xScale : xScale;

        _spriteRenderer.transform.localScale = new Vector3(xScale, 1f, 1f);
        _spriteRenderer.sortingOrder = baseSorting;
        _spriteRenderer.sprite = _bodyPart.GetSprite(direction, animation);

        if (_headRenderer)
        {
            _headRenderer.sprite = _headPart.GetSprite(direction, animation);
            _headRenderer.sortingOrder = baseSorting + 3;
        }
        if (_leftArmRenderer)
        {
            _leftArmRenderer.sprite = _armPart.GetSprite(direction, animation);
            _leftArmRenderer.transform.localPosition = _leftArmOffsets[direction];
            _leftArmRenderer.transform.localScale = _leftArmScales[direction];
            _leftArmRenderer.sortingOrder = baseSorting + 2;
        }

        if (_rightArmRenderer)
        {
            _rightArmRenderer.sprite = _armPart.GetSprite(direction, animation);
            _rightArmRenderer.transform.localPosition = _rightArmOffsets[direction];
            _rightArmRenderer.transform.localScale = _rightArmScales[direction];
            _rightArmRenderer.sortingOrder = baseSorting + 2;
        }

        _attackRenderer.enabled = _attackAnimationTime < 2f;
        if (_attackAnimationTime < 2f)
        {
            if (_attackAnimationTime == 0)
            {
                int effectIndex = _isPlayer || _gameState.BossAttackPart == null ? 0 : _gameState.BossAttackPart.EffectIndex;

                var effect = _attackEffects[Mathf.Clamp(effectIndex, 0, _attackEffects.Length - 1)];
                effect.transform.position = transform.position + new Vector3(Direction.x, Direction.y, 0);
                effect.Emit(10);
                
                if (!_isPlayer)
                {
                    if (_gameState.BossAttackPart != null) _audioSource.PlayOneShot(_gameState.BossAttackPart.AttackAudioClip);
                    StartCoroutine(PlayAttackEffect(effect, _gameState.BossAttackPart == null ? 0 : _gameState.BossAttackPart.EffectOffset));
                }
                else
                {
                    effect.transform.position = transform.position + new Vector3(Direction.x, Direction.y, 0);
                    effect.Emit(10);
                    _audioSource.PlayOneShot(_attackAudioClip);
                }
            }
            
            int attackAnimation = (int)Mathf.Repeat(_attackAnimationTime, 2f);
            _attackRenderer.sprite = _attackPart.GetSprite(direction, attackAnimation);
            _attackRenderer.transform.localPosition = _attackOffsets[direction];
            _attackRenderer.transform.localScale = _attackScales[direction];
            _attackRenderer.sortingOrder = baseSorting + (direction == 2 ? -1 : 3);
            _attackAnimationTime += Time.deltaTime * _attackAnimationSpeed;
        }

        if (!_isPlayer && _gameState.BossOutOfSpace)
        {
            transform.DOShakePosition(.1f, .2f);
            _gameState.BossOutOfSpace = false;
        }
    }

    private IEnumerator PlayAttackEffect(ParticleSystem effect, float offset)
    {
        for (int i = 0; i < _gameState.BossAttackTiles.Count; i++)
        {
            effect.transform.position = new Vector3(_gameState.BossAttackTiles[i].x, _gameState.BossAttackTiles[i].y, 0) + 
                                        new Vector3(.5f, .5f);
            effect.Emit(10);
            yield return new WaitForSeconds(offset);
        }
    }

    private void OnEnable()
    {
        SetHitEffect(Color.clear);
        SetColor(Color.white);
    }

    public void Init(BossPart bodyPart, BossPart headPart, BossPart armPart)
    {
        if (!_isPlayer) {
            _armPart = armPart;
            _bodyPart = bodyPart;
            _headPart = headPart;
        }
        _gameState = FindObjectOfType<GameController>().GameState;
        _lastPosition = BoardPosition;
        transform.position = new Vector3(BoardPosition.x, BoardPosition.y,0);
        transform.position += new Vector3(.5f, .5f);
    }

    private IEnumerator HitEffect()
    {
        SetHitEffect(_hitEffectColor);
        yield return new WaitForSeconds(.3f);
        SetHitEffect(Color.clear);
    }

    private void SetHitEffect(Color color)
    {
        var _materialPropertyBlock = new MaterialPropertyBlock();
        transform.DOShakePosition(.1f, .2f);
        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_HitColor", color);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        if (!_isPlayer)
        {
            _leftArmRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor("_HitColor", color);
            _leftArmRenderer.SetPropertyBlock(_materialPropertyBlock);
            _rightArmRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor("_HitColor", color);
            _rightArmRenderer.SetPropertyBlock(_materialPropertyBlock);
            _headRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor("_HitColor", color);
            _headRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }

    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
        if (_leftArmRenderer != null) _leftArmRenderer.color = color;
        if (_rightArmRenderer != null) _rightArmRenderer.color = color;
        if (_headRenderer != null) _headRenderer.color = color;
    }

    public IEnumerator DazedEffect(float duration)
    {
        _barRenderer.transform.parent.DOScale(1f, .1f);
        SetColor(Color.gray);

        _barRenderer.size = new Vector2(2f, 1f);
        _barRenderer.transform.localScale = new Vector3(0, 1, 1);
        _barRenderer.transform.DOScale(1f, duration);

        if (duration > .4f)
        {
            yield return new WaitForSeconds(duration - .4f);
            transform.DOShakePosition(.1f, .1f);
            yield return new WaitForSeconds(.2f);
            transform.DOShakePosition(.1f, .2f);
            yield return new WaitForSeconds(.2f);
        } else yield return new WaitForSeconds(duration);
        
        SetColor(Color.white);
        
        _barRenderer.transform.parent.DOScale(0f, .1f);
    }

    public IEnumerator DeathCoroutine(float duration)
    {
        _spriteRenderer.transform.DOScale(0f, .5f);
        for (int i = 0; i < _deathEffects.Length; i++)
        {
            _deathEffects[i].Play();
        }

        yield return new WaitForSeconds(duration);
        _spriteRenderer.transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }
}
