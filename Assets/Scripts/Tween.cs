using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween : MonoBehaviour
{

    public Vector3 moveAmount = Vector3.zero;
    public float smoothTime = 0.5f;
    private Vector3 startingPosition;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private bool movingTowardTarget = true;
    public bool cycle = true;
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
        targetPosition = transform.position + moveAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if(movingTowardTarget) {
            transform.position = Vector3.SmoothDamp(transform.position, 
                                targetPosition, ref velocity, smoothTime);
            if((transform.position - targetPosition).sqrMagnitude < 0.1f)
            {
                movingTowardTarget = false;
            }
        } else if (cycle) {
            transform.position = Vector3.SmoothDamp(transform.position, 
                                startingPosition, ref velocity, smoothTime);
            if((transform.position - startingPosition).sqrMagnitude < 0.1f)
            {
                movingTowardTarget = true;
            }
        }

    }

    public void changeTween(Vector3 newMove, float newTime, bool newCycle)
    {
        startingPosition = transform.position;
        targetPosition = transform.position + newMove;
        movingTowardTarget = true;
        smoothTime = newTime;
        cycle = newCycle;
    }
}
