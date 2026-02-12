using UnityEngine;

public class NPC_Guide : NPC_Base
{
    [Tooltip("Reference to the dialogue manager gameobject")]
    public DialogueManager dm;

    [Tooltip("The ID of each STARTING line of dialogue.")]
    public string[] dialogueIDSequence = { };

    [SerializeField]
    [Tooltip("Whether this guide NPC should require the player to interact with them before moving to the next location. If left unchecked, the NPC will move on as soon as the player gets within range.")]
    [Header("WARNING: If this is enabled, this \nGameObject MUST have the \"NPC\" tag!")]
    private bool requireInteract;

    [Space(20)]

    [SerializeField]
    [Tooltip("The range at which this NPC will detect the player and move to its next location automatically. Only required if Require Interact is toggled on.")]
    private float detectionRadius;

    [SerializeField]
    [Tooltip("How fast the guide should move (in Units per second).")]
    private float speed;

    [SerializeField]
    [Tooltip("A list of nodes for the guide to visit, in order. The guide will stop at the final node.")]
    private Transform[] guideNodes;
    private int nodeID;
    private Vector3 target;

    private float targetAcceptanceRange = 0.5f; // constant value that provides a bit of leeway for how far away the pest can be from its target and still count it as "reached"


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start(); // we call the base start function to ensure everything is set up correctly

        // instantiate class-specific variables below
        target = transform.position; // set target to current position until player approaches or interacts

    }

    // Update is called once per frame
    void Update() // no need to override since the parent class has no update function
    {
        if (!requireInteract && Vector3.Distance(transform.position, target) <= targetAcceptanceRange) // if it's reached its target, is within detection range of the player, and doesn't require an interaction to move on
        {
            if (nodeID < guideNodes.Length && detectPlayer()) // if the guide hasn't reached the end of the path, move to the next node
            {
                target = guideNodes[nodeID].transform.position;
                nodeID++;
            }
        }

        if (Vector3.Distance(transform.position, target) >= targetAcceptanceRange) // if we aren't close enough to the target, then...
        {
            Vector3 moveDir = new Vector3(target.x - transform.position.x, target.y - transform.position.y, target.z - transform.position.z).normalized; // get direction of movement
            moveDir *= speed; moveDir *= Time.deltaTime;
            transform.position = new Vector3(transform.position.x + moveDir.x, transform.position.y + moveDir.y, transform.position.z + moveDir.z); // move guide
        }

    }

    protected override void OnInteract()
    {
        // interaction code here
        dm.ShowDialogue(dialogueIDSequence[nodeID]);
        NextNode();
    }

    public void NextNode()
    {
        target = guideNodes[nodeID].transform.position;
        nodeID++;
    }

    private bool detectPlayer()
    {
        Vector3 pos = transform.position;

        Collider[] hits = Physics.OverlapSphere(pos, detectionRadius); // run a spherecast;

        if (hits.Length > 0) // if we've hit something
        {
            
            for (int i = 0; i < hits.Length; i++)
            {
                GameObject c = hits[i].gameObject; // store a reference to the hit object's GameObject

                if (c.tag == "Player") // we only want to check for the player
                {
                    return true;
                }
                

            }
        }


        return false;
    }

    private void OnDrawGizmos()
    {
        if (!requireInteract)
        {
            Gizmos.color = new Color(1, 1, 0, 0.25f); // transparent yellow
            Gizmos.DrawSphere(transform.position, detectionRadius);
        }
    }
}
