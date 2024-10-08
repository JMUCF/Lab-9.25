using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Avoider : MonoBehaviour
{
    public GameObject target;

    public GameObject node;
    Vector3 finalTargetNode = Vector3.zero;
    public List<GameObject> nodes = new List<GameObject>();
    public List<GameObject> viableNodes = new List<GameObject>();

    private NavMeshAgent agent;
    [Header("NavMesh Agent Stats")]
    [SerializeField] private float range, speed;


    public LayerMask layer;
    public bool showGizmos;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if(agent == null) { Debug.LogWarning(this + " needs a navmesh agent Component and baked nav mesh surface to run!"); }
        layer = 1 << 6;

        agent.speed = speed;
        range = 3f;
        StartCoroutine("GeneratePoissonDisc");
    }

    private void Update()
    {
        transform.LookAt(target.transform.position);
        MoveToNode(finalTargetNode);
    }

    IEnumerator GeneratePoissonDisc()
    {
        while (true)
        {
            Debug.Log("in generate poisson disc enumerator");
            foreach (var point in nodes)
            {
                Destroy(point);
            }
            nodes.Clear();
            viableNodes.Clear();
            PoissonDiscSampler2 sampler = new PoissonDiscSampler2(5, 5, 1f);
            foreach (Vector2 sample in sampler.Samples())
            {
                GameObject temp = Instantiate(node, new Vector3(sample.x, 0, sample.y), Quaternion.identity);
                nodes.Add(temp);
                CheckIfNodeVisible(temp);
            }
            finalTargetNode = PickBestNode();
            yield return new WaitForSeconds(2.5f);
        }
    }

    private void CheckIfNodeVisible(GameObject node)
    //Raycast from each point to the player. Currently it does not recognize if a node is visible by the player to mark it as red, it only recognizes which ones are viable, which I guess is all we need
    {
        RaycastHit hit;
        Renderer nodeRenderer = node.GetComponent<Renderer>();

        Vector3 directionToTarget = (target.transform.position - node.transform.position).normalized;

        if (Physics.Raycast(node.transform.position, directionToTarget, out hit, range, layer))
        {
            if(hit.transform.gameObject == target)
            {
                nodeRenderer.material.color = Color.red;
            }
            else
            {
                nodeRenderer.material.color = Color.green;
                viableNodes.Add(node);
            }
        }
    }

    private Vector3 PickBestNode()
    {
        //loop through all the nodes
        //calculate distance to node for each one
        //move to the closest node
        float lowestValue = 100f;
        float temp;

        Vector3 targetNode = Vector3.zero;

        foreach(var node in viableNodes)
        {
            temp = Vector3.Distance(transform.position, node.transform.position);
            Debug.Log(temp);
            if(temp < lowestValue)
            {
                lowestValue = temp;
            targetNode = node.transform.position;
            }
        }
        Debug.Log(lowestValue);
        Debug.Log(targetNode);
        return targetNode;
    }

    private void MoveToNode(Vector3 targetPos)
    {
        //var step = speed * Time.deltaTime;
        //transform.position = age;
        agent.SetDestination(targetPos);
    }
    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            foreach (GameObject point in nodes) //draws a ray from all of the points to the player up to the max range
            {
                Vector3 directionToTarget = (transform.position - point.transform.position).normalized;
                Gizmos.color = Color.red;
                Gizmos.DrawRay(point.transform.position, directionToTarget * range);
            }
        }
        
    }
}

public class PoissonDiscSampler2
{
    private const int k = 30;  // Maximum number of attempts before marking a sample as inactive.

    private readonly Rect rect;
    private readonly float radius2;  // radius squared
    private readonly float cellSize;
    private Vector2[,] grid;
    private List<Vector2> activeSamples = new List<Vector2>();

    /// Create a sampler with the following parameters:
    ///
    /// width:  each sample's x coordinate will be between [0, width]
    /// height: each sample's y coordinate will be between [0, height]
    /// radius: each sample will be at least `radius` units away from any other sample, and at most 2 * `radius`.
    public PoissonDiscSampler2(float width, float height, float radius)
    {
        rect = new Rect(0, 0, width, height);
        radius2 = radius * radius;
        cellSize = radius / Mathf.Sqrt(2);
        grid = new Vector2[Mathf.CeilToInt(width / cellSize),
                           Mathf.CeilToInt(height / cellSize)];
    }

    /// Return a lazy sequence of samples. You typically want to call this in a foreach loop, like so:
    ///   foreach (Vector2 sample in sampler.Samples()) { ... }
    public IEnumerable<Vector2> Samples()
    {
        // First sample is choosen randomly
        yield return AddSample(new Vector2(Random.value * rect.width, Random.value * rect.height));

        while (activeSamples.Count > 0)
        {

            // Pick a random active sample
            int i = (int)Random.value * activeSamples.Count;
            Vector2 sample = activeSamples[i];

            // Try `k` random candidates between [radius, 2 * radius] from that sample.
            bool found = false;
            for (int j = 0; j < k; ++j)
            {

                float angle = 2 * Mathf.PI * Random.value;
                float r = Mathf.Sqrt(Random.value * 3 * radius2 + radius2); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
                Vector2 candidate = sample + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Accept candidates if it's inside the rect and farther than 2 * radius to any existing sample.
                if (rect.Contains(candidate) && IsFarEnough(candidate))
                {
                    found = true;
                    yield return AddSample(candidate);
                    break;
                }
            }

            // If we couldn't find a valid candidate after k attempts, remove this sample from the active samples queue
            if (!found)
            {
                activeSamples[i] = activeSamples[activeSamples.Count - 1];
                activeSamples.RemoveAt(activeSamples.Count - 1);
            }
        }
    }

    private bool IsFarEnough(Vector2 sample)
    {
        GridPos pos = new GridPos(sample, cellSize);

        int xmin = Mathf.Max(pos.x - 2, 0);
        int ymin = Mathf.Max(pos.y - 2, 0);
        int xmax = Mathf.Min(pos.x + 2, grid.GetLength(0) - 1);
        int ymax = Mathf.Min(pos.y + 2, grid.GetLength(1) - 1);

        for (int y = ymin; y <= ymax; y++)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                Vector2 s = grid[x, y];
                if (s != Vector2.zero)
                {
                    Vector2 d = s - sample;
                    if (d.x * d.x + d.y * d.y < radius2) return false;
                }
            }
        }

        return true;

        // Note: we use the zero vector to denote an unfilled cell in the grid. This means that if we were
        // to randomly pick (0, 0) as a sample, it would be ignored for the purposes of proximity-testing
        // and we might end up with another sample too close from (0, 0). This is a very minor issue.
    }

    /// Adds the sample to the active samples queue and the grid before returning it
    private Vector2 AddSample(Vector2 sample)
    {
        activeSamples.Add(sample);
        GridPos pos = new GridPos(sample, cellSize);
        grid[pos.x, pos.y] = sample;
        return sample;
    }

    /// Helper struct to calculate the x and y indices of a sample in the grid
    private struct GridPos
    {
        public int x;
        public int y;

        public GridPos(Vector2 sample, float cellSize)
        {
            x = (int)(sample.x / cellSize);
            y = (int)(sample.y / cellSize);
        }
    }
}