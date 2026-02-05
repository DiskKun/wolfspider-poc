using UnityEngine;

public class Baby : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Where to put the baby on the parent's back, offset from the parent's origin.")]
    private Vector2 carryOffset;

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
            transform.parent = other.gameObject.transform; // literally child this to the parent
            transform.localPosition = new Vector3(carryOffset.x, 0.5f, carryOffset.y); // set local position to be appropriately placed on the parent's back
            gameObject.tag = "Carried"; // update tag so player doesn't constantly point at the baby on its back
        }
    }
}
