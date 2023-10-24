using UnityEngine;
using System.Collections;

public class Momentum : MonoBehaviour {
	public Vector3 velocity;
	// Use this for initialization
	void Start () {
		velocity = new Vector2(Random.Range(-9,9),Random.Range(-5,5));
		velocity=(velocity-transform.position).normalized*2f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(velocity*Time.deltaTime);

	}
}
