using System.Collections.Generic;
using System.Dynamic;
using Godot;

/// <summary>
/// Tracks placement of all nodes on the screen
/// </summary>
public partial class GridManager
{
    private readonly int gridSize = 80;
    private readonly int gridWidth;
    private readonly int gridHeight;

#if DEBUG
    private Font defaultFont;
#endif // #if DEBUG

    private HashSet<GridNode>[] gridNodes;

    // Full hashsets of all the units for tick purposes
    private HashSet<GridNode> AllyUnits;
    private HashSet<GridNode> EnemyUnits;
    private HashSet<GridNode> GenericUnits;

    public GridManager(int inGridSize = 80)
    {
        gridSize = inGridSize;

        gridWidth = Consts.ScreenWidth / gridSize;
        gridHeight = Consts.ScreenHeight / gridSize;

        // Extra 1 grid padding on all edges, + 1 extra catchall (index 0)
        int totalGrids = (gridWidth + 2) * (gridHeight + 2) + 1;

        gridNodes = new HashSet<GridNode>[totalGrids];
        
        for (int i = 0; i < totalGrids; i++)
        {
            gridNodes[i] = new HashSet<GridNode>();
        }

        AllyUnits = new HashSet<GridNode>();
        EnemyUnits = new HashSet<GridNode>();
        GenericUnits = new HashSet<GridNode>();

#if DEBUG
        defaultFont = new Control().GetFont("font");
#endif // #if DEBUG
    }
    
    public void Tick(float delta)
    {
        foreach (GridNode allyUnit in AllyUnits)
            allyUnit.Tick(delta);

        foreach (GridNode enemyUnit in EnemyUnits)
            enemyUnit.Tick(delta);

        foreach (GridNode genericUnit in GenericUnits)
            genericUnit.Tick(delta);
    }

// MARK: Adding and removing nodes
    public void AddGridNode(GridNode node, Vector2 position)
    {
        // Add to full sets
        GetReleventSet(node.nodeType).Add(node);

        // Add to grid
        int gridIndex = GetGridIndex(position);
        node.gridIndex = gridIndex;
        gridNodes[GetGridIndex(position)].Add(node);
    }

    public void MoveGridNode(GridNode node, Vector2 newPosition)
    {
        int newGridIndex = GetGridIndex(newPosition);
        if (newGridIndex != node.gridIndex)
        {
#if DEBUG
        if (!gridNodes[node.gridIndex].Contains(node))
            Logger.Log($"Grid Node {node.Name} was not at node index {node.gridIndex} when moving from there", LogLevel.warn);
#endif // #if DEBUG

            gridNodes[node.gridIndex].Remove(node);
            gridNodes[newGridIndex].Add(node);
            node.gridIndex = newGridIndex;
        }
    }

    public void RemoveGridNode(GridNode node)
    {
#if DEBUG
        if (!gridNodes[node.gridIndex].Contains(node))
            Logger.Log($"Grid Node {node.Name} was not at node index {node.gridIndex} when removed", LogLevel.warn);
#endif // #if DEBUG
        gridNodes[node.gridIndex].Remove(node);
        GetReleventSet(node.nodeType).Remove(node);
    }

    public void AddUniversalNode(GridNode node)
    {
        node.gridIndex = 0;
        gridNodes[0].Add(node);
        GetReleventSet(node.nodeType).Add(node);

    }

    public void RemoveUniversalNode(GridNode node)
    {
#if DEBUG
        if (node.gridIndex != 0 || !gridNodes[0].Contains(node))
            Logger.Log($"Attemped to remove universal node {node} that wasn't at universal index");
#endif
        gridNodes[0].Remove(node);
        GetReleventSet(node.nodeType).Remove(node);
    }

// MARK: Querying Nodes
    public GridNode GetNodeAtPosition(Vector2 position)
    {
        // Check all nearby grids
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int queryIndex = GetGridIndex(position + new Vector2(i, j) * gridSize);
                foreach (GridNode node in gridNodes[queryIndex])
                {
                    node.UpdateHitbox();
                    if (node.GetHitbox().GetContainsHit(position).Item1)
                    {
                        return node;
                    }
                }
            }
        }
        return null;
    }

    public void PushContainedNodes(Vector2 point1, Vector2 point2, HashSet<GridNode> outArray)
    {
        // Create hitbox
        RectHitbox selectBox = new RectHitbox(null, (point1 + point2) / 2, (point2 - point1).Abs() / 2);

        // Check all grid squares in between
        for (int i = (int) Mathf.Min(point1.x, point2.x) - gridSize; i <= (int) Mathf.Max(point1.x, point2.x) + gridSize; i += gridSize)
        {
            for (int j = (int) Mathf.Min(point1.y, point2.y) - gridSize; j <= (int) Mathf.Max(point1.y, point2.y) + gridSize; j += gridSize)
            {
                int queryIndex = GetGridIndex(new Vector2(i, j));
                foreach (GridNode gridNode in gridNodes[queryIndex])
                {
                    // Center is contained within rectangle
                    // TODO: Instead, have some rectangle overlap with hitbox code
                    gridNode.UpdateHitbox();
                    if (gridNode.GetHitbox().OverlapsHitbox(selectBox).Item1)
                    {
                        outArray.Add(gridNode);
                    }
                }
            }
        }
    }

// MARK: Util
    /// <summary> Gets grid index corresponding to a given position</summary>
    private int GetGridIndex(Vector2 position)
    {
        int xIndex = position.x < 0 ? 0 :
            (position.x >= Consts.ScreenWidth ? gridWidth : (int)position.x / gridSize) + 1;
        int yIndex = position.y < 0 ? 0 : 
            (position.y >= Consts.ScreenHeight ? gridHeight : (int)position.y / gridSize) + 1;
        return  yIndex * (gridWidth + 2) + xIndex + 1;
    }

    private HashSet<GridNode> GetReleventSet(GridNodeType nodeType)
    {
        if (nodeType == GridNodeType.Ally)
            return AllyUnits;
        if (nodeType == GridNodeType.Enemy)
            return EnemyUnits;
        return GenericUnits;
    }

#if DEBUG

    public void DrawUnitHitboxes(Node2D drawNode)
    {
        foreach (GridNode allyUnit in AllyUnits)
            allyUnit.DrawHitbox(drawNode);    

        foreach (GridNode enemyUnit in EnemyUnits)
            enemyUnit.DrawHitbox(drawNode);

        foreach (GridNode genericUnit in GenericUnits)
            genericUnit.DrawHitbox(drawNode);
    }

    public void DrawGrid(Node2D drawBase)
    {
        // Horizontal lines
        for (int i = gridSize; i < Consts.ScreenHeight; i += gridSize)
        {
            drawBase.DrawLine(new Vector2(0, i), new Vector2(Consts.ScreenWidth, i), Colors.Black);
        }

        // Vertical lines
        for (int i = gridSize; i < Consts.ScreenWidth; i += gridSize)
        {
            drawBase.DrawLine(new Vector2(i, 0), new Vector2(i, Consts.ScreenHeight), Colors.Black);
        }

            // Grid counts
        for (int i = gridSize / 2; i < Consts.ScreenWidth; i += gridSize)
        {
            for (int j = gridSize / 2; j < Consts.ScreenHeight; j+= gridSize)
            {
                int gridIndex = GetGridIndex(new Vector2(i, j));
                drawBase.DrawString(defaultFont, new Vector2(i - 35, j - 25), gridNodes[gridIndex].Count.ToString());
            }
        }
    }
#endif // #if DEBUG
}
