using System.Collections.Generic;
using UnityEngine;

public class CatChaser : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public Vector2 roomMin = new Vector2(0.55f, -13.13f);
    public Vector2 roomMax = new Vector2(22.86f, -1.01f);
    public float cellSize = 0.5f;
    public float clearanceRadius = 0.35f;
    public LayerMask obstacleMask;
    public float gridRebuildInterval = 0.5f;

    private bool[,] blocked;
    private int width, height;
    private Vector2 moveTarget;
    private float rebuildTimer;

    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    void Start()
    {
        width = Mathf.CeilToInt((roomMax.x - roomMin.x) / cellSize);
        height = Mathf.CeilToInt((roomMax.y - roomMin.y) / cellSize);
        blocked = new bool[width, height];

        BuildGrid();
        moveTarget = transform.position;
    }

    // Mark every cell that is too close to an obstacle
    void BuildGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                blocked[x, y] = Physics2D.OverlapCircle(CellToWorld(new Vector2Int(x, y)), clearanceRadius, obstacleMask) != null;
    }

    void Update()
    {
        if (target == null) return;

        rebuildTimer += Time.deltaTime;
        if (rebuildTimer >= gridRebuildInterval)
        {
            rebuildTimer = 0f;
            BuildGrid();
        }

        // Pick a new waypoint each time we reach the current one
        if ((Vector2)transform.position == moveTarget)
            moveTarget = NextWaypoint();

        transform.position = Vector2.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
    }

    // Search outward from the player
    Vector2 NextWaypoint()
    {
        Vector2Int catCell = WorldToCell(transform.position);
        Vector2Int playerCell = WorldToCell(target.position);
        if (catCell == playerCell) return target.position;

        var visited = new HashSet<Vector2Int> { playerCell };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(playerCell);

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = cell + dir;
                if (visited.Contains(next) || !CornerClear(cell, dir)) continue;
                if (next == catCell) return CellToWorld(cell);
                if (!IsOpen(next)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return transform.position; 
    }

    // Diagonal moves need both side cells open so the cat can't clip corners
    bool CornerClear(Vector2Int from, Vector2Int dir)
    {
        if (dir.x == 0 || dir.y == 0) return true;
        return IsOpen(new Vector2Int(from.x + dir.x, from.y)) && IsOpen(new Vector2Int(from.x, from.y + dir.y));
    }

    bool IsOpen(Vector2Int c)
    {
        return c.x >= 0 && c.x < width && c.y >= 0 && c.y < height && !blocked[c.x, c.y];
    }

    Vector2 CellToWorld(Vector2Int c)
    {
        return new Vector2(roomMin.x + (c.x + 0.5f) * cellSize, roomMin.y + (c.y + 0.5f) * cellSize);
    }

    Vector2Int WorldToCell(Vector2 p)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt((p.x - roomMin.x) / cellSize), 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((p.y - roomMin.y) / cellSize), 0, height - 1);
        return new Vector2Int(x, y);
    }
}