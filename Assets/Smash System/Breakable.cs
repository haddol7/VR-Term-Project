using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public List<GameObject> breakablePieces;
    public int hitsToBreak = 3;
    private int currentHits = 0;
    public string breakingStoneTag = "BreakingStone";

    void Start()
    {
        // 아무것도 안 함! 그냥 놔두기
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(breakingStoneTag))
        {
            currentHits++;
            Debug.Log("충돌 횟수: " + currentHits + "/" + hitsToBreak);

            if (currentHits >= hitsToBreak)
            {
                Break();
            }
        }
    }

    public void Break()
    {
        Debug.Log("Break 시작!");

        // Stone_Big의 월드 위치 저장
        Vector3 stonePosition = transform.position;

        foreach (var item in breakablePieces)
        {
            if (item != null)
            {
                // 월드 위치 먼저 저장
                Vector3 worldPos = item.transform.position;

                // 부모에서 분리
                item.transform.SetParent(null);

                // 월드 위치 유지
                item.transform.position = worldPos;

                // 활성화
                item.SetActive(true);

                Debug.Log(item.name + " 생성 위치: " + item.transform.position);

                // Rigidbody & Collider
                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb == null) rb = item.AddComponent<Rigidbody>();

                BoxCollider box = item.GetComponent<BoxCollider>();
                if (box == null) box = item.AddComponent<BoxCollider>();
            }
        }

        gameObject.SetActive(false);
    }
}