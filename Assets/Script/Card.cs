using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Class to represent each card
public class Card : MonoBehaviour
{
    private int spriteID;
    private int id;
    private bool flipped;
    private bool turning;
    [SerializeField]
    private Image img;

    private bool isMatched = false;
    public bool IsMatched { get { return isMatched; } }
    public bool IsFlipped { get { return flipped; } }

    // Flip card animation
    // If changeSprite is specified, will rotate 90 degrees, change to back/front sprite before flipping another 90 degrees
    private IEnumerator Flip90(Transform thisTransform, float time, bool changeSprite)
    {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));
        float rate = 1.0f / time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }
        // Change sprite and flip another 90 degrees
        if (changeSprite)
        {
            flipped = !flipped;
            ChangeSprite();
            StartCoroutine(Flip90(transform, time, false));
        }
        else
            turning = false;
    }

    // Perform a 180-degree flip
    public void Flip()
    {
        turning = true;
        AudioPlayer.Instance.PlayAudio(0);
        StartCoroutine(Flip90(transform, 0.25f, true));
    }

    // Toggle front/back sprite
    private void ChangeSprite()
    {
        if (spriteID == -1 || img == null) return;
        if (flipped)
            img.sprite = CardGameManager.Instance.GetSprite(spriteID);
        else
            img.sprite = CardGameManager.Instance.CardBack();
    }

    // Call fade animation
    public void Inactive()
    {
        isMatched = true;
        StartCoroutine(Fade());
    }

    // Play fade animation by changing alpha of img's color
    private IEnumerator Fade()
    {
        float rate = 1.0f / 2.5f;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            img.color = Color.Lerp(img.color, Color.clear, t);

            yield return null;
        }
    }

    // Set card to be active color
    public void Active()
    {
        isMatched = false;
        if (img)
            img.color = Color.white;
    }

    // SpriteID getter and setter
    public int SpriteID
    {
        set
        {
            spriteID = value;
            flipped = true;
            ChangeSprite();
        }
        get { return spriteID; }
    }

    // Card ID getter and setter
    public int ID
    {
        set { id = value; }
        get { return id; }
    }

    // Reset card to default rotation
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        flipped = true;
        ChangeSprite();
    }

    // Flip card immediately without animation
    public void FlipImmediate()
    {
        turning = false;
        flipped = true;
        ChangeSprite();
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    // Card onclick event
    public void CardBtn()
    {
        if (flipped || turning) return;
        if (!CardGameManager.Instance.canClick()) return;
        Flip();
        StartCoroutine(SelectionEvent());
    }

    // Inform manager card is selected with a slight delay
    private IEnumerator SelectionEvent()
    {
        yield return new WaitForSeconds(0.5f);
        CardGameManager.Instance.cardClicked(spriteID, id);
    }

    // Set matched status
    public void SetMatched(bool matched)
    {
        isMatched = matched;
    }
}
