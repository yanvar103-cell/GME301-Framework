using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // AI is handled separately below since hitting it triggers gameplay logic (Death, scoring, Explosion), not just a sound.
    [SerializeField] private LayerMask _layerAI;
    [SerializeField] private LayerMask _layerExplosive;

    [Header("Ammo")]
    [SerializeField] private int _maxAmmo = 50;
    private int _currentAmmo;
    //Everything else (Barrier, Wall, and any future surface type) is just a layer + a sound
    //Add new entries here in the Inspector -- no code changes needed to support a new layer

    [System.Serializable]
    private class LayerImpactSound
    {
        public string label;// Inspector-only readability, e.g. "Barrier", "Wall", "Glass"
        public LayerMask layer;
        public AudioClip impactSound;
    }

    [Header("Other Shootable Layers")]
    [SerializeField] private List<LayerImpactSound> _impactSounds = new List<LayerImpactSound>();

    private void Start()
    {
        Cursor.visible = false;//just hides the pointer graphic
        //Cursor.lockState = CursorLockMode.Locked;//cursor is hidden, snapped to the center of the screen
        _currentAmmo = _maxAmmo;
        UIManager.Instance.UpdateAmmo(_currentAmmo);
    }

    void Update()
    {
        if (!GameManager.Instance.IsRoundActive) return;

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
        AudioManager.Instance.PlayWeaponFire(); //fires on every shot, hit or miss

        Ray _rayOrigin = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit _hitInfo;

        if (Physics.Raycast(_rayOrigin, out _hitInfo, Mathf.Infinity, GetCombinedMask()))
        {
            HandleHit(_hitInfo);
        }
    }

    private LayerMask GetCombinedMask()
    {
        LayerMask _combined = _layerAI | _layerExplosive;
        foreach (LayerImpactSound _entry in _impactSounds)
        {
            _combined |= _entry.layer;
        }
        return _combined;
    }

    private void HandleHit(RaycastHit _hitInfo)
    {
        int _hitLayer = _hitInfo.collider.gameObject.layer;

        if (IsInLayerMask(_hitLayer, _layerAI))
        {
            AI _agent = _hitInfo.collider.GetComponent<AI>();
            if (_agent != null)
            {
                _agent.Death(); // AI.cs plays its own death sound internally
                Debug.Log("You hit the agent!");
            }
            return;
        }

        if (IsInLayerMask(_hitLayer, _layerExplosive))
        {
            ExplosiveBarrel _barrel = _hitInfo.collider.GetComponent<ExplosiveBarrel>();
            if (_barrel != null) _barrel.Explode();
            Debug.Log("You hit the barrel!");
            return;
        }

        // check every configured layer/sound entry -- first match wins
        foreach (LayerImpactSound _entry in _impactSounds)
        {
            if (IsInLayerMask(_hitLayer, _entry.layer))
            {
                AudioManager.Instance.PlaySFXClip(_entry.impactSound);
                return;
            }
        }
    }

    private bool IsInLayerMask(int _layer, LayerMask _mask)
    {
        return (_mask.value & (1 << _layer)) != 0;
    }
}