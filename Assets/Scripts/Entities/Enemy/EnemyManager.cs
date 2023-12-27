using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    List<GameObject> enemyPool;
    [SerializeField] private int numOfEnemies = 20;
    float spawnTimer = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        enemyPool = new List<GameObject>();

        InstantiateEnemies();

        StartCoroutine(SpawnEnemies());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void InstantiateEnemies()
    {
        var enemy1 = Resources.Load<GameObject>("Prefabs/Enemy1");
        var enemy2 = Resources.Load<GameObject>("Prefabs/Enemy2");

        for (int i = 0; i < numOfEnemies; i++)
        {
            if(i % 2 ==0)
            {
                enemyPool.Add(Instantiate(enemy2, gameObject.transform));
                enemyPool[i].GetComponent<Enemy>().enemType = 2;
            }
            else
            {
                enemyPool.Add(Instantiate(enemy1, gameObject.transform));
                enemyPool[i].GetComponent<Enemy>().enemType = 1;
            }
            
            enemyPool[i].GetComponent<Enemy>().alive = false;
            enemyPool[i].SetActive(false);
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
