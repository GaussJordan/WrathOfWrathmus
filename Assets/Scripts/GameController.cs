using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]
public class EndingSequenceAnimation : ISequenceStep
{
    public async UniTask Run(CancellationToken token)
    {
        await GameObject.FindObjectOfType<GameController>().EndingSequenceMidAnimation(token);
    }
}

public class GameController : MonoBehaviour
{
    public BodyPartsConfig BodyPartsConfig;
    public GameState GameState = new GameState();
    public Sequence _finalSequence;
    public BuildBossView _buildBossView;
    public List<TileBase> ObstacleTiles;
    public List<Tilemap> Tilemaps;
    public Tilemap WarningTilemap;
    public BossController BossController;
    public PlayerController PlayerController;
    public List<LevelConfig> LevelConfigs;
    public Animation CircleFadeoutAnimation;
    public AudioSource _battleMusic;
    public AudioSource _menuMusic;
    public RetryScreet _retryScreen;
    public List<BossPart> EndingRequiredParts;
    public ParticleSystem _finalSmokeEffect;
    public AudioSource _audioSource;
    public AudioClip _deathAudioSource;
    public AudioClip _winAudioClip;
    public AudioClip _summonAudioClip;
    public AudioClip _summonAudioClip2;
    public AudioClip _finalAudioClip;
    public Animation _spawnAnimation;

    public List<Sequence> _deathSequences;

    private CancellationTokenSource cts;
    
    private void Awake()
    {
        GameState.UnlockedParts.AddRange(BodyPartsConfig.Parts);
        GameState.ObstacleTiles = ObstacleTiles;
        GameState.WarningTilemap = WarningTilemap;
        GameState.LevelIndex = PlayerPrefs.GetInt("LevelIndex", 0);

        cts = new CancellationTokenSource();
        
        _buildBossView.Init(GameState);
        GameLoop(cts.Token);
    }

    private void OnApplicationQuit()
    {
        cts.Cancel();
    }

    private async UniTask GameLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (GameState.LevelIndex >= LevelConfigs.Count) GameState.LevelIndex = LevelConfigs.Count - 1;
            LevelConfig selectedLevel = LevelConfigs[GameState.LevelIndex];

            GameState.LockedBody = selectedLevel.LockedBody != null;
            GameState.LockedHead = selectedLevel.LockedHead != null;
            GameState.LockedArm = selectedLevel.LockedArm != null;
            GameState.SelectedBody = selectedLevel.LockedBody;
            GameState.SelectedHead = selectedLevel.LockedHead;
            GameState.SelectedArm = selectedLevel.LockedArm;
            GameState.BaseTilemap = Tilemaps[selectedLevel.TilemapIndex];

            GameState.RequiredStrength = selectedLevel.RequiredStrength;
            GameState.UnlockedParts = selectedLevel.UnlockedParts;
            
            _buildBossView.Init(GameState);
            
            _menuMusic.Play();
            _menuMusic.volume = 0f;
            _menuMusic.DOFade(.25f, .5f);
            
            await UniTask.WaitForSeconds(1f, cancellationToken: token);
            
            await _buildBossView.ShowView(token);
            
            for (int i = 0; i < Tilemaps.Count; i++)
            {
                Tilemaps[i].gameObject.SetActive(selectedLevel.TilemapIndex == i);
            }
            
            GameState.Paused = true;
            GameState.BossLives = 3;
            GameState.PlayerLives = 3;
            GameState.BossPosition = selectedLevel.BossSpawnPoint;
            GameState.BossDirection = new Vector2Int(0, -1);
            GameState.PlayerPosition = selectedLevel.PlayerSpawnPoint;
            GameState.PlayerDirection = new Vector2Int(0, 1);
            
            PlayerController.GetComponent<CharacterView>()
                .Init(GameState.SelectedBody, GameState.SelectedHead, GameState.SelectedArm);
            
            CircleFadeoutAnimation.transform.parent.position = PlayerController.transform.position;
            CircleFadeoutAnimation.transform.parent.localScale = Vector3.one;
            
            _menuMusic.DOFade(0f, 0.5f);

            BossController.GetComponent<CharacterView>()
                .Init(GameState.SelectedBody, GameState.SelectedHead, GameState.SelectedArm);
            
            
            await Summon(token);

