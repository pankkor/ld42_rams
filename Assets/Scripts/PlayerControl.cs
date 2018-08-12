using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public enum PlayerNumber {
        First,
        Second
    }
    public PlayerNumber playerNumber = PlayerNumber.First;

    public float moveForce = 365.0f;
    public Vector2 maxMovementSpeed = new Vector2(5.0f, 6.0f);
    public Vector2 maxSpeed = new Vector2(30.0f, 30.0f);
    public float jumpImpulse = 9.1f;
    public float fallGravityMultiplier = 1.5f;
    public float lowJumpMultiplier = 1.0f;
    public float inAirHorizontalForceMultiplier = 1.5f;
    public Vector2 linearDrag = new Vector2(0.7f, 0.95f);
    public Vector2 linearDragStunned = new Vector2(0.9f, 0.98f);

    public float dashImpulse = 15.0f;
    public float knockBackImpulse = 12.0f;

    public float topHitStunDelay = 0.2f;
    public float hitStunDelay = 0.12f;

    public Collider2D topHitbox;
    public Collider2D bottomHitbox;
    public Collider2D leftHitbox;
    public Collider2D rightHitbox;

    enum AudioType {
        Jump,
        Dash,
        Hit,
    }

    public AudioClip[] jumpAudioClips;
    public AudioClip[] dashAudioClips;
    public AudioClip[] hitAudioClips;

    [HideInInspector]
    public Rigidbody2D rb;

    private Animator animator;              // Reference to the player's animator component.

    private bool isFacingRight = true;
    private bool inputJump = false;
    private bool inputDash = false;
    private float inputX = 0.0f;

    public float jumpDelay = 0.1f;
    public float dashDelay = 1.0f;
    public float dashDuration = 0.2f;

    private float jumpTime = 0.0f;
    private float dashTime = 0.0f;
    private float dashEndTime = 0.0f;
    private float stunTime = 0.0f;

    private bool _isJumping = false;
    private bool isJumping {
        get { return _isJumping; }
        set {
            if (_isJumping != value) {
                if (value) {
                    playAudio(AudioType.Jump, transform.position);
                }
            }
            _isJumping = value;
        }
    }

    private bool _isStunned = false;
    private bool isStunned {
        get { return _isStunned; }
        set {
            if (_isStunned != value) {
                animator.SetBool("hit", value);

                if (value) {
                    playAudio(AudioType.Hit, transform.position);
                }
            }
            _isStunned = value;
        }
    }

    private bool _isDashing = false;
    public bool isDashing {
        get { return _isDashing; }
        private set {
            if (_isDashing != value) {
                animator.SetBool("dash", value);

                if (value) {
                    playAudio(AudioType.Dash, transform.position);
                }
            }
            _isDashing = value;
        }
    }

    private float _dashCooldown = 1.0f;
    private float dashCooldown {
        get { return _dashCooldown; }
        set {
            animator.SetFloat("dashCooldown", value);
            _dashCooldown = value;
        }
    }

    private bool shouldBeHit = false;
    private Vector3 knockBackImpulseVector;

    private bool isDashingRight = false;

    private string jumpButton = "Jump";
    private string horizontalAxis = "Horizontal";
    private string verticalAxis = "Vertical";
    private string action0Button = "Action0";
    private string action1Button = "Action1";

    void Awake() {
        // Setting up references.
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // button strings according to player number
        string playerSuffix = PlayerSuffix(playerNumber);
        
        if (transform.position.x > 0.0) {
            FlipX();
        }

        jumpButton += playerSuffix;
        horizontalAxis += playerSuffix;
        verticalAxis += playerSuffix;
        action0Button += playerSuffix;
        action1Button += playerSuffix;
    }

    void Update() {
        // input
        inputJump = Input.GetButton(jumpButton);
        inputDash = Input.GetButton(action0Button);
        inputX = Input.GetAxis(horizontalAxis);

        // flip player's sprite
        if (!isDashing && (inputX > 0 && !isFacingRight || inputX < 0 && isFacingRight)) {
            FlipX();
        }

        animator.SetBool("hasInputX", inputX > 0.1f || inputX < -0.1f );
        animator.SetFloat("velocityY", Mathf.Clamp(Mathf.Abs(rb.velocity.y) / maxMovementSpeed.y, 0.0f, 1.0f));
    }

    void FixedUpdate() {
        bool isOnGround = bottomHitbox.IsTouchingLayers(1 << LayerMask.NameToLayer("Ground"));
        bool isOnPlayer = bottomHitbox.IsTouchingLayers(1 << LayerMask.NameToLayer("PlayerTopHitbox"));
//         bool isAtPlayerLeft = leftHitbox.IsTouchingLayers(1 << LayerMask.NameToLayer("Player"));
//         bool isAtPlayerRight = rightHitbox.IsTouchingLayers(1 << LayerMask.NameToLayer("Player"));
//         bool isTouchingPlayer = isAtPlayerLeft || isAtPlayerRight;

        animator.SetBool("grounded", isOnGround || isOnPlayer);

        if (shouldBeHit) {
            shouldBeHit = false;
            rb.AddForce(knockBackImpulseVector, ForceMode2D.Impulse);
        }

        isStunned = Time.time < stunTime;
        isDashing = !isStunned && Time.time < dashEndTime;
        bool canMove = !isStunned && !isDashing;
        bool canDash = canMove && Time.time > dashTime;

        dashCooldown = Mathf.Clamp((Time.time - dashTime) / dashDelay, 0.0f, 1.0f);

        if (canDash && inputDash) {
            dashTime = Time.time + dashDelay;
            dashEndTime = Time.time + dashDuration;
            isDashing = true;
            canMove = false;
            isDashingRight = isFacingRight;

            // dash physics
            rb.velocity = (new Vector2(0.0f, 0.0f));
            rb.AddForce(new Vector2((isDashingRight ? 1.0f : -1.0f) * dashImpulse, 0.0f), ForceMode2D.Impulse);
        }

        // don't fall down while dashing
        rb.gravityScale = isDashing ? 0.0f : 1.0f;

        // move player by force in x direction
        if (canMove && inputX * rb.velocity.x < maxMovementSpeed.x) {
            // make in air movement a more sensitive
            float k = isOnGround ? 1.0f : inAirHorizontalForceMultiplier;
            rb.AddForce(Vector2.right * inputX * moveForce * k);
        }

        // liear drag x (friction simulation, yes built in friction is not arcade enough)
        Vector2 drag = isStunned ? linearDragStunned : linearDrag;
        rb.velocity = rb.velocity * drag;

        // clamp velocity
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x),
                                Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y));

        // prevent jump spamming by introducing jump delay
        bool canJump = rb.velocity.y < 0.00001 && canMove && Time.time > jumpTime;
        // TODO vel.y < 0.0001 ????

        // is on ground or player
        if (isOnGround || isOnPlayer) {
            if (canJump) {
                isJumping = false;
            }

            if (canJump && (inputJump || isOnPlayer)) {
                jumpTime = Time.time + jumpDelay;
                isJumping = true;

                // jump physics
                rb.velocity = (new Vector2(rb.velocity.x, 0.0f));
                rb.AddForce(new Vector2(0f, jumpImpulse), ForceMode2D.Impulse);
            }
        } else {
            // is in air
            if (rb.velocity.y < 0.0f) {
                // fall down quicker
                rb.AddForce(Physics2D.gravity * fallGravityMultiplier);
            } else if (isJumping && rb.velocity.y > 0.0f && !inputJump) {
                // jump lower if jump button not pressed
                rb.AddForce(Physics2D.gravity * lowJumpMultiplier);
            }
        }
    }

    void FlipX() {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void TopHit(Vector3 fromPosition) {
        Hit(topHitStunDelay, fromPosition);
    }

    public void DashHit(Vector3 fromPosition) {
        Hit(hitStunDelay, fromPosition);
    }

    void Hit(float stunDelay, Vector3 aKnockBackImpulseVector) {
        if (Time.time < stunTime) return;

        stunTime = Time.time + stunDelay;
        isStunned = true;
        isJumping = false;

        shouldBeHit = true;
        knockBackImpulseVector = aKnockBackImpulseVector;
    }

    string PlayerSuffix(PlayerNumber playerNumber) {
        switch(playerNumber) {
            case PlayerNumber.First: return "-p0";
            case PlayerNumber.Second: return "-p1";
        }
        Debug.LogError("Unknown player number type");
        return "-p0";
    }

    public Vector3 calcKnockBackImpulse(Vector3 toPosition) {
        return (toPosition - transform.position).normalized * knockBackImpulse;
    }

    private void playAudio(AudioType type, Vector3 position) {
        AudioClip[] clips = audioClipsForType(type);
        if (clips.Length > 0) {
            int i = Random.Range(0, clips.Length);
            AudioSource.PlayClipAtPoint(clips[i], position);
        }
    }

    private AudioClip[] audioClipsForType(AudioType type) {
        switch(type) {
            case AudioType.Jump: return jumpAudioClips;
            case AudioType.Dash: return dashAudioClips;
            case AudioType.Hit: return hitAudioClips;
        }
        Debug.LogError("Unknown autido type");
        return jumpAudioClips;
    }

}
