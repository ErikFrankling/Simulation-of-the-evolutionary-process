using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : Organism, ICollider
{
	public override int InputNodes { get { return 6; } }
	public override int InputAngles { get { return 6; } }
	public override float visionAngle { get { return Mathf.Deg2Rad * 35; } }
	public override int VisionRange { get { return 18; } }




	public Predator(Controler controler, GameObject render, Vector2 position, float rotation) : base(controler, render, position, rotation) { }

	public Predator(Controler controler, GameObject render, Vector2 position, float rotation, Organism parent) : base(controler, render, position, rotation, parent) { }

	protected override float[] Movment()
	{
		/*double[] inputs = new double[InputNodes];
		float visionAngleSegment = visionAngle / (InputAngles - 1);

		for (int i = 0; i < InputAngles; i++)
		{
			float currentAngle = rotation - (visionAngle / 2) + (visionAngleSegment * i);

			RaycastHit2D hit = Physics2D.Raycast(position, Controler.RadTo2D(currentAngle), VisionRange, LayerMask.GetMask("Prey"));

			if (hit.collider != null)
				inputs[i] = Mathf.InverseLerp(VisionRange, 1, hit.distance);
			else
				inputs[i] = 0;
		}

		double[] cal = genome.Calculate(inputs);

		return ((float)cal[0] * Mathf.Deg2Rad * controler.turnSpeed, (float)cal[1]);*/

		float[] inputs = new float[InputNodes];
		float visionAngleSegment = visionAngle / (InputAngles - 1);

		for (int i = 0; i < InputAngles; i++)
		{
			float currentAngle = rotation - (visionAngle / 2) + (visionAngleSegment * i);

			RaycastHit2D hit = Physics2D.Raycast(position, Controler.RadTo2D(currentAngle), VisionRange, LayerMask.GetMask("Prey"));

			if (hit.collider != null)
				inputs[i] = Mathf.InverseLerp(VisionRange, 1, hit.distance);
			else
				inputs[i] = 0;
		}
		//for (int i = 0; i < InputAngles; i++)
		//{
		//	float currentAngle = rotation - (visionAngle / 2) + (visionAngleSegment * i);

		//	RaycastHit2D hit = Physics2D.Raycast(position, Controler.RadTo2D(currentAngle), VisionRange, LayerMask.GetMask("Edge"));

		//	if (hit.collider != null)
		//		inputs[i + InputAngles] = Mathf.InverseLerp(VisionRange, 1, hit.distance);
		//	else
		//		inputs[i + InputAngles] = 0;
		//}

		return genome.Calculate(inputs);
		/*Prey nearestPrey = null;

		foreach (Organism organism in organismsInRange)
		{
			if (organism is Prey)
			{
				if (nearestPrey == null)
					nearestPrey = (Prey)organism;

				else if (Vector2.Distance(position, organism.position) < Vector2.Distance(position, nearestPrey.position))
					nearestPrey = (Prey)organism;
			}
		}

		organismsInRange = new List<Organism>();
		
		if (energy > controler.ReproductionRequirement)
			return Vector2.zero;

		if (nearestPrey != null)
		{
			return (nearestPrey.position - position).normalized;
		}
		else if (Random.Range(0, controler.turnChance) == 0)
		{
			return Controler.OnUnitCircle();
		}
		else
			return rotation;*/
	}

	public void Collision(ICollider collider)
	{
		if (collider is Prey)
		{
			ticksSinceLastFood = controler.ticks;
			energy = Mathf.Clamp(controler.energyAnimalFood + energy, controler.energyMin, controler.energyMax);
		}
	}
}
