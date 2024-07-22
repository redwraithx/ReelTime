using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSoundObject : MonoBehaviour
{
    public void SelfDestruct(float timeLeft)
    {
        Destroy(this.gameObject, timeLeft);
    }
}
