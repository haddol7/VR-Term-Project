using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy_Damage : MonoBehaviour
{
    [Header("애니메이터 설정")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider mycollider;

    [Header("데미지 임계값 설정")]
    [SerializeField] private float lightDamageThreshold = 3f; 
    [SerializeField] private float heavyDamageThreshold = 8f; 

    [Header("애니메이션 파라미터 이름")]
    [SerializeField] private string lightDamageTrigger = "LightDamage";
    [SerializeField] private string heavyDamageTrigger = "HeavyDamage";


    void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= heavyDamageThreshold)
        {
            PlayHeavyDamageAnimation();
            StartCoroutine(DieAndRevive());
        }
        else if (impactSpeed >= lightDamageThreshold)
        {
            PlayLightDamageAnimation();
            
        }
    }

    private void PlayLightDamageAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(lightDamageTrigger);
        }
    }

    private void PlayHeavyDamageAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(heavyDamageTrigger);
            mycollider.enabled = false;
        }
    }
    IEnumerator DieAndRevive()
    {
        yield return new WaitForSeconds(3.0f);
        animator.SetTrigger("ReviveTrigger");
        Debug.Log("OK");
        mycollider.enabled = true;
    }
}
