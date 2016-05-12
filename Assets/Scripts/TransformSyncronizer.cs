using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TransformSyncronizer : NetworkBehaviour {

	[SyncVar]
	private Vector3 position;
	[SyncVar]
	private float rotation_y;

	void Update ()
	{
		if (isServer) {
			position = transform.position;
			var v = transform.rotation * Vector3.forward;
			rotation_y = Mathf.Atan2(v.x, v.z);
		} else {
			transform.position = Vector3.Lerp(transform.position, position, 0.4f);
			transform.rotation = Quaternion.Euler(0, rotation_y * Mathf.Rad2Deg, 0);
		}
	}
}
