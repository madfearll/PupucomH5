using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupController : MonoBehaviour
{
    private Vector3 m_inputDownDir;
    private float m_inputDownAngle;
    private BubbleGroup m_group;
    private Vector2 m_inputDownPosition;
    private GameSettings m_settings;

    private void Awake()
    {
        m_group = GetComponent<BubbleGroup>();
    }

    private void Start()
    {
        m_settings = GameCtrl.Inst.Settings;
    }

    // Update is called once per frame
    private void Update()
    {
        if (GameCtrl.Inst.GameEnd) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            m_inputDownDir = _GetMouseDirection(Input.mousePosition);
            m_inputDownAngle = m_group.transform.eulerAngles.z;
            m_inputDownPosition = Input.mousePosition;

            if (GameCtrl.Inst.InputType == EInputType.Joystick)
            {
                GameCtrl.Inst.StickUI.SetActive(true);
                var stickUITrans = GameCtrl.Inst.StickUI.transform as RectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(stickUITrans.parent as RectTransform, m_inputDownPosition, null,
                    out Vector2 position);
                stickUITrans.anchoredPosition = position;
                
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (GameCtrl.Inst.InputType == EInputType.Joystick)
            {
                GameCtrl.Inst.StickUI.SetActive(false);
            }
        }

        if (Input.GetMouseButton(0))
        {
            var deltaAngle = 0f;
            switch (GameCtrl.Inst.InputType)
            {
                case EInputType.Rotate:
                    deltaAngle = _GetDeltaAngleRotate();
                    break;
                case EInputType.Slide:
                    deltaAngle = _GetDeltaAngleSlide();
                    break;
                case EInputType.Joystick:
                    deltaAngle = _GetDeltaAngleStick();
                    
                    //hack 临时加的显示和死区
                    var stickUITrans = GameCtrl.Inst.StickUI.transform as RectTransform;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(stickUITrans, Input.mousePosition, null,
                        out Vector2 position);
                    position = Vector3.ClampMagnitude(position, 60);
                    GameCtrl.Inst.Joystick.anchoredPosition = position;
                    if (position.sqrMagnitude < 30 * 30)
                    {
                        m_inputDownDir =  Input.mousePosition - (Vector3)m_inputDownPosition;
                        m_inputDownAngle = m_group.transform.eulerAngles.z;
                        deltaAngle = 0;
                    }
                    break;
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
        return (Input.mousePosition.x - m_inputDownPosition.x) / Screen.width * m_settings.slideRange;
    }

    private float _GetDeltaAngleRotate()
    {
        var screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        var inputDir = Input.mousePosition - screenCenter;
        #if UNITY_STANDALONE
        var deltaAngle = Vector2.SignedAngle(m_inputDownDir, inputDir);
        #else
        var deltaAngle = Vector2.SignedAngle(m_inputDownDir, inputDir) * m_settings.mobileRotateScale;
        #endif
        return deltaAngle;
    }

    private float _GetDeltaAngleStick()
    {
        var inputDir = (Vector2)Input.mousePosition - m_inputDownPosition;
        var deltaAngle = Vector2.SignedAngle(m_inputDownDir, inputDir);
        return deltaAngle;
    }
}
