using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Card : MonoBehaviour
{
      public int cardID;
    public bool isFlipped = false;
    public Sprite cardFace;
    public Sprite cardBack;

    private Image cardImage;
    private Animator animator;

    void Start()
    {
        cardImage = GetComponent<Image>();
        animator = GetComponent<Animator>();
        ShowCardBack();
    }

    public void OnCardClicked()
    {
        if (!isFlipped)
        {
            FlipCard();
           
        }
    }

    public void FlipCard()
    {
        isFlipped = true;
        animator.Play("FlipToFront");
        cardImage.sprite = cardFace;
    }

    public void HideCard()
    {
        isFlipped = false;
        animator.Play("FlipToBack");
        cardImage.sprite = cardBack;
    }

    public void ShowCardBack()
    {
        cardImage.sprite = cardBack;
    }
}
