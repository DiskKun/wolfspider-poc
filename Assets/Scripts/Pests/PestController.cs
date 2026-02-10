using UnityEngine;

public class PestController : MonoBehaviour
{

    [Tooltip("Add transforms of nodes here for pests to follow. Pests will loop back to the first node in the list after reaching the final node.")]
    public Transform[] pathNodes;
    [Tooltip("The amount of time the pest should wait at each node for before moving on to the next.")]
    public float nodePause;
    [Tooltip("The maximum distance (in Units) that the pest can target away from the next node. Setting this to 0 will make the pest target the node perfectly every time.")]
    public float nodeDistance;
    [Tooltip("How fast the pest should move (in Units per second).")]
    public float speed;

    private float targetAcceptanceRange = 0.5f; // constant value that provides a bit of leeway for how far away the pest can be from its target and still count it as "reached"

    private Vector3 target = new Vector3();
    private int nodeID;
    private Vector3 spawnPosition = new Vector3();

    private float pauseTimer;

    // Update is called once per frame
    void Update()
    {
        if (pauseTimer <= 0)
        {
            Vector3 moveDir = new Vector3(target.x - transform.position.x, target.y - transform.position.y, target.z - transform.position.z).normalized; // get direction of movement
            moveDir *= speed; moveDir *= Time.deltaTime;
            transform.position = new Vector3(transform.position.x + moveDir.x, transform.position.y + moveDir.y, transform.position.z + moveDir.z); // move pest
            if (Time.timeScale > 0) { transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(moveDir.z, moveDir.x) * Mathf.Rad2Deg), 0); } //rotate to proper angle

            if (Vector3.Distance(transform.position, target) <= targetAcceptanceRange) // activate when within range of target location AND if path nodes have been assigned
            {
                pauseTimer = nodePause; // reset pause timer

                nodeID++;
                if (nodeID == pathNodes.Length)
                {
                    nodeID = 0; // loop back to first node in list
                }

                getTargetPos(nodeID); // get new target
            }
            
        } else
        {
            pauseTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerControl pc = other.gameObject.GetComponent<PlayerControl>();
        if (other.gameObject.tag == "Player" && pc.pounceCooldown - pc.pCDTimer <= pc.pounceDuration) //collision with player while pouncing
        {
            pc.webSilkAmount += 1;
            pc.playEatSound(); // tell the player to play the eat sound effect
            gameObject.SetActive(false); // make this inactive to free up more spawns
        }
    }

    private void OnEnable() // when this object is set active (spawned) by its parent spawner, also when scene is loaded if this is a standalone enemy
    {
        spawnPosition = transform.position; // set fallback spawn position in case path nodes have not been assigned

        nodeID = 0;
        pauseTimer = 0;

        if (pathNodes.Length > 0)
        {
            getTargetPos(nodeID);
        }
        
    }

    private void getTargetPos(int node)
    {
        float randomDir = Random.Range(0f, Mathf.PI * 2); // random direction around a circle
        float randomDist = Random.Range(0f, nodeDistance); // random distance from target

        Vector3 tPos;

        if (pathNodes.Length > 0) // if nodes have been assigned
        {
            tPos = pathNodes[node].position; // next node position
        } else
        {
            tPos = spawnPosition; // fallback position
        }

        target = new Vector3((Mathf.Cos(randomDir) * randomDist), 0, (Mathf.Sin(randomDir) * randomDist)).normalized; // set new target offset
        target = new Vector3(target.x + tPos.x, tPos.y, target.z + tPos.z); // set new target position PLUS offset

    }
}
