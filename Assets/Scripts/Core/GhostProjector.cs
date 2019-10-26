using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostProjector : MonoBehaviour
{
    Shape m_ghostShape = null;
    bool m_hitBottom = false;
    [SerializeField] Color m_color = new Color (1f, 1f, 1f, 0.15f);

    [HideInInspector] public Vector3 validGhostPosition;

    public void DrawGhost(Shape originalShape, Board gameBoard)
    {
        if (!m_ghostShape)
        {
            m_ghostShape = Instantiate(originalShape, originalShape.transform.position, originalShape.transform.rotation) as Shape;

            m_ghostShape.gameObject.name = "GhostShape";

            SpriteRenderer [] allRenderer = m_ghostShape.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer r in allRenderer)
            {
                r.color = m_color;
            }
        }
        else
        {
            m_ghostShape.transform.position = originalShape.transform.position;
            m_ghostShape.transform.rotation = originalShape.transform.rotation;
            m_ghostShape.transform.localScale = Vector3.one;

            foreach (Transform child in m_ghostShape.transform)
            {
                child.transform.rotation = Quaternion.identity;
                Debug.Log($"<color=green>YAY I ROTATED!</color>");
            }
        }

        m_hitBottom = false;

        while (!m_hitBottom)
        {
            m_ghostShape.MoveDown();
            if(!gameBoard.IsValidPosition(m_ghostShape))
            {
                m_ghostShape.MoveUp();
                validGhostPosition = m_ghostShape.transform.position;
                m_hitBottom = true;
            }
        }
    }

    public void Reset() 
    {
        Destroy(m_ghostShape.gameObject);
    }
}
