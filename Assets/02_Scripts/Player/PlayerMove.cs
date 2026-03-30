using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 7.0f;
    [SerializeField] float jumpForce = 5.0f;

    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] LayerMask groundLayer;

    private float inputX;
    private float inputZ;

    private bool isGrounded
    {
        get
        {
            return Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundLayer);
        }
    }
    private bool isJumping = false;

    private Rigidbody rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (!IsOwner) return;

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
        }

    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        Vector3 moveDir = (transform.forward * inputZ + transform.right * inputX).normalized;

        if (moveDir.magnitude > 0)
        {
            Vector3 nextPosition = transform.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }

        if (anim != null)
        {
            anim.SetFloat("MoveSpeed", moveDir.magnitude);
        }   
        
        if(isJumping)
        {
            Jump();
            isJumping = false;
        }

        //rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
    }
    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
