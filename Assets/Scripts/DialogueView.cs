using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShowDialogueLine : ISequenceStep
{
    [SerializeField] private string _message;
    [SerializeField] private BossPart _bossPart;
    [SerializeField] private AudioClip _audioClip;
    
    public async UniTask Run(CancellationToken token)
    {
        await GameObject.FindObjectOfType<DialogueView>().ShowDialogue(_bossPart, _message, _audioClip, token);
    }
}

public class DialogueView : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private Text _name;
    [SerializeField] private Text _message;
    [SerializeField] private Image _icon;
    [SerializeField] private BossPart _bossPart;
    [SerializeField] private float _animationTime;
    [SerializeField] private AudioSource _audioSource;

    public async UniTask ShowDialogue(BossPart bossPart, string message, AudioClip audioClip, CancellationToken token)
    {
        _content.SetActive(true);
        _bossPart = bossPart;
        _message.text = "";
        _name.text = bossPart.PaperdollName;
        
        if (audioClip != null) _audioSource.PlayOneShot(audioClip);

        float duration = audioClip != null ? audioClip.length : 3f;
        _message.DOText(message, duration, true);
        _content.transform.localScale = Vector3.zero;
        _content.transform.DOScale(1f, .2f);
        StartCoroutine(AnimateSpeechCoroutine(duration));
        await UniTask.Delay(TimeSpan.FromSeconds(duration + .5f), cancellationToken: token);
        _content.transform.DOScale(0f, .2f);
        await UniTask.Delay(TimeSpan.FromSeconds(.2f), cancellationToken: token);
        _content.SetActive(false);
    }

    private IEnumerator AnimateSpeechCoroutine(float duration)
    {
        if (_bossPart == null || _bossPart.PaperdollSprites.Count == 0) yield break;
        
        for (int i = 0; i < duration / _animationTime; i++)
        {
            _icon.sprite = _bossPart.PaperdollSprites[i % _bossPart.PaperdollSprites.Count];
            yield return new WaitForSeconds(_animationTime);
        }
        _icon.sprite = _bossPart.PaperdollSprites[0];
    }
}
