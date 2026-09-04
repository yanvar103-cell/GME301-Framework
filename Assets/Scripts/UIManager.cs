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

    [Header("Timer Settings")]
    [SerializeField] private float _matchDuration = 120f; //seconds, adjust to round length
    private float _timeRemaining;
    private bool _timerRunning = false;

    private void Start()
    {
        _timeRemaining = _matchDuration;
        UpdateTimeRemainingDisplay();
        _timerRunning = true; // set false here and call StartTimer() elsewhere if you want manual control
    }

    private void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _timerRunning = false;
            //game-over logic here
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
}