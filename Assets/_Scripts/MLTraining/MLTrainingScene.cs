using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLTrainingScene : MonoBehaviour
{

    public GameObject roadsParent;
    public GameObject driverAgentPrefab;
    public GameObject rewardingBall;

    private NodePath[] _nodePaths;
    private MLDriverAgent _driverAgent;
    private PathCrawler _pathCrawler;
    public int startingPathIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        _nodePaths = roadsParent.transform.GetComponentsInChildren<NodePath>();
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateRewardballPosition();

        if (_pathCrawler.CheckChangedNodes())
        {
            UpdateRewardballPosition();
        }
    }

    public void InitializeScene(MLDriverAgent agent)
    {
        NodePath startingPath = _nodePaths[startingPathIndex]; // _nodePaths[Random.Range(0, _nodePaths.Length)];
        Quaternion startingRot = Quaternion.LookRotation(startingPath.nodes[1] - startingPath.nodes[0]);
        _pathCrawler = agent.GetComponent<PathCrawler>();

        agent.transform.position = startingPath.nodes[0];
        agent.transform.position += agent.transform.forward * -0.5f;
        agent.transform.rotation = startingRot;
        agent.transform.parent = transform;
        agent.GetComponent<Rigidbody>().velocity = Vector3.zero;

        _pathCrawler.Initialize(startingPath);
        var nextNode = _pathCrawler.nextThreeNodes[1];

        rewardingBall.transform.position = new Vector3(nextNode.x, 5f, nextNode.y);
        _pathCrawler.CheckChangedNodes(clear: true);
        //UpdateRewardballPosition();
        _driverAgent = agent;
    }

    private void UpdateRewardballPosition()
    {
        rewardingBall.transform.position = new Vector3(_pathCrawler.currentNodePosition.x, 0.2f, _pathCrawler.currentNodePosition.z);
    }
}
