using UnityEngine;
using System.Collections;
using TMPro;

public class WebControl : MonoBehaviour
{
    [Tooltip("Sets whether or not the hardware cursor is visible. You will need to restart the scene to see this change.")]
    public bool cursorVisible;
    public PlayerControl player;
    [Tooltip("The GameObject that contains the LineRenderer and MeshCollider components for the web bridges")]
    public GameObject bridgeGameObject;
    [Tooltip("The GameObject that contains the LineRenderer and MeshCollider components for the web ropes")]
    public GameObject ropeGameObject;
    [Tooltip("Sets the speed at which the web icon rotates.")]
    public float webRotationSpeed;

    [Tooltip("The amount of web silk the player currently has")]
    public int webSilkAmount = 0;

    //public Material webIconMaterial;
    //public Material pullTextMaterial;

    [Tooltip("The amount of silk needed for each use of the bridge")]
    public int bridgeSilkCost = 1;

    public float ropePullSpeed = 1;

    public TextMeshProUGUI silkText;


    [Header("Silk Animation Settings")]
    [Tooltip("The icon that will animate towards the UI")]
    public GameObject silkIconPrefab;
    [Tooltip("The animation curve for the silk icon's travel")]
    public AnimationCurve silkAnimationCurve;



    [Tooltip("The colour the webicon will turn when the target is invalid.")]
    Color disallowWebColor = new Color(1, 0, 0, 1);
    [Tooltip("The colour the webicon will turn when the target is bridgeable.")]
    Color allowBridgeColor = new Color(1, 1, 1, 1);
    [Tooltip("The colour the webicon will turn when the target is pullable.")]
    Color allowPullColor = new Color(0, 0, 1, 1);
    [Tooltip("The color the webicon will turn when the player can click to pull the item they are attached to.")]
    Color clickToPullColor = new Color(0, 1, 0, 1);

    private AudioSource audioSource; //audio source to play SFX
    [Space(40)]

    public SFX_WebShot SFX_WebShot;
    private AudioClip webShotSFX;

    public SFX_WebPull SFX_WebPull;
    private AudioClip webPullSFX;
    private float pullRepeatDelay = 0.5f; // delay between pull sounds
    private float pullSoundTimer = 0f;



    bool canBridge = false;
    bool canPull = false;

    Rigidbody rb;
    MeshRenderer meshRenderer;

    LineRenderer bridgeRenderer;
    MeshCollider bridgeMeshCollider;

    LineRenderer ropeRenderer;

    Rigidbody pulling; // the object the player is pulling
    Transform pullPoint;

    RaycastHit camToWebHit; // contains the raycasthit data for the raycast between the camera and the webicon; used for placing the webicon
    RaycastHit playerToWebHit; // contains the raycasthit data for the raycast between the player and the webicon; used for shooting the web

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = cursorVisible; // set the hardware cursor's visiblity.
        meshRenderer = GetComponent<MeshRenderer>(); // the meshrenderer of the webicon is needed to change its material's colour
        rb = GetComponent<Rigidbody>(); // the rigidbody of the webicon is needed to change its position and rotation

        bridgeRenderer = bridgeGameObject.GetComponent<LineRenderer>();
        bridgeMeshCollider = bridgeGameObject.GetComponent<MeshCollider>();
        ropeRenderer = ropeGameObject.GetComponent<LineRenderer>();


        audioSource = GetComponent<AudioSource>();

