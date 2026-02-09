using System;
using Unity.VisualScripting;
using UnityEngine;

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

    [Tooltip("The amount of web silk the player currently has")]
    public int webSilkAmount = 0;

    private Vector2 pInput; // player movement input
    private bool pounceInput; // pounce input 

    private Vector3 vel; // velocity for use with movement
    private float minVelToTurn = 0.2f;

    [Space(20)]

    [Tooltip("How far away (in units) to detect enemies from, for pounce purposes.")]
    public float enemyDetectionRadius;
    [SerializeField]
    private bool turnTowardsEnemies; // enable and disable this feature

    private Vector3 targetPos; // enemy/NPC to target

    private AudioSource audioSource; //audio source to play SFX
    [Space(40)]

    public SFX_Pounce SFX_Pounce;
    private AudioClip pounceSFX;

    public SFX_Eat SFX_Eat;
    private AudioClip eatSFX;

    public SFX_Walk SFX_Walk;
    private AudioClip walkSFX;
    private float walkRepeatDelay = 0.1f; // delay between footstep sounds
    private float walkSoundTimer = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // get attached rigidbody (we'll be using a capsule collider for 3D movement
        vel = new Vector3(); // reset velocity
        pounceDuration = Mathf.Clamp(pounceDuration, 0f, pounceCooldown); // clamp pounce duration so that it's not longer than cooldown to prevent weirdness from the implementation
        pCDTimer = 0; // reset cooldown

        audioSource = GetComponent<AudioSource>();

        pounceSFX = SFX_Pounce.pounceSFX; // load audio hooks
        eatSFX = SFX_Eat.eatSFX;
        walkSFX = SFX_Walk.walkSFX;
    }

    // Update is called once per frame
    void Update()
    {
        if (pCDTimer > 0) { pCDTimer -= Time.deltaTime; } // reduce cooldown timer

        if (pounceCooldown - pCDTimer >= pounceDuration) {
            pInput.x = Input.GetAxisRaw("Horizontal"); // we use two separate operations here as it's slightly
            pInput.y = Input.GetAxisRaw("Vertical");   // more efficient than creating a new Vector2 every frame
            if (Mathf.Abs(pInput.x) > 0 || Mathf.Abs(pInput.y) > 0) // if the player is inputting movement
            {
                walkSoundTimer -= Time.deltaTime;
                if (walkSoundTimer <= 0f) // play footstep sound on a repeating timer
                {
                    walkSoundTimer = walkRepeatDelay;
                    audioSource.PlayOneShot(walkSFX);
                }
            }
        } else
        {
            pInput.x = 0; pInput.y = 0; // keep them at zero for the duration
            walkSoundTimer = 0; // reset footstep timer to queue up a walk SFX as soon as the player starts moving again
        }
        
        if (Input.GetButtonDown("Jump") && pCDTimer <= 0) { pounceInput = true; pCDTimer = pounceCooldown; } // queue up a pounce in Update to ensure responsiveness
        if (Input.GetButton("Sprint")) { sprintInput = sprintMultiplier; } else { sprintInput = 1; } // use floats instead of bools here to save on operations and make code a bit cleaner
    }

    private void FixedUpdate()
    {
        RaycastHit rh;
        if ( Physics.SphereCast(transform.position, 0.25f, Vector3.down, out rh, 0.27f) ) { vel.y = 0; }

        vel += Vector3.Normalize(new Vector3(pInput.x, 0, pInput.y)) * speed * sprintInput * Time.deltaTime; // adjust player velocity, normalize increase to ensure consistency
        vel += new Vector3(0, -gravity, 0) * Time.deltaTime; // apply gravity
        if (new Vector3(vel.x, 0, vel.y).magnitude > minVelToTurn) // as long as the player is moving more than a certain amount...
        {
            // rotate the transform based on velocity
            transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(vel.z, vel.x) * Mathf.Rad2Deg + 90), 0); //rotate to proper angle
        }

        if (detectEnemies() && turnTowardsEnemies)
        {
            // rotate to face nearby enemy
            transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(targetPos.z - transform.position.z, targetPos.x - transform.position.x) * Mathf.Rad2Deg + 90), 0); //rotate to proper angle
        }

        if (pounceInput)
        {
            // apply velocity based on player ANGLE rather than INPUT. This means players can still pounce even when not specifically moving.
            vel = Vector3.Normalize(new Vector3(-Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y), 0, -Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y))) * pounceStrength * Time.deltaTime;
            vel.y = pounceJumpStrength;
            audioSource.PlayOneShot(pounceSFX, 1f); // play pounce SFX
            pounceInput = false;
        }
        vel = new Vector3(vel.x * (1-friction), vel.y, vel.z * (1-friction)); //reduce horizontal velocity based on specified friction
        rb.linearVelocity = vel; //update player velocity

    }

    private bool detectEnemies()
    {
        Vector3 pos = transform.position;

        Collider[] hits = Physics.OverlapSphere(pos, enemyDetectionRadius); // run a spherecast;
        bool hitPest = false;

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
                        hitPest = true;
                    }
                } else if (c.tag == "Baby") // but we PRIORITIZE babies
                {
                    targetPos = c.transform.position; // immediately set target position
                    return true; // we don't need anything else since this takes absolute priority
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
            Gizmos.DrawSphere(transform.position, enemyDetectionRadius);
        }
    }
}
