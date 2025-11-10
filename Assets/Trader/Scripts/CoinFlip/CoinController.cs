using UnityEngine;

public class CoinController : MonoBehaviour
{
public Transform coinTransform;       // 코인 오브젝트 Transform
    public float spinDuration = 1.0f;     // 회전 지속 시간
    public float spinSpeed = 720f;        // 회전 속도 (deg/sec)
    [Range(0f,1f)]
    public float headsProbability = 0.6f; // 앞면 나올 확률

    public float zRotateMagnitude = 30f;  // Z축 회전 뒤틀림 강도
    public float zPosMagnitude = -7f;     // Z축 위치 얼마나 이동할지 (예: 0 → zPosMagnitude)
    

    private bool isFlipping = false;
    private bool resultHeads;
    private float startAngleX;
    private float startAngleZ;
    private float startPosZ;

    void Start()
    {

        Vector3 initEuler = coinTransform.eulerAngles;
        startAngleX = initEuler.x;
        startAngleZ = initEuler.z;
        startPosZ = coinTransform.position.z;
    }

    public void FlipCoin()
    {
        StartCoroutine(FlipRoutine());
    }

    private System.Collections.IEnumerator FlipRoutine()
    {
        isFlipping = true;
        // 앞면 나올지 미리 결정
        resultHeads = (Random.value < headsProbability);
        float elapsed = 0f;
        float targetAngleX = startAngleX + spinSpeed * spinDuration;
        Vector3 startPos = coinTransform.position;
        Vector3 targetPos = new Vector3(startPos.x, startPos.y, startPosZ + zPosMagnitude);

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            
            // X축 회전만
            float currentAngleX = Mathf.Lerp(startAngleX, targetAngleX, t);
            
            // Z축 위치 이동 (0 → -7 → 0)
            float currentPosZ = Mathf.Lerp(startPosZ, targetPos.z, t < 0.5f ? (t * 2f) : ((1f - t) * 2f));
            
            coinTransform.eulerAngles = new Vector3(
                currentAngleX,
                coinTransform.eulerAngles.y,
                startAngleZ  // Z 회전 고정
            );
            
            coinTransform.position = new Vector3(
                coinTransform.position.x,
                coinTransform.position.y,
                currentPosZ
            );
            
            yield return null;
        }

        // 회전 끝난 뒤 앞/뒷면 정리 & Z축 원위치 복귀
        float finalPosZ = startPosZ; // 위치 Z축 원위치

        if (resultHeads)
        {
            // 앞면: X=90, Z=0
            coinTransform.eulerAngles = new Vector3(90f, coinTransform.eulerAngles.y, 0f);
        }
        else
        {
            // 뒷면: X=90, Z=180
            coinTransform.eulerAngles = new Vector3(90f, coinTransform.eulerAngles.y, 180f);
        }

        coinTransform.position = new Vector3(
            coinTransform.position.x,
            coinTransform.position.y,
            finalPosZ
        );

        Debug.Log("결과: " + (resultHeads ? "앞면" : "뒷면"));
        isFlipping = false;
    }
}
