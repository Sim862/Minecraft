using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDamaged : MonoBehaviour
{
    public GameObject damagedEffect;

    public void DamagedEff()
    {
        StartCoroutine(PlayDamagedEff());
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            DamagedEff();
        }
    }
   
}
