using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    GameManager _gameManager;
    NetworkManager _networkManager;

    List<GameObject> enemyPool;
    [SerializeField] private int numOfEnemies = 20;
    float spawnTimer = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager._instance;
        _networkManager = NetworkManager._instance;

        enemyPool = new List<GameObject>();

        if (_networkManager._isHost)
        {
            InstantiateEnemies();
            StartCoroutine(SpawnEnemies());
        }
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Executed by the host, creates new enemies ans saves the into a pool
    private void InstantiateEnemies()
    {
        var enemy1 = Resources.Load<GameObject>("Prefabs/Enemy1");
        var enemy2 = Resources.Load<GameObject>("Prefabs/Enemy2");

        for (int i = 0; i < numOfEnemies; i++)
        {
            GameObject enemy = null;
            if(i % 2 == 0)
            {
                enemy = Instantiate(enemy2, gameObject.transform);
                enemyPool.Add(enemy);
                enemyPool[i].GetComponent<Enemy>().enemType = 2;
            }
            else
            {
                enemy = Instantiate(enemy1, gameObject.transform);
                enemyPool.Add(enemy);
                enemyPool[i].GetComponent<Enemy>().enemType = 1;
            }

            //Give netid
            enemy.GetComponent<Enemy>()._enemyData.netId = System.Guid.NewGuid().ToString();

            enemyPool[i].GetComponent<Enemy>().alive = false;
            enemyPool[i].GetComponent<Enemy>()._local = true;
            enemyPool[i].SetActive(false);

            //Broadcast enemy creation
            _networkManager.SendEnemy(Action.CREATE, enemy.GetComponent<Enemy>()._enemyData);
        }
    }

    public void InstantiateEnemyClient(EnemyData data)
    {
        GameObject enemy = Instantiate(Resources.Load<GameObject>("Prefabs/Enemy"+data.enemType), gameObject.transform);
        enemy.GetComponent<Enemy>()._enemyData = data;
        enemy.SetActive(false);

        enemyPool.Add(enemy);
    }

    public void UpdateRemote(EnemyData data)
    {
        foreach (var e in enemyPool)
        {
            if (e.GetComponent<Enemy>()._enemyData.netId == data.netId)
            {
                e.GetComponent<Enemy>().UpdateDataFromRemote(data);
                break;
            }
        }
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTimer);

            int result = CheckDeadEnemies();

            if(result != -1)
            {
                SpawnEnemy(enemyPool[result]);

                //Broadcast
                _networkManager.SendEnemy(Action.UPDATE, enemyPool[result].GetComponent<Enemy>()._enemyData);
            }
        }
    }

    private void SpawnEnemy(GameObject enemy)
    {
        enemy.GetComponent<Enemy>().alive = true;
        enemy.GetComponent<Enemy>().health = 10;
        enemy.GetComponent<Enemy>().animator.SetBool("Dead", false);

        enemy.GetComponent<Enemy>().sprite.color = Color.white;

        Vector3 spawnPos = new Vector3(Random.Range(-19,19), Random.Range(19,-19));

        enemy.transform.position = spawnPos;

        enemy.transform.rotation = Quaternion.identity;

        enemy.SetActive(true);

        NetworkManager._instance.SendEnemy(Action.UPDATE, enemy.GetComponent<Enemy>()._enemyData);
    }

    int CheckDeadEnemies()
    {
        for (int i = 0;i < numOfEnemies;i++) 
        {
            if (enemyPool[i].GetComponent<Enemy>().alive == false)
            {
                return i;
            }
        }
        return -1;
    }

}
