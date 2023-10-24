using UnityEngine;
using System.Collections;

public class Flock : MonoBehaviour {
	private readonly float VIS_DIST = 3f;
	private readonly float SEP_DIST = 2f;
	private readonly float AVO_DIST = 15f;
	private readonly float COL_LENGTH = 2f;//How far we can see using our collider for obstacles.
	private readonly float BALL_DIST = 10f;//How far we can see the ball
	private readonly float VIEW_ANGLE = 90f;
	private Vector3 velocity = Vector3.zero;
	private Vector3 lastVel = Vector3.zero;
	private float speed;
	private FlockControl fc;
	
	void Start() {
		speed = 3;
		velocity = new Vector3(Random.value * 2 - 1, Random.value * 2 - 1).normalized * speed;
		BoxCollider2D bc = GetComponent<BoxCollider2D>();
		bc.offset = new Vector2(0, COL_LENGTH / 2);
		bc.size = new Vector2(1.5f, COL_LENGTH);
	}

	public void Setup(FlockControl fc) {
		this.fc = fc;
		
	}

	void Update() {
		lastVel = velocity;
		wrap();
		Vector3 inter = flockForces(); //Individually weighted
		Vector3 seb = seekBall() * 1.25f;
		Vector3 avo = avoid() * 4f;
		Vector3 obs = obstacles() * 1.0f;
		setColor(seb);
		velocity = (velocity + inter + seb + avo + obs).normalized * speed;
		velocity = Vector3.Lerp(lastVel, velocity, .75f);//Smoooooth
		transform.Translate(velocity * Time.deltaTime, Space.World);
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x) - 90));
	}

	void setColor(Vector3 seb) {
		if (seb.Equals(Vector3.zero)) {
			if (getViewCount(VIS_DIST) != 0) {
				gameObject.GetComponent<Renderer>().material.color = Color.white;
			} else {
				gameObject.GetComponent<Renderer>().material.color = Color.blue;
			}
		} else {
			gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
	}

	Vector3 flockForces() {
		Vector3 sep = Vector3.zero;
		Vector3 ali = Vector3.zero;
		Vector3 coh = Vector3.zero;
		int visCount = 0;
		int sepCount = 0;
		for (int i=0; i<fc.flockSize(); i++) {
			Flock boid = fc.getBoid(i);
			if (boid && boid != this) {
				float dist = Vector3.Distance(transform.position, boid.transform.position);
				if (dist < SEP_DIST) { //The separation force does not take into account view angle
					sep += (transform.position - boid.transform.position) / Mathf.Pow(dist, 2f); //Force becomes exponentially larger as we get closer to the other Flock
					sepCount++;
				}
				if (canSee(boid)) {
					ali += boid.velocity; //Point us to the average velocity
					coh += boid.transform.position; //Find the average position
					visCount++;
				}
			}
		}
		if (sepCount != 0) {//Avoid division by zero when we are alone
			sep /= sepCount;
		}
		if (visCount != 0) {//The count given by the separation check may be different than this check
			coh /= visCount;
			coh = coh - transform.position; //Now we have the average position, but we need to convert it to a relative vector
			
			ali /= visCount;
		}
		return sep * 3f + ali * .8f + coh * .5f;//Return the sum of our inter-bird forces, weighted
	}
	
	Vector3 avoid() { //Avoid all predators nearby
		Vector3 force = Vector3.zero;
		int count = 0;
		for (int i = 0; i < fc.predSize(); i++) {
			Predator pred = fc.getPred(i);
			if (pred) {
				float dist = Vector3.Distance(transform.position, pred.transform.position);
				if (dist < AVO_DIST) { //Very similar to sep above
					count++;
					force += (transform.position - pred.transform.position) / Mathf.Pow(dist, 2f);//Pointing away from them, and we want it to get exponentially stronger as they approach eachother.
				}
			}
		}
		if (count > 0) {
			force /= count;
		}
		return force;
	}
	
	Vector3 seekBall() {
		Vector3 pos = Vector3.zero;
		Vector3 ballPos = fc.getBallPos();
		if (!ballPos.Equals(Vector3.back) && getViewCount(VIS_DIST) == 0) {//Vector3.back here is used like "-1" to signify no ball exists
			if (Vector3.Distance(transform.position, ballPos) < BALL_DIST) {
				pos = ballPos - transform.position;
			}
		}
		return pos;
	}
	
	int getViewCount(float viewDist) {
		int count = 0;
		for (int i=0; i<fc.flockSize(); i++) {
			Flock boid = fc.getBoid(i);
			if (boid && boid != this && Vector3.Distance(transform.position, boid.transform.position) < viewDist && inView(boid)) {
				count++;
			}
		}
		return count;
	}
	
	Vector3 obstacles() {
		Vector3 force = Vector3.zero;
		int count = 0;
		for (int i = 0; i < fc.obstSize(); i++) { //Formula in part thanks to: http://stackoverflow.com/a/1084899
			GameObject obst = fc.getObst(i);
			if (obst){
				Vector3 delta = obst.transform.position - transform.position;
				Vector3 feeler = velocity * 2f;
				float radius = obst.transform.localScale.x / 2;
				
				float a = Vector3.Dot(feeler, feeler);
				float b = 2 * Vector3.Dot(delta, feeler);
				float c = Vector3.Dot(delta, delta) - radius * radius;
				
				float dist = Mathf.Clamp((delta.magnitude - radius), .001f, delta.magnitude);
				float discriminant = b * b - 4 * a * c;
				if (discriminant >= 0) {
					// ray didn't totally miss sphere,
					// so there is a solution to
					// the equation.
					discriminant = Mathf.Sqrt(discriminant);
					float t1 = (-b - discriminant) / (2 * a);
					float t2 = (-b + discriminant) / (2 * a);
					
					// 3x HIT cases:
					//          -o->             --|-->  |            |  --|->
					// Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 
					if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1)) {
						count++;
						force -= delta.normalized / Mathf.Pow(dist, 2f);
					}
					
				}
			}
		}
		if (count != 0) {
			force /= count;
		}
		return force;
		
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
		if (coll.gameObject.CompareTag("Food")) { //Not a safe way to handle collisions, really should add some component to check on.
			Destroy(coll.gameObject);
			fc.spawnBall();
		}
	}

	private bool canSee(Flock f) {
		return Vector3.Distance(transform.position, f.transform.position) < VIS_DIST && inView(f);
	}
	
	private bool inView(Flock f) {
		float dir = Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x);
		Vector3 delta = f.transform.position - transform.position;
		float angleTo = Mathf.Rad2Deg * Mathf.Atan2(delta.y, delta.x);
		return withinAngle(dir, angleTo, VIEW_ANGLE);
	}
	
	static bool withinAngle(float a, float b, float dist) {
		return (360 - Mathf.Abs(a - b) % 360 < dist || Mathf.Abs(a - b) % 360 < dist);
	}
}
