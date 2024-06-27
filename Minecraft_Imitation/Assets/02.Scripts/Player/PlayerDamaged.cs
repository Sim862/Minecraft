using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDamaged : MonoBehaviour
{
    public GameObject damagedEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlayDamagedEff()
    {
        // 피격 UI를 활성화
        damagedEffect.SetActive(true);

        // 0.3초간 대기
        yield return new WaitForSeconds(0.3f);

        // 피격 UI를 비활성화.
        damagedEffect.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            StartCoroutine(PlayDamagedEff());
        }
    }
}
