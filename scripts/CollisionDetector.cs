using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
	public ICollider colliderObject;
	public int id;

	public void OnTriggerStay2D(Collider2D trigger)
	{
		//Debug.Log(collider.GetType() +  " " + trigger.gameObject.GetComponent<CollisionDetector>().collider.GetType());
		colliderObject.Collision(trigger.gameObject.GetComponent<CollisionDetector>().colliderObject);
	}
}
