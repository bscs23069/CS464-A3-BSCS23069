using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Numbers (the same on the prefab in every level)")]
    [SerializeField] int targetScore = 5;          // coins needed before the goal counts
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;    // a beat before coming back
    [SerializeField] float respawnProtection = 1.5f;

    [Header("Set in each level")]
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text heartsText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text messageText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    int score;
    bool over;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        messageText.text = "";
        UpdateScore();
        UpdateHearts();
        UpdateLives();
    }

    public void AddScore(int amount)
    {
        if (over)
            return;

        score += amount;
        UpdateScore();
        if (score == targetScore)
            ShowMessage("The goal is open!");
    }

    public void ReachedGoal()
    {
        if (over)
            return;

        if (score >= targetScore)
            Win();
        else
            ShowMessage($"Collect {targetScore - score} more coins first");
    }

    public void PlayerDied()
    {
        if (over)
            return;

        lives--;
        UpdateLives();

        if (lives <= 0)
            Lose();
        else
            StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        // wait a beat: a knockback queued for the next step must not land on the respawned player
        yield return new WaitForSeconds(respawnDelay);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.position = respawnPoint.position;
        player.transform.position = respawnPoint.position;
        rb.linearVelocity = Vector3.zero;
        player.GetComponent<PlayerController>().ResetState();
        player.Restore(respawnProtection);
    }

    public void SetCheckpoint(Transform point) => respawnPoint = point;

    void Win()
    {
        over = true;
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Lose()
    {
        over = true;
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        // timeScale survives a scene load, so put it back first
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : 0);
    }

    // each text is set only when its number changes
    void UpdateScore() => scoreText.text = $"Coins: {score}/{targetScore}";
    void UpdateLives() => livesText.text = $"Lives: {Mathf.Max(lives, 0)}";
    public void UpdateHearts() => heartsText.text = $"Hearts: {Mathf.Max(player.Current, 0)}/{player.Max}";

    void ShowMessage(string text)
    {
        messageText.text = text;
        StartCoroutine(ClearMessage(text));
    }

    IEnumerator ClearMessage(string text)
    {
        yield return new WaitForSeconds(2f);
        if (messageText.text == text)
            messageText.text = "";
    }
}
