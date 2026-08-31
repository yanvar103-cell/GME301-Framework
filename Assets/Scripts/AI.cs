using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    private List<Transform> _wayPoints;
    private NavMeshAgent _agent;
    private int _currentPoint = 0;

    void Start()
    {
        _wayPoints = new List<Transform>();
        _wayPoints.Add(GameObject.FindGameObjectWithTag("StartPoint").transform);
        _wayPoints.Add(GameObject.FindGameObjectWithTag("EndPoint").transform);

        _agent = GetComponent<NavMeshAgent>();
        if(_agent != null)
        {
            _agent.destination = _wayPoints[_currentPoint].position;
            Debug.Log("Distance to StartPoint: " + _agent.remainingDistance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_agent.remainingDistance <= 0.1)
        {
            _currentPoint = 1;
            _agent.SetDestination(_wayPoints[_currentPoint].position);
            Debug.Log("Distance to EndPoint: " + _agent.remainingDistance);
        }
        
    }
}
