using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public enum SoundLevel { None, Quiet, Normal, Loud};

    public SphereCollider colliderEmitter;
    public SoundLevel Emitter;

    public AI aiRef;
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.C))
            {
                Emitter = SoundLevel.Quiet;
                colliderEmitter.radius = 1.0f;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                Emitter = SoundLevel.Normal;
                colliderEmitter.radius = 2.5f; 
            }
            else
            {
                Emitter = SoundLevel.Loud;
                colliderEmitter.radius = 5.0f;
            }
        }
        else
        {
            Emitter = SoundLevel.None;
            colliderEmitter.radius = 0f; 
        }
	}
}
