using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }//to make this class as singleton
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates for example from previous scene load
            return;
        }
        Instance = this;
    }

    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private List<Transform> _barriers;
    

    public List<Transform> GetBarriers()
    {
        return _barriers;
    }    

    void Start()
    {
        InvokeRepeating(nameof(SpawnAgent), 0f, _spawnInterval);
    }

    private void SpawnAgent()
    {
        Instantiate(_agentPrefab, _startPoint.transform.position, Quaternion.identity);
    }
}
