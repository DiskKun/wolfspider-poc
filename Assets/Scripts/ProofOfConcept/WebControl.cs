using UnityEngine;
using System.Collections;

public class WebControl : MonoBehaviour
{
    [Tooltip("Sets whether or not the hardware cursor is visible. You will need to restart the scene to see this change.")]
    public bool cursorVisible;
    public GameObject player;
    [Tooltip("The GameObject that contains the LineRenderer and MeshCollider components for the web")]
    public GameObject webGameObject;
    [Tooltip("Sets the speed at which the web icon rotates.")]
    public float webRotationSpeed;

    [Tooltip("The colour the webicon will turn when the target is invalid.")]
    Color disallowWebColor = new Color(1, 0, 0, 1); 
    [Tooltip("The colour the webicon will turn when the target is valid.")]
    Color allowWebColor = new Color(1, 1, 1, 1);

    bool canWeb = false;

    Rigidbody rb;
    MeshRenderer meshRenderer;

    LineRenderer webRenderer;
    MeshCollider webMeshCollider;

    RaycastHit camToWebHit; // contains the raycasthit data for the raycast between the camera and the webicon; used for placing the webicon
    RaycastHit playerToWebHit; // contains the raycasthit data for the raycast between the player and the webicon; used for shooting the web

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = cursorVisible; // set the hardware cursor's visiblity.
        meshRenderer = GetComponent<MeshRenderer>(); // the meshrenderer of the webicon is needed to change its material's colour
        rb = GetComponent<Rigidbody>(); // the rigidbody of the webicon is needed to change its position and rotation

        webRenderer = webGameObject.GetComponent<LineRenderer>();
        webMeshCollider = webGameObject.GetComponent<MeshCollider>();

    }

    // Update is called once per frame
    void Update()
    {
        CastWebIconRays();

        if (Input.GetMouseButtonDown(0))
        {
            if (canWeb)
            {
                webRenderer.SetPosition(0, player.transform.position + Vector3.down * 0.5f); // position 1 of the web line is the player's position + an offset that ensures the player can immediately walk onto the web
                StartCoroutine(SpinWebBridge(10, 0.1f));
            }
            
        }
    }

    IEnumerator SpinWebBridge(int stepsToTake, float timeToSpin)
    {
        Vector3 webDistance = (camToWebHit.point - player.transform.position); // distance between the player and the webicon at time of spin
        Mesh mesh = new Mesh();

        int step = 0;
        while (step < stepsToTake)
        {
            // incrementally increase the size of the web towards the target
            webRenderer.SetPosition(1, player.transform.position + webDistance * step/stepsToTake);
            webRenderer.BakeMesh(mesh, true);
            webMeshCollider.sharedMesh = mesh;
            step += 1;
            yield return new WaitForSeconds(timeToSpin / stepsToTake);
        }
        webRenderer.SetPosition(1, camToWebHit.point); 
        webRenderer.BakeMesh(mesh, true);
        webMeshCollider.sharedMesh = mesh; // set the web's mesh collider to the mesh of the linerenderer
    }

    

    private void FixedUpdate()
    {
        MoveWebIcon();
        
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
        Physics.Raycast(ray, out camToWebHit, 100, LayerMask.GetMask("Default", "Webbable")); // cast the ray


        if (camToWebHit.transform.gameObject.layer != LayerMask.NameToLayer("Webbable") || Physics.Raycast(player.transform.position, (camToWebHit.point - player.transform.position).normalized, out playerToWebHit, (camToWebHit.point - player.transform.position).magnitude - 0.1f, LayerMask.GetMask("Webbable", "Default")))
        {
            // if the player points to a non-webbable surface, or if a webbable was hit between the webicon and the player, something is blocking the line of sight. update the webicon's colour to show this.
            meshRenderer.material.color = disallowWebColor;
            canWeb = false;
        }
        else
        {
            meshRenderer.material.color = allowWebColor;
            canWeb = true;
        }
    }
}
