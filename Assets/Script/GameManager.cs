using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject cardPrefab;
    public Transform cardGrid;
    public int rows = 2;
    public int columns = 2;

    public List<Sprite> cardFaceSprites; // قائمة صور وجوه البطاقات
    public Sprite cardBackSprite; // صورة خلفية البطاقة

    private List<Card> flippedCards = new List<Card>();
    private int matchesFound = 0;
    private int totalMatches;
    private int score = 0;

    public Text scoreText;

    void Awake()
    {
        // تنفيذ نمط Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadGame();
        totalMatches = (rows * columns) / 2;
        GenerateCards();
        UpdateScoreUI();
    }

    void GenerateCards()
    {
        if (cardFaceSprites.Count < totalMatches)
        {
            Debug.LogError("عدد Sprites وجوه البطاقات غير كافٍ لعدد البطاقات المطلوبة.");
            return;
        }

        // إنشاء قائمة بـ card IDs
        List<int> cardIDs = new List<int>();
        for (int i = 0; i < totalMatches; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        // خلط الـ card IDs
        Shuffle(cardIDs);

        // إنشاء البطاقات
        for (int i = 0; i < cardIDs.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardGrid);
            Card card = cardObj.GetComponent<Card>();
            card.cardID = cardIDs[i];
            card.cardFace = cardFaceSprites[cardIDs[i]];
            card.cardBack = cardBackSprite;
            card.ShowCardBack();
        }

        // ضبط إعدادات GridLayoutGroup
        AdjustGridLayout();
    }

    void AdjustGridLayout()
    {
        var gridLayout = cardGrid.GetComponent<GridLayoutGroup>();

        float gridWidth = cardGrid.GetComponent<RectTransform>().rect.width;
        float gridHeight = cardGrid.GetComponent<RectTransform>().rect.height;

        float cellWidth = gridWidth / columns;
        float cellHeight = gridHeight / rows;

        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void AddFlippedCard(Card card)
    {
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (flippedCards[0].cardID == flippedCards[1].cardID)
        {
            // تطابق
            score += 10;
            matchesFound++;
            UpdateScoreUI();
           

            // تعطيل البطاقات المتطابقة
            flippedCards[0].GetComponent<Button>().interactable = false;
            flippedCards[1].GetComponent<Button>().interactable = false;
        }
        else
        {
            // عدم تطابق
            flippedCards[0].HideCard();
            flippedCards[1].HideCard();
            
        }

        flippedCards.Clear();

        // تحقق من انتهاء اللعبة
        if (matchesFound >= totalMatches)
        {
            GameOver();
        }
    }

    void GameOver()
    {
       
        Debug.Log("انتهت اللعبة! النتيجة النهائية: " + score);
        // يمكن إضافة منطق لإعادة البدء أو الانتقال إلى المستوى التالي
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    public void SaveGame()
    {
        // تنفيذ منطق الحفظ
    }

    public void LoadGame()
    {
        // تنفيذ منطق التحميل
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }
}
