using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance; // ????? ?????? ???? ??? ??? ????
    public WeaponDatabase weaponDB;
    public MonsterDatabase monsterDB;
    public EffectDatabase effectDB;

    private Dictionary<string, List<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabDictionary;

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
        if (weaponDB == null) Debug.LogError("WeaponDatabase is not assigned!");
        if (monsterDB == null) Debug.LogError("MonsterDatabase is not assigned!");

        if (weaponDB != null)
        {
            foreach (var weaponData in weaponDB.allWeapons)
            {
                if (weaponData == null || weaponData.skills == null) continue;

                foreach (var skillData in weaponData.skills)
                {
                    if (skillData != null && skillData.skillPrefab != null)
                    {
                        CreatePool(skillData.skillName, skillData.skillPrefab, skillData.poolSize); // skillData.poolSize ?? ????
                    }
                }
            }
        }

        if (monsterDB != null)
        {
            foreach (var monsterData in monsterDB.allMonsters)
            {
                if (monsterData == null) continue;

                if (monsterData.monsterPrefab != null)
                {
                    CreatePool(monsterData.monsterName, monsterData.monsterPrefab, monsterData.poolSize);
                }

                if (monsterData is RangeMonsterData rangedData && rangedData.skills != null)
                {
                    foreach (var skillData in rangedData.skills)
                    {
                        if (skillData != null && skillData.skillPrefab != null)
                        {
                            CreatePool(skillData.skillName, skillData.skillPrefab, skillData.poolSize); // skillData.poolSize ?? ????
                        }
                    }
                }
            }
        }

        if (effectDB != null)
        {
            foreach (var effectPrefab in effectDB.allEffects)
            {
                if (effectPrefab != null)
                {
                    // ????? ???????? '???'?? ?¡¾?? ?????? ? ????
                    CreatePool(effectPrefab.name, effectPrefab, 10);
                }
            }
        }
    }

    private void CreatePool(string tag, GameObject prefab, int size)
    {
        //1. ???? ???? ???? ???? ??????(????)?? ???¡Æ??
        //2. poolDictionary?? ?? ???? ??????? ?? ??? ??? ????? ??¡Æ??
        if (prefab == null || poolDictionary.ContainsKey(tag))
        {
            return;
        }

        // ??? ?????? ?????? ??? ?????????? c#?? GC(???????¡À???)?? ????????? ??? ?????? ???????? ??? ?????? ??? ????
        // ??? ????????? ??? ?¥í????? ????? ?????? ??????? ???? ?????? ?¡À???? ??????? ????? ????? GC Spike x
        List<GameObject> objectPool = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            //1. projectilePrefab, 2. spawnPoint.position, spawnPoint.rotation (transform)
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            objectPool.Add(obj);
        }

        //???????? ???
        poolDictionary.Add(tag, objectPool);

        //????????? ???¢¯”³
        prefabDictionary.Add(tag, prefab);
        Debug.Log($"Pool for '{tag}' created with size {size}.");
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        //??????? ????
        List<GameObject> pool = poolDictionary[tag];

        for (int i = 0; i < pool.Count; i++)
        {
            //???? ??????? ????????? ?????? ???????? <- ??? obj.SetActive(false); ????? ??¡Æ?
            if (!pool[i].activeInHierarchy)
            {
                GameObject objectToSpawn = pool[i];

                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
                objectToSpawn.SetActive(true);

                return objectToSpawn;
            }
        }

        //???? ???? ???¢Ò?? ????? ??????? ????
        GameObject newObj = Instantiate(prefabDictionary[tag], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        pool.Add(newObj);
        Debug.LogWarning($"Pool with tag '{tag}' was extended.");
        return newObj;
    }
}