using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string targetWord = "CAT";
    public float touchDistance = 1.1f;
    public float instructionsDisplayTime = 3f;

    public List<LetterBlock> blocks = new List<LetterBlock>();
    public SpriteRenderer exitRenderer;

    [Header("UI")]
    public Text instructionsText;
    public GameObject winLosePanel;
    public Text winLoseText;

    private bool wordFormed = false;
    private bool gameOver = false;
    private float instructionsTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (winLosePanel != null) winLosePanel.SetActive(false);
        if (instructionsText != null) instructionsText.text = "Spell " + targetWord + "!";
        instructionsTimer = instructionsDisplayTime;
    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }

        CheckWord();

        if (instructionsTimer > 0f)
        {
            instructionsTimer -= Time.deltaTime;
            if (instructionsTimer <= 0f && instructionsText != null)
            {
                instructionsText.gameObject.SetActive(false);
            }
        }
    }

    public void CheckWord()
    {
        if (wordFormed) return;
        if (blocks.Count != targetWord.Length) return;

        List<LetterBlock> ordered = blocks.OrderBy(b => b.transform.position.x).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            if (char.ToUpper(ordered[i].letter) != targetWord[i])
            {
                return;
            }
        }

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            float distance = Vector2.Distance(ordered[i].transform.position, ordered[i + 1].transform.position);
            if (distance > touchDistance)
            {
                return;
            }
        }

        wordFormed = true;
        if (exitRenderer != null) exitRenderer.color = Color.green;
    }

    public void Win()
    {
        if (gameOver || !wordFormed) return;
        ShowEndScreen("You Win!");
    }

    public void Lose(string reason)
    {
        if (gameOver) return;
        ShowEndScreen("You Lose!");
    }

    void ShowEndScreen(string message)
    {
        gameOver = true;
        Time.timeScale = 0f;

        if (winLosePanel != null) winLosePanel.SetActive(true);
        if (winLoseText != null) winLoseText.text = message;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
