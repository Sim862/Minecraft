using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    public ObjectParticleData.ParticleName particleName;
    public float speed = 100;
    public bool canPickUp = false;
    public Rigidbody rigidbody;
    static int player_Layer = int.MaxValue;
    static int block_Layer = int.MaxValue;

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {

        if(player_Layer == int.MaxValue)
            player_Layer = LayerMask.NameToLayer("Player");
        if(block_Layer == int.MaxValue)
            block_Layer = LayerMask.NameToLayer("Block");

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
    }

    public bool test = false;
    private void Update()
    {
        if (test)
        {
            transform.position = new Vector3(transform.position.x, Random.Range(1, 20), transform.position.z);
            transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 90), 0, 0));
            test = false;
            Fire(transform.forward);
        }
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Fire(Vector3 direction)
    {
        startPosition = transform.position;
        rigidbody.AddForce(direction * speed);
        rigidbody.useGravity = true;
        StartCoroutine(Cor_Distory());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == player_Layer)
        {
            if (canPickUp)
            {

            }
            else
            {
                //other.GetComponent<PlayerMove>().UpdateHP(-5);
                transform.position -= (transform.position + rigidbody.velocity - other.transform.position).normalized * 0.3f;
                startPosition = transform.position; ;
                rigidbody.velocity = Vector3.zero;
            }
        }
        else if (other.gameObject.layer == block_Layer)
        {
            transform.position -= (transform.position + rigidbody.velocity - other.transform.position).normalized * 0.3f;
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
