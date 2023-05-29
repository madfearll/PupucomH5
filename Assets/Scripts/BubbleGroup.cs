using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class BubbleGroup : MonoBehaviour
{
    private Tilemap m_tile;
    private Grid m_grid;

    public BubbleItem this[int x, int y] => GetBubble(x, y);
    public List<BubbleItem> BubbleList { get; private set; } = new(); //所有的节点
    public Vector2 CellSize { get; private set; }

    //三消相关变量
    private List<BubbleItem> m_toMatchList = new();
    private Stack<BubbleItem> m_stack = new();
    private List<BubbleItem> m_result = new();

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
    }

    public void RemoveBubble(BubbleItem bubble)
    {
        m_toMatchList.Remove(bubble);
        BubbleList.Remove(bubble);
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
            m_result.Clear();
            m_stack.Clear();
            m_stack.Push(start);
            m_result.Add(start);

            while (m_stack.Count > 0)
            {
                var bubble = m_stack.Pop();
                foreach (var neighbour in bubble.GetNeighbours())
                {
                    if (neighbour.Color != bubble.Color || m_result.Contains(neighbour)) continue;

                    m_stack.Push(neighbour);
                    m_result.Add(neighbour);
                }
            }

            if (m_result.Count >= 3)
            {
                foreach (var bubble in m_result)
                {
                    bubble.OnMatch();
                }

                _DropDisconnectedBubbles();
                //GameCtrl.Inst.ApplyImpulse( 0.1f, 0.15f);

                OnMatched();
            }
        }

        m_toMatchList.Clear();
    }

    protected virtual void OnMatched()
    {
        if (m_result.Count == 0) return;
        var pos = Vector3.zero;
        foreach (var bubble in m_result)
        {
            pos += bubble.transform.position;
        }
        pos /= m_result.Count;
        GameCtrl.Inst.ApplyCameraImpulse((pos - transform.position).normalized * 1.5f);
        
        GameCtrl.Inst.PlaySfx(Constants.SFX_MATCH);
    }

    private void _DropDisconnectedBubbles()
    {
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
                if (connectedList.Contains(neighbour)) continue;
                m_stack.Push(neighbour);
                connectedList.Add(neighbour);
            }
        }

        for (var i = BubbleList.Count - 1; i >= 0; i--)
        {
            var bubble = BubbleList[i];
            if (!connectedList.Contains(bubble)) bubble.OnDisconnect();
        }
    }
}

