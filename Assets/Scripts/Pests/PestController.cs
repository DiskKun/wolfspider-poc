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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerControl pc = other.gameObject.GetComponent<PlayerControl>();
        if (other.gameObject.tag == "Player" && pc.pounceCooldown - pc.pCDTimer <= pc.pounceDuration) //collision with player while pouncing
        {
            pc.webSilkAmount += 1;
            Destroy(gameObject);
        }
    }
}
