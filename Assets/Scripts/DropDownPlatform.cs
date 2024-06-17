using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDownPlatform : MonoBehaviour
{
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        col.enabled = UserInput.instance.MoveInput.y > -0.4;
        
    }
}
