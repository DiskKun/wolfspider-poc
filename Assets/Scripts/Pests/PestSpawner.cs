using Unity.VisualScripting;
using UnityEngine;

public class PestSpawner : MonoBehaviour
{
    [SerializeField] [Tooltip("The pest prefab to spawn.")]
    private GameObject pestToSpawn;
    [SerializeField] [Tooltip("The maximum number of pests that can be spawned by this spawner at one time.")]
    private int maxSpawns;
    [SerializeField] [Tooltip("How long to wait between spawns.")]
    private float spawnDelay;

    [Space(20)]

    [SerializeField] [Tooltip("Add transforms of nodes here for spawned pests to follow. Pests will loop back to the first node in the list after reaching the final node.")]
    private Transform[] pathNodes;
    [SerializeField] [Tooltip("The amount of time spawned pests should wait at each node for before moving on to the next.")]
    private float nodePause;
    [SerializeField] [Tooltip("The maximum distance (in Units) that spawned pests can target away from the next node. Setting this to 0 will make the pest target the node perfectly every time.")]
    private float nodeDistance;
    [SerializeField] [Tooltip("How fast spawned pests should move (in Units per second).")]
    private float speed;

    private GameObject[] pestPool;
    private float spawnTimer;

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "pestSpawner.png", true);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pestPool = new GameObject[maxSpawns];

        for (int i = 0; i < maxSpawns; i++)
        {
            pestPool[i] = Instantiate(pestToSpawn, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity); // set up object pool for performance optimization

            PestController pc = pestPool[i].GetComponent<PestController>(); // set relevant data in spawned pest
            pc.pathNodes = pathNodes;
            pc.nodePause = nodePause;
            pc.nodeDistance = nodeDistance;
            pc.speed = speed;

            pestPool[i].SetActive(false); // immediately disable spawned pest

        }
        spawnTimer = spawnDelay;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            int freePest = checkActivePests();
            if (freePest >= 0) //check to see if a pooled pest is not currently in use
            {
                pestPool[freePest].transform.position = transform.position; // set the pest to the spawner's position
                pestPool[freePest].transform.rotation = Quaternion.identity; // reset rotation
                pestPool[freePest].SetActive(true); // activate this pest
            }
            spawnTimer = spawnDelay; // reset spawn timer regardless of whether pest was spawned or not
        }
    }

    private int checkActivePests()
    {

        for (int i = 0; i < pestPool.Length; i++)
        {
            if (!pestPool[i].activeSelf)
            {
                return i; // immediately breaks the loop, returning the first available pest
            }
        }

        return -1; // if it makes it this far, no pests are available
    }
}
