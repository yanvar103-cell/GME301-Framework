using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Round Settings")]
    [SerializeField, Range(0f, 100f)] private float _winThresholdPercent = 75f;//show it with slider

    [Header("Game Over UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _resultTitleText;   //"You Win!" / "You Lose!"
    [SerializeField] private TextMeshProUGUI _resultStatsText;   //"Enemies Killed: 15/20 (75%)"
    [SerializeField] private Button _restartButton;

    private int _totalSpawned = 0;
    private int _totalKilled = 0;
    private bool _roundEnded = false;

    public bool IsRoundActive { get; private set; } = true;

    private void Start()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_restartButton != null) _restartButton.onClick.AddListener(RestartGame);
    }

    public void RegisterSpawn()
    {
        _totalSpawned++;
    }

    public void RegisterKill()
    {
        _totalKilled++;
    }

    //called by UIManager when the Time Remaining countdown hits zero
    public void EndRound()
    {
        if (_roundEnded) return;
        _roundEnded = true;
        IsRoundActive = false;

        //main Win/Lose condition logic
        float _percentKilled = _totalSpawned > 0 ? (_totalKilled / (float)_totalSpawned) * 100f : 100f; //if _totalSpawned > 0 true-calc the percentage ((_totalKilled / (float)_totalSpawned) * 100f). if false-return 100f(to avoid 0/0 error)
        bool _won = _percentKilled >= _winThresholdPercent; //if _percentKilled >= _winThresholdPercent is true _won is true;

        ShowGameOver(_won, _percentKilled);

        if (SpawnManager.Instance != null) SpawnManager.Instance.StopSpawning();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowGameOver(bool _won, float _percentKilled)
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        if (_resultTitleText != null) _resultTitleText.text = _won ? "You Win!" : "You Lose!";
        if (_resultStatsText != null)
            _resultStatsText.text = $"Enemies Killed: {_totalKilled}/{_totalSpawned} ({_percentKilled:0}%)";
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}