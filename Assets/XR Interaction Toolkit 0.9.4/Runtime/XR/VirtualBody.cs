using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualBody : MonoBehaviour
{
    bool m_CollisionFlag;
    public bool CollisionFlag { get { return m_CollisionFlag; } set { m_CollisionFlag = value; }  }

    bool m_OnFloor;
    public bool OnFloor { get { return m_OnFloor; } set { m_OnFloor = value; } }

    // The rigidbody of the virtual body 
    [Tooltip("The rigidbody of the virtual body.")]
    Rigidbody m_RigidBodyCharacter;
    public Rigidbody RigidBodyCharacter { get { return m_RigidBodyCharacter; } set { m_RigidBodyCharacter = value; } }

    private void Start()
    {
        RigidBodyCharacter = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        if (contact.normal.y < .7)
        {
            CollisionFlag = true;
            RigidBodyCharacter.velocity = Vector3.zero;
        }
        else
        {
            OnFloor = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionFlag = false;
    }
}