using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask _maskShootable;

    [Header("Ammo")]
    [SerializeField] private int _maxAmmo = 50;
    private int _currentAmmo;

    private void Start()
    {
        Cursor.visible = false;//just hides the pointer graphic
        //Cursor.lockState = CursorLockMode.Locked;//cursor is hidden, snapped to the center of the screen
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }
        if (Camera.main == null)
        {
            Debug.LogError("Player: No camera tagged 'MainCamera' found in the scene!");
            return;
        }
        _currentAmmo--;
        UIManager.Instance.UpdateAmmo(_currentAmmo);

        Ray _rayOrigin = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit _hitInfo;

        if (Physics.Raycast(_rayOrigin, out _hitInfo, Mathf.Infinity, _maskShootable))
        {
            Debug.Log($"Raycast hit: {_hitInfo.collider.name}");
            AI _agent = _hitInfo.collider.GetComponentInParent<AI>();
            if (_agent != null)
            {
                _agent.Death();
                Debug.Log("You hit the agent!");
            }
        }

    }
}