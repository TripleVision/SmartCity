using UnityEngine;

public class MLTrainingScene : MonoBehaviour
{
    [SerializeField] private GameObject _roadsParent;
    [SerializeField] private GameObject _agentPrefab;
    [SerializeField] private int agentsCount = 5;
    [SerializeField] private float _startPositionOffset = 0.5f;

    private NodePath[] _nodePaths;
    private MLDriverAgent[] _agents;

    void Start()
    {
        _nodePaths = _roadsParent.transform.GetComponentsInChildren<NodePath>();
        //_agents = GetComponentsInChildren<MLDriverAgent>();
        InitializeScene();
    }

    public void InitializeScene()
    {
        for (int i = 0; i < agentsCount; i++)
        {
            var startingPathIndex = i;
            SpawnAgent(startingPathIndex);
        }
    }

    private void SpawnAgent(int startingPathIndex)
    {
        var agentClone = Instantiate(_agentPrefab, GetPosition(startingPathIndex), GetRotation(startingPathIndex), transform);
        MLDriverAgent agent = agentClone.GetComponent<MLDriverAgent>();
        ResetAgent(agent, startingPathIndex);
    }

    public void ResetAgent(MLDriverAgent agent, int startingPathIndex)
    {
        agent.StartingPathIndex = startingPathIndex;

        NodePath startingPath = _nodePaths[startingPathIndex];

        agent.transform.position = GetPosition(startingPathIndex);
        agent.transform.position -= agent.transform.forward * _startPositionOffset;
        agent.transform.rotation = GetRotation(startingPathIndex);
        agent.ResetRigidBody();

        agent.PathCrawler.Initialize(startingPath);

        var nextNode = agent.PathCrawler.nextThreeNodes[1];
        agent.SetPositionRewardingBall(new Vector3(nextNode.x, 5f, nextNode.y));

        agent.PathCrawler.CheckChangedNodes(clear: true);
    }

    private Quaternion GetRotation (int pathIndex)
    {
        NodePath path = _nodePaths[pathIndex];
        return Quaternion.LookRotation(path.nodes[1] - path.nodes[0]);
    }

    private Vector3 GetPosition(int pathIndex)
    {
        NodePath path = _nodePaths[pathIndex];
        return path.nodes[0];
    }


}
