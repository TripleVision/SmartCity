using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class MLDriverAgent : Agent
{
	CarPercepts carPercepts;
	CarController carController;
	PathCrawler pathCrawler;
	MLTrainingScene scene;

	public float maxEpisodeTime = 15f;
	public float maxNegativeReward = -100f;
	public float accelerationOutput;
	public float steeringOutput;
	public float brakeOutput;
	//private float _elapsedTime = 0f;

	public void Start()
	{
		scene = transform.GetComponentInParent<MLTrainingScene>();
	}

	public override void Initialize()
	{
		carPercepts = GetComponent<CarPercepts>();
		carController = GetComponent<CarController>();
		pathCrawler = GetComponent<PathCrawler>();
	}

	public void FixedUpdate()
	{
		//_elapsedTime += Time.fixedDeltaTime;
		//if (_elapsedTime > maxEpisodeTime)
		//{
		//	EndEpisode();
		//}

		if (GetCumulativeReward() < maxNegativeReward)
		{
			EndEpisode();
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// 3 + 8 + 1 + 1 = 13

		//SUM 6 Track self position and rotation velocity and direction
		//sensor.AddObservation(transform.position.x);
		//sensor.AddObservation(transform.position.z);
		sensor.AddObservation(transform.rotation.y);
		sensor.AddObservation(carController.velocity);
		//sensor.AddObservation(transform.forward.x);
		//sensor.AddObservation(transform.forward.z);


		//SUM 2 observe direction to node
		//sensor.AddObservation((pathCrawler.currentNodePosition - transform.position).magnitude);

		//float angle = Vector3.SignedAngle(transform.forward, pathCrawler.currentNodePosition - transform.position,
		//								 transform.up);
		//sensor.AddObservation(angle/180f);
		//Debug.Log(angle / 180f);

		// 2 + 6  = 8 Track current node position, and subsequent three node's positions
		//sensor.AddObservation(pathCrawler.currentNodePosition.x);
		//sensor.AddObservation(pathCrawler.currentNodePosition.z);

		//foreach (Vector3 node in pathCrawler.nextThreeNodes)
		//{
		//	sensor.AddObservation(node.x);
		//	sensor.AddObservation(node.z);
		//}

		// Track status of approaching traffic signal, if applicable
		// -1 -> None; 0 -> Red; 1 -> Yellow; 2 -> Green; 3 -> Stop Sign
		//sensor.AddObservation(carPercepts.approachingTrafficSignalType);

		// Track speed limit of current path
		//sensor.AddObservation(pathCrawler.maxVelocity);

        // Track status of raycast percepts coming from the front of the car
        // By default, car casts 31 rays
        // carPercepts.GetCollisions(out List<float> distances, "Car");
        //foreach (float distance in distances)
        //{
        //    sensor.AddObservation(distance);
        //}

    }

	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		//float verticalAxis = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
		//float horizontalAxis = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
		//float brakeValue = Mathf.Clamp(actionBuffers.ContinuousActions[2], 0f, 1f);

		//Continues updated
		//int verticalAxis = (int)actionBuffers.ContinuousActions[0];
		//float horizontalAxis = actionBuffers.ContinuousActions[1];
		//float brakeValue = 0; // actionBuffers.ContinuousActions[2]< 0.5f? 0f: 1f

		//Continues & Discrete
		var moveAction = actionBuffers.DiscreteActions[0];
		int verticalAxis = (int)moveAction == 2 ? -1 : moveAction;
		//Debug.Log(moveAction);
		float horizontalAxis = actionBuffers.ContinuousActions[0];
		float brakeValue = 0; // actionBuffers.ContinuousActions[2]< 0.5f? 0f: 1f

		accelerationOutput = verticalAxis;
		steeringOutput = horizontalAxis;
		brakeOutput = brakeValue;

		// Debug.Log("Vertical: " + verticalAxis + " Horizontal: " + horizontalAxis + " Brake: " + brakeValue);
		carController.SetInput(verticalAxis, horizontalAxis, brakeValue);

		if (Vector3.Angle(transform.up, Vector3.up) > 45f)
		{
			Debug.Log("Car is upside down");
			AddReward(-1f);
			EndEpisode();
		}

		if (carPercepts.CollidedWithObject(out string tag, clear: true))
		{
			if (tag == "Car" || tag == "TrafficSignal" || tag == "Sidewalk")
			{
				Debug.Log("Collided with " + tag + "! Resetting...");
				AddReward(-1f);
				EndEpisode();
			}
		}

        if (carPercepts.TriggeredWithObjects(out string triggerTag, clear: true))
        {
            if (triggerTag == "RewardingBall")
            {
                //Debug.Log("Touched Rewarding Ball  <color=green>Got Reward</color>");
                AddReward(0.1f);
            }
        }

        //if (carRuleEnforcer.CheckRanTrafficSignal(clear: true))
        //{
        //	Debug.Log("Ran traffic signal");
        //	AddReward(-0.5f);
        //}
        if (pathCrawler.IsInOtherLane())
		{
			// Debug.Log("Went into other lane");
			AddReward(-1f);
		}
		//if (carController.velocity > pathCrawler.maxVelocity)
		//{
		//	// Debug.Log("Exceeded max velocity");
		//	AddReward(-0.2f);
		//}

		// This is triggering when the level is reset, which I think is throwing off the rewards

		//if (pathCrawler.CheckChangedNodes(clear: true))
		//{
		//	//Debug.Log("Changed nodes");
		//	AddReward(1f);
		//}

		AddReward(-1f/MaxStep);
	}

	public override void OnEpisodeBegin()
	{
		//_elapsedTime = 0f;

		carController.velocity = 0;

		scene.InitializeScene(this);
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		var continuousActionsOut = actionsOut.ContinuousActions;
		var discreteActionsOut = actionsOut.DiscreteActions;
		continuousActionsOut[0] = Input.GetAxis("Horizontal");

		if (Input.GetAxis("Vertical") != 0)
        {
			discreteActionsOut[0] = Input.GetAxis("Vertical") > 0f ? 1 : 2;
		}
		else
        {
			discreteActionsOut[0] = 0;
		}

		//continuousActionsOut[1] = Input.GetAxis("Horizontal");
		//continuousActionsOut[2] = Input.GetAxis("Jump");
	}
}