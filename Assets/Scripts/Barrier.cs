using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    [Header("Cooldown / Recharge")]
    [SerializeField] private float _rechargeCooldown = 5f; // seconds after being destroyed before it's usable again
    private bool _isDestroyed = false;

    [Header("Audio")]
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _destroyedSound;
    [SerializeField] private AudioClip _chargingSound; // one-shot, plays once as the recharge cooldown begins

    [Header("Visuals (optional)")]
    [SerializeField] private MeshRenderer _visualModel; // disabled while destroyed, re-enabled on recharge
    [SerializeField] private Collider _barrierCollider; // disabled while destroyed so it can't be shot or hidden-behind

    // AI.cs checks this before letting a duck pick this barrier as a hiding spot
    public bool IsUsable => !_isDestroyed;

    private void Awake()
    {
        _currentHealth = _maxHealth;

        if (_barrierCollider == null) _barrierCollider = GetComponent<Collider>();
        if (_visualModel == null) _visualModel = GetComponent<MeshRenderer>();
    }

    public void TakeDamage(int _amount = 1)
    {
        if (_isDestroyed) return; // already down, ignore hits until it recharges

        _currentHealth -= _amount;

        if (_hitSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXClip(_hitSound);

        if (_currentHealth <= 0)
        {
            DestroyBarrier();
        }
    }

    private void DestroyBarrier()
    {
        _isDestroyed = true;
        _currentHealth = 0;

        if (_destroyedSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXClip(_destroyedSound);

        if (_visualModel != null) _visualModel.enabled = false;
        if (_barrierCollider != null) _barrierCollider.enabled = false;

        if (_chargingSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXClip(_chargingSound);

        StartCoroutine(RechargeRoutine());
    }

    private IEnumerator RechargeRoutine()
    {
        yield return new WaitForSeconds(_rechargeCooldown);
        Recharged();
    }

    private void Recharged()
    {
        _isDestroyed = false;
        _currentHealth = _maxHealth;

        if (_visualModel != null) _visualModel.enabled = true;
        if (_barrierCollider != null) _barrierCollider.enabled = true;
    }
}