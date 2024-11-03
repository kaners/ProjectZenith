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
            GameManager.Instance.AddFlippedCard(this);
        }
    }

    public void FlipCard()
    {
        isFlipped = true;
        animator.SetTrigger("FlipToFront");
        cardImage.sprite = cardFace;
      
    }

    public void HideCard()
    {
        isFlipped = false;
        animator.SetTrigger("FlipToBack");
        cardImage.sprite = cardBack;
    }

    public void ShowCardBack()
    {
        cardImage.sprite = cardBack;
    }
}
