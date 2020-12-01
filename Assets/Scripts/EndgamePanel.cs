using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndgamePanel : MonoBehaviour
{
    [SerializeField, Header("Panel")] private TextMeshProUGUI endgameStatus;
    [SerializeField] private TextMeshProUGUI endgameScore;

    [SerializeField, Header("Back Button")] private Image fader;
    [SerializeField] private CanvasGroup mainMenuPanel;

    private CanvasGroup selfCanvasGroup;
    
    public static event Action ResetGame; 

    public void OpenPanel(bool victory, float score)
    {
        if (selfCanvasGroup == null) selfCanvasGroup = GetComponent<CanvasGroup>();
        if (victory) endgameStatus.text = "Victory";
        else endgameStatus.text = "Game Over";

        endgameScore.text = score.ToString("N0");
        selfCanvasGroup.alpha = 0;
        selfCanvasGroup.DOFade(1f, 2f);
    }

    public void BackButton()
    {
        fader.gameObject.SetActive(true);
        fader.DOColor(Color.black, 2f).onComplete = () =>
        {
            mainMenuPanel.gameObject.SetActive(true);
            mainMenuPanel.alpha = 1;
            ResetGame?.Invoke();
        };
    }
}