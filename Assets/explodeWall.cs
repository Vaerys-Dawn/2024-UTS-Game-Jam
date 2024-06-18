using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explodeWall : MonoBehaviour
{
    PlayerMovement player;
    [SerializeField] GameObject explodeParticle;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bonking");
        if (collision.collider.CompareTag("Player"))
        {

            Vector3 direction = (player.transform.transform.position - this.transform.position).normalized;
        
            Debug.Log("Bonking Again");
            if (!player.IsDashing)
            {
                player.playBonk();
                return;
            }
            Debug.Log("Bonking Again Again");
            Vector3 objTrans = Vector3.zero;
            Quaternion objRot = Quaternion.identity;
            gameObject.transform.GetPositionAndRotation(out objTrans, out objRot);
            objRot.eulerAngles = new Vector3(objRot.eulerAngles.x, objRot.eulerAngles.y, objRot.eulerAngles.z +  direction.x > 0 ? 90 : 270);
            GameObject obj = Instantiate(explodeParticle, new Vector3(objTrans.x + (direction.x > 0 ? 1 : -1), objTrans.y, objTrans.z), objRot);
            ParticleSystem par = obj.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = par.shape;
            shape.scale = new Vector3(transform.localScale.y / 2, 1, 1);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
