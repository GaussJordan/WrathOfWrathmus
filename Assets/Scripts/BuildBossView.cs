using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BuildBossView : MonoBehaviour
{
    [SerializeField] private BossPartUIItem _headPart;
    [SerializeField] private BossPartUIItem _bodyPart;
    [SerializeField] private BossPartUIItem _armPart;
    [SerializeField] private BossPartUIItem _armPart2;
    [SerializeField] private List<BossPartUIItem> _bossParts;

    [SerializeField] private Button _summonButton;
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _strengthText;
    [SerializeField] private Text _requiredStrengthText;
    
    private GameState _gameState;
    private bool summon = false;

    public async UniTask ShowView(CancellationToken token)
    {
        gameObject.SetActive(true);
        summon = false;

        while (!summon && !token.IsCancellationRequested)
        {
            await _summonButton.OnClickAsync(token);

            int strength = _gameState.SelectedBody != null ? _gameState.SelectedBody.Strength : 0;
            strength += _gameState.SelectedHead != null ? _gameState.SelectedHead.Strength : 0;
            strength += _gameState.SelectedArm != null ? _gameState.SelectedArm.Strength : 0;
            
            if (_gameState.SelectedArm != null && _gameState.SelectedBody != null &&
                _gameState.SelectedHead != null && strength >= _gameState.RequiredStrength) summon = true;
        }
        
        gameObject.SetActive(false);
    }

    public void Init(GameState gameState)
    {
        _gameState = gameState;

        _levelText.text = "Level " + (gameState.LevelIndex + 1);

        _headPart.Init(_gameState.SelectedHead);
        _bodyPart.Init(_gameState.SelectedBody);
        _armPart.Init(_gameState.SelectedArm);
        _armPart2.Init(_gameState.SelectedArm);

        _headPart.LockImage.enabled = _gameState.LockedHead;
        _bodyPart.LockImage.enabled = _gameState.LockedBody;
        _armPart.LockImage.enabled = _gameState.LockedArm;
        _armPart2.LockImage.enabled = _gameState.LockedArm;

        int strength = _gameState.SelectedBody != null ? _gameState.SelectedBody.Strength : 0;
        strength += _gameState.SelectedHead != null ? _gameState.SelectedHead.Strength : 0;
        strength += _gameState.SelectedArm != null ? _gameState.SelectedArm.Strength : 0;

        _strengthText.text = "Strength: " + strength;
        _requiredStrengthText.text = "Required: " + _gameState.RequiredStrength + "+";

        if (_gameState.SelectedArm != null && _gameState.SelectedBody != null &&
            _gameState.SelectedHead != null && strength >= _gameState.RequiredStrength) _summonButton.interactable = true;
        else _summonButton.interactable = false;

            for (int i = 0; i < _bossParts.Count; i++)
            {
                if (i >= _gameState.UnlockedParts.Count) _bossParts[i].gameObject.SetActive(false);
                else
                {
                    _bossParts[i].gameObject.SetActive(true);

                    _bossParts[i].SelectButton.onClick.RemoveAllListeners();

                    var bossPart = _gameState.UnlockedParts[i];
                    int index = i;

                    bool selected = bossPart == _gameState.SelectedArm ||
                                    bossPart == _gameState.SelectedBody ||
                                    bossPart == _gameState.SelectedHead;
                    _bossParts[i].SelectedImage.enabled = selected;
                    _bossParts[i].Init(bossPart);
                    bool locked = bossPart.PartType == PartType.Arm && _gameState.LockedArm ||
                                  bossPart.PartType == PartType.Head && _gameState.LockedHead ||
                                  bossPart.PartType == PartType.Body && _gameState.LockedBody;

                    _bossParts[i].LockImage.enabled = locked;

                    _bossParts[i].SelectButton.onClick.AddListener((() =>
                    {
                        SelectBossPart(index);
                    }));
                }
            }
    }

    public void SelectBossPart(int partIndex)
    {
        var bossPart = _gameState.UnlockedParts[partIndex];
        if (bossPart.PartType == PartType.Head && _gameState.LockedHead == false) _gameState.SelectedHead = bossPart;
        if (bossPart.PartType == PartType.Body && _gameState.LockedBody == false) _gameState.SelectedBody = bossPart;
        if (bossPart.PartType == PartType.Arm && _gameState.LockedArm == false) _gameState.SelectedArm = bossPart;
        Init(_gameState);
    }
}
