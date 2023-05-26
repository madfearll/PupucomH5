using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SimpleAnimation : MonoBehaviour
{
    [SerializeField] [Min(1)] private int _fps = 10;
    [SerializeField] private List<Sprite> _clips;
    [SerializeField] private bool _loop = true;

    private int m_index = 0;
    private Image m_image;
    private SpriteRenderer m_sprite;
    private float m_timer;

    void Awake()
    {
        if (_clips == null || _clips.Count == 0)
        {
            Debug.LogWarning("clips empty:" + name);
            return;
        }

        m_image = GetComponent<Image>();
        m_sprite = GetComponent<SpriteRenderer>();
    }

    private void _ResetIndex()
    {
        if (!m_image && !m_sprite)
        {
            Debug.LogWarning("no sprite renderer or image attached:" + name);
            m_index = -1;
        }
        else
        {
            m_index = 0;
        }
    }

    private void Update()
    {
        if (m_index < 0) return;
        m_timer += Time.deltaTime;
        if (m_timer > 1f / _fps)
        {
            if (_loop)
            {
                m_index = (m_index + 1) % _clips.Count;
            }
            else
            {
                m_index++;
                if (m_index >= _clips.Count)
                {
                    m_index = -1;
                    return;
                }
            }

            if (m_image) m_image.sprite = _clips[m_index];
            if (m_sprite) m_sprite.sprite = _clips[m_index];
            m_timer = 0;
        }
    }

    private void OnEnable()
    {
        _ResetIndex();
    }
}
