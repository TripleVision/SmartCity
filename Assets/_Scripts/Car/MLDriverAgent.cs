using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using Assets._Scripts.Car;

public class MLDriverAgent : Agent, ICarAgent
{
	private CarPercepts _carPercepts;
	private CarController _carController;
	private PathCrawler _pathCrawler;
	private MLTrainingScene _scene;
	private Rigidbody _rigidBody;

	[SerializeField] private GameObject _rewardingBall;
	[SerializeField] private float _maxNegativeReward = -100f;

	[Header("Debug")]
	[SerializeField] private float _accelerationOutput;
	[SerializeField] private float _steeringOutput;
	[SerializeField] private float _brakeOutput;

	public PathCrawler PathCrawler => _pathCrawler;
    public int StartingPathIndex { get; set; }
	public Transform Transform => transform;

	public GameObject RewardingBall => _rewardingBall;

    public void Start()
	{
		_scene = transform.GetComponentInParent<MLTrainingScene>();
	}

	public override void Initialize()
	{
		_carPercepts = GetComponent<CarPercepts>();
		_carController = GetComponent<CarController>();
		_pathCrawler = GetComponent<PathCrawler>();
		_rigidBody = GetComponent<Rigidbody>();
	}
	
	public override void OnEpisodeBegin()
	{
		_carController.velocity = 0;
		_scene.ResetAgent(this, StartingPathIndex);
	}

	public void FixedUpdate()
	{
		if (GetCumulativeReward() < _maxNegativeReward)
		{
			EndEpisode();
		}
	}

    public void ResetRigidBody()
    {
		_rigidBody.velocity = Vector3.zero;
		_rigidBody.angularVelocity = Vector3.zero;
		_rigidBody.ResetInertiaTensor();
	}

	private void Update()
    {
		// Maybe it should be on collision detection moment?
		if (_pathCrawler.CheckChangedNodes())
		{
			UpdateRewardballPosition();
		}
	}

    private void UpdateRewardballPosition()
	{
		var newPosition = new Vector3(_pathCrawler.currentNodePosition.x, 0.2f, _pathCrawler.currentNodePosition.z);
		SetPositionRewardingBall(newPosition);
	}

	public void SetPositionRewardingBall(Vector3 newPosition)
    {
		_rewardingBall.transform.position = newPosition;

	}

	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(transform.rotation.y);
		sensor.AddObservation(_carController.velocity);
    }

	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		//Continues & Discrete
		var moveAction = actionBuffers.DiscreteActions[0];
		int verticalAxis = (int)moveAction == 2 ? -1 : moveAction;
		//Debug.Log(moveAction);
		float horizontalAxis = actionBuffers.ContinuousActions[0];
		float brakeValue = 0; // actionBuffers.ContinuousActions[2]< 0.5f? 0f: 1f

		_accelerationOutput = verticalAxis;
		_steeringOutput = horizontalAxis;
		_brakeOutput = brakeValue;

		// Debug.Log("Vertical: " + verticalAxis + " Horizontal: " + horizontalAxis + " Brake: " + brakeValue);
		_carController.SetInput(verticalAxis, horizontalAxis, brakeValue);

		if (Vector3.Angle(transform.up, Vector3.up) > 45f)
		{
			Debug.Log("Car is upside down");
			AddReward(-1f);
			EndEpisode();
		}

		if (_carPercepts.CollidedWithObject(out string tag, clear: true))
		{
			if (tag == "Car" || tag == "TrafficSignal" || tag == "Sidewalk")
			{
				//Debug.Log("Collided with " + tag + "! Resetting...");
				AddReward(-1f);
				EndEpisode();
			}
		}

        if (_carPercepts.TriggeredWithObjects(out string triggerTag, clear: true))
        {
            if (triggerTag == "RewardingBall")
            {
                //Debug.Log("Touched Rewarding Ball  <color=green>Got Reward</color>");
                AddReward(0.1f);
            }
        }

        if (_pathCrawler.IsInOtherLane())
		{
			// Debug.Log("Went into other lane");
			AddReward(-1f);
		}

		AddReward(-1f/MaxStep);
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
	}

    public void SetDetectableBallTag(string tag)
    {
		_rewardingBall.tag = tag;
    }
}