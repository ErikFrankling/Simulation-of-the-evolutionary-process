using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Organism
{
	public int id;

	public int children;

	public int startingTick;

	public int ticksSinceLastFood = -1;

	public bool isDead = false;
	public enum DeathCauses { Predator, Prey, Starvation, Age, Edge };
	public DeathCauses deathCause;

	public Controler controler;
	public GameObject render;

	public Vector2 position;

	public int age = 0;

	public float energy;

	public List<Organism> organismsInRange = new List<Organism>();

	public float rotation;

	public float geneSpeed;

	public virtual int OutputsNodes { get { return 2; } }
	public virtual int InputNodes { get { return 12; } }
	public virtual int BiasNodes { get { return 1; } }
	
	public virtual float AddNodeMutationRate { get { return 50; } }
	public virtual float ConnectionWeightMutationRate { get { return AddNodeMutationRate + 20; } }
	public virtual float AddConnectionMutationRate { get { return ConnectionWeightMutationRate + 15; } }
	public virtual float RemoveConnectionMutationRate { get { return AddConnectionMutationRate + 1; } }

	public virtual float ConnectionWeightMutationChange { get { return 0.5f; } }

	public virtual float visionAngle { get { return Mathf.Deg2Rad * 35; } }
	public virtual int InputAngles { get { return 6; } }

	public virtual int VisionRange { get { return 18; } }


	public Genome genome;

	public Organism(Controler controler, GameObject render, Vector2 position, float rotation)
	{
		this.position = position;
		this.controler = controler;
		this.render = render;

		startingTick = controler.ticks;

		energy = controler.energyStart + (UnityEngine.Random.Range(-controler.energyStart, controler.energyStart) * 0.25f);

		render.transform.position = position;
		render.GetComponent<CollisionDetector>().colliderObject = (ICollider)this;

		id = controler.GetId();

		this.rotation = rotation;


		genome = new Genome(this);

		//render.transform.rotation = Quaternion.Euler(Vector3.forward * (rotation - 90));
	}

	public Organism(Controler controler, GameObject render, Vector2 position, float rotation, Organism parent)
	{
		this.position = position;
		this.controler = controler;
		this.render = render;

		startingTick = controler.ticks;

		energy = controler.energyStart + (UnityEngine.Random.Range(-controler.energyStart, controler.energyStart) * 0.25f);

		render.transform.position = position;

		id = controler.GetId();

		this.rotation = rotation;

		render.GetComponent<CollisionDetector>().colliderObject = (ICollider)this;
		render.GetComponent<CollisionDetector>().id = id;

		genome = new Genome(this, parent.genome);

		//render.transform.rotation = Quaternion.Euler(Vector3.forward * (rotation - 90));
	}

	double turnRateTemp = 0;
	double moveTemp = 0;

	object genomeTemp = null;
	object genomeTemp2 = null;

	Genome.connectionGene[] connectionGenestemp = null;

	Dictionary<int, double> activationsTemp = null;
	Genome.nodeGene[] nodeTemp = null;

	public void Update()
	{
		//Rotating and moving


			//using StreamWriter swTotal = File.AppendText(Controler.pathDebug);
			//{

			//	swTotal.WriteLine(id + ";" + render.transform.position.x + ";" + render.transform.position.y + ";" + position.x + ";" + position.y + ";" + rotation);
				
			//}

		float[] cal = Movment();

		rotation += cal[0] * Mathf.Deg2Rad * controler.turnSpeed;
		//if (position.x > (controler.size / 2) + 7)
		//	Debug.Log(1);
		if (position.x > controler.size / 2)
		{
			Vector2 temp = Controler.RadTo2D(rotation);
			temp = new Vector2((cal[1] > 0) ? -1 : 1 * Mathf.Abs(temp.x), temp.y);
			rotation = -Vector2.SignedAngle(Vector2.up, temp) * Mathf.Deg2Rad;
		}
		else if (position.y > controler.size / 2)
		{
			Vector2 temp = Controler.RadTo2D(rotation);
			temp = new Vector2(temp.x, (cal[1] > 0) ? -1 : 1 * Mathf.Abs(temp.y));
			rotation = Vector2.SignedAngle(Vector2.up, temp) * Mathf.Deg2Rad;
		}
		else if (position.x < -controler.size / 2)
		{
			Vector2 temp = Controler.RadTo2D(rotation);
			temp = new Vector2((cal[1] > 0) ? 1 : -1 * Mathf.Abs(temp.x), temp.y);
			rotation = -Vector2.SignedAngle(Vector2.up, temp) * Mathf.Deg2Rad;
		}
		else if (position.y < -controler.size / 2)
		{
			Vector2 temp = Controler.RadTo2D(rotation);
			temp = new Vector2(temp.x, (cal[1] > 0) ? 1 : -1 * Mathf.Abs(temp.y));
			rotation = Vector2.SignedAngle(Vector2.up, temp) * Mathf.Deg2Rad;
		}

		position += Controler.RadTo2D(rotation) * controler.speed * cal[1];
		//if ((Controler.RadTo2D(rotation) * controler.speed * cal[1]).magnitude > controler.speed)
		//	Debug.Log((Controler.RadTo2D(rotation) * controler.speed * cal[1]).magnitude);
		//Debug.Log(rotation + " 2D: " + Controler.RadTo2D(rotation) + " pos: " + position);

		render.transform.position = position;
		//render.transform.rotation = Quaternion.Euler(Vector3.forward * rotation);

		//Removing moving energy
		energy = Mathf.Clamp(energy - controler.baseEnergyCost - controler.energyAnimalMoveCost * Mathf.Abs(cal[1]), controler.energyMin, controler.energyMax);

		//dying of starvation
		if (energy <= 0)
			Die(DeathCauses.Starvation);

		//Reproduction
		if (energy > controler.ReproductionRequirement)
		{
			if (controler.Reproduce(this))
				energy -= controler.ReproductionCost;
		}

		//Dying of Age
		if (age > controler.MinAge)
		{
			if (UnityEngine.Random.Range(0, 25) == 0)
			{
				Die(DeathCauses.Age);
			}
		}

		age++;
	}

	public void Die(DeathCauses deathCause)
	{
		isDead = true;
		this.deathCause = deathCause;
	}

	protected abstract float[] Movment();
}
