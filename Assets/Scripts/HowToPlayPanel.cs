using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HowToPlayPanel : MonoBehaviour
{
    public CanvasGroup[] panels;
    public GameObject nextButton, previousButton, startButton;
    public TextMeshProUGUI pageText;

    private int page;

    private void Start()
    {
        previousButton.SetActive(false);
        startButton.SetActive(false);
        UpdatePage();
    }

    public void NextPage()
    {
        panels[page++].DOFade(0f, 0.75f);
        panels[page].DOFade(1f, 0.75f);

        previousButton.SetActive(true);
        
        if (page + 1 == panels.Length)
        {
            nextButton.SetActive(false);
            startButton.SetActive(true);
        }
        UpdatePage();
    }

    public void PreviousPage()
    {
        panels[page--].DOFade(0f, 0.75f);
        panels[page].DOFade(1f, 0.75f);
        
        nextButton.SetActive(true);
        startButton.SetActive(false);

        if (page == 0) previousButton.SetActive(false);
        UpdatePage();
    }

    public void StartGame()
    {
        gameObject.SetActive(false);
        GameManager.FinishHowToPlay();
    }

    void UpdatePage()
    {
        pageText.text = $"{page + 1}/{panels.Length}";
    }
}
