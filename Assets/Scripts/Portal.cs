using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public void onTeleport()
    {
        GetComponent<AudioSource>().Play();
    } 
}
