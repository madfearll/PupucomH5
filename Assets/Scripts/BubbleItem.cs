using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class BubbleItem : ColorItem, IPoolable
{
    public bool IsMoving => (m_group.CellToLocal(CellPosition) - transform.localPosition).sqrMagnitude > 0.0001f;
    public Vector2 Velocity => m_body.velocity;

    protected BubbleGroup m_group;
    protected Rigidbody2D m_body;
    protected Transform m_root;

    private float m_spring;
    private Vector2 m_springForce;
    private Vector2 m_springVelocity;
    private GameSettings m_settings;
    private Vector3 m_prevPos;
    private bool m_isDisconnected = false;

    public virtual Vector3Int CellPosition
    {
        get => m_group.WorldToCell(transform.position);
        set => transform.position = m_group.CellToWorld(value);
    }

    private List<BubbleItem> m_neighbourBubbleList = new();

    protected override void Awake()
    {
        base.Awake();
        m_group = GetComponentInParent<BubbleGroup>();
        m_body = GetComponent<Rigidbody2D>();
        m_body.gravityScale = 0;
        m_root = transform.FindDeepChild("m_Root");
    }

    protected override void Start()
    {
        base.Start();
        m_settings = GameCtrl.Inst.Settings;
    }

    protected virtual void Update()
    {
        _UpdateRotateForce();//temp，应该做成position damp
        _UpdateSpring();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_group != null || m_isDisconnected) return;
        var other = col.GetComponent<BubbleItem>();
        if (other == null || other.m_group == null) return;//对方不是被黏住的球
        _StickToBubble(other);
    }

    public void Init(Vector3 position, EColor color, Vector2 vel)
    {
        transform.position = position;
        Color = color;
        m_body.velocity = vel;
        m_prevPos = position;
    }

    public virtual void OnSpawned()
    {
        m_group = GetComponentInParent<BubbleGroup>();
        m_body.gravityScale = 0;
        m_body.velocity = Vector2.zero;
        m_body.angularVelocity = 0f;
        m_springForce = Vector2.zero;
        m_springVelocity = Vector2.zero;
        m_root.localPosition = Vector3.zero;
        m_root.localScale = Vector3.one;
        m_isDisconnected = false;
        m_body.gravityScale = 0;
    }

    public virtual void OnDespawned()
    {
        m_group = null;
    }

    public List<BubbleItem> GetNeighbours()
    {
        m_neighbourBubbleList.Clear();
        foreach (var bubble in m_group.BubbleList)
        {
            if (bubble == this) continue;
            var dir = bubble.transform.position - transform.position;
            if (dir.sqrMagnitude > m_group.CellSize.y * m_group.CellSize.y) continue;
            m_neighbourBubbleList.Add(bubble);
        }

        return m_neighbourBubbleList;
    }

    public virtual void OnInsert()
    {
        m_body.velocity = Vector2.zero;
    }

    public virtual void OnMatch()
    {
        var explosion = GameCtrl.Inst.Spawn<ColorItem>("BubbleExplosion");
        explosion.Color = Color;
        explosion.transform.position = transform.position;
        this.SetTimeout(() => GameCtrl.Inst.Despawn(explosion), 2);
        m_group.ApplyImpact(transform.position, m_settings.matchImpactForce);
        m_group.DestroyBubble(this);
    }

    public virtual void OnDisconnect()
    {
        var position = transform.TransformPoint(m_root.localPosition);
        m_root.localPosition = Vector3.zero;
        transform.position = position;
        m_springForce = Vector2.zero;
        m_springVelocity = Vector2.zero;
        m_group.RemoveBubble(this);

        transform.parent = null;
        m_isDisconnected = true;
        m_body.gravityScale = 1;
        m_body.AddForce(
            (position - m_group.transform.position).normalized * m_settings.disconnectImpulse +
            Vector3.up * m_settings.disconnectImpulse,//多叠加一个向上的impulse增强表现
            ForceMode2D.Impulse);
        this.SetTimeout(() => GameCtrl.Inst.Despawn(this), 3f);
    }

    public virtual void AddForce(Vector2 force)
    {
        m_springForce += force;
        m_springForce = Vector2.ClampMagnitude(m_springForce, m_settings.maxForce);
    }

    public void ApplyImpact(Vector3 pos, float impactForce)
    {
        m_group.ApplyImpact(pos, impactForce);
    }

    private void _UpdateRotateForce()
    {
        if (!m_group) return;
        var force = (transform.position - m_prevPos) / Time.deltaTime;
        AddForce(force * m_settings.rotateForce);
        m_prevPos = transform.position;
    }
    
    private void _UpdateSpring()
    {
        if (!m_group) return;
        Vector2 spring = -m_settings.spring * m_root.localPosition;
        var acc = spring + m_springForce;
        m_springVelocity = m_settings.damp * (m_springVelocity + acc);
        m_root.localPosition += (Vector3) m_springVelocity * Time.deltaTime;
        m_springForce = Vector3.zero;

        //加一点缩放增强Q弹的感觉
        var maxVel = 20f;
        if (m_springVelocity.sqrMagnitude > 0.01f)
        {
            m_root.localScale = Vector3.one + (Vector3) (m_springVelocity / maxVel);
        }
        else
        {
            m_root.localScale = Vector3.one;
        }
    }
    
    private void _StickToBubble(BubbleItem other)
    {
        m_group = other.m_group;
        var cellPos = m_group.WorldToCell(transform.position);
        Vector2 backPos = transform.position;
        var impactSpeed = Velocity.magnitude;
        var impactDir = Velocity.normalized;
        if (impactDir == Vector2.zero)
        {
            impactDir = (transform.position - m_group.transform.position).normalized;
        }

        //如果该位置有东西，则往后退
        while (m_group[cellPos.x, cellPos.y])
        {
            backPos -= impactDir * m_group.CellSize.y * 0.5f;
            cellPos = m_group.WorldToCell(backPos);
            //cellPos += backDir;
        }
        
        m_group.AddBubble(this);

        var targetPos = m_group.CellToLocal(cellPos);
        transform.DOLocalMove(targetPos, 0.1f);
        OnInsert();
        m_group.ApplyImpact(m_group.CellToWorld(cellPos), m_settings.stickImpactForce * impactSpeed / 5f);//temp，假定球的最大速度为5
        GameCtrl.Inst.PlaySfx(Constants.SFX_GENERATE);
    }
}

