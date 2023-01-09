using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollider
{
	public abstract void Collision(ICollider collider);
}
