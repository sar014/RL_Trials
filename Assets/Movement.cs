using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backLeft;
    Vector3 movDirection;
    Rigidbody objRb;
    public float speed;
    public float motorForce = 1500f;
    public float maxTurnAngle = 30f;
    public float maxBrakeTorque = 20f;
    public float brakeForce = 0;
    public float currentTurnAngle = 0f;
    // Start is called before the first frame update
    void Start()
    {
        objRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() 
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // objRb.velocity = movDirection * speed;
        
        // // Apply steering
        // frontLeft.steerAngle = currentTurnAngle;
        // frontRight.steerAngle = currentTurnAngle;

        // Apply motor force to the rear wheels
        // backLeft.motorTorque = motorForce;
        // backRight.motorTorque = motorForce;
    }


    // Method to be called in OnActionReceived to set movement direction
    public void SetMovementDirection(float moveX, float moveZ)
    {
        // movDirection = new Vector3(moveX, 0, moveZ);
        // currentTurnAngle = maxTurnAngle * moveX;

        // Apply steering based on the horizontal input (moveX)
        float turnAngle = maxTurnAngle * moveZ;
        frontLeft.steerAngle = turnAngle;
        frontRight.steerAngle = turnAngle;

        // Apply motor torque based on forward/backward input (moveZ)
        backLeft.motorTorque = moveX * motorForce;
        backRight.motorTorque = moveX * motorForce;

    }
}
