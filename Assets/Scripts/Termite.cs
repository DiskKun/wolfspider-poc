using UnityEngine;

public class Termite : MonoBehaviour
{

    public float wanderTime = 1;
    public float wanderRange = 5;
    public GameObject enemyPrefab;

    float moveTimer;
    Vector2 targetPosition;
    Vector2 startingPosition;
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = (Vector3)rb.position + new Vector3(Random.Range(-wanderRange, wanderRange), Random.Range(-wanderRange, wanderRange), 0);
        startingPosition = transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        if (rb.position == targetPosition)
        {
            targetPosition = transform.position + new Vector3(Random.Range(-wanderRange, wanderRange), Random.Range(-wanderRange, wanderRange), 0);
            startingPosition = transform.position;
            moveTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        moveTimer += Time.deltaTime;
        rb.MovePosition(Vector2.Lerp(startingPosition, targetPosition, moveTimer / wanderTime));
        Vector2 direction = targetPosition - startingPosition;
        rb.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            SpawnClone(10);
            SpawnClone(10);
            Destroy(gameObject);
        }
    }

    void SpawnClone(float distance)
    {
        Instantiate(enemyPrefab, new Vector3(Random.Range(-distance, distance), Random.Range(-distance, distance), 0), Quaternion.identity);
    }
}
