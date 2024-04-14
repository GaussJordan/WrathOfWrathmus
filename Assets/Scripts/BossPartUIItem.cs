using UnityEngine;
using UnityEngine.UI;

public class BossPartUIItem : MonoBehaviour
{
    public Button SelectButton;
    public Image IconImage;
    public Image SelectedImage;
    public Image LockImage;
    public BossPart BossPart;

    public void Init(BossPart bossPart)
    {
        BossPart = bossPart;
        if (BossPart != null)
        {
            IconImage.color = Color.white;
            IconImage.sprite = bossPart.GetSprite(0, 0);
        }
        else IconImage.color = Color.black;
    }
}