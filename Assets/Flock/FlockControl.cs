using UnityEngine;
using System.Collections;

public class FlockControl : MonoBehaviour { //Creates the 4 different types of objects, contains them and handles ball respawning
	public static readonly float ROOM_WIDTH = 38; //Actually twice this
	public static readonly float ROOM_HEIGHT = 30;
	public static bool makeBoids = true;
	public static bool makeObs = true;
	public static bool makeBall = true;
	public static bool makePred = true;
	public GameObject prefBoid;
	private int spawnCount = 300;//Total to spawn
	private int birdCount = 0;//Number spawned
	private Flock[] boids;
	public GameObject prefObst;
	private int obstCount = 10;
	private GameObject[] obstacles;
	private static readonly float MIN_SIZE = 3f;
	private static readonly float MAX_SIZE = 10f;
	public GameObject prefBall;
	private GameObject curBall;
	public GameObject prefPred;
	private int predCount = 3;
	private Predator[] preds;
	
	void Start() {
		Time.timeScale=1.5f;
		boids = new Flock[spawnCount];
		preds = new Predator[predCount];
		obstacles = new GameObject[obstCount];
		if (makeBoids) {
			for (int i = 0; i < spawnCount; i++) {
				spawn();
			}
		}
		if (makePred) {
			for (int i = 0; i < predCount; i++) {
				preds [i] = Instantiate<GameObject>(prefPred).GetComponent<Predator>();
				preds [i].transform.position = new Vector3(Random.Range(-ROOM_WIDTH, ROOM_WIDTH), Random.Range(-ROOM_HEIGHT, ROOM_HEIGHT));
				preds [i].setup(this);
			}
		}
		if (makeObs) {
			for (int i = 0; i < obstCount; i++) {
				obstacles [i] = Instantiate<GameObject>(prefObst);
				float size = Random.Range(MIN_SIZE, MAX_SIZE);
				obstacles [i].transform.localScale = new Vector3(size, size, 1f);
				obstacles [i].transform.position = new Vector3(Random.Range(-ROOM_WIDTH, ROOM_WIDTH), Random.Range(-ROOM_HEIGHT, ROOM_HEIGHT));
			}
		}
		if (makeBall) {
			spawnBall();
		}
	}

	void Update() { //Just so we can speed up or slow down simulation
		float timeAdjust = .4f;
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Time.timeScale += timeAdjust;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			Time.timeScale -= timeAdjust;
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			Time.timeScale = 1f;
		}
	}

	void spawn() {
		if (spawnCount > 0) {
			GameObject boid = Instantiate<GameObject>(prefBoid);
			boid.transform.position = new Vector3(Random.Range(-ROOM_WIDTH, ROOM_WIDTH), Random.Range(-ROOM_HEIGHT, ROOM_HEIGHT));
			boids [birdCount] = boid.GetComponent<Flock>();
			boids [birdCount].Setup(this);
			birdCount++;
			spawnCount--;
		}
	}

	public int flockSize() {
		return birdCount;
	}

	public Flock getBoid(int i) {
		return boids [i];
	}

	public int predSize() {
		return predCount;
	}

	public Predator getPred(int i) {
		return preds [i];
	}

	public int obstSize() {
		return obstCount;
	}

	public GameObject getObst(int i) {
		return obstacles [i];
	}

	public Vector3 getBallPos() {
		if (curBall) {
			return curBall.transform.position;
		} else {
			return Vector3.back;
		}
	}

	public void spawnBall() { //Spawn a ball in an open slot
		Vector2 position;
		Collider2D[] colliders;
		bool hasCollision=false;
		do { //Assuming we do not have a collision, this runs once
			hasCollision=false;
			position = new Vector3(Random.Range(-ROOM_WIDTH, ROOM_WIDTH), Random.Range(-ROOM_HEIGHT, ROOM_HEIGHT));
			colliders = Physics2D.OverlapCircleAll(position,2f);
			for (int i = 0; i < colliders.Length; i++) {
				if (colliders[i].gameObject.CompareTag("Obstacle")){ //We are colliding with an obstacle and need to go over again.
					hasCollision=true;
				}

			}

		} while (hasCollision);
		curBall = Instantiate<GameObject>(prefBall);
		curBall.transform.position = position;
	}
}
