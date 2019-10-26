using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holder : MonoBehaviour
{
    [SerializeField] Transform m_holderXform;
    [HideInInspector] public Shape m_heldShape = null;

    [SerializeField] float m_heldShapeScale = 0.5f;

    [HideInInspector] public bool m_canRelease = false;

    public void Catch(Shape shape)
    {
        if (m_heldShape)
        {
            Debug.LogWarning($"<color=red><b>HOLDER WARNING!</b> Release a shape before trying to hold!</color>");
            return;
        }
        if (!shape)
        {
            Debug.LogWarning($"<color=red><b>HOLDER WARNING!</b> Invalid shape!</color>");
            return;
        }

        if (m_holderXform)
        {
            shape.transform.position = m_holderXform.position + shape.m_visualCenterOffset;
            shape.transform.localScale = new Vector3(m_heldShapeScale,m_heldShapeScale,m_heldShapeScale);
            m_heldShape = shape;
        }
        else
        {
            Debug.LogWarning($"<color=red><b>HOLDER WARNING!</b> Holder has no transform assigned</color>");
        }
    }

    public Shape Release()
    {
        m_heldShape.transform.localScale = Vector3.one;
        Shape shapeToRelease = m_heldShape;
        m_heldShape = null;

        m_canRelease = false;
        return shapeToRelease;
    }
}


