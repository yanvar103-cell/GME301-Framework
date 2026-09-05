using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    private Transform _startPoint;
    private Transform _endPoint;
    private NavMeshAgent _agent;
    private Animator _anim;

    private enum AIStates
    {
        Run,
        Hide,
        Death
    }
    [SerializeField] private AIStates _currentState;

    private List<Transform> _barriers;
    [SerializeField] private Transform _currentBarrier;
    [SerializeField] private int _hideLimit = 5;
    private int _hideCount = 0;

    [SerializeField] private float _minHideTime = 3f;
    [SerializeField] private float _maxHideTime = 6f;
    [SerializeField] private int _pointsAward = 50;
    private bool _isDead = false;

    private static HashSet<Transform> _occupiedBarriers = new HashSet<Transform>();

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _barriers = SpawnManager.Instance.GetBarriers();
        _startPoint = GameObject.FindGameObjectWithTag("StartPoint").transform;
        _endPoint = GameObject.FindGameObjectWithTag("EndPoint").transform;
    }

    private void OnEnable()
    {
        /*if (_endPoint == null) return; //for safety because after awake _endPoint is still null(Start() hasn't run yet!) 

        _currentState = AIStates.Run;
        _agent.isStopped = false;
        PickNewBarrier();
        Debug.Log($"[{GetInstanceID()}] OnEnable → PickNewBarrier: _currentBarrier = {(_currentBarrier != null ? _currentBarrier.name : "NULL")}, distance to it: {(_currentBarrier != null ? Vector3.Distance(transform.position, _currentBarrier.position).ToString() : "N/A")}");
        */
    }

    public void Spawn()
    {
        if (_endPoint == null) return;

        _isDead = false;
        _hideCount = 0;
        _currentBarrier = null;
        _currentState = AIStates.Run;
        _agent.isStopped = false;
        PickNewBarrier();
    }
    
    void Update()// Update is called once per frame
    {
        if (_isDead) return; // stop all logic once dead

        if (_anim != null)
            _anim.SetFloat("Speed", _agent.velocity.magnitude);

        switch (_currentState)
        {
            case AIStates.Run:
                //Debug.Log("Running");
                RunState();
                break;
            case AIStates.Hide:
                //Debug.Log("Hiding");
                break;
            case AIStates.Death:
                //Debug.Log("Dying");
                break;
        }
    }

    private void RunState()
    {
        if (_agent.hasPath && !_agent.pathPending && _agent.remainingDistance <= 0.1f)
        {
            if (_currentBarrier != null)
            {
                _currentState = AIStates.Hide;
                _anim.SetBool("Hiding", true);
                StartCoroutine(HideRoutine());
            }
            else ReachedEndPoint();
        }
    }

    private void ReachedEndPoint()
    {
        Debug.Log($"[{GetInstanceID()}] Reached End Point! Position: {transform.position}, EndPoint: {_endPoint.position}, Distance: {Vector3.Distance(transform.position, _endPoint.position)}, HideCount was: {_hideCount}");
        AudioManager.Instance.PlayAICompletedTrack();
        Invoke(nameof(ReturnToPool), 0f);//return to pool immediately without delay
    }

    IEnumerator HideRoutine()
    {
        float _hideTime = Random.Range(_minHideTime, _maxHideTime);
        yield return new WaitForSeconds(_hideTime);
        if (_isDead) yield break;//don't wait if dead
        _anim.SetBool("Hiding", false);
        _currentState = AIStates.Run;
        PickNewBarrier();
    }

    private void PickNewBarrier()
    {
        ReleaseCurrentBarrier(); //free the one we're leaving before claiming a new one

        List<Transform> _validBarriers = GetBarriersAhead();

        if (_hideCount >= _hideLimit || _validBarriers.Count == 0) // no barriers left, just head to end point
        {
            //Debug.Log($"[{GetInstanceID()}] No valid barriers ahead (found {_validBarriers.Count})! Heading straight to EndPoint. HideCount: {_hideCount}");
            _currentBarrier = null;
            _agent.SetDestination(_endPoint.position);
            return;
        }
        int _randomIndex = Random.Range(0, _validBarriers.Count);
        _currentBarrier = _validBarriers[_randomIndex];
        _occupiedBarriers.Add(_currentBarrier); //claim it
        _agent.SetDestination(_currentBarrier.position);
        _hideCount++;
        //Debug.Log($"[{GetInstanceID()}] Picked barrier: {_currentBarrier.name} (out of {_validBarriers.Count} valid options)");
    }

    public void Death()
    {
        if (_isDead) return;
        _isDead = true;
        _currentState = AIStates.Death;
        _agent.isStopped = true;
        ReleaseCurrentBarrier(); //free it up for other agents immediately on death

        if (_anim != null) _anim.SetTrigger("Death");
        AudioManager.Instance.PlayAIDeath();
        GameManager.Instance.RegisterKill();
        //add score in ScoreManager
        ScoreManager.Instance.AddScore(_pointsAward);

        //Destroy(gameObject, 2f);//give 2 sec to finish death animation
        Invoke(nameof(ReturnToPool), 2f); //2sec to death anim, then deactivate instead of destroying
    }

    void ReturnToPool()
    {
        _agent.isStopped = true;
        _agent.ResetPath(); //clear stale path/remainingDistance before it goes back in the pool
        ReleaseCurrentBarrier(); //safety net in case Death() path wasn't taken (reached end)
        gameObject.SetActive(false); //returns to pool, ready to be reused
        //reset states for next use
        _isDead = false;
        _currentState = AIStates.Run;
        _hideCount = 0;
        _currentBarrier = null;
    }

    private List<Transform> GetBarriersAhead()
    {
        List<Transform> _ahead = new List<Transform>();
        float _distanceToEnd = GetPathDistance(transform.position, _endPoint.position);
        foreach (Transform _barrier in _barriers)
        {
            Barrier _barrierComp = _barrier.GetComponent<Barrier>();
            if (_barrierComp != null && !_barrierComp.IsUsable) continue; // skip destroyed/recharging barriers

            float _barrierDistanceToEnd = GetPathDistance(_barrier.position, _endPoint.position);
            if (_barrierDistanceToEnd < _distanceToEnd && !_occupiedBarriers.Contains(_barrier))
                _ahead.Add(_barrier);
        }
        return _ahead;
    }

    private float GetPathDistance(Vector3 _from, Vector4 _to)
    {
        NavMeshPath _path = new NavMeshPath();
        NavMesh.CalculatePath(_from, _to, NavMesh.AllAreas, _path);

        float _distance = 0f;
        for (int i = 0; i < _path.corners.Length - 1; i++)
        {
            _distance += Vector3.Distance(_path.corners[i], _path.corners[i + 1]);
        }
        return _distance;
    }

    private void ReleaseCurrentBarrier()
    {
        if (_currentBarrier != null) _occupiedBarriers.Remove(_currentBarrier);
    }

    public static void ClearOccupiedBarriers()//after reloading scenes we must cleanUp this static hashset from spawnManager
    {
        _occupiedBarriers.Clear();
    }

}