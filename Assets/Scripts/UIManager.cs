using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI _scoreValueText;
    [SerializeField] private TextMeshProUGUI _aiCountValueText;
    [SerializeField] private TextMeshProUGUI _timeValueText;
    [SerializeField] private TextMeshProUGUI _ammoValueText;

    [Header("Instructions")]
    [SerializeField] private TextMeshProUGUI _instructionsText;
    [SerializeField] private float _winConditionDisplayDuration = 5f;

    [Header("Timer Settings")]
    [SerializeField] private float _matchDuration = 30f; //seconds, adjust to round length
    private float _timeRemaining;
    private bool _timerRunning = false;

    private void Start()
    {
        _timeRemaining = _matchDuration;
        UpdateTimeRemainingDisplay();
        _timerRunning = true; // set false here and call StartTimer() elsewhere if you want manual control

        ShowStartInstructions();
    }

    private void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
        {
            //game-over logic here
            _timeRemaining = 0f;
            _timerRunning = false;
            GameManager.Instance.EndRound();
        }
        UpdateTimeRemainingDisplay();
    }

    public void StartTimer()
    {
        _timerRunning = true;
    }

    public void StopTimer()
    {
        _timerRunning = false;
    }

    public void UpdateScore(int _newScore)
    {
        if (_scoreValueText != null)
            _scoreValueText.text = _newScore.ToString();
    }

    public void UpdateAICount(int _count)
    {
        if (_aiCountValueText != null)
            _aiCountValueText.text = _count.ToString();
    }

    public void UpdateAmmo(int _currentAmmo)
    {
        if (_ammoValueText != null)
            _ammoValueText.text = _currentAmmo.ToString();
    }

    private void UpdateTimeRemainingDisplay()
    {
        if (_timeValueText == null) return;

        int _minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        int _seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        _timeValueText.text = $"{_minutes:00}:{_seconds:00}";
    }

    private void ShowStartInstructions()
    {
        if (_instructionsText == null) return;

        _instructionsText.text = $"Kill at least {GameManager.Instance.GetWinThresholdPercent()} of enemies before time runs out to win!";
        _instructionsText.gameObject.SetActive(true);
        StartCoroutine(HideInstructionsAfterDelay(_winConditionDisplayDuration));
    }

    private IEnumerator HideInstructionsAfterDelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        _instructionsText.gameObject.SetActive(false);
    }

    // called by Player.cs whenever ammo hits 0
    public void ShowReloadPrompt()
    {
        if (_instructionsText == null) return;

        StopAllCoroutines(); // cancel any pending start-instructions hide, so it can't override this
        _instructionsText.text = "Press R to Reload";
        _instructionsText.gameObject.SetActive(true);
    }

    // called by Player.cs once reload finishes
    public void HideReloadPrompt()
    {
        if (_instructionsText == null) return;

        _instructionsText.gameObject.SetActive(false);
    }
}