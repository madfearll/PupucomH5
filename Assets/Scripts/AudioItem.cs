using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioItem : MonoBehaviour, IPoolable
{
    private AudioSource m_source;
    private void Awake()
    {
        m_source = GetComponent<AudioSource>();
    }

    public void Play(string sfx, float volume)
    {
        m_source.clip = Resources.Load<AudioClip>(sfx);
        m_source.time = 0;
        m_source.volume = volume;
        m_source.Play();
    }
    
    public void OnSpawned()
    {
    }

    public void OnDespawned()
    {
    }
}
