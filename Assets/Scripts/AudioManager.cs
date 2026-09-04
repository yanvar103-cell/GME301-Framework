using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource; // looping background music
    [SerializeField] private AudioSource _sfxSource;    // one-shot sound effects

    [Header("Clips")]
    [SerializeField] private AudioClip _backgroundMusic;
    [SerializeField] private AudioClip _weaponFireClip;
    [SerializeField] private AudioClip _aiDeathClip;
    [SerializeField] private AudioClip _barrierHitClip;
    [SerializeField] private AudioClip _wallHitClip;
    [SerializeField] private AudioClip _aiCompletedTrackClip;

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (_musicSource == null || _backgroundMusic == null) return;
        _musicSource.clip = _backgroundMusic;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource == null) return;
        _musicSource.Stop();
    }

    public void PlayWeaponFire() => PlaySFX(_weaponFireClip);
    public void PlayAIDeath() => PlaySFX(_aiDeathClip);
    public void PlayAICompletedTrack() => PlaySFX(_aiCompletedTrackClip);
    
    // generic entry point - lets any component (like HitResponder) play its own assigned clip without AudioManager needing a dedicated method per object type
    public void PlaySFXClip(AudioClip _clip) => PlaySFX(_clip);

    private void PlaySFX(AudioClip _clip)
    {
        if (_clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(_clip); //allows overlapping SFX without cutting each other off
    }
}