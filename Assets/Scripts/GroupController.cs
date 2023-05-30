using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupController : MonoBehaviour
{
    private Vector3 m_inputDownDir;
    private float m_inputDownAngle;
    private BubbleGroup m_group;
    private float m_inputDownPosition;

    private void Awake()
    {
        m_group = GetComponent<BubbleGroup>();
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameCtrl.Inst.GameEnd) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            m_inputDownDir = _GetMouseDirection(Input.mousePosition);
            m_inputDownAngle = m_group.transform.eulerAngles.z;
            m_inputDownPosition = Input.mousePosition.x;
        }

        if (Input.GetMouseButton(0))
        {
            var deltaAngle = 0f;
            if (GameCtrl.Inst.Settings.inputType == EInputType.Rotate)
            {
                deltaAngle = _GetDeltaAngleRotate();
            }
            else
            {
                deltaAngle = _GetDeltaAngleSlide();
            }
            m_group.transform.eulerAngles = new Vector3(0, 0, m_inputDownAngle + deltaAngle);
        }
        
    }

    private Vector3 _GetMouseDirection(Vector3 mousePos)
    {
        var screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        return mousePos - screenCenter;
    }

    private float _GetDeltaAngleSlide()
    {
        return (Input.mousePosition.x - m_inputDownPosition) / Screen.width * 720f;
    }

    private float _GetDeltaAngleRotate()
    {
        var screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        var inputDir = Input.mousePosition - screenCenter;
        var deltaAngle = Vector2.SignedAngle(m_inputDownDir, inputDir);
        return deltaAngle;
    }
}
