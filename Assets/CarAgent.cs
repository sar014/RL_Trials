using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
    public Material winMaterial;
    public Material loseMaterial;
    public GameObject plane;
    public Movement movementScript;
    public List<Transform> checkpoints;
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    private int nextCheckpointIndex;
    MeshRenderer meshRenderer;

    private void Start() {
        meshRenderer = plane.GetComponent<MeshRenderer>();
        Time.timeScale = 1f;
    }

    public override void OnEpisodeBegin()
    {
        // Reset the agent's position and orientation
        transform.localPosition = new Vector3(-0.47f, 0f, -17.51f);
        transform.localRotation = Quaternion.identity;
        nextCheckpointIndex = 0;

        // Reset velocity
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reactivate all checkpoints
        foreach (var checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations: Agent's position
        sensor.AddObservation(transform.localPosition);

        // 3 observations: Position of the next checkpoint
        sensor.AddObservation(checkpoints[nextCheckpointIndex].localPosition);

        // 1 observation: Angle difference between the car's forward direction and the direction to the next checkpoint
        Vector3 directionToCheckpoint = (checkpoints[nextCheckpointIndex].position - transform.position).normalized;
        float angleToCheckpoint = Vector3.Angle(transform.forward, directionToCheckpoint);
        sensor.AddObservation(angleToCheckpoint);

        // 2 observations: Agent's velocity (x and z)
        Rigidbody rb = GetComponent<Rigidbody>();
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // // Get actions from the agent
        // float moveX = actions.ContinuousActions[0];  // Force along x axis
        // float moveZ = actions.ContinuousActions[1];  // Force along z axis

        // // Debug.Log($"Training Action Values - moveX: {moveX}, moveZ: {moveZ}, brake: {brake}");

        // // Set movement direction using the Movement script
        // movementScript.SetMovementDirection(moveX, moveZ);

        // // You can also check if the car is moving forward
        // if (Vector3.Dot(transform.forward, GetComponent<Rigidbody>().velocity) < 0)
        // {
        //     SetReward(-0.01f); // Extra penalty for moving backward
        // }

        float forwardAmount = 0f;
        float turnAmount = 0f;

        // Discrete action for moving forward/backward
        switch (actions.DiscreteActions[0]) {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 1f; break;
            case 2: forwardAmount = -1f; break;
        }

        // Discrete action for turning left/right
        switch (actions.DiscreteActions[1]) {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }

        // Set the movement using the Movement script
        movementScript.SetMovementDirection(forwardAmount, turnAmount);

        // // Extra penalty for moving backward
        // if (Vector3.Dot(transform.forward, GetComponent<Rigidbody>().velocity) < 0)
        // {
        //     SetReward(-0.01f);
        // }





        // Get the direction vector to the next checkpoint
        Vector3 directionToCheckpoint = (checkpoints[nextCheckpointIndex].position - transform.position).normalized;

        // Get the alignment between the car's forward direction and the direction to the next checkpoint
        float alignment = Vector3.Dot(transform.forward, directionToCheckpoint);

        // Reward the car for better alignment
        // SetReward(0.5f);

        // Additional reward for successfully reaching the checkpoint
        if (Vector3.Distance(transform.position, checkpoints[nextCheckpointIndex].position) < 1.0f)
        {
            SetReward(2.0f);
            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Count;
        }

        // Penalize the car for moving backward
        if (forwardAmount < 0)
        {
            SetReward(-1f);
        }

        // Penalize the car for not turning when it's needed
        if (alignment < 0.5f && turnAmount == 0f)
        {
            SetReward(-1f);
        }

        // Penalty for low speed
        float speed = GetComponent<Rigidbody>().velocity.magnitude;
        if (speed < 1f)
        {
            SetReward(-0.2f); // Penalty for moving too slowly
        }
        else
        {
            SetReward(0.5f); // Small reward proportional to speed
        }

        if (Vector3.Distance(transform.position, checkpoints[nextCheckpointIndex].position) > 10f)
        {
            SetReward(-0.5f); // Penalize if the car takes too long to reach a checkpoint
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // var continuousActions = actionsOut.ContinuousActions;
        // continuousActions[0] = Input.GetAxis("Horizontal");  // Left/Right movement
        // continuousActions[1] = Input.GetAxis("Vertical");    // Forward/Backward movement

        // // Set movement direction for player control (heuristic)
        // movementScript.SetMovementDirection(continuousActions[0], continuousActions[1]);

        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 2;

        discreteActions[1] = 0;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActions[1] = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[1] = 2;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            SetReward(1f);
            
            meshRenderer.material = winMaterial;

            // Deactivate the checkpoint
            other.gameObject.SetActive(false);

            nextCheckpointIndex = (nextCheckpointIndex + 1) % checkpoints.Count;

            // If it's the last checkpoint, end the episode
            if (nextCheckpointIndex == 0)
            {
                EndEpisode();
            }
        }

        if (other.CompareTag("Wall"))
        {
            meshRenderer.material = loseMaterial;
            SetReward(-1f);
            EndEpisode();
        }
    }
}
