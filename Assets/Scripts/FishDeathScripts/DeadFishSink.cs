using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadFishSink : MonoBehaviour
{
    [SerializeField] private float timeInSecondsTillDestroy = 2f;
    
    private float speed = 2f;
    private SpriteRenderer deadFishSpriteRenderer;

    public Sprite deadFishSprite;

    private void Start()
    {
        deadFishSpriteRenderer = GetComponent<SpriteRenderer>();

        if (timeInSecondsTillDestroy < 0)
        {
            timeInSecondsTillDestroy = 0.5f;
        }
    }


    public void NowDead()
    {
        gameObject.GetComponent<Animator>().enabled = false;

        deadFishSpriteRenderer.flipY = true;
        deadFishSpriteRenderer.sprite = deadFishSprite;


        StartCoroutine("DestroyTimer");
    }


    private void Update()
    {
            transform.Translate(new Vector2(-1, -1) * speed * Time.deltaTime);

            if (speed > 0)
            {
                speed -= 0.001f;
                
            }
            else
            {
                speed = 0.001f;

            }
        
    }


    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(timeInSecondsTillDestroy);

        Destroy(gameObject);
    }
}
