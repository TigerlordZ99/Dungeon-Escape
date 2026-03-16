using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rb;
    private Animator animator;
    private int lastDirection = 0; // Default facing down

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        Vector2 move = new Vector2(h, v) * speed;
        rb.linearVelocity = move;

        // Update animation direction
        if (h < 0) { animator.SetInteger("Direction", 1); lastDirection = 1; }       // Left
        else if (h > 0) { animator.SetInteger("Direction", 2); lastDirection = 2; }  // Right
        else if (v < 0) { animator.SetInteger("Direction", 0); lastDirection = 0; }  // Down
        else if (v > 0) { animator.SetInteger("Direction", 3); lastDirection = 3; }  // Up
        else { animator.SetInteger("Direction", lastDirection); }
    }
}