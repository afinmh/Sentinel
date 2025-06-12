using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FSM : MonoBehaviour
{

    protected Transform Playertransform;
    protected Vector3 Dest;


    protected List<GameObject> Waypoints = new List<GameObject>();

    protected virtual void fsm_init() { }
    protected virtual void fsm_update() { }
    protected virtual void fsm_fixedUpdate() { }
    void Start()
    {
        fsm_init();
    }
    void Update()
    {
        fsm_update();
    }

    private void FixedUpdate()
    {
        fsm_fixedUpdate();
    }
}