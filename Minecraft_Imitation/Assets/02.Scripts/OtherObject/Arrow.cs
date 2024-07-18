using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public ObjectParticleData.ParticleName particleName;
    public bool canPickUp = false;
    public Rigidbody rigidbody;
    private static float speed = 200;
    private static int player_Layer = int.MaxValue;
    private static int block_Layer = int.MaxValue;
    private static int mob_Layer = int.MaxValue;
    private bool isPlayer = false;
    // Start is called before the first frame update
    void Awake()
    {

        if(player_Layer == int.MaxValue)
            player_Layer = LayerMask.NameToLayer("Player");
        if(block_Layer == int.MaxValue)
            block_Layer = LayerMask.NameToLayer("Block");
        if (mob_Layer == int.MaxValue)
            mob_Layer = LayerMask.NameToLayer("Mob");

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Fire(Vector3 direction, float loadTime, bool isPlayer = false)
    {
        this.isPlayer = isPlayer;
        transform.LookAt(transform.position + direction);
        rigidbody.AddForce(direction * speed * loadTime);
        rigidbody.useGravity = true;
        StartCoroutine(Cor_Distory());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == player_Layer)
        {
            if(!canPickUp && !isPlayer)
            {
                canPickUp = true;
                other.GetComponent<PlayerMove>().UpdateHP(-1);
                transform.position -= (transform.position - (transform.position - rigidbody.velocity * 10)).normalized * 0.2f;
                rigidbody.velocity = Vector3.zero;
            }
        }
        else if(other.gameObject.layer == mob_Layer)
        {
            if (!canPickUp && isPlayer)
            {
                canPickUp = true;
                other.GetComponent<Mob>().UpdateHP(PlayerManager.instance.player.transform, -10, 1f);
                transform.position -= (transform.position - (transform.position - rigidbody.velocity * 10)).normalized * 0.2f;
                rigidbody.velocity = Vector3.zero;
            }
        }
        else if (other.gameObject.layer == block_Layer)
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < 0.5)
            {
                transform.position -= (transform.position - (transform.position - rigidbody.velocity * 10)).normalized * 0.2f;
            }
            else
            {
                transform.position += (transform.position - (transform.position - rigidbody.velocity * 10)).normalized * 0.2f;
            }
            transform.LookAt(transform.position + rigidbody.velocity);
            rigidbody.velocity = Vector3.zero;
            rigidbody.useGravity = false;
            canPickUp = true;
        }
    }

    IEnumerator Cor_Distory()
    {
        yield return new WaitForSeconds(20);
        Destroy(gameObject);
    }
}
