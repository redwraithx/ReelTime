using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    public float tapForce;
    public IPlayerListener listener;

    private bool didJump = false;
    private bool isDead = false;

    void Start()
    {
        Input.simulateMouseWithTouches = true;
    }

    void Update()
    {
        if (!isDead && Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SoundManager.PlaySound(SoundManager.Sound.Bobbing_SpaceBar);
            didJump = true;
        }
    }

    private void FixedUpdate()
    {
        if (didJump)
        {
            //GetComponent<Rigidbody2D>().AddForce(Vector2.up * tapForce, ForceMode2D.Force);
            //GetComponent<Rigidbody2D>().AddForce(Vector2.up * tapForce);
            GetComponent<Rigidbody2D>().velocity = Vector2.up * tapForce;
            didJump = false;
        }
    }

    public void PlayerDead()
    {
        GetComponent<Rigidbody2D>().simulated = false;
        isDead = true;
    }

    public void ResetPlayer()
    {
        GetComponent<Rigidbody2D>().simulated = true;
        isDead = false;
        gameObject.SetActive(true);

        transform.position = new Vector3(-1.8f, 3, 0);
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        listener.OnCollision(col.gameObject);
    }

    public interface IPlayerListener
    {
        void OnCollision(GameObject obj);
    }
}
