using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class BubbleGroup : MonoBehaviour
{
    private Tilemap m_tile;
    private Grid m_grid;

    public BubbleItem this[int x, int y] => GetBubble(x, y);
    public List<BubbleItem> BubbleList { get; private set; } = new(); //所有的节点
    public Vector2 CellSize { get; private set; }
    public float Radius { get; private set; }

    //三消相关变量
    private List<BubbleItem> m_toMatchList = new();
    private Stack<BubbleItem> m_stack = new();
    private List<BubbleItem> m_matched = new();//被三消的球
    private List<BubbleItem> m_disconnected = new();//断开连接的球

    protected virtual void Awake()
    {
        m_tile = GetComponentInChildren<Tilemap>();
        m_grid = GetComponentInChildren<Grid>();
        CellSize = m_grid.cellSize;
        GetComponentsInChildren(BubbleList);
    }

    private void Start()
    {
        GameCtrl.Inst.Group = this;
    }

    protected void Update()
    {
        if (GameCtrl.Inst.GameEnd) return;
        _UpdateMatch();
    }

    public Vector3Int WorldToCell(Vector3 position) => m_tile.WorldToCell(position);
    public Vector3 CellToWorld(Vector3Int cellPosition) => m_tile.CellToWorld(cellPosition);
    public Vector3 CellToLocal(Vector3Int cellPosition) => m_tile.CellToLocal(cellPosition);

    public BubbleItem GetBubble(int x, int y)
    {
        foreach (var bubble in BubbleList)
        {
            var cellPosition = bubble.CellPosition;
            if (cellPosition.x == x && cellPosition.y == y) return bubble;
        }

        return null;
    }

    public void AddBubble(BubbleItem bubble)
    {
        bubble.transform.parent = m_tile.transform;
        m_toMatchList.Add(bubble);
        BubbleList.Add(bubble);
        
        _RefreshRadius();
    }

    public void RemoveBubble(BubbleItem bubble)
    {
        m_toMatchList.Remove(bubble);
        BubbleList.Remove(bubble);
        
        _RefreshRadius();
    }

    public BubbleItem CreateBubble(string bubbleName)
    {
        return GameCtrl.Inst.Spawn<BubbleItem>(bubbleName, transform);
    }

    public void DestroyBubble(BubbleItem bubble)
    {
        GameCtrl.Inst.Despawn(bubble);
        BubbleList.Remove(bubble);
        m_toMatchList.Remove(bubble);
        
        _RefreshRadius();
    }

    private void _RefreshRadius()
    {
        Radius = 0;
        foreach (var bubble in BubbleList)
        {
            var radius = (bubble.transform.position - transform.position).magnitude;
            if (radius > Radius) Radius = radius;
        }
    }

    public virtual void ApplyImpact(Vector2 impactPoint, float impactForce)
    {
        var impactRange = CellSize.y * 10f;
        foreach (var bubbleItem in BubbleList)
        {
            var direction = (Vector2) bubbleItem.transform.position - impactPoint;
            var mag = direction.magnitude;
            if (mag > impactRange) continue;
            bubbleItem.AddForce((impactRange - mag) / impactRange * impactForce * direction.normalized);
        }
    }

    private void _UpdateMatch()
    {
        if (m_toMatchList.Count == 0) return;
        foreach (var bubble in m_toMatchList)
        {
            if (bubble.IsMoving) return; //泡泡还在生成中，不消除
        }
        
        for (int i = m_toMatchList.Count - 1; i >= 0; i--)
        {
            var start = m_toMatchList[i];
            m_matched.Clear();
            m_stack.Clear();
            m_stack.Push(start);
            m_matched.Add(start);

            while (m_stack.Count > 0)
            {
                var bubble = m_stack.Pop();
                foreach (var neighbour in bubble.GetNeighbours())
                {
                    if (neighbour.Color != bubble.Color || m_matched.Contains(neighbour)) continue;

                    m_stack.Push(neighbour);
                    m_matched.Add(neighbour);
                }
            }

            if (m_matched.Count >= 3)
            {
                _RefreshDisconnectedBubbles();

                GameCtrl.Inst.Combo = m_matched.Count + m_disconnected.Count;
                
                foreach (var bubble in m_matched)
                {
                    bubble.OnMatch();
                }
                foreach (var bubble in m_disconnected)
                {
                    bubble.OnDisconnect();
                }
                
                OnMatched();
            }
        }

        m_toMatchList.Clear();
    }

    protected virtual void OnMatched()
    {
        if (m_matched.Count == 0) return;
        var pos = Vector3.zero;
        foreach (var bubble in m_matched)
        {
            pos += bubble.transform.position;
        }
        pos /= m_matched.Count;
        GameCtrl.Inst.ApplyCameraImpulse((pos - transform.position).normalized * 1.5f);

        GameCtrl.Inst.PlaySfx(Constants.SFX_MATCH, 0.5f);
    }

    private void _RefreshDisconnectedBubbles()
    {
        m_disconnected.Clear();
        var origin = GetBubble(0, 0);
        var connectedList = new List<BubbleItem>();
        m_stack.Clear();
        m_stack.Push(origin);
        connectedList.Add(origin);

        while (m_stack.Count > 0)
        {
            var bubble = m_stack.Pop();
            foreach (var neighbour in bubble.GetNeighbours())
            {
                if (connectedList.Contains(neighbour) || m_matched.Contains(neighbour)) continue;
                m_stack.Push(neighbour);
                connectedList.Add(neighbour);
            }
        }

        for (var i = BubbleList.Count - 1; i >= 0; i--)
        {
            var bubble = BubbleList[i];
            if (!connectedList.Contains(bubble) && !m_matched.Contains(bubble))
            {
                m_disconnected.Add(bubble);
            }
        }
    }
}

