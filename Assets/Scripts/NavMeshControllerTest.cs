using UnityEngine;
using System.Collections;

public class NavMeshControllerTest : MonoBehaviour {
	private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter m_Character;
    private NavMeshAgent agent;
    void Start() {
		agent = GetComponent<NavMeshAgent>();
		m_Character = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
    }

    void Update() {
		if (agent == null)
			return;
		
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				agent.SetDestination(hit.point);
			}
		}

		m_Character.Move(agent.desiredVelocity, false /* crouch */, false /* m_Jump */);
    }
}
