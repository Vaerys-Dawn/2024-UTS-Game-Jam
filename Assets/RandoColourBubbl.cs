using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandoColourBubbl : MonoBehaviour
{

    [SerializeField] Sprite[] _sprites;
    SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        int rand = Random.Range(0, _sprites.Length);

        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = _sprites[rand];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
