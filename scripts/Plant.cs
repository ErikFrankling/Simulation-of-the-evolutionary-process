using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Organism;

public class Plant : ICollider
{
	public GameObject gameObject;
	public bool growth;
	public int growthTick;
	private Controler controler;

	public Plant(int x, int y, Controler controler)
	{
		growthTick = controler.ticks;
		growth = Random.Range(0f, 1f) > controler.startingPlants ? false : true;
		gameObject = Object.Instantiate(controler.plantPrefab, new Vector3(x, y, 0), Quaternion.Euler(Vector3.zero));
		gameObject.SetActive(growth);
		this.controler = controler;

		gameObject.GetComponent<CollisionDetector>().colliderObject = this;
		if(growth)
			controler.plantPopulation++;
	}

	public void Grow()
	{
		growthTick = controler.ticks;
		growth = true;
		gameObject.SetActive(growth);//Mathf.Clamp01(growth + controler.plantGrowthPerTick);
		controler.plantPopulation++;


		/*if (growth)
			gameObject.SetActive(true);
		else
			gameObject.SetActive(false);*/
	}

	public void Collision(ICollider collider)
	{
		if (collider is Prey)
		{
			if(growth == true)
				controler.plantPopulation--;
			growth = false;
			gameObject.SetActive(growth);
		}
	}
}
