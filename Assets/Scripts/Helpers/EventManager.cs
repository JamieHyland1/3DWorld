using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class EventManager : MonoBehaviour
{
    public static EventManager current;
    private void Awake(){
        current = this;
    }
    public event Action<Vector3, Vector3> OnPlayerEnter; 
    public void OnPlayerTriggerEnter(Vector3 pos, Vector3 dir){
        if(OnPlayerEnter != null){
            OnPlayerEnter(pos,dir);
        }
    }

    
}
