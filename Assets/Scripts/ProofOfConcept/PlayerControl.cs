using System;
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

    private Vector2 pInput; // player movement input
    private bool pounceInput; // pounce input 

    private Vector3 vel; // velocity for use with movement
    private float minVelToTurn = 0.1f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // get attached rigidbody (we'll be using a capsule collider for 3D movement
        vel = new Vector3(); // reset velocity
        pounceDuration = Mathf.Clamp(pounceDuration, 0f, pounceCooldown); // clamp pounce duration so that it's not longer than cooldown to prevent weirdness from the implementation
    }

    // Update is called once per frame
    void Update()
    {
        if (pCDTimer > 0) { pCDTimer -= Time.deltaTime; } // reduce cooldown timer

        if (pounceCooldown - pCDTimer >= pounceDuration) {
            pInput.x = Input.GetAxisRaw("Horizontal"); // we use two separate operations here as it's slightly
            pInput.y = Input.GetAxisRaw("Vertical");   // more efficient than creating a new Vector2 every frame
        } else
        {
            pInput.x = 0; pInput.y = 0; // keep them at zero for the duration
        }
        
        if (Input.GetButtonDown("Jump") && pCDTimer <= 0) { pounceInput = true; pCDTimer = pounceCooldown; } // queue up a pounce in Update to ensure responsiveness
    }

    private void FixedUpdate()
    {
        RaycastHit rh;
        if ( Physics.SphereCast(transform.position, 0.25f, Vector3.down, out rh, 0.27f) ) { vel.y = 0; }

        vel += Vector3.Normalize(new Vector3(pInput.x, 0, pInput.y)) * speed * Time.deltaTime; // adjust player velocity, normalize increase to ensure consistency
        vel += new Vector3(0, -gravity, 0) * Time.deltaTime; // apply gravity
        if (new Vector3(vel.x, 0, vel.y).magnitude > minVelToTurn) // as long as the player is moving more than a certain amount...
        {
            // rotate the transform based on velocity
            transform.eulerAngles = new Vector3(0, -(Mathf.Atan2(vel.z, vel.x) * Mathf.Rad2Deg + 90), 0); //rotate to proper angle
        }

        if (pounceInput)
        {
            // apply velocity based on player ANGLE rather than INPUT. This means players can still pounce even when not specifically moving.
            vel = Vector3.Normalize(new Vector3(-Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y), 0, -Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y))) * pounceStrength * Time.deltaTime;
            vel.y = pounceJumpStrength;
            pounceInput = false;
        }
        vel = new Vector3(vel.x * (1-friction), vel.y, vel.z * (1-friction)); //reduce horizontal velocity based on specified friction
        rb.linearVelocity = vel; //update player velocity

    }
}
