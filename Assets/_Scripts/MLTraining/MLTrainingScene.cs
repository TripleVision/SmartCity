using Assets._Scripts.Car;
using PathCreation;
using System;
using Unity.MLAgents;
using UnityEngine;

public class MLTrainingScene : MonoBehaviour
{
    [SerializeField] private GameObject _roadsParent;
    [SerializeField] private GameObject _agentPrefab;
    //[SerializeField] private int agentsCount = 5;
    [SerializeField] private float _startPositionOffset = 0.5f;
    [SerializeField] private float _offsetFromGround = 0.5f;

    private NodePath[] _nodePaths;
    private Agent[] _agents;

    SimpleMultiAgentGroup _agentsGroup;

    void Awake()
    {
        Init();
    }
    private void Init()
    {
        _nodePaths = _roadsParent.transform.GetComponentsInChildren<NodePath>();
        _agents = GetComponentsInChildren<Agent>();
        _agentsGroup = new SimpleMultiAgentGroup();
    }

    private void Start()
    {
        SetupAgents();
    }

    public void SetupAgents()
    {
        for (int i = 0; i < _agents.Length; i++)
        {
            ResetAgent(_agents[i], i);
        }
    }

    //public void SpawnAgents()
    //{
    //    for (int i = 0; i < agentsCount; i++)
    //    {
    //        var startingPathIndex = i;
    //        SpawnAgent(startingPathIndex);
    //    }
    //}

    //private void SpawnAgent(int startingPathIndex)
    //{
    //    var agentClone = Instantiate(_agentPrefab, GetPosition(startingPathIndex), GetRotation(startingPathIndex), transform);
    //    MLDriverAgent agent = agentClone.GetComponent<MLDriverAgent>();
    //    ResetAgent(agent, startingPathIndex);
    //}

    public void ResetAgent(Agent agent, int startingPathIndex)
    {

        var carAgent = agent as ICarAgent;
        carAgent.StartingPathIndex = startingPathIndex;

        NodePath startingPath = _nodePaths[startingPathIndex];
        ResetAgentPosition(agent, startingPathIndex);
        carAgent.ResetRigidBody();
        carAgent.PathCrawler.Initialize(startingPath);

        var nextNode = carAgent.PathCrawler.nextThreeNodes[1];
        carAgent.SetPositionRewardingBall(new Vector3(nextNode.x, 5f, nextNode.y));
        SetUpRewardBallTag(agent, startingPathIndex);
        SetBallParent(agent, transform);

        carAgent.PathCrawler.CheckChangedNodes(clear: true);
    }

    private void SetBallParent(Agent agent, Transform transform)
    {
        var carAgent = agent as ICarAgent;

        carAgent.RewardingBall.transform.parent = transform;
    }

    private static void SetUpRewardBallTag(Agent agent, int startingPathIndex)
    {
        var carAgent = agent as ICarAgent;

        carAgent.SetDetectableBallTag($"Ball_{startingPathIndex}");
    }

    private void ResetAgentPosition(Agent agent, int startingPathIndex)
    {
        var carAgent = agent as ICarAgent;

        carAgent.Transform.position = GetPosition(startingPathIndex);
        carAgent.Transform.position -= carAgent.Transform.forward * _startPositionOffset;
        carAgent.Transform.rotation = GetRotation(startingPathIndex);
    }

    private Quaternion GetRotation (int pathIndex)
    {
        NodePath path = _nodePaths[pathIndex];
        return Quaternion.LookRotation(path.nodes[1] - path.nodes[0]);
    }

    private Vector3 GetPosition(int pathIndex)
    {
        return _nodePaths[pathIndex].nodes[0] + Vector3.up * _offsetFromGround;
    }

    [ContextMenu("Reset Agents")]
    public void ResetAgents()
    {
        Init();

        for (int i = 0; i < _agents.Length; i++)
        {
            var agent = _agents[i];
            ResetAgentPosition(agent, i);
            SetUpRewardBallTag(agent, i);
        }
    }
}