        webShotSFX = SFX_WebShot.webShotSFX; // load audio hooks
        webPullSFX = SFX_WebPull.webPullSFX;

    }

    // Update is called once per frame
    void Update()
    {
        CastWebIconRays();

        if (Input.GetMouseButtonDown(0))
        {
            if (canBridge && webSilkAmount >= bridgeSilkCost)
            {
                webSilkAmount -= bridgeSilkCost;

                bridgeRenderer.SetPosition(0, player.transform.position + Vector3.down * 0.5f); // position 1 of the web line is the player's position + an offset that ensures the player can immediately walk onto the web
                StartCoroutine(SpinWebBridge(10, 0.1f));

                audioSource.PlayOneShot(webShotSFX, 1f); // play web shot sfx

            }
            else if (canBridge && webSilkAmount < bridgeSilkCost)
            {
                StartCoroutine(FlashTextColor(silkText, Color.red));
            } else if (canPull)
            {
                ropeRenderer.SetPosition(0, player.transform.position + Vector3.down * 0.5f); // position 1 of the web line is the player's position + an offset that ensures the player can immediately walk onto the web

                StartCoroutine(SpinWebRope(10, 0.1f));

                audioSource.PlayOneShot(webShotSFX, 1f); // play web shot sfx
            }

        }
        if (Input.GetMouseButtonDown(1))
        {
            if (pulling)
            {
                pulling = null; // right click to release the object
                ropeRenderer.SetPosition(0, Vector3.zero);
                ropeRenderer.SetPosition(1, Vector3.zero);
            }
        }
        if (pulling)
        {
            ropeRenderer.SetPosition(0, player.transform.position);
            ropeRenderer.SetPosition(1, pullPoint.position);
            
            pullSoundTimer -= Time.deltaTime;
            if (pullSoundTimer <= 0) // play pull sound on a repeating timer
            {
                audioSource.PlayOneShot(webShotSFX, 1f); // play web pull sfx
                pullSoundTimer = pullRepeatDelay; // reset timer
            }
        } else // if nothing is being pulled at the moment
        {
            pullSoundTimer = 0f; // reset pull sound timer to queue a sound up the next time the player pulls the rope
        }
        silkText.text = webSilkAmount.ToString();

    }

    IEnumerator FlashTextColor(TextMeshProUGUI text, Color color, int numberOfFlashes = 3, float flashSpeed = 0.1f)
    {
        Color originalColor = text.color;

        while (numberOfFlashes > 0)
        {
            text.color = color;
            yield return new WaitForSeconds(flashSpeed);
            text.color = originalColor;
            yield return new WaitForSeconds(flashSpeed);
            numberOfFlashes -= 1;
        }
    }

    public void GetSilk(int amount)
    {
        StartCoroutine(AnimateSilkCollect(amount));
    }

    IEnumerator AnimateSilkCollect(int amount)
    {
        GameObject animObj = Instantiate(silkIconPrefab, Camera.main.WorldToScreenPoint(player.transform.position), Quaternion.identity, silkText.transform.parent.parent);


        float t = 0;
        Vector3 startPos = animObj.transform.position;
        while (animObj.transform.position != silkText.transform.parent.position)
        {
            animObj.transform.position = Vector3.Lerp(startPos, silkText.transform.parent.position, silkAnimationCurve.Evaluate(t));
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(animObj);
        webSilkAmount += amount;
    }




    IEnumerator SpinWebBridge(int stepsToTake, float timeToSpin)
    {
        // get the TOP of the gameobject

        Vector3 bridgePoint = camToWebHit.point;
        bridgePoint.y = camToWebHit.transform.position.y + camToWebHit.transform.localScale.y / 2;


        Vector3 webDistance = ((bridgePoint) - player.transform.position); // distance between the player and the webicon at time of spin
        Mesh mesh = new Mesh();

        int step = 0;
        while (step < stepsToTake)
        {
            // incrementally increase the size of the web towards the target
            bridgeRenderer.SetPosition(1, player.transform.position + webDistance * step / stepsToTake);
            bridgeRenderer.BakeMesh(mesh, true);
            bridgeMeshCollider.sharedMesh = mesh;
            step += 1;
            yield return new WaitForSeconds(timeToSpin / stepsToTake);
        }
        bridgeRenderer.SetPosition(1, bridgePoint);
        bridgeRenderer.BakeMesh(mesh, true);
        bridgeMeshCollider.sharedMesh = mesh; // set the web's mesh collider to the mesh of the linerenderer
    }

    IEnumerator SpinWebRope(int stepsToTake, float timeToSpin)
    {
        Vector3 webDistance = (camToWebHit.point - player.transform.position); // distance between the player and the webicon at time of spin

        int step = 0;
        while (step < stepsToTake)
        {
            // incrementally increase the size of the web towards the target
            ropeRenderer.SetPosition(1, player.transform.position + webDistance * step / stepsToTake);
            step += 1;
            yield return new WaitForSeconds(timeToSpin / stepsToTake);
        }
        ropeRenderer.SetPosition(1, camToWebHit.point);
        pulling = camToWebHit.transform.gameObject.GetComponent<Rigidbody>(); // set the pull object
        pullPoint = camToWebHit.transform.gameObject.GetComponentInChildren<PullPoint>().gameObject.transform; // get the transform of the pullpoint child
        pullPoint.position = camToWebHit.point; // move the pullpoint child to the place where the player clicked on the object
    }



    private void FixedUpdate()
    {
        MoveWebIcon();

        if (pulling)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 force = (player.transform.position - pullPoint.transform.position).normalized * Time.deltaTime * ropePullSpeed;
                force.y = 0;
                pulling.AddForce(force);
                //pulling.MoveRotation((pullPoint.transform.position - player.transform.position).normalized)
            }
        }
    }



    void MoveWebIcon()
    {
        rb.position = camToWebHit.point;  // set the webicon's position to the first webbable hit between the mouse and the camera
        rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles + Vector3.forward * Time.deltaTime * webRotationSpeed)); // animate the webicon by rotating it along the Z axis
    }

    void CastWebIconRays()
    {
        Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition); // the ray between the camera and the mouse position
        Physics.Raycast(ray, out camToWebHit, 100, LayerMask.GetMask("Default", "Webbable", "Pullable")); // cast the ray
        //meshRenderer.material = webIconMaterial;

        if (camToWebHit.transform.gameObject.layer == LayerMask.NameToLayer("Webbable") && !Physics.Raycast(player.transform.position, (camToWebHit.point - player.transform.position).normalized, out playerToWebHit, (camToWebHit.point - player.transform.position).magnitude - 0.1f, LayerMask.GetMask("Webbable", "Pullable", "Default")))
        {
            // if the player points to a non-webbable surface, or if a webbable was hit between the webicon and the player, something is blocking the line of sight. update the webicon's colour to show this.
            meshRenderer.material.color = allowBridgeColor;
            canBridge = true;
            canPull = false;
        }
        else if (camToWebHit.transform.gameObject.layer == LayerMask.NameToLayer("Pullable"))
        {
            meshRenderer.material.color = allowPullColor;
            canPull = true;
            canBridge = false;
        }
        else if (pulling)
        {
            meshRenderer.material.color = clickToPullColor;
            //meshRenderer.material = pullTextMaterial;
            canPull = false;
            canBridge = false;
        }
        else
        {
            meshRenderer.material.color = disallowWebColor;
            canBridge = false;
            canPull = false;
        }
    }
}
