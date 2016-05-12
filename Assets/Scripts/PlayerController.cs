using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter character;
    private NavMeshAgent agent;

	[SyncVar]
	int syncTime;
	[SyncVar]
	float agentSpeed;
	[SyncVar]
	float agentBaseOffset;
	[SyncVar(hook="OnAgentPositionUpdated")]
	Vector3 agentPosition;
	[SyncVar]
	Quaternion agentRotation;

	private class SyncCorners : SyncListStruct<Vector3> {}
	private SyncCorners syncCorners = new SyncCorners();
	private float updated_time;
	private float delayFromServer = 0;

    public override void OnStartServer()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		agent.updatePosition = true;
		agentBaseOffset = agent.baseOffset;
		agentPosition = transform.position;
		agentRotation = transform.rotation;
		character = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
    }

	void OnAgentPositionUpdated(Vector3 posision)
	{
		updated_time = Time.time;
		agentPosition = posision;
		if (!isServer) {
			byte error;
			var connection = NetworkManager.singleton.client.connection;
			int delay = NetworkTransport.GetRemoteDelayTimeMS(connection.hostId,
															  connection.connectionId,
															  syncTime,
															  out error);
			if (error == 0) {
				delayFromServer = (float)delay * 0.001f;
			}
		}
	}

    void Update()
    {
		if (isServer) {
			syncCorners.Clear();
			for (var i = 0; i < agent.path.corners.Length; ++i) {
				syncCorners.Add(agent.path.corners[i]);
			}
			agentSpeed = agent.desiredVelocity.magnitude;
			agentPosition = transform.position;
			agentRotation = transform.rotation;
			syncTime = NetworkTransport.GetNetworkTimestamp();
		} else {
			float distance = agentSpeed * delayFromServer;
			var offset = new Vector3(0f, agentBaseOffset, 0f);
			float sum = 0f;
			Vector3 pos = agentPosition;
			Vector3 target = agentPosition; // fallback
			var tmp_velocity = Vector3.zero;
			for (var i = 1; i < syncCorners.Count; ++i) {
				var next_pos = syncCorners[i] + offset;
				var step = (next_pos - pos).magnitude;
				sum += step;
				if (sum >= distance) {
					Vector3 velocity = (next_pos - pos).normalized * agentSpeed;
					tmp_velocity = velocity; // todo
					target = pos + velocity * Time.deltaTime;
					break;
				}
				pos = next_pos;
			}
			
			float elapsed = Time.time - updated_time;
			// transform.position = target + elapsed * tmp_velocity;
			transform.position = Vector3.Lerp(transform.position, target + elapsed * tmp_velocity, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, agentRotation, 0.1f);
		}

		// debug
		for (var i = 0; i < syncCorners.Count; ++i) {
			Color col = (i == syncCorners.Count -1) ? new Color(0.5f, 1f, 0.5f) : new Color(0.5f, 0.5f, 1f);
			DebugExtension.DebugCircle(syncCorners[i], col, 0.30f, 0 /* duration */, false /* depthTest */);
			DebugExtension.DebugCircle(syncCorners[i], col, 0.26f, 0 /* duration */, false /* depthTest */);
			DebugExtension.DebugCircle(syncCorners[i], col, 0.24f, 0 /* duration */, false /* depthTest */);
		}
		
		if (isServer) {
            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
		}

        if (isLocalPlayer) {
			if (Input.GetMouseButtonDown(0)) {
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out hit)) {
					CmdFire(hit.point);
				}
			}
		}
   }

    [Command]
    void CmdFire(Vector3 point)
    {
		agent.SetDestination(point);
    }



}
