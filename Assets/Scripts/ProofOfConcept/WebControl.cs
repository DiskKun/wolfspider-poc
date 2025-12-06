using UnityEngine;

public class WebControl : MonoBehaviour
{
    [Tooltip("Sets whether or not the hardware cursor is visible. You will need to restart the scene to see this change.")]
    public bool cursorVisible;
    public GameObject player;
    [Tooltip("Sets the speed at which the web icon rotates.")]
    public float webRotationSpeed;

    [Tooltip("The colour the webicon will turn when the target is invalid.")]
    Color disallowWebColor = new Color(1, 0, 0, 1); 
    [Tooltip("The colour the webicon will turn when the target is valid.")]
    Color allowWebColor = new Color(1, 1, 1, 1);

    bool canWeb = false;

    Rigidbody rb;
    Rigidbody playerRB;
    MeshRenderer meshRenderer;

    RaycastHit camToWebHit; // contains the raycasthit data for the raycast between the camera and the webicon; used for placing the webicon
    RaycastHit playerToWebHit; // contains the raycasthit data for the raycast between the player and the webicon; used for shooting the web

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = cursorVisible; // set the hardware cursor's visiblity.
        meshRenderer = GetComponent<MeshRenderer>(); // the meshrenderer of the webicon is needed to change its material's colour
        rb = GetComponent<Rigidbody>(); // the rigidbody of the webicon is needed to change its position and rotation
        playerRB = player.GetComponent<Rigidbody>(); // the rigidbody of the player is needed to cast a ray between it and the webicon
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition); // the ray between the camera and the mouse position
        Physics.Raycast(ray, out camToWebHit, 100, LayerMask.NameToLayer("Webbable")); // cast the way only detecting hits from the "webbable" layer

        
        if (Physics.Raycast(playerRB.position, (camToWebHit.point - playerRB.position).normalized, out playerToWebHit, (camToWebHit.point - playerRB.position).magnitude - 0.1f, LayerMask.NameToLayer("Webbable"))) {
            // if a webbable was hit between the webicon and the player, something is blocking the line of sight. update the webicon's colour to show this.
            meshRenderer.material.color = disallowWebColor;
            canWeb = false;
        } else
        {
            meshRenderer.material.color = allowWebColor;
            canWeb = true;
        }
    }

    private void FixedUpdate()
    {
        
        rb.position = camToWebHit.point;  // set the webicon's position to the first webbable hit between the mouse and the camera
        rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles + Vector3.forward * Time.deltaTime * webRotationSpeed)); // animate the webicon by rotating it along the Z axis

    }
}
