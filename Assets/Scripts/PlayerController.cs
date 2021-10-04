using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int speed;
    public float launchMultiplier;
    public float launchSpeed;
    public float maxChargeTime;

    float horizontal;
    float vertical;
    Rigidbody2D rigidbody2d;
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);
    float startChargeTime = 0;
    float chargeTime = 0;
    float launchTime = 0;
    float scale = Constants.BASE_SCALE;
    bool keyDown = false;
    bool keyUp = false;
    bool mouse0Down = false;
    int ricoMultiplier = 0;

    enum PlayerStates {Launching, Charging, Slashing, Walking, Idle}
    PlayerStates playerState = PlayerStates.Idle;

    enum AnimProperties { isIdle = 0, isCharging = 1, isLaunching = 2, isSlashing = 3}
    AnimProperties animState = AnimProperties.isIdle;
    static class Constants
    {
        public const float BASE_SCALE = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SetPlayerState(PlayerStates.Idle, AnimProperties.isIdle);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerState);
        Debug.Log(animator.GetInteger("CurrentState"));
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        if (!(playerState == PlayerStates.Launching))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (playerState == PlayerStates.Walking || playerState == PlayerStates.Idle) keyDown = true;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (playerState == PlayerStates.Charging) keyUp = true;
            }
        } else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (playerState == PlayerStates.Launching) mouse0Down = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (mouse0Down) 
        {
            WindSlash();
            mouse0Down = false;
        }
        if (keyDown)
        {
            startChargeTime = Time.time;
            SetPlayerState(PlayerStates.Charging, AnimProperties.isCharging);
            rigidbody2d.velocity = Vector2.zero;
            keyDown = false;
        }
        if (keyUp && playerState == PlayerStates.Charging)
        {
            chargeTime = Time.time - startChargeTime;
            //max launch 3 sec
            if (chargeTime > maxChargeTime)
            {
                chargeTime = maxChargeTime;
            }

            launchTime = chargeTime / 2;
            SetPlayerState(PlayerStates.Launching, AnimProperties.isLaunching);
            keyUp = false;
        }
        if (playerState == PlayerStates.Charging)
        {
            if (0.2f < scale)
            {
                scale = Constants.BASE_SCALE - ((Time.time - startChargeTime) / maxChargeTime);
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        if (playerState == PlayerStates.Launching  || playerState == PlayerStates.Slashing)
        {
            Launch();
        }
        else if (playerState == PlayerStates.Idle)
        {
            Vector2 position = rigidbody2d.position;
            position.x = position.x + speed * horizontal * Time.deltaTime;
            position.y = position.y + speed * vertical * Time.deltaTime;

            rigidbody2d.MovePosition(position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall" && playerState == PlayerStates.Slashing)
        {
            launchTime += 1.5f;  
        }
    }

    void Launch()
    {
        if (launchTime > 0)
        {
            rigidbody2d.velocity = (lookDirection * launchTime * launchMultiplier) + (lookDirection * (launchSpeed + launchSpeed * ricoMultiplier));
            launchTime -= Time.deltaTime;
        }
        else
        { 
            startChargeTime = 0.0f;
            chargeTime = 0.0f;
            launchTime = 0.0f;
            scale = Constants.BASE_SCALE;
            transform.localScale = new Vector3(scale, scale, scale);
            SetPlayerState(PlayerStates.Idle, AnimProperties.isIdle);
        }
    }

    void WindSlash()
    {
        SetPlayerState(PlayerStates.Slashing, AnimProperties.isSlashing);
        transform.localScale = new Vector3(Constants.BASE_SCALE, Constants.BASE_SCALE, Constants.BASE_SCALE);
    }

    public void EndWindSlash()
    {
        SetPlayerState(PlayerStates.Launching, AnimProperties.isLaunching);
        transform.localScale = new Vector3(scale, scale, scale);
    }
    void SetPlayerState(PlayerStates newPlayerState, AnimProperties animProperty)
    {
        animator.SetInteger("CurrentState", (int)animProperty);
        playerState = newPlayerState;
        animState = animProperty;
    }

}
