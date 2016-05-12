using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour {

	public GameObject enemyPrefab;
	const int ENEMYNUM = 4;
	private GameObject[] enemies;

	void Start()
	{
		if (isServer) {
			enemies = new GameObject[ENEMYNUM];
			for (var i = 0; i < ENEMYNUM; ++i) {
				var go = Instantiate(enemyPrefab,
									 new Vector3(Random.Range(-10f, 10f),
												 1f,
												 Random.Range(-10f, 10f)),
									 Quaternion.identity) as GameObject;
				enemies[i] = go;
				NetworkServer.Spawn(go);
			}
		}
	}
}
