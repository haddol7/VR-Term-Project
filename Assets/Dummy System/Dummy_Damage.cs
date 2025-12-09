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

    [Header("사운드 파일 연결")]
    private AudioSource audioSource;
    public AudioClip hitSound; 
    public AudioClip dieSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /*void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= heavyDamageThreshold)
        {
            PlayHeavyDamageAnimation();
            audioSource.PlayOneShot(dieSound);
            StartCoroutine(DieAndRevive());
        }
        else if (impactSpeed >= lightDamageThreshold)
        {
            PlayLightDamageAnimation();
            audioSource.PlayOneShot(hitSound);
        }
    }*/

    void OnCollisionEnter(Collision collision)
    {
        // 1. 태그 확인: "Stickable" (새 무기) 혹은 "BreakingStone" (그냥 돌) 일 때만 반응
        if (collision.gameObject.CompareTag("Stickable") || collision.gameObject.CompareTag("BreakingStone"))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;

            // 디버그용: 충돌한 물체 이름과 속도를 콘솔에 찍어봄 (테스트할 때 도움됨)
            Debug.Log($"충돌 감지! 대상: {collision.gameObject.name} / 태그: {collision.gameObject.tag} / 속도: {impactSpeed}");

            if (impactSpeed >= heavyDamageThreshold)
            {
                PlayHeavyDamageAnimation();
                if (dieSound != null) audioSource.PlayOneShot(dieSound); // 오디오 없으면 에러나니까 체크
                StartCoroutine(DieAndRevive());
            }
            else if (impactSpeed >= lightDamageThreshold)
            {
                PlayLightDamageAnimation();
                if (hitSound != null) audioSource.PlayOneShot(hitSound);
            }
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