            if (EndingRequiredParts.Contains(GameState.SelectedBody) &&
                EndingRequiredParts.Contains(GameState.SelectedHead) &&
                EndingRequiredParts.Contains(GameState.SelectedArm))
            {
                await FinalSequence(token);

                GameState.LevelIndex = 0;
            }
            else
            {
                if (selectedLevel.FirstTimeSequence != null && GameState.SeenLevelSequence < GameState.LevelIndex)
                {
                    GameState.SeenLevelSequence = GameState.LevelIndex;
                    await selectedLevel.FirstTimeSequence.Run(token);
                }
                
                PlayerController.enabled = true;
                GameState.Paused = false;
            
                bool win = await PlayLevel(token);
                PlayerController.enabled = false;
                GameState.Paused = true;
                GameState.ClearTileStates(2);
                
                if (win)
                {
                    BossController.GetComponent<CharacterView>().enabled = false;
                    StartCoroutine(BossController.GetComponent<CharacterView>().DeathCoroutine(2f));
                    _audioSource.PlayOneShot(_winAudioClip);
                    GameState.LevelIndex++;
                    PlayerPrefs.SetInt("LevelIndex", GameState.LevelIndex);
                    
                    await UniTask.WaitForSeconds(1f, cancellationToken: token);
                    
                    CircleFadeoutAnimation.transform.parent.position = PlayerController.transform.position;
                    CircleFadeoutAnimation.transform.parent.localScale = Vector3.one;
                    CircleFadeoutAnimation.Play("ZoomIn");
                }
                else
                {
                    CircleFadeoutAnimation.transform.parent.position = PlayerController.transform.position;
                    CircleFadeoutAnimation.transform.parent.localScale = Vector3.one;
                    CircleFadeoutAnimation.Play("ZoomIn");
                    await DeathSequence(token);
                }
            }
           
            BossController.GetComponent<CharacterView>().enabled = true;
            BossController.gameObject.SetActive(false);
        }
    }

    private async UniTask Summon(CancellationToken token)
    {
        CircleFadeoutAnimation.Play("ZoomOutIn");
        CircleFadeoutAnimation.transform.parent.DOMove(BossController.transform.position, 1f);
        CircleFadeoutAnimation.transform.parent.DOScale(3f, 1f);

        await UniTask.WaitForSeconds(1f, cancellationToken: token);
        _audioSource.PlayOneShot(_summonAudioClip2);
        _spawnAnimation.transform.position = BossController.transform.position;
        _spawnAnimation.Play();
        await UniTask.WaitForSeconds(1f, cancellationToken: token);
        
        _audioSource.PlayOneShot(_summonAudioClip);
        
        BossController.gameObject.SetActive(true);
        
        CircleFadeoutAnimation.Play("ZoomOut");
    }

    private async UniTask DeathSequence(CancellationToken token)
    {
        _audioSource.PlayOneShot(_deathAudioSource);
        await UniTask.WaitForSeconds(.5f, cancellationToken: token);

        string[] deathMessages = new string[]
        {
            "But what a cruel fate!",
            "Hang in there friendo!",
            "Oh no, you died!",
            "We'll get 'em next time!",
            "Let's get that revenge!",
        };
        
        await _retryScreen.ShowView("Retry",deathMessages[Random.Range(0,deathMessages.Length)],token);
    }

    private async UniTask FinalSequence(CancellationToken token)
    {
        BossController.transform.DOShakePosition(.1f, .2f);
        await UniTask.WaitForSeconds(1f, cancellationToken: token);
        CircleFadeoutAnimation.transform.parent.localPosition = BossController.transform.position + Vector3.up * .5f;
        CircleFadeoutAnimation.transform.parent.localScale = Vector3.one * 2f;
        CircleFadeoutAnimation.Play("ZoomIn");
        await _finalSequence.Run(token);

        await _retryScreen.ShowView("Restart", "This is it, thank you for playing!", token);
    }

    private async UniTask<bool> PlayLevel(CancellationToken token)
    {
        _battleMusic.Play();
        _battleMusic.volume = 0f;
        _battleMusic.DOFade(.5f, .5f);
        while (!token.IsCancellationRequested && GameState.PlayerLives > 0 && GameState.BossLives > 0)
        {
            await UniTask.Yield(token);
        }

        _battleMusic.DOFade(0f, 0.5f);
        
        return GameState.PlayerLives > 0;
    }

    public async UniTask EndingSequenceMidAnimation(CancellationToken token)
    {
        GameState.BossDirection = Vector2Int.up;
        CircleFadeoutAnimation.Play("ZoomInFully");
        await UniTask.WaitForSeconds(1f, cancellationToken: token);
        _finalSmokeEffect.Play();
        _audioSource.PlayOneShot(_finalAudioClip);
    }
}
