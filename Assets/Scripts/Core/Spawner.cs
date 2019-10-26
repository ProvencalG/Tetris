using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Shape[] m_allShapes;
    [SerializeField] Transform[] m_queuedXforms = new Transform[3];

    [SerializeField] ParticleTrigger m_spawnFX;

    Shape[] m_queuedShapes = new Shape[3];

    [SerializeField] float m_queueShapeScale = 0.7f;
    
    // Must be initialized in the Start of the GameController to avoid conflict
    public void InitQueue()
    {
        for (int i = 0; i < m_queuedShapes.Length; i++)
        {
            m_queuedShapes[i] = null;
        }
        FillQueue();
    }

    void FillQueue()
    {
        for (int i = 0; i < m_queuedShapes.Length; i++)
        {
            if (!m_queuedShapes[i])
            {
                m_queuedShapes[i] = Instantiate(m_allShapes[GetRandomShape()], transform.position, Quaternion.identity) as Shape; 
                m_queuedShapes[i].transform.position = m_queuedXforms[i].position + m_queuedShapes[i].m_visualCenterOffset;
                m_queuedShapes[i].transform.localScale = new Vector3(m_queueShapeScale, m_queueShapeScale, m_queueShapeScale);
            }
        }
    }

    // Get a random place in the array containing all shapes
    // All 7 Tetris shapes must be allocated in the inspector
    int GetRandomShape()
    {
        int i = Random.Range(0,m_allShapes.Length);
        return i;
    }

       // To invoke in GameController: Gets the random shape, by its position in the array, and call its spawning
    public Shape SpawnRandomShape()
    {
        Shape shape = null;
        shape = GetQueuedShape();
        shape.transform.position = transform.position;
        shape.transform.localScale = Vector3.one;

        //StartCoroutine(GrowShape(shape, transform.position, 2f));

        if (m_spawnFX)
        {
            m_spawnFX.Play();
        }

        if (shape)
        {
            return shape;
        }
        else
        {
            Debug.Log($"<color=red><b>ERROR!</b> Invalid shape in spawner!</color>");
            return null;
        }
    }

    /*
    // Spawn a shape at spawner location, given a number corresponding to its place in the shape array
    Shape SpawnShape(int shapeNum)
    {
        Shape shape = null;
        shape = GetQueuedShape();
        shape.transform.position = transform.position;
        shape.transform.localScale = Vector3.one;

        if (shape)
        {
            return shape;
        }
        else
        {
            Debug.Log($"<color=red><b>ERROR!</b> Invalid shape in spawner!</color>");
            return null;
        }
    }

    // To invoke in GameController: Gets the specific shape by its place in the array and call its spawning
    public Shape SpawnSpecificShape(int i)
    {
        Shape shape = null;
        shape = SpawnShape(i);
        return shape;
    }
     */

    Shape GetQueuedShape()
    {
        Shape firstShape = null;

        if (m_queuedShapes[0])
        {
            firstShape = m_queuedShapes[0];
        }

        for (int i = 1; i < m_queuedShapes.Length; i++)
        {
            m_queuedShapes[i-1] = m_queuedShapes[i];
            m_queuedShapes[i-1].transform.position = m_queuedXforms[i-1].position + m_queuedShapes[i].m_visualCenterOffset;
        }

        m_queuedShapes[m_queuedShapes.Length - 1] = null;

        FillQueue();

        return firstShape;
    }

    IEnumerator GrowShape(Shape shape, Vector3 pos, float growTime = 0.5f)
    {
        float size = 0.5f;
        growTime = Mathf.Clamp(growTime, 0.05f, 2f);
        float sizeDelta = Time.deltaTime/growTime;

        while (size < 1f)
        {
            shape.transform.localScale = new Vector3 (size, size, size);
            size += sizeDelta;
            shape.transform.position = pos;
            yield return null;
        }

        shape.transform.localScale = Vector3.one;
    }
}
