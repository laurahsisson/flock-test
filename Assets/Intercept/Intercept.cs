using UnityEngine;
using System.Collections;

public class Intercept : MonoBehaviour {
	public Vector3 velocity;
	public float lastAngle=0;
	bool started=false;
	public GameObject marker;
	// Use this for initialization
	void Start () {
		Invoke("intercept",.1f);
		velocity = new Vector3(0,2);
	}
	
	// Update is called once per frame
	void Update () {
		if (started){
			transform.Translate(velocity*Time.deltaTime);
		}
	}
	void intercept(){ //
		lastAngle=transform.rotation.eulerAngles.z;
		Momentum m =((Momentum)FindObjectOfType(typeof(Momentum)));
		Vector3 vr = m.velocity-velocity;
		Vector3 sr = m.transform.position-transform.position;
		float tc = sr.magnitude/vr.magnitude;
		Vector3 st = m.transform.position+(m.velocity*tc);
		Vector3 ds = st-transform.position;
		marker.transform.position=st;
		transform.rotation=Quaternion.Euler(new Vector3(0,0,Mathf.Rad2Deg*Mathf.Atan2 (ds.y,ds.x)-90));
		Debug.Log(transform.rotation.eulerAngles.z-lastAngle);
		//velocity=(st-transform.position).normalized*2f;

		started=true;
		Invoke("intercept",.1f);
	}
}
