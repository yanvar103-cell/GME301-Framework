using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask _maskShootable;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray _rayOrigin = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit _hitInfo;

            if (Physics.Raycast(_rayOrigin, out _hitInfo, Mathf.Infinity, _maskShootable))
            {
                AI _agent = _hitInfo.collider.GetComponent<AI>();
                if (_agent != null)
                {
                    _agent.Death();
                    Debug.Log("You hit the agent!");
                }
            }
        }
    }
}