using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance; // �̱��� ������ ���� �ڱ� �ڽ� ����
    public WeaponDatabase weaponDB;
    public MonsterDatabase monsterDB;

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
        if (weaponDB == null) Debug.LogError("WeaponDatabase is not assigned in ObjectPooler!");
        if (monsterDB == null) Debug.LogError("MonsterDatabase is not assigned in ObjectPooler!");

        // 1. ���� ����� ��ȸ�ϸ� Ǯ ���� �Լ� ȣ��
        if (weaponDB != null)
        {
            foreach (var weaponData in weaponDB.allWeapons)
            {
                CreatePool(weaponData.weaponName, weaponData.projectilePrefab, weaponData.poolSize);
            }
        }

        // 2. ���� ����� ��ȸ�ϸ� �Ȱ��� Ǯ ���� �Լ� ȣ��
        if (monsterDB != null)
        {
            foreach (var monsterData in monsterDB.allMonsters)
            {
                CreatePool(monsterData.monsterName, monsterData.monsterPrefab, monsterData.poolSize);
            }
        }
    }
    private void CreatePool(string tag, GameObject prefab, int size)
    {
        //1. ���� ���⿡ �Ҵ�� �߻�ü ������(���赵)�� ���°�?
        //2. poolDictionary�� �� ���� �̸����� �� Ǯ�� �̹� ��ϵǾ� �ִ°�?
        if (prefab == null || poolDictionary.ContainsKey(tag))
        {
            return;
        }

        // �̰� �ñ��ؼ� ã�ƺôµ� �޸� �Ҵ��س����� c#�� GC(�������÷���)�� �ֱ������� �޸� ��ȸ�ϸ鼭 �������� �ʴ� ������ �޸� ����
        // �ٵ� ������Ʈ�� �̸� �ν��Ͻ�ȭ �س��� ��Ƽ��� �����״� �ϴϱ� ������ �÷��Ͱ� �����߿� �۵��� ���ؼ� GC Spike x
        List<GameObject> objectPool = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            //1. projectilePrefab, 2. spawnPoint.position, spawnPoint.rotation (transform)
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            objectPool.Add(obj);
        }

        //�������� ���
        poolDictionary.Add(tag, objectPool);

        //��������� �޲ٴ¿뵵
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

        //�ڵ尡���� ì���
        List<GameObject> pool = poolDictionary[tag];

        for (int i = 0; i < pool.Count; i++)
        {
            //���� Ȯ������ ������Ʈ�� ��Ȱ��ȭ �����ΰ�? <- �Ʊ� obj.SetActive(false); �ߴ��� ã�°�
            if (!pool[i].activeInHierarchy)
            {
                GameObject objectToSpawn = pool[i];

                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
                objectToSpawn.SetActive(true);

                return objectToSpawn;
            }
        }

        //���� Ȱ��ȭ ���¶�� �߰��� ������Ʈ ����
        GameObject newObj = Instantiate(prefabDictionary[tag], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        pool.Add(newObj);
        Debug.LogWarning($"Pool with tag '{tag}' was extended.");
        return newObj;
    }
}