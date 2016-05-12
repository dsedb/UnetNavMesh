using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EnemyController : NetworkBehaviour {
	private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter character;
    private NavMeshAgent agent;
	private GameObject[] wayPoints;

    public override void OnStartServer()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		agent.updatePosition = true;
		character = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
		wayPoints = GameObject.FindGameObjectsWithTag("waypoint");
		StartCoroutine(loop());
    }
	IEnumerator loop()
	{
		for (;;) {
			var p = wayPoints[Random.Range(0, wayPoints.Length)].transform.position;
			agent.SetDestination(p);
			while (agent.pathPending)
				yield return null;
			var now = Time.time;
			while (agent.remainingDistance > agent.stoppingDistance && (Time.time - now) < 5f) {
				if (agent.remainingDistance > agent.stoppingDistance)
					character.Move(agent.desiredVelocity, false, false);
				else
					character.Move(Vector3.zero, false, false);
				yield return null;
			}
		}
	}
}
