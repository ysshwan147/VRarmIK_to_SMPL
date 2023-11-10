using UnityEngine;
using VRArmIKtoSMPL;

public class AvatarOffset : MonoBehaviour {

	public Transform targetHead;
	public UpperBodyTransform upperBody;

	float height = 1.5f;

	// Use this for initialization
	void Start () {
		height = upperBody.playerHeightHmd - upperBody.headHmdHeightOffset;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = targetHead.position;
		pos.y -= height;

        transform.position = pos;
	}
}
