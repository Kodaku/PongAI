using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed;
    private Rigidbody2D rb;
    private Vector3 startPosition;
    // Start is called before the first frame update
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        Launch();
    }

    public void Reset()
    {
        rb.velocity = Vector2.zero;
        // startPosition = new Vector2(Random.Range(3, 33), Random.Range(1, 20));
        transform.position = startPosition;
        Launch();
    }

    private void Launch()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(0, 2) == 0 ? -1 : 1;
        rb.velocity = new Vector2(speed * x, speed * y);
    }

    void Update()
    {
        float x = rb.velocity.normalized.x > 0 ? 1 : -1;
        float y = rb.velocity.normalized.y > 0 ? 1 : -1;
        rb.velocity = new Vector2(speed * x, speed * y);
    }

    public Vector2 GetVelocityDirection()
    {
        float x = rb.velocity.normalized.x > 0 ? 1 : -1;
        float y = rb.velocity.normalized.y > 0 ? 1 : -1;
        return new Vector2(x, y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
