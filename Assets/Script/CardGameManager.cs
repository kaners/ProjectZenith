using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameState
{
    public float timeElapsed;
    public int gameSize;
    public int[] cardSpriteIDs;
    public bool[] cardIsMatched;
    public bool[] cardIsFlipped;
}

public class CardGameManager : MonoBehaviour
{
    public static CardGameManager Instance;
    public static int gameSize = 2;

    // Game object instance
    [SerializeField]
    private GameObject prefab;
    // Parent object of cards
    [SerializeField]
    private GameObject cardList;
    // Sprite for card back
    [SerializeField]
    private Sprite cardBack;
    // All possible sprites for card front
    [SerializeField]
    private Sprite[] sprites;
    // List of cards
    private Card[] cards;

    // We place cards on this panel
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject info;
    // For preloading
    [SerializeField]
    private Card spritePreload;
    // Other UI elements
    [SerializeField]
    private Text sizeLabel;
    [SerializeField]
    private Slider sizeSlider;
    [SerializeField]
    private Text timeLabel;
    private float time;

    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameStart = false;
        panel.SetActive(false);
    }

    // Preload card images to prevent lag
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
    }

    // Start a new game
    public void StartCardGame()
    {
        if (gameStart) return; // Return if game already running
        gameStart = true;
        // Toggle UI
        panel.SetActive(true);
        info.SetActive(false);
        // Set cards, size, position
        SetGamePanel();
        // Renew gameplay variables
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        // Allocate sprites to cards
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
    }

    // Initialize cards, size, and position based on game size
    private void SetGamePanel()
    {
        // If game size is odd, we should have 1 card less
        int isOdd = gameSize % 2;

        cards = new Card[gameSize * gameSize - isOdd];
        // Remove all game objects from parent
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // Calculate position between each card & start position of each card based on the Panel
        RectTransform panelSize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float rowSize = panelSize.sizeDelta.x;
        float colSize = panelSize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = rowSize / gameSize;
        float yInc = colSize / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        // For each row
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            // For each column
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                // If it's the last card and game size is odd, move the middle card to last spot
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // Create card prefab
                    c = Instantiate(prefab);
                    // Assign parent
                    c.transform.parent = cardList.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].ID = index;
                    // Modify its size
                    c.transform.localScale = new Vector3(scale, scale);
                }
                // Assign location
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;
            }
            curY += yInc;
        }
    }

    // Reset face-down rotation of all cards
    void ResetFace()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetRotation();
    }

    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        // Display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }

    // Allocate pairs of sprites to card instances
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        // Sprite selection
        for (i = 0; i < cards.Length / 2; i++)
        {
            // Get a random sprite
            int value = Random.Range(0, sprites.Length - 1);
            // Ensure the sprite hasn't been selected already
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        // Reset card sprites
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        // Allocate sprites to cards in pairs
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }
    }

    // Slider update game size
    public void SetGameSize()
    {
        gameSize = (int)sizeSlider.value;
        sizeLabel.text = gameSize + " X " + gameSize;
    }

    // Return sprite based on its ID
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }

    // Return card back sprite
    public Sprite CardBack()
    {
        return cardBack;
    }

    // Check if clickable
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }

    // Card onclick event
    public void cardClicked(int spriteId, int cardId)
    {
        // First card selected
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        {
            // Second card selected
            if (spriteSelected == spriteId)
            {
                // Correctly matched
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cards[cardSelected].SetMatched(true);
                cards[cardId].SetMatched(true);
                cardLeft -= 2;
                CheckGameWin();
            }
            else
            {
                // Incorrectly matched
                cards[cardSelected].Flip();
                cards[cardId].Flip();
            }
            cardSelected = spriteSelected = -1;
        }
    }

    // Check if game is completed
    private void CheckGameWin()
    {
        // Win game
        if (cardLeft == 0)
        {
            EndGame();
            AudioPlayer.Instance.PlayAudio(1);
        }
    }

    // Stop game
    private void EndGame()
    {
        gameStart = false;
        panel.SetActive(false);
         info.SetActive(true);
    }

    public void GiveUp()
    {
        EndGame();
    }

    public void DisplayInfo(bool i)
    {
        info.SetActive(i);
    }

    // Track elapsed time
    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time.ToString("F2") + "s";
        }
    }

    // Save the game state
    public void SaveGame()
    {
        GameState gameState = new GameState();
        gameState.timeElapsed = time;
        gameState.gameSize = gameSize;
        int totalCards = cards.Length;
        gameState.cardSpriteIDs = new int[totalCards];
        gameState.cardIsMatched = new bool[totalCards];
        gameState.cardIsFlipped = new bool[totalCards];

        for (int i = 0; i < totalCards; i++)
        {
            gameState.cardSpriteIDs[i] = cards[i].SpriteID;
            gameState.cardIsMatched[i] = cards[i].IsMatched;
            gameState.cardIsFlipped[i] = cards[i].IsFlipped;
        }

        string json = JsonUtility.ToJson(gameState);
        PlayerPrefs.SetString("SavedGame", json);
        PlayerPrefs.Save();
    }

    // Load the game state
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            string json = PlayerPrefs.GetString("SavedGame");
            GameState gameState = JsonUtility.FromJson<GameState>(json);

            // Restore game variables
            time = gameState.timeElapsed;
            gameSize = gameState.gameSize;
            SetGamePanel(); // Reinitialize the game panel with the saved gameSize

            int totalCards = gameState.cardSpriteIDs.Length;
            cardLeft = totalCards;

            for (int i = 0; i < totalCards; i++)
            {
                cards[i].SpriteID = gameState.cardSpriteIDs[i];
                cards[i].Active();

                // Restore card state
                if (gameState.cardIsMatched[i])
                {
                    cards[i].SetMatched(true);
                    cards[i].Inactive();
                    cardLeft -= 1;
                }
                else if (gameState.cardIsFlipped[i])
                {
                    cards[i].FlipImmediate();
                }
                else
                {
                    cards[i].ResetRotation();
                }
            }

            gameStart = true;
            info.SetActive(false);
            panel.SetActive(true);
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }

    // UI Button methods
    public void OnSaveButtonClicked()
    {
        SaveGame();
    }

    public void OnLoadButtonClicked()
    {
        LoadGame();
    }
}
