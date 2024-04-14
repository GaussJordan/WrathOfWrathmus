using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RetryScreet : MonoBehaviour
{
    [SerializeField] private Text _messageText;
    [SerializeField] private Text _buttonText;
    [SerializeField] private Button _button;

    public async UniTask ShowView(string buttonText, string messageText, CancellationToken token)
    {
        gameObject.SetActive(true);
        _buttonText.text = buttonText;
        _messageText.text = messageText;
        await _button.OnClickAsync(token);
        gameObject.SetActive(false);
    }
}
