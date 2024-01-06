using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    GameManager _gameManager;
    NetworkManager _networkManager;

    List<GameObject> enemyPool;
    List<GameObject> enemyCircleArea;
    List<GameObject> bossPool;
    List<Transform> players;

    [SerializeField] private int numOfEnemies = 20;
    [SerializeField] private int numOfEnemiesInCircle = 20;
    [SerializeField] private int numOfBosses = 2;

    [SerializeField] float spawn_radius = 15.0f;
    float spawnTimer = 2.0f;
    float spawnBossTimer = 10.0f;

    bool gathered = false;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager._instance;
        _networkManager = NetworkManager._instance;

        enemyPool = new List<GameObject>();
        enemyCircleArea = new List<GameObject>();
        bossPool = new List<GameObject>();

        if (_networkManager._isHost || _gameManager._singlePlayer)
        {

            InstantiateEnemies();
            StartCoroutine(SpawnEnemies());
            InstantiateBoss();
            StartCoroutine(SpawnBosses());
            InstantiateEnemyCircle();
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GatherAllPlayersTransforms()
    {
        players = new List<Transform>
        {
            GameManager.Instance._localPlayer.transform
        };

        foreach (var player in GameManager.Instance._remotePlayers)
        {
            players.Add(player.transform);
        }
    }
    
    public void UpdateRemote(EnemyData data)
    {
        foreach (var e in enemyPool)
        {
            if (e != null && e.GetComponent<Enemy>()._enemyData.netId == data.netId)
            {
                e.GetComponent<Enemy>().UpdateDataFromRemote(data);
                break;
            }
        }

        foreach (var e in enemyCircleArea)
        {
            if (e != null && e.GetComponent<Enemy>()._enemyData.netId == data.netId)
            {
                e.GetComponent<Enemy>().UpdateDataFromRemote(data);
                break;
            }
        }

        foreach (var e in bossPool)
        {
            if (e != null && e.GetComponent<EnemyBoss>()._enemyData.netId == data.netId)
            {
                e.GetComponent<EnemyBoss>().UpdateDataFromRemote(data);
                break;
            }
        }
    }

    #region Instantiate POOL
    // Executed by the host, creates new enemies ans saves the into a pool
    private void InstantiateEnemies()
    {
        var enemy1 = Resources.Load<GameObject>("Prefabs/Enemy1");
        var enemy2 = Resources.Load<GameObject>("Prefabs/Enemy2");

        for (int i = 0; i < numOfEnemies; i++)
        {
            GameObject enemy = null;
            if (i % 2 == 0)
            {
                enemy = Instantiate(enemy2, new Vector3(50, 50, 0), Quaternion.identity, transform);
                enemyPool.Add(enemy);
                enemyPool[i].GetComponent<Enemy>().enemType = 2;
            }
            else
            {
                enemy = Instantiate(enemy1, new Vector3(50, 50, 0), Quaternion.identity, transform);
                enemyPool.Add(enemy);
                enemyPool[i].GetComponent<Enemy>().enemType = 1;
            }

            //Give netid
            enemy.GetComponent<Enemy>()._enemyData.netId = System.Guid.NewGuid().ToString();

            enemyPool[i].GetComponent<Enemy>().alive = false;
            enemyPool[i].GetComponent<Enemy>()._local = true;
            enemyPool[i].SetActive(false);

            //Broadcast enemy creation
            if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.CREATE, enemy.GetComponent<Enemy>()._enemyData);
        }
    }

    private void InstantiateBoss()
    {
        var enemyBoss = Resources.Load<GameObject>("Prefabs/Enemy3");

        for (int i = 0; i < numOfEnemies; i++)
        {
            GameObject enemy = null;

            enemy = Instantiate(enemyBoss, new Vector3(50, 50, 0), Quaternion.identity, transform);
            bossPool.Add(enemy);
            bossPool[i].GetComponent<EnemyBoss>().enemType = 3;

            //Give netid
            enemy.GetComponent<EnemyBoss>()._enemyData.netId = System.Guid.NewGuid().ToString();

            bossPool[i].GetComponent<EnemyBoss>().alive = false;
            bossPool[i].GetComponent<EnemyBoss>()._local = true;
            bossPool[i].SetActive(false);

            //Broadcast enemy creation
            if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.CREATE, enemy.GetComponent<EnemyBoss>()._enemyData);
        }
    }

    private void InstantiateEnemyCircle()
    {
        var enemy4 = Resources.Load<GameObject>("Prefabs/Enemy4");

        for (int i = 0; i < numOfEnemiesInCircle; i++)
        {
            GameObject enemy = null;

            enemy = Instantiate(enemy4, new Vector3(50, 50, 0), Quaternion.identity,transform);
            enemyCircleArea.Add(enemy);
            enemyCircleArea[i].GetComponent<Enemy>().enemType = 4;

            //Give netid
            enemy.GetComponent<Enemy>()._enemyData.netId = System.Guid.NewGuid().ToString();

            enemyCircleArea[i].GetComponent<Enemy>().alive = false;
            enemyCircleArea[i].GetComponent<Enemy>()._local = true;
            enemyCircleArea[i].SetActive(false);

            //Broadcast enemy creation
            if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.CREATE, enemy.GetComponent<Enemy>()._enemyData);
        }
    }

    public void InstantiateEnemyClient(EnemyData data)
    {

        GameObject enemy = Instantiate(Resources.Load<GameObject>("Prefabs/Enemy" + data.enemType), new Vector3(50, 50, 0), Quaternion.identity);
        enemy.GetComponent<Enemy>()._enemyData = data;
        enemy.SetActive(false);

        if (data.enemType == 3)
        {
            bossPool.Add(enemy);
        }
        else if (data.enemType == 4)
        {
            enemyCircleArea.Add(enemy);
        }
        else
        {
            enemyPool.Add(enemy);
        }

    }

    #endregion 

    #region Spawners
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
                if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.UPDATE, enemyPool[result].GetComponent<Enemy>()._enemyData);
            }
        }
    }

    private void SpawnEnemy(GameObject enemy)
    {
        enemy.GetComponent<Enemy>().alive = true;
        enemy.GetComponent<Enemy>().health = 10;
        enemy.GetComponent<Enemy>().animator.SetBool("Dead", false);

        Vector3 spawnPos = new Vector3(Random.Range(-19,19), Random.Range(19,-19));

        enemy.transform.position = spawnPos;

        enemy.transform.rotation = Quaternion.identity;

        enemy.SetActive(true);

        if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.UPDATE, enemy.GetComponent<Enemy>()._enemyData);
    }

    IEnumerator SpawnBosses()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnBossTimer);

            int result = CheckDeadBosses();

            if (result != -1)
            {
                SpawnBoss(bossPool[result]);

                SpawnEnemiesWaveInSphere();

                //Broadcast
                if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.UPDATE, bossPool[result].GetComponent<EnemyBoss>()._enemyData);
            }
        }
    }

    private void SpawnBoss(GameObject enemy) 
    {
        enemy.GetComponent<EnemyBoss>().alive = true;
        enemy.GetComponent<EnemyBoss>().health = 10;
        enemy.GetComponent<EnemyBoss>().animator.SetBool("Dead", false);

        Vector3 spawnPos = new Vector3(Random.Range(-19, 19), Random.Range(19, -19));

        enemy.transform.position = spawnPos;

        enemy.transform.rotation = Quaternion.identity;

        enemy.SetActive(true);

        if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.UPDATE, enemy.GetComponent<EnemyBoss>()._enemyData);

    }

    private void SpawnEnemiesWaveInSphere()
    {
        //Only to get all players transforms
        if(!gathered)
        {
            GatherAllPlayersTransforms();
            gathered = true;
        }

        //Generate an average position between all players 

        Vector3 averagePos = Vector2.zero;

        foreach (var player_pos in players)
        {
            averagePos += player_pos.position;
        }
        averagePos /= players.Count;

        float thetaBetweenEnemies = (2 * Mathf.PI) / numOfEnemiesInCircle;

        for (int i = 0; i < numOfEnemiesInCircle; i++)
        {

            //Generate a random position on the sphere surface around the average position
            float x = averagePos.x + spawn_radius * Mathf.Sin(i * thetaBetweenEnemies);
            float y = averagePos.y + spawn_radius * Mathf.Cos(i * thetaBetweenEnemies);

            //In order to not spawn enemies out of the map
            DelimitatePosInsideMap(ref x, ref y);

            //SPAWN ENEMIES HERE

            int result = CheckDeadEnemies(true);

            if (result != -1)
            {

                enemyCircleArea[result].transform.SetPositionAndRotation(new Vector3(x, y, 0.0f), Quaternion.identity);

                SpawnEnemyInCircle(enemyCircleArea[result]);

            }

        }

    }

    private static void DelimitatePosInsideMap(ref float x, ref float y)
    {
        if (-19.0f > x || x > 19.0f)
        {
            x = Random.Range(-19.0f, 19.0f);
        }

        if (-19.0f > y || y > 19.0f)
        {
            y = Random.Range(-19.0f, 19.0f);
        }
    }

    private void SpawnEnemyInCircle(GameObject enemy)
    {
        enemy.GetComponent<Enemy>().alive = true;
        enemy.GetComponent<Enemy>().health = 10;
        enemy.GetComponent<Enemy>().animator.SetBool("Dead", false);

        enemy.SetActive(true);

        if (!_gameManager._singlePlayer) _networkManager.SendEnemy(Action.UPDATE, enemy.GetComponent<Enemy>()._enemyData);
    }

    int CheckDeadEnemies(bool circle = false)
    {
        if(circle) 
        {
            for (int i = 0; i < numOfEnemiesInCircle; i++)
            {

                if (enemyCircleArea[i].GetComponent<Enemy>().alive == false)
                {
                    return i;
                }

            }

        }
        else
        {
            for (int i = 0; i < numOfEnemies; i++)
            {

                if (enemyPool[i].GetComponent<Enemy>().alive == false)
                {
                    return i;
                }

            }
        }

        return -1;

    }

    int CheckDeadBosses()
    {
        for (int i = 0; i < numOfBosses; i++)
        {
            if (bossPool[i].GetComponent<EnemyBoss>().alive == false)
            {
                return i;
            }
        }
        return -1;
    }

    #endregion

}
