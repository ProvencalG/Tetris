using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
public bool m_canRotate = true;

GameObject[] m_bottomHitSquareFX;
GameObject[] m_snapShapeFX;
public string m_bottomHitSquareTag;
public string m_snapShapeFXTag;

void Start() 
{
    // Get all Landing Effects by finding their tag
    if ((m_bottomHitSquareTag != null)&&(m_bottomHitSquareTag != ""))
    {
        m_bottomHitSquareFX = GameObject.FindGameObjectsWithTag(m_bottomHitSquareTag);
    }
    else
    {
        Debug.Log("<color=red><b>Warning!</b> No BottomHitSquareFX Tag string found on any shape</color>");
    }

    // Get all Snap Shape Effects by finding their tag
    if ((m_snapShapeFXTag != null)&&(m_snapShapeFXTag != ""))
    {
        m_snapShapeFX = GameObject.FindGameObjectsWithTag(m_snapShapeFXTag);
    }
    else
    {
        Debug.Log("<color=red><b>Warning!</b> No SnapShapeFX Tag string found on any shape</color>");
    }
}

public void LandShapeFX()
{
    int i = 0;
    foreach (Transform child in gameObject.transform)
    {
        if (m_bottomHitSquareFX[i])
        {
            m_bottomHitSquareFX[i].transform.position = child.position;

            ParticleTrigger particleTrigger = m_bottomHitSquareFX[i].GetComponent<ParticleTrigger>();
            if (particleTrigger)
            {
                particleTrigger.Play();
            }
            i++;
        }

    }
}

public void SnapShapeFX()
{
    int i = 0;
    foreach (Transform child in gameObject.transform)
    {
        if (m_snapShapeFX[i])
        {
            m_snapShapeFX[i].transform.position = child.position;

            ParticleTrigger particleTrigger = m_snapShapeFX[i].GetComponent<ParticleTrigger>();
            if (particleTrigger)
            {
                particleTrigger.Play();
            }
            i++;
        }

    }
}

public Vector3 m_visualCenterOffset;

    // Modify the position of the object given its movement
    void Move(Vector3 moveDirection)
    {
        transform.position += moveDirection;
    }

    // Set the requested movement to Left, Right, Up or Down
    public void MoveLeft(){
        Move(new Vector3(-1,0,0));
    }

    public void MoveRight(){
        Move(new Vector3(1,0,0));
    }

    public void MoveUp(){
        Move(new Vector3(0,1,0));
    }

    public void MoveDown(){
        Move(new Vector3(0,-1,0));
    }

    // Rotate the object 90 degres clockwise
    public void RotateRight()
    {
        if(m_canRotate)
        {
            transform.Rotate(0,0,-90);

            foreach (Transform child in gameObject.transform)
            {
                child.transform.Rotate(0,0,90);
            }
        }
    }

    // Rotate the object 90 degres counter clockwise
    public void RotateLeft()
    {
        if(m_canRotate)
        {
            transform.Rotate(0,0,90);

            foreach (Transform child in gameObject.transform)
            {
                child.transform.Rotate(0,0,-90);
            }
        }
    }

    public void RotateClockwise(bool clockwise)
    {
        if (clockwise)
        {
            RotateRight();
        }
        else
        {
            RotateLeft();
        }
    }
}
