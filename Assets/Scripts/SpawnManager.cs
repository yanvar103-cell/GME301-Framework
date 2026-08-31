using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _agent;
    private Transform _startPoint;
    private Transform _endPoint;
    void Start()
    {
        _startPoint = GameObject.FindGameObjectWithTag("StartPoint").transform;
        _endPoint = GameObject.FindGameObjectWithTag("EndPoint").transform;
        Instantiate(_agent, _startPoint.transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
