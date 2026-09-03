using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }//to make this class as singleton
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); //prevent duplicates for example from previous scene load
            return;
        }
        Instance = this;
    }

    [SerializeField] private int _poolSize = 10;
    private List<GameObject> _pool = new List<GameObject>();
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
        AI.ClearOccupiedBarriers(); //reset shared state before anything spawns

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject _agent = Instantiate(_agentPrefab);
            _agent.SetActive(false); //start inactive, sitting in the pool
            _pool.Add(_agent);
        }
        
        InvokeRepeating(nameof(SpawnAgent), 0f, _spawnInterval);
    }

    private void SpawnAgent()
    {
        //Instantiate(_agentPrefab, _startPoint.transform.position, Quaternion.identity);
        GameObject _agentObj = GetAgentFromPool();
        if(_agentObj != null)
        {
            _agentObj.SetActive(true); //activate existing inactive agent instead of Instantiate new
            NavMeshAgent _agent = _agentObj.GetComponent<NavMeshAgent>();
            _agent.Warp(_startPoint.position);//place it to start, warp properly syncs agent to the NavMesh
            _agentObj.GetComponent<AI>().Spawn();
        }
    }

    GameObject GetAgentFromPool()
    {
        foreach(GameObject _agent in _pool)
        {
            if (_agent.activeInHierarchy == false)
                return _agent; //found an inactive agent, reuse it
        }
        return null; //pool exhausted, all agents currently active
    }
}
