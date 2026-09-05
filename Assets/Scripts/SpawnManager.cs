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

    [Header("Explosive Barrels")]
    [SerializeField] private GameObject _barrelPrefab;
    [SerializeField] private int _barrelCount = 5;
    [SerializeField] private float _minSpacingBetweenBarrels = 6f;

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
        SpawnBarrels();
    }

    private void SpawnBarrels()
    {
        if (_barrelPrefab == null) return;

        List<Vector3> _placedPositions = new List<Vector3>();
        NavMeshTriangulation _triangulation = NavMesh.CalculateTriangulation();
        if (_triangulation.indices.Length == 0)
        {
            Debug.LogWarning("SpawnManager: NavMesh triangulation is empty -- is there a baked NavMesh in the scene?");
            return;
        }

        for (int i = 0; i < _barrelCount; i++)
        {
            Vector3 _spawnPos = GetSpacedRandomNavMeshPoint(_triangulation, _placedPositions);
            _placedPositions.Add(_spawnPos);
            Instantiate(_barrelPrefab, _spawnPos, Quaternion.identity);
        }
    }

    //picks a uniformly random point across the ENTIRE navmesh surface
    //retrying if it lands too close to an already-placed barrel
    private Vector3 GetSpacedRandomNavMeshPoint(NavMeshTriangulation _triangulation, List<Vector3> _placedPositions)
    {
        const int _maxAttempts = 30;

        for (int _attempt = 0; _attempt < _maxAttempts; _attempt++)
        {
            Vector3 _candidate = GetRandomPointOnNavMesh(_triangulation);

            bool _tooClose = false;
            foreach (Vector3 _existing in _placedPositions)
            {
                if (Vector3.Distance(_candidate, _existing) < _minSpacingBetweenBarrels)
                {
                    _tooClose = true;
                    break;
                }
            }

            if (!_tooClose) return _candidate;
        }

        Debug.LogWarning("SpawnManager: Couldn't find a sufficiently spaced point after max attempts, placing anyway.");
        return GetRandomPointOnNavMesh(_triangulation);
    }

    // returns a uniformly random point on the navmesh, weighted by triangle area so
    // larger open areas aren't under-represented compared to small triangles
    private Vector3 GetRandomPointOnNavMesh(NavMeshTriangulation _triangulation)
    {
        Vector3[] _vertices = _triangulation.vertices;
        int[] _indices = _triangulation.indices;//a flat list of integers, where every group of 3 consecutive numbers describes one triangle, by pointing at three entries in vertices

        int _triangleCount = _indices.Length / 3;
        float[] _cumulativeAreas = new float[_triangleCount];
        float _totalArea = 0f;

        for (int t = 0; t < _triangleCount; t++)
        {
            Vector3 _a = _vertices[_indices[t * 3]]; //corner A of triangle, by * 3 jumping to the start of that triangle's 3-index block
            Vector3 _b = _vertices[_indices[t * 3 + 1]]; //corner B of triangle
            Vector3 _c = _vertices[_indices[t * 3 + 2]]; //corner C of triangle

            _totalArea += Vector3.Cross(_b - _a, _c - _a).magnitude * 0.5f; //two edge vectors of the triangle, magnitude(length) of cross product of two vectors gives you the area of the parallelogram(2*triangle area)
            _cumulativeAreas[t] = _totalArea; //keeps accumulating
        }
        //getting a random triangle, bigger triangles have more probability
        float _randomValue = Random.Range(0f, _totalArea);
        int _chosenTriangle = 0;
        for (int t = 0; t < _triangleCount; t++)
        {
            if (_randomValue <= _cumulativeAreas[t])
            {
                _chosenTriangle = t;
                break;
            }
        }
        //getting a random point in randomly picked triangle
        Vector3 _p1 = _vertices[_indices[_chosenTriangle * 3]];
        Vector3 _p2 = _vertices[_indices[_chosenTriangle * 3 + 1]];
        Vector3 _p3 = _vertices[_indices[_chosenTriangle * 3 + 2]];

        // random barycentric coordinates -> uniformly random point within the triangle
        float _r1 = Random.value; //Random.Range(0f, 1f) will be used to scale the p2-p1 vector
        float _r2 = Random.value; //Random.Range(0f, 1f)
        //this if helps to pick right half of parallelogram(required triangle)
        if (_r1 + _r2 > 1f)
        {
            _r1 = 1f - _r1;
            _r2 = 1f - _r2;
        }

        return _p1 + _r1 * (_p2 - _p1) + _r2 * (_p3 - _p1);// gives you a positions inside the half of parallelogram(triangle), _p1 + (scaled direction toward _p2) + (scaled direction toward _p3), position + direction = new position.
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

            GameManager.Instance.RegisterSpawn();
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

    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnAgent));
    }

}
