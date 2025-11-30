using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 1;
    Rigidbody2D rb;
    Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Screen.SetResolution(1600, 1200, false);
    }

    // Update is called once per frame
    void Update()
    {
        direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * Time.deltaTime * speed);
        rb.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
    }
}
