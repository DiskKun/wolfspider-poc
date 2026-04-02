using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{

    Rigidbody rb;

    [SerializeField]
    private float speed = 1; // speed to add to velocity every physics update
    [SerializeField]
    [Tooltip("Amount of friction to be applied every physics update. Values closer to 1 stop more quickly, smaller values are more slippery. Cannot go above 1 or below 0.")]
    [Range(0f, 1f)]
    private float friction = 0.2f; // amount to adjust velocity by every physics update (1 - friction, e.g. 0.2 friction multiplies velocity by 0.8 every update)
                                   // doing it this way lets us define custom physics
                                   // and overall allows for more control, customizability, etc.
                                   // generally resulting in better-feeling movement
    [SerializeField]
    [Tooltip("Amount of gravity to be applied each frame. Higher values apply more gravity. Negative values will make things float up, so be careful!")]
    private float gravity = 1f; // we use custom gravity as well for more control over the game feel

    [Space(20)]
    [SerializeField]
    [Tooltip("Amount to multiply movement speed by while sprinting.")]
    private float sprintMultiplier = 2f;

    private float sprintInput;

    [Space(20)]
    [SerializeField]
    [Tooltip("Strength of the velocity impulse added to the player when pouncing.")]
    private float pounceStrength = 10;
    [SerializeField]
    [Tooltip("Strength of the vertical impulse added to the player when pouncing.")]
    private float pounceJumpStrength = 1;
    [Tooltip("Time (in seconds) before the player can pounce again.")]
    public float pounceCooldown = 1;
    [NonSerialized]
    public float pCDTimer = 0; // internal cooldown timer
    [Tooltip("Amount of time after pouncing that movement keys have no effect. Clamped to Pounce Cooldown.")]
    public float pounceDuration = 0.1f; // time in seconds for the pounce to lock you out of movement, no greater than pounceCooldown
    [Tooltip("How far away from the mouse we should stop pouncing from.")]
    public float pounceMouseYield = 0.1f; // distance from the mouse that the pounce should be canceled from

    private Vector2 pInput; // player movement input
    private bool pounceInput; // pounce input 
    private Vector3 pounceEndpoint;

    private Vector3 vel; // velocity for use with movement
    private float minVelToTurn = 0.2f;

    [Space(20)]

    [Tooltip("How far away (in units) the player's mouse can be from an NPC to be counted as \"hovered.\"")]
    public float NPCHoverDist;

    [Tooltip("How far away (in units) to detect entities from, for interaction purposes.")]
    public float entityDetectionRadius;
    //[SerializeField]
    private bool turnTowardsEnemies; // enable and disable this feature

    private Vector3 targetPos; // enemy/NPC to target
    private GameObject targetObject; // GameObject belonging to the target

    private Transform webIconTransform;

    private AudioSource audioSource; //audio source to play SFX
    [Space(40)]

    public SFX_Pounce SFX_Pounce;
    private AudioClip[] pounceSFX;

    public SFX_Eat SFX_Eat;
    private AudioClip eatSFX;

    public SFX_Walk SFX_Walk;
    private AudioClip[] walkSFX;
    private float walkRepeatDelay = 0.25f; // delay between footstep sounds
    private float walkSoundTimer = 0f;

    [NonSerialized]
    public bool movementPaused = false;

    public Vector3[] spawnPoints; // where to spawn for each level
    private int level;

    [SerializeField]
    private Image fadeOut; // image to fade over everything else when teleporting
    [SerializeField]
    private float teleportDelay; // how long to wait before teleporting to the next level
    private float tpTimer; // the actual timer
    private GameManager gm;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // get attached rigidbody (we'll be using a capsule collider for 3D movement
        vel = new Vector3(); // reset velocity
        pounceDuration = Mathf.Clamp(pounceDuration, 0f, pounceCooldown); // clamp pounce duration so that it's not longer than cooldown to prevent weirdness from the implementation
        pCDTimer = 0; // reset cooldown
        webIconTransform = GameObject.Find("WebIcon").GetComponent<Transform>(); // using find here because I'm lazy and it shouldn't be a performance hit at this small of a scale
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        audioSource = GetComponent<AudioSource>();

        pounceSFX = SFX_Pounce.pounceSFX; // load audio hooks
        eatSFX = SFX_Eat.eatSFX;
        walkSFX = SFX_Walk.walkSFX;

        level = 0; // reset level
    }

    // Update is called once per frame
    void Update()
    {
        if (movementPaused) { rb.linearVelocity = new Vector3(); return; }
        
        if (pCDTimer > 0) { pCDTimer -= Time.deltaTime; } // reduce cooldown timer

        if (pounceCooldown - pCDTimer >= pounceDuration) {
            pInput.x = Input.GetAxisRaw("Horizontal"); // we use two separate operations here as it's slightly
            pInput.y = Input.GetAxisRaw("Vertical");   // more efficient than creating a new Vector2 every frame
            if (Mathf.Abs(pInput.x) > 0 || Mathf.Abs(pInput.y) > 0) // if the player is inputting movement
            {
                walkSoundTimer -= Time.deltaTime;
                if (walkSoundTimer <= 0f) // play footstep sound on a repeating timer
                {
                    walkSoundTimer = walkRepeatDelay/sprintInput;
                    audioSource.PlayOneShot(walkSFX[UnityEngine.Random.Range(0,walkSFX.Length-1)], 0.1f);
                }
            }
        } else
        {
            pInput.x = 0; pInput.y = 0; // keep them at zero for the duration
            walkSoundTimer = 0; // reset footstep timer to queue up a walk SFX as soon as the player starts moving again
        }
        
        if (Input.GetMouseButtonDown(0)) // queue up a pounce in Update to ensure responsiveness
        {
            var entityHit = detectEntities();
            var dist = new Vector3();
            if (entityHit == "NPC")
            {
                dist = webIconTransform.position - targetObject.GetComponent<Transform>().position;
                dist.y = 0;
            }

            if (entityHit == "NPC" && Vector3.Magnitude(dist) <= NPCHoverDist)
            {
                targetObject.GetComponent<NPC_Base>().NPC_Interaction.Invoke(); // call the interaction function
                
            } else if (pCDTimer <= 0)
            {
                WebControl wc = webIconTransform.gameObject.GetComponent<WebControl>();
                if (!wc.canPull && !wc.canBridge && !wc.pulling)
                {
                    pounceInput = true; pCDTimer = pounceCooldown; pounceEndpoint = webIconTransform.position;
                    
                }
                
            }
            
        }
        if (Input.GetButton("Sprint")) { sprintInput = sprintMultiplier; } else { sprintInput = 1; } // use floats instead of bools here to save on operations and make code a bit cleaner

        if (tpTimer > 0)
        {
            tpTimer -= Time.deltaTime;
            fadeOut.color = new Color(0, 0, 0, 1-(tpTimer / teleportDelay)); // fade to black
            if (tpTimer <= 0) { teleportToNextLevel(); }
        } else if (tpTimer < 0)
        {
            tpTimer = Math.Min(0, tpTimer + Time.deltaTime);
            fadeOut.color = new Color(0, 0, 0, (Math.Max(-teleportDelay, -tpTimer) / teleportDelay)); // fade back in
        }
    }

    private void FixedUpdate()
    {
        if (movementPaused) { rb.linearVelocity = new Vector3(); return; }

        RaycastHit rh;
        if ( Physics.SphereCast(transform.position, 0.25f, Vector3.down, out rh, 0.27f) ) { vel.y = 0; }

        vel += Vector3.Normalize(new Vector3(pInput.x, 0, pInput.y)) * speed * sprintInput * Time.deltaTime; // adjust player velocity, normalize increase to ensure consistency
        vel += new Vector3(0, -gravity, 0) * Time.deltaTime; // apply gravity
        /*if (new Vector3(vel.x, 0, vel.y).magnitude > minVelToTurn) // as long as the player is moving more than a certain amount...
        {
            // rotate the transform based on velocity
            transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(vel.z, vel.x) * Mathf.Rad2Deg + 90), 0); //rotate to proper angle
        }

        if (detectEnemies() != "none" && turnTowardsEnemies)
        {
            // rotate to face nearby enemy
            transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(targetPos.z - transform.position.z, targetPos.x - transform.position.x) * Mathf.Rad2Deg + 90), 0); //rotate to proper angle
        }*/

        var mVect = transform.position - webIconTransform.position;
        transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(mVect.z, mVect.x) * Mathf.Rad2Deg - 90), 0); //rotate to face mouse

        if (pounceInput)
        {
            // apply velocity based on player ANGLE rather than INPUT. This means players can still pounce even when not specifically moving.
            vel = Vector3.Normalize(new Vector3(-Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y), 0, -Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y))) * pounceStrength * Time.deltaTime;
            vel.y = pounceJumpStrength;
            audioSource.PlayOneShot(pounceSFX[UnityEngine.Random.Range(0, pounceSFX.Length - 1)], 0.5f); // play pounce SFX
            pounceInput = false;
        }



        vel = new Vector3(vel.x * (1 - friction), vel.y, vel.z * (1 - friction)); //reduce horizontal velocity based on specified friction
        if (pounceCooldown - pCDTimer < pounceDuration)
        {
            var dist = new Vector3();
            dist = transform.position - pounceEndpoint;
            dist.y = 0;
            if (Vector3.Magnitude(dist) <= pounceMouseYield)
            {
                vel = new Vector3(vel.x * (0.5f), vel.y, vel.z * (0.5f)); //apply strong friction if pouncing and nearby endpoint so as not to overshoot
            }
            
        }
        rb.linearVelocity = vel; //update player velocity

    }

    private string detectEntities()
    {
        Vector3 pos = transform.position;

        Collider[] hits = Physics.OverlapSphere(pos, entityDetectionRadius); // run a spherecast;
        string hitPest = null;

        if (hits.Length > 0) // if we've hit something
        {
            float maxDistance = 99999; // set this to a ludicrously high value first - we'll be using this to recognize the closest enemy

            for (int i = 0; i < hits.Length; i++)
            {
                GameObject c = hits[i].gameObject; // store a reference to the hit object's GameObject

                if (c.tag == "Pest") // we only want to check pests
                {
                    float cDist = Vector3.Distance(transform.position, c.transform.position);
                    if (cDist < maxDistance)
                    {
                        maxDistance = cDist; // store the new closest enemy distance
                        targetPos = c.transform.position; // update target position if the enemy is closer than the previous distance
                        hitPest = "Pest";
                    }
                } else if (c.tag == "Baby") // but we PRIORITIZE babies
                {
                    targetPos = c.transform.position; // immediately set target position
                    return "Baby"; // we don't need anything else since this takes absolute priority

                } else if (c.tag == "NPC") // we also prioritize NPCs to interact, although slightly less than babies.
                {
                    targetPos = c.transform.position; // immediately set target position
                    targetObject = c.gameObject; // we need to get the NPC's GameObject for interaction functionality
                    return "NPC"; // we don't need anything else since this takes priority
                }

            }
        }


        return hitPest;
    }

    public void playEatSound()
    {
        audioSource.PlayOneShot(eatSFX, 1f);
    }

    private void OnDrawGizmos()
    {
        if (turnTowardsEnemies)
        {
            Gizmos.color = new Color(0,0,1,0.25f); // transparent blue
            Gizmos.DrawSphere(transform.position, entityDetectionRadius);
        }
    }

    public void startTeleportTimer()
    {
        tpTimer = teleportDelay;
    }
    void teleportToNextLevel()
    {
        level++;
        if (level < spawnPoints.Length)
        {
            transform.position = spawnPoints[level];
            tpTimer = -teleportDelay - 1;
        } else
        {
            SceneManager.LoadScene(gm.NextScene); // load next scene from black
        }
        
        
    }
}
