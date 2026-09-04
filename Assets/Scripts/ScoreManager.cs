using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private int _totalScore = 0;
    private int _killCount = 0;

    
    void Start()
    {
        // push initial values so the HUD isn't blank before the first kill
        UIManager.Instance.UpdateScore(_totalScore);
        UIManager.Instance.UpdateAICount(_killCount);
    }

    public void AddScore(float _score)
    {
        _totalScore += Mathf.RoundToInt(_score);
        _killCount++; // every scored kill counts as one enemy defeated

        UIManager.Instance.UpdateScore(_totalScore);
        UIManager.Instance.UpdateAICount(_killCount);
    }

}
