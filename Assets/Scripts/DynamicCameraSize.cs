using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class DynamicCameraSize : MonoBehaviour
{
    private CinemachineVirtualCamera m_vCamera;
    
    private void Awake()
    {
        m_vCamera = GetComponent<CinemachineVirtualCamera>();
        var originSize = m_vCamera.m_Lens.OrthographicSize;
        m_vCamera.m_Lens.OrthographicSize = ((float) Screen.height / Screen.width) / (1920f / 1080f) * originSize;
    }
}
