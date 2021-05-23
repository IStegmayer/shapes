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

    enum PlayerState {Launching, Charging, Slashing, Walking, Idle}
    PlayerState playerState;

    enum AnimProperties { Charge, isSlashing }
    static class Constants
    {
        public const float BASE_SCALE = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerState = PlayerState.Idle;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerState);
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        if (!(playerState == PlayerState.Launching))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("GetKeyDown");
                animator.SetTrigger(AnimProperties.Charge.ToString());
                if (!(playerState == PlayerState.Charging)) keyDown = true;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("GetKeyUp");
                if (playerState == PlayerState.Charging) keyUp = true;
            }
        } else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (playerState == PlayerState.Launching) mouse0Down = true;
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
        if (keyDown && playerState != PlayerState.Slashing)
        {
            startChargeTime = Time.time;
            playerState = PlayerState.Charging;
            rigidbody2d.velocity = Vector2.zero;
            keyDown = false;
        }
        if (keyUp && playerState == PlayerState.Charging)
        {
            chargeTime = Time.time - startChargeTime;
            //max launch 3 sec
            if (chargeTime > maxChargeTime)
            {
                chargeTime = maxChargeTime;
            }

            launchTime = chargeTime / 2;
            playerState = PlayerState.Launching;
            keyUp = false;
        }
        if (playerState == PlayerState.Charging)
        {
            if (0.2f < scale)
            {
                scale = Constants.BASE_SCALE - ((Time.time - startChargeTime) / maxChargeTime);
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        if (playerState == PlayerState.Launching  || playerState == PlayerState.Slashing)
        {
            Launch();
        }
        else if (playerState == PlayerState.Idle)
        {
            Vector2 position = rigidbody2d.position;
            position.x = position.x + speed * horizontal * Time.deltaTime;
            position.y = position.y + speed * vertical * Time.deltaTime;

            rigidbody2d.MovePosition(position);
        }
    }

    void Launch()
    {
        if (launchTime > 0)
        {
            rigidbody2d.velocity = (lookDirection * launchTime * launchMultiplier) + (lookDirection * launchSpeed);
            launchTime -= Time.deltaTime;
        }
        else
        { 
            startChargeTime = 0.0f;
            chargeTime = 0.0f;
            launchTime = 0.0f;
            scale = Constants.BASE_SCALE;
            transform.localScale = new Vector3(scale, scale, scale);
            playerState = PlayerState.Idle;
        }
    }

    void WindSlash()
    {
        transform.localScale = new Vector3(Constants.BASE_SCALE, Constants.BASE_SCALE, Constants.BASE_SCALE);
        playerState = PlayerState.Slashing;
        animator.SetBool(AnimProperties.isSlashing.ToString(), true);
    }

    public void EndWindSlash()
    {
        transform.localScale = new Vector3(scale, scale, scale);
        animator.SetBool(AnimProperties.isSlashing.ToString(), false);
    }
}
