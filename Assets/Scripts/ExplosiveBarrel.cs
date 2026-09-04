using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float _explosionRadius = 5f;
    [SerializeField] private LayerMask _aiLayer;
    [SerializeField] private LayerMask _barrelLayer; // used to detect neighboring barrels for chain reactions

    [Header("Chain Reaction")]
    [SerializeField] private float _chainDelay = 0.15f; // slight stagger so chained blasts read as a chain, not one instant flash

    [Header("Effects")]
    [SerializeField] private GameObject _explosionVFXPrefab;
    [SerializeField] private AudioClip _explosionSFX;

    private bool _hasExploded = false;

    public void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        //instant-kill any AI caught in the blast radius
        Collider[] _aiHits = Physics.OverlapSphere(transform.position, _explosionRadius, _aiLayer);
        foreach (Collider _col in _aiHits)
        {
            AI _agent = _col.GetComponent<AI>();
            if (_agent != null) _agent.Death();
        }

        // chain-react any other barrels caught in the blast radius
        Collider[] _barrelHits = Physics.OverlapSphere(transform.position, _explosionRadius, _barrelLayer);
        foreach (Collider _col in _barrelHits)
        {
            if (_col.gameObject == gameObject) continue; // skip self

            ExplosiveBarrel _otherBarrel = _col.GetComponent<ExplosiveBarrel>();
            if (_otherBarrel != null && !_otherBarrel._hasExploded)
            {
                _otherBarrel.Invoke(nameof(Explode), _chainDelay);
            }
        }

        if (_explosionVFXPrefab != null)
            Instantiate(_explosionVFXPrefab, transform.position, Quaternion.identity);

        if (_explosionSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXClip(_explosionSFX);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
