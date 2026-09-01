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
    private Transform _currentBarrier;

    [SerializeField] private float _minHideTime = 1f;
    [SerializeField] private float _maxHideTime = 3f;
    [SerializeField] private int _pointsAward = 50;
    private bool _isDead = false;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _barriers = SpawnManager.Instance.GetBarriers();

        _startPoint = GameObject.FindGameObjectWithTag("StartPoint").transform;
        _endPoint = GameObject.FindGameObjectWithTag("EndPoint").transform;

        _currentState = AIStates.Run;

        
        if(_agent != null)
        {
            _agent.destination = _startPoint.position;
            //Debug.Log("Distance to StartPoint: " + _agent.remainingDistance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDead) return; // stop all logic once dead

        switch (_currentState)
        {
            case AIStates.Run:
                Debug.Log("Running");
                RunState();
                break;
            case AIStates.Hide:
                Debug.Log("Hiding");
                break;
            case AIStates.Death:
                Debug.Log("Dying");
                break;
        }
        
    }

    private void RunState()
    {
        if (_agent.remainingDistance <= 0.1)
        {
            if(_currentBarrier != null)
            {
                _currentState = AIStates.Hide;
                StartCoroutine(HideRoutine());
            }
            _agent.SetDestination(_endPoint.position);
            Debug.Log("Distance to EndPoint: " + _agent.remainingDistance);
        }
    }

    IEnumerator HideRoutine()
    {
        float _hideTime = Random.Range(_minHideTime, _maxHideTime);
        yield return new WaitForSeconds(_hideTime);
        if (_isDead) yield break;//don't wait if dead
        _currentState = AIStates.Run;
        PickNewBarrier();
    }

    private void PickNewBarrier()
    {
        if(_barriers.Count == 0) // no barriers left, just head to end point
        {
            _currentBarrier = null;
            _agent.SetDestination(_endPoint.position);
        }
        int _randomIndex = Random.Range(0, _barriers.Count);
        _agent.SetDestination(_barriers[_randomIndex].position);
    }

    public void Death()
    {
        if (_isDead) return;
        _isDead = true;
        _currentState = AIStates.Death;
        _agent.isStopped = true;

        if (_anim != null) _anim.SetTrigger("Death");
        //add score in ScoreManager
        ScoreManager.Instance.AddScore(_pointsAward);

        Destroy(gameObject, 2f);//give 2 sec to finish death animation
    }
}
