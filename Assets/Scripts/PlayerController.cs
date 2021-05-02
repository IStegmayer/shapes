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
    Vector2 lookDirection = new Vector2(1, 0);
    float startChargeTime = 0;
    float chargeTime = 0;
    float launchTime = 0;
    float scale = 1.0f;
    bool keyDown = false;
    bool keyUp = false;

    enum PlayerState {Launching, Charging, Walking, Idle}
    PlayerState playerState;

    // Start is called before the first frame update
    void Start()
    {
        playerState = PlayerState.Idle;
        rigidbody2d = GetComponent<Rigidbody2D>();
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
                if (!(playerState == PlayerState.Charging)) keyDown = true;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("GetKeyUp");
                if (playerState == PlayerState.Charging) keyUp = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (keyDown)
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
            scale = 1.0f - ((Time.time - startChargeTime) / maxChargeTime);
            if (0.2f < scale && scale < 1.0f) 
               transform.localScale = new Vector3(scale, scale, scale);
        }

        if (playerState == PlayerState.Launching)
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
            scale = 1.0f;
            transform.localScale = new Vector3(scale, scale, scale);
            playerState = PlayerState.Idle;
        }
    }
}
