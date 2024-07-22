using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadFishSpawn : MonoBehaviour
{
    public GameObject deadFishPrefab;
    public GameObject bubblesPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            SpawnDeadFish();
        }
    }

    public void SpawnDeadFish()
    {
        // spawn dead fish
        GameObject deadFish = Instantiate(deadFishPrefab, transform.position, Quaternion.identity);

        // spawn bubbles
        GameObject bubbles = Instantiate(bubblesPrefab, transform.position, Quaternion.identity);


    }

}
