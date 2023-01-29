using PathCreation;
using UnityEngine;

public class MLTrainingScene : MonoBehaviour
{
    [SerializeField] private GameObject _roadsParent;
    [SerializeField] private GameObject _agentPrefab;
    //[SerializeField] private int agentsCount = 5;
    [SerializeField] private float _startPositionOffset = 0.5f;
    [SerializeField] private float _offsetFromGround = 0.5f;

    private NodePath[] _nodePaths;
    private MLDriverAgent[] _agents;

    void Awake()
    {
        Init();
    }
    private void Init()
    {
        _nodePaths = _roadsParent.transform.GetComponentsInChildren<NodePath>();
        _agents = GetComponentsInChildren<MLDriverAgent>();
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

    public void ResetAgent(MLDriverAgent agent, int startingPathIndex)
    {
        agent.StartingPathIndex = startingPathIndex;

        NodePath startingPath = _nodePaths[startingPathIndex];
        ResetAgentPosition(agent, startingPathIndex);
        agent.ResetRigidBody();
        agent.PathCrawler.Initialize(startingPath);

        var nextNode = agent.PathCrawler.nextThreeNodes[1];
        agent.SetPositionRewardingBall(new Vector3(nextNode.x, 5f, nextNode.y));

        agent.PathCrawler.CheckChangedNodes(clear: true);
    }

    private void ResetAgentPosition(MLDriverAgent agent, int startingPathIndex)
    {
        agent.transform.position = GetPosition(startingPathIndex);
        agent.transform.position -= agent.transform.forward * _startPositionOffset;
        agent.transform.rotation = GetRotation(startingPathIndex);
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
            ResetAgentPosition(_agents[i], i);
        }
    }
}
