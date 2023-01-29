using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPercepts : MonoBehaviour
{
    public class RaycastInfo {
        public float forwardOffset;
        public float sidewaysOffset;
        public float verticalOffset;
        public float angleFromForward;
        public float distance;
        public Vector3 hitPoint;
        public GameObject hitObject;

        public RaycastInfo(float forwardOffset, float sidewaysOffset, float verticalOffset,
                           float angleFromForward)
        {
            this.forwardOffset = forwardOffset;
            this.sidewaysOffset = sidewaysOffset;
            this.verticalOffset = verticalOffset;
            this.angleFromForward = angleFromForward;
            this.distance = 0f;
            this.hitPoint = Vector3.zero;
            this.hitObject = null;
        }

        public Vector3 GetOrigin(Transform transform) {
            return transform.position + transform.forward * forwardOffset +
                transform.right * sidewaysOffset + transform.up * verticalOffset;
        }

        public Vector3 GetDirection(Transform transform) {
            return Quaternion.Euler(0, angleFromForward, 0) * transform.forward;
        }
    }

    [Header("Car Raycast Configuration")]
    [SerializeField][Range(0, 20)]
    [Tooltip("Number of pairs of rays to cast in addition to one ray in the center")]
    private int numRaycastPairs = 15;
    [SerializeField]
    [Tooltip("The length of the rays cast in units (default 10)")]
    private float _rayLength = 10f;
    [SerializeField][Range(1, 120)]
    [Tooltip("Raycasts will be cast evenly spaced around the car's front up to this angle")]
    private float maxRaycastAngle = 85f;
    [SerializeField]
    [Tooltip("The distance between the center and the front of the car")]
    private float _forwardRaycastOffset = 0.75f;
    [SerializeField]
    [Tooltip("The y distance between the car's origin and where the raycast originates")]
    private float _verticalRaycastOffset = -0.1f;


    private PathCrawler _pathCrawler;
    private bool _collidedWithObject = false;
    private bool _triggeredWithObject = false;
    private string _collidedWithObjectTag;
    private string _triggeredWithObjectTag;

    [HideInInspector]
    public int approachingTrafficSignalType = -1;
    public List<RaycastInfo> raycasts = new();
    [HideInInspector]
    public List<float> raycastCollisionDistances;

    void Start()
    {
        raycasts = new List<RaycastInfo>();
        HandleRaycastCountChange();
        raycastCollisionDistances = new List<float>();

        foreach (RaycastInfo raycast in raycasts)
        {
            raycastCollisionDistances.Add(-1);
        }

        if (!TryGetComponent<PathCrawler>(out _pathCrawler))
        {
            Debug.Log("CarPercepts must be attached to a PathCrawler");
        }
    }

    void FixedUpdate()
    {
        //HandleRaycastCountChange();
        //CheckCollisions();
        //GetCollisions(out raycastCollisionDistances);
        // DrawDebugLines();
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag != "Street")
        {
            _collidedWithObject = true;
            _collidedWithObjectTag = other.gameObject.tag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Street")
        {
            _triggeredWithObject = true;
            _triggeredWithObjectTag = other.gameObject.tag;
        }
    }

    public bool CollidedWithObject(out string tag, bool clear = false)
    {
        tag = "";
        if (_collidedWithObject)
        {
            if (clear)
            {
                _collidedWithObject = false;
            }
            tag = _collidedWithObjectTag;
            return true;
        }
        return false;
    }

    public bool TriggeredWithObjects(out string triggerTag, bool clear)
    {
        triggerTag = "";
        if (_triggeredWithObject)
        {
            if (clear)
            {
                _triggeredWithObject = false;
            }
            triggerTag = _triggeredWithObjectTag;
            return true;
        }
        return false;
    }

    public bool GetCollisions(out List<float> distances, string objTag="") {
        distances = new List<float>();
        foreach (RaycastInfo raycast in raycasts) {
            if (raycast.hitObject != null && raycast.hitObject != transform.GetChild(0).gameObject) {
                if (objTag == "" || raycast.hitObject.tag == objTag) {
                    distances.Add(raycast.distance);
                    continue;
                }
            }
            distances.Add(-1);
        }
        return distances.Count > 0;
    }



    void CheckCollisions()
    {
        foreach (RaycastInfo raycast in raycasts)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycast.GetOrigin(transform), raycast.GetDirection(transform), out hit,
                                _rayLength))
            {
                if (hit.collider.gameObject != transform.GetChild(0).gameObject)
                {
                    raycast.hitObject = hit.collider.gameObject;
                    raycast.distance = hit.distance;
                    raycast.hitPoint = hit.point;
                    continue;
                }
            }
            raycast.hitObject = null;
            raycast.distance = -1;
            raycast.hitPoint = Vector3.zero;
        }
    }

    void HandleRaycastCountChange()
    {
        if (numRaycastPairs == 0)
            return;
        if (raycasts.Count != numRaycastPairs * 2 + 1 ||
            raycasts[raycasts.Count - 1].angleFromForward != maxRaycastAngle)
        {
            raycasts.Clear();
            raycasts.Add(new RaycastInfo(_forwardRaycastOffset, 0, _verticalRaycastOffset, 0));
            for (int i = 0; i < numRaycastPairs; i++)
            {
                float angle = (i + 1) / (float)numRaycastPairs * maxRaycastAngle;
                raycasts.Add(new RaycastInfo(_forwardRaycastOffset, 0, _verticalRaycastOffset, angle));
                raycasts.Add(new RaycastInfo(_forwardRaycastOffset, 0, _verticalRaycastOffset, -angle));
            }
        }
    }

    [ExecuteInEditMode]

    private void OnDrawGizmosSelected()
    {
        DrawDebugLines();
    }

    void DrawDebugLines()
    {
        foreach (RaycastInfo raycast in raycasts)
        {
            Color color = raycast.hitObject == null ? Color.red : Color.green;
            Vector3 origin = raycast.GetOrigin(transform);
            Vector3 endPoint = origin + raycast.GetDirection(transform).normalized * _rayLength;
            Debug.DrawLine(origin, endPoint, color);
        }
    }
}
