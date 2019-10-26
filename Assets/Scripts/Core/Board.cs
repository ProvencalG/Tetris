using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] Transform m_spriteToDraw;

    public int m_boardHeight = 30;
    public int m_boardWidth = 10;
    [SerializeField] int m_header = 8;

    // Store inactive shapes that have landed here
    Transform[,] m_grid;

    [HideInInspector] public int m_completedRows = 0;

    [SerializeField] float m_gridLandingDelay = 0.2f;

    [SerializeField] ParticleTrigger[] m_rowGlowFX = new ParticleTrigger[4];

    [HideInInspector] public bool isClearingRows = false;

    void Awake() 
    {
        m_grid = new Transform[m_boardWidth,m_boardHeight];
    }

    // Start is called before the first frame update
    void Start()
    {
        LayoutBoard(m_boardHeight - m_header, m_boardWidth);
    }

    // Check if the given position is within board bounds
    bool IsWithinBoard (int x, int y)
    {
        return (x >= 0 && x < m_boardWidth && y >= 0);
    }

    // Check is the given shape has all its squares in board bounds
    // And if they are is a non occupied cell
    public bool IsValidPosition(Shape shape)
    {
        foreach (Transform child in shape.transform)
        {
            Vector2 pos = Vectorf.Round(child.position);

            if (!IsWithinBoard((int) pos.x, (int) pos.y))
            {
                return false;
            }

            if (IsOccupied((int)pos.x, (int)pos.y, shape))
            {
                return false;
            }
        }
    
        return true;
    }

    // Set a 2D board and call to draw the sprite to draw at each cell
    void LayoutBoard(int boardHeight, int boardWidth)
    {
        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                DrawSprite(x, y);
            }
        }
    }

    // Draw the sprite to draw at each cell
    void DrawSprite(int x, int y)
    {
        if(m_spriteToDraw != null)
        {
            Transform clone;
        clone = Instantiate(m_spriteToDraw, new Vector3(x,y,0), Quaternion.identity) as Transform;
        clone.name = $"{m_spriteToDraw.name} x = {x.ToString()}, y = {y.ToString()}";
        clone.transform.parent = transform;
        }
        else{
            Debug.Log($"<color=red><b>ERROR </b>Assign a sprite to draw in the inspector</color>");
        }
    }

    public void StoreShapeInGrid(Shape shape)
    {
        if (!shape)
        {
            return;
        }

        foreach (Transform child in shape.transform)
        {
            Vector2 pos = Vectorf.Round(child.position);
            m_grid[(int) pos.x, (int) pos.y] = child;
        }
    }

    bool IsOccupied(int x, int y, Shape shape)
    {
        // Says that it is occupied if:
        // the grid at this position contains something AND that something is not the active shape
        return (m_grid[x,y] !=null && m_grid[x,y].parent != shape.transform);
    }

    bool IsComplete(int y)
    {
        // Check if every cells in a row is occupied
        for (int x = 0; x < m_boardWidth; x++)
        {
            if (m_grid[x,y] == null)
            {
                return false;
            }
        }
        return true;
    }

    // Clear everything on a row given its height
    void ClearRow(int y)
    {
        for (int x = 0; x < m_boardWidth; x++)
        {
            if(m_grid[x,y] != null)
            {
                Destroy(m_grid[x,y].gameObject);
            }

            m_grid[x,y] = null;
        }
    }

    void ShiftOneRowDown(int y)
    {
        for (int x = 0; x < m_boardWidth; x++)
        {
            if (m_grid[x,y] != null)
            {
                // Copy the actual row to the row below. This is only array information, not game objects
                m_grid[x,y-1] = m_grid[x,y];
                // Delete the actual row in the array
                m_grid[x,y] = null;
                // Move the objects down one row
                m_grid[x,y-1].position += new Vector3(0,-1,0);
            }
        }
    }

    // Given the starting row position, shift every rows above it down.
    void ShiftRowsDown(int startY)
    {
        for (int i = startY; i < m_boardHeight; i++)
        {
            ShiftOneRowDown(i);
        }
    }

    // Check every rows and clear them if empty
    public IEnumerator CheckAndClearAllRows()
    {
        isClearingRows = true;
        m_completedRows = 0;

        // For effects only
        for (int y = 0; y < m_boardHeight; y++)
        {
            if(IsComplete(y))
            {
                ClearRowEffect(m_completedRows, y);
                m_completedRows++;
            }
        }
        yield return new WaitForSeconds(m_gridLandingDelay);

        // To really clear the rows
        for (int y = 0; y < m_boardHeight; y++)
        {
            if(IsComplete(y))
            {
                // Increment the completed rows counter by one, it's used by the game controller
                m_completedRows++;
                ClearRow(y);
                ShiftRowsDown(y+1);
                //yield return new WaitForSeconds(0.02f);
                y--;
            }
        }
        isClearingRows = false;
    }

    public bool IsOverLimit(Shape shape)
    {
        foreach (Transform child in shape.transform)
        {
            if (child.transform.position.y >= (m_boardHeight - m_header - 1))
            {
                return true;
            }
        }
        return false;
    }

    void ClearRowEffect(int i, int y)
    {
        if (m_rowGlowFX[i])
        {
            m_rowGlowFX[i].transform.position = new Vector3 (0,y,-1);
            m_rowGlowFX[i].Play();
        }
    }
}
