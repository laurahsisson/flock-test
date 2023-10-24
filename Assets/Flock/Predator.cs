using UnityEngine;
using System.Collections;

public class Predator : MonoBehaviour {
	FlockControl fc;
	Vector3 velocity = Vector3.zero;
	float speed = 4f;
	public bool printInfo = false;
	// Use this for initialization
	void Start() {
		velocity = new Vector3(Random.value * 2 - 1, Random.value * 2 - 1).normalized * speed;
	}
	
	// Update is called once per frame
	void Update() {

		wrap();
		velocity += group() + hunt() + obstacles();
		velocity = velocity.normalized * speed;
		transform.Translate(velocity * Time.deltaTime, Space.World);
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x) - 90));
	}

	private Vector3 group() {//Make all the predators group up
		Vector3 force = Vector3.zero;
		for (int i = 0; i < fc.predSize(); i++) {
			Predator pred = fc.getPred(i);
			if (fc.getPred(i) != this) {
				Vector3 delta = pred.transform.position - transform.position;
				float dist = Vector3.Distance(pred.transform.position, transform.position);
				float A = 80f;//Starting power
				float B = 10f;
				float n = 1.1f;//How much it increases exponentially
				float m = -.01f;
				float magnitude = -(A / Mathf.Pow(dist, n)) + (B / Mathf.Pow(dist, m));
				
				force += delta.normalized * magnitude;
			}
			
		}
		return force;
	}

	private Vector3 hunt() { //Hunt down the prey
		Vector3 force = Vector3.zero;
		for (int i = 0; i < fc.flockSize(); i++) {
			if (fc.getBoid(i)) {
				Vector3 delta = fc.getBoid(i).transform.position - transform.position;
				float dist = Vector3.Distance(fc.getBoid(i).transform.position, transform.position);
				float B = 160f;
				float m = 2f;
				
				float magnitude = (B / Mathf.Pow(dist, m));
				if (printInfo) {
					Debug.DrawRay(transform.position, delta.normalized * magnitude);
				}
				force += delta.normalized * magnitude;
			}
		}
		return force;
		
	}

	private Vector3 obstacles() { //Avoid obstacles
		Vector3 force = Vector3.zero;
		
		for (int i=0; i< fc.obstSize(); i++) {
			GameObject obst = fc.getObst(i);
			Vector3 delta = obst.transform.position - transform.position;
			float size = obst.transform.localScale.x / 2;//Find the radius, not diameter.
			float dist = Vector3.Distance(obst.transform.position, transform.position);
			float A = 1.5f;//Starting power
			float n = 2f;//How much it increases exponentially
			bool print = false;
			if (dist - size < 0) {
				print = true;
			}
			dist = Mathf.Clamp((dist - size), .001f, dist); //Make sure it is never zero, safe subtracton
			
			float magnitude = -(A / Mathf.Pow(dist, n));
			force += delta.normalized * magnitude;
			
		}
		return force;
		
	}

	public void setup(FlockControl fc) {
		this.fc = fc;
	}

	void wrap() {
		float posX = transform.position.x;
		float posY = transform.position.y;
		if (posX > FlockControl.ROOM_WIDTH) {
			posX = -FlockControl.ROOM_WIDTH;//+2f;//+Random.Range(0,2f);
		}
		if (posX < -FlockControl.ROOM_WIDTH) {
			posX = FlockControl.ROOM_WIDTH;//-2f;//-Random.Range(0,2f);
		}
		if (posY > FlockControl.ROOM_HEIGHT) {
			posY = -FlockControl.ROOM_HEIGHT;//+2f;//+Random.Range(0,2f);
		}
		if (posY < -FlockControl.ROOM_HEIGHT) {
			posY = FlockControl.ROOM_HEIGHT;//-2f;//-Random.Range(0,2f);
		}
		transform.position = new Vector3(posX, posY);
	}
	
	void OnTriggerEnter2D(Collider2D coll) {
		//newBoid(Clone)
		if (coll.gameObject.CompareTag("Prey")) {
			Destroy(coll.gameObject);
		}
	}
}
