using UnityEngine;

public class TermiteDestroyTemp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.eulerAngles = new Vector3(0, Random.value * 360, 0); // randomize facing direction
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

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
