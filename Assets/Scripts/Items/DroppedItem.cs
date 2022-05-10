﻿using System.Collections;
using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private float despawnTime;
    
    public Item OriginalItem { private get; set; }
    
    private PlayerInventory _inventory;

    private void Start()
    {
        _inventory = FindObjectOfType<PlayerInventory>();
        GetComponent<SpriteRenderer>().sprite = OriginalItem.GetComponent<SpriteRenderer>().sprite;
        
        StartCoroutine(Rescale());
        StartCoroutine(Despawn());
    }

    private IEnumerator Rescale()
    {
        while (true)
        {
            for (var i = 0; i < 25; ++i)
            {
                transform.localScale = new Vector3(transform.localScale.x - 0.01f, transform.localScale.y - 0.01f);
                yield return new WaitForSeconds(0.05f);
            }
            
            for (var i = 0; i < 25; ++i)
            {
                transform.localScale = new Vector3(transform.localScale.x + 0.01f, transform.localScale.y + 0.01f);
                yield return new WaitForSeconds(0.05f);
            }
            
            yield return new WaitForSeconds(0.001f);
        }
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(OriginalItem);
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (_inventory.Insert(OriginalItem))
        {
            Destroy(gameObject);
        }
    }
}