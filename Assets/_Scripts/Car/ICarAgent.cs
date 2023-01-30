
using Unity.MLAgents;
using UnityEngine;

namespace Assets._Scripts.Car
{
    public interface ICarAgent
    {
        public GameObject RewardingBall { get; }
        public Transform Transform { get; }
        public int StartingPathIndex { get; set; }
        public PathCrawler PathCrawler { get;  }
        void ResetRigidBody();
        void SetPositionRewardingBall(Vector3 position);
        void SetDetectableBallTag(string tag);
    }
}
