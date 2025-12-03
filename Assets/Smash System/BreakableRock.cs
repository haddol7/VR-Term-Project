using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableRock : MonoBehaviour
{
    [Header("조각난 돌들")]
    public List<GameObject> brokenPieces; // 조각들 여기 연결

    [Header("설정")]
    public float explosionForce = 100f;

    private bool isBroken = false;

    void Start()
    {
        Debug.Log("BreakableRock 시작! 돌 이름: " + gameObject.name);

        // 조각들 처음에 꺼두기
        foreach (GameObject piece in brokenPieces)
        {
            if (piece != null)
                piece.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 뭐든 닿으면 로그 찍기
        Debug.Log("=== 충돌 발생! ===");
        Debug.Log("충돌한 오브젝트: " + collision.gameObject.name);
        Debug.Log("태그: " + collision.gameObject.tag);
        Debug.Log("================");

        // Weapon 태그 체크
        if (collision.gameObject.CompareTag("Weapon"))
        {
            Debug.Log(">>> WEAPON 확인! 부수기 시작!");
            Smash();
        }
        else
        {
            Debug.Log(">>> Weapon 태그 아님. 무시.");
        }
    }

    void Smash()
    {
        Debug.Log("Smash 함수 실행됨!");

        if (isBroken)
        {
            Debug.Log("이미 부서진 상태, 무시");
            return;
        }

        if (brokenPieces.Count == 0)
        {
            Debug.LogError("조각이 하나도 연결 안 됨!");
            return;
        }

        isBroken = true;
        Debug.Log("조각 개수: " + brokenPieces.Count);

        // 조각들 활성화 + 폭발
        foreach (GameObject piece in brokenPieces)
        {
            if (piece != null)
            {
                Debug.Log("조각 활성화: " + piece.name);
                piece.SetActive(true);
                piece.transform.parent = null; // 독립시키기

                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddExplosionForce(explosionForce, transform.position, 2f);
                }
            }
        }

        // 온전한 돌 끄기
        Debug.Log("온전한 돌 비활성화");
        gameObject.SetActive(false);
    }
}