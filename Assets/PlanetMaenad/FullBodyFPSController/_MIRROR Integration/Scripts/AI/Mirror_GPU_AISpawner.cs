using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Mirror_GPU_AISpawner : NetworkBehaviour
{
    public Transform SpawnPoint;
    public bool SpawnOnStart;
    public bool SpawnOnEnable;
    public int SpawnAmount;
    [Space(10)]


    public bool ConstantSpawn;
    public int ConstantMaxAmount = 100;
    public int SpawnBurstAmount = 3;
    public float SpawnRateInSeconds = 1;
    public float SpawnRadius = 15f;
    [Space(10)]

    public GameObject[] SpawnObjectPrefabs;
    [Space(10)]


    public List<GameObject> SpawnedObjects;


    internal WaitForSeconds StartSpawnDelay = new WaitForSeconds(3f);
    internal WaitForSeconds ConstantSpawnRateDelay;


    void OnEnable()
    {
        if (isServer)
        {
            ConstantSpawnRateDelay = new WaitForSeconds(SpawnRateInSeconds);

            if (SpawnOnEnable && !SpawnOnStart)
            {
                StartCoroutine(StartSpawnObjectsDelay());
            }

            if (ConstantSpawn)
            {
                StartCoroutine(ConstantSpawnDelay());
            }
        }
      
    }
    void Start()
    {
        if (isServer)
        {
            ConstantSpawnRateDelay = new WaitForSeconds(SpawnRateInSeconds);

            if (!SpawnOnEnable && SpawnOnStart)
            {
                StartCoroutine(StartSpawnObjectsDelay());
            }

            if (ConstantSpawn)
            {
                StartCoroutine(ConstantSpawnDelay());
            }
        }
   
    }


    IEnumerator StartSpawnObjectsDelay()
    {
        yield return StartSpawnDelay;

        SpawnObjects(SpawnPoint.transform.position, SpawnRadius, SpawnAmount);
    }
    IEnumerator ConstantSpawnDelay()
    {
        while (ConstantSpawn)
        {
            yield return ConstantSpawnRateDelay;

            CheckEmptySpawnedList();

            if (SpawnedObjects.Count < ConstantMaxAmount)
            {
                SpawnObjects(SpawnPoint.transform.position, SpawnRadius, SpawnBurstAmount);
            }
        }
    }

    public void CheckEmptySpawnedList()
    {
        if (SpawnedObjects.Count > 0)
        {
            for (int i = 0; i < SpawnedObjects.Count; i++)
            {
                if (SpawnedObjects[i] == null)
                {
                    SpawnedObjects.Remove(SpawnedObjects[i]);
                }
            }
        }
    }




    public void SpawnObjects()
    {

        //Maxed out Spawns
        if (SpawnedObjects.Count >= SpawnAmount)
        {
            return;
        }

        //Choose Objects To Spawn 
        foreach (var _ in Enumerable.Range(0, SpawnAmount))
        {
            if (SpawnedObjects.Count < SpawnAmount)
            {
                var RandomSpawnObject = Random.Range(0, SpawnObjectPrefabs.Length);
                var targetObject = SpawnObjectPrefabs[RandomSpawnObject];

                NavMeshHit navMeshHit;
                var randomNavHit = NavMesh.SamplePosition(Vector3.zero + Random.insideUnitSphere * SpawnRadius, out navMeshHit, Mathf.Infinity, NavMesh.AllAreas);
                var randomYRot = new Vector3(0, Random.Range(0f, 360f), 0);

                if (randomNavHit)
                {
                    var go = Instantiate(targetObject, navMeshHit.position, Quaternion.Euler(randomYRot));
                    go.SetActive(true);

                    if (!SpawnedObjects.Contains(go))
                    {
                        SpawnedObjects.Add(go);
                    }

                    NetworkServer.Spawn(go);
                }
            }
        }


    }
    public void SpawnObjects(Vector3 SpawnCenter, float SpawnRange, int SpawnCount)
    {

        //Maxed out Spawns
        if (SpawnedObjects.Count >= ConstantMaxAmount)
        {
            return;
        }

        //Choose Objects To Spawn 
        foreach (var _ in Enumerable.Range(0, SpawnCount))
        {
            if (SpawnedObjects.Count < ConstantMaxAmount)
            {
                var RandomSpawnObject = Random.Range(0, SpawnObjectPrefabs.Length);
                var targetObject = SpawnObjectPrefabs[RandomSpawnObject];

                NavMeshHit navMeshHit;
                var randomNavHit = NavMesh.SamplePosition(SpawnCenter + Random.insideUnitSphere * SpawnRange, out navMeshHit, Mathf.Infinity, NavMesh.AllAreas);
                var randomYRot = new Vector3(0, Random.Range(0f, 360f), 0);

                if (randomNavHit)
                {
                    var go = Instantiate(targetObject, navMeshHit.position, Quaternion.Euler(randomYRot));
                    go.SetActive(true);

                    if (!SpawnedObjects.Contains(go))
                    {
                        SpawnedObjects.Add(go);
                    }

                    NetworkServer.Spawn(go);
                }
            }
        }


    }

    public void ChangeSpawnAmount(string Count)
    {
        SpawnAmount = int.Parse(Count);
    }
    public void DestroyObjects()
    {
        if (isServer)
        {
            for (int i = 0; i < SpawnedObjects.Count; i++)
            {
                if (SpawnedObjects[i] != null)
                {
                    NetworkServer.Destroy(SpawnedObjects[i].gameObject);
                }
            }

            SpawnedObjects.Clear();
        }

    }



}


#if UNITY_EDITOR
[CustomEditor(typeof(Mirror_GPU_AISpawner))]
public class CustomInspectorMirror_GPU_AISpawner : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Mirror_GPU_AISpawner Mirror_GPU_AISpawner = (Mirror_GPU_AISpawner)target;


        EditorGUILayout.LabelField("_____________________________________________________________________________");

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Spawn Objects"))
        {
            Mirror_GPU_AISpawner.SpawnObjects();
        }
        GUILayout.Space(5);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Destroy Spawned Objects"))
        {
            Mirror_GPU_AISpawner.DestroyObjects();
        }


    }

}
#endif