using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance; // 싱글톤 패턴을 위한 자기 자신 참조
    public WeaponDatabase weaponDB;

    Dictionary<string, List<GameObject>> poolDictionary;
    Dictionary<string, GameObject> prefabDictionary;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolDictionary = new Dictionary<string, List<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();

        InitializePools();
    }

    void InitializePools()
    {
        //디버깅 디버깅
        if (weaponDB == null)
        {
            Debug.LogError("WeaponDatabase is not assigned in ObjectPooler!");
            return;
        }


        //무기수만큼
        foreach (var weaponData in weaponDB.allWeapons)
        {
            //1. 현재 무기에 할당된 발사체 프리팹(설계도)이 없는가?
            //2. poolDictionary에 이 무기 이름으로 된 풀이 이미 등록되어 있는가?
            if (weaponData.projectilePrefab == null || poolDictionary.ContainsKey(weaponData.weaponName))
                continue;

            // 이거 궁금해서 찾아봤는데 메모리 할당해놓은거 c#의 GC(가비지컬렉터)가 주기적으로 메모리 순회하면서 참조하지 않는 데이터 메모리 날림
            // 근데 오브젝트를 미리 인스턴스화 해놓고 액티브로 껐다켰다 하니까 가비지 컬렉터가 게임중엔 작동을 안해서 GC Spike x
            List<GameObject> objectPool = new List<GameObject>();

            for (int i = 0; i < weaponData.poolSize; i++)
            {
                //1. projectilePrefab, 2. spawnPoint.position, spawnPoint.rotation (transform)
                GameObject obj = Instantiate(weaponData.projectilePrefab, transform);
                obj.SetActive(false);
                objectPool.Add(obj);
            }

            //메인으로 사용
            poolDictionary.Add(weaponData.weaponName, objectPool);

            //찐빠났을때 메꾸는용도
            prefabDictionary.Add(weaponData.weaponName, weaponData.projectilePrefab);
        }
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        //코드가독성 챙기기
        List<GameObject> pool = poolDictionary[tag];

        for (int i = 0; i < pool.Count; i++)
        {
            //지금 확인중인 오브젝트가 비활성화 상태인가? <- 아까 obj.SetActive(false); 했는지 찾는거
            if (!pool[i].activeInHierarchy)
            {
                GameObject objectToSpawn = pool[i];

                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
                objectToSpawn.SetActive(true);

                return objectToSpawn;
            }
        }

        //전부 활성화 상태라면 추가로 오브젝트 생성
        GameObject newObj = Instantiate(prefabDictionary[tag], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        pool.Add(newObj);
        Debug.LogWarning($"Pool with tag '{tag}' was extended.");
        return newObj;
    }
}