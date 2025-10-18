using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class SpriteGroup
{
    public Sprite[] sprites;
}

public class UI_Tutorial : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private String[] descriptions;
    [SerializeField] private SpriteGroup[] sprites;

    [SerializeField] private int currentStep = 0;
    private Coroutine imageChanger;
    public int CurrentStep
    {
        get => currentStep;
        private set
        {
            currentStep = value;
            if(currentStep == 0)
            {
                leftButton.gameObject.SetActive(false);
            }
            else if (currentStep == 5)
            {
                rightButton.gameObject.SetActive(false);
            }
            else 
            {
                leftButton.gameObject.SetActive(true);
                rightButton.gameObject.SetActive(true);
            }

            ShowImage(currentStep);
        }
    }

    private void Start()
    {
        CurrentStep = 0;
        leftButton.onClick.AddListener(() => NextImage(-1));
        rightButton.onClick.AddListener(() => NextImage(1));
    }

    private void NextImage(int gap)
    {
        int nextStep = (CurrentStep + gap) % sprites.Length;
        CurrentStep = nextStep;
    }

    private void ShowImage(int currentStep)
    {
        if(imageChanger != null)
        {
            StopCoroutine(imageChanger);
        }

        imageChanger = StartCoroutine(ChangeImage(currentStep));
    }

    private IEnumerator ChangeImage(int currentStep)
    {
        Sprite[] currentSprites = sprites[currentStep].sprites;
        string description = descriptions[currentStep];
        descriptionText.text = description;

        int curIndex = 0;

        while (true)
        {
            tutorialImage.sprite = currentSprites[curIndex];
            curIndex = (curIndex + 1) % currentSprites.Length;
            yield return new WaitForSeconds(0.08f);
        }
    }
}
