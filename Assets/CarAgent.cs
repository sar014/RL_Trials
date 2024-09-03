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
        Time.timeScale = 5f;
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

        // 2 observations: Agent's velocity (x and z)
        Rigidbody rb = GetComponent<Rigidbody>();
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get actions from the agent
        float moveX = actions.ContinuousActions[0];  // Force along x axis
        float moveZ = actions.ContinuousActions[1];  // Force along z axis

        // Debug.Log($"Training Action Values - moveX: {moveX}, moveZ: {moveZ}, brake: {brake}");

        // Set movement direction using the Movement script
        movementScript.SetMovementDirection(moveX, moveZ);

        // You can also check if the car is moving forward
        if (Vector3.Dot(transform.forward, GetComponent<Rigidbody>().velocity) < 0)
        {
            SetReward(-0.01f); // Extra penalty for moving backward
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");  // Left/Right movement
        continuousActions[1] = Input.GetAxis("Vertical");    // Forward/Backward movement

        // Set movement direction for player control (heuristic)
        movementScript.SetMovementDirection(continuousActions[0], continuousActions[1]);
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
