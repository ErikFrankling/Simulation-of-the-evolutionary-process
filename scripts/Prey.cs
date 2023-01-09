using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Mathematics;
using UnityEngine;

public class Prey : Organism, ICollider
{
	public override float visionAngle { get { return Mathf.Deg2Rad * 35; } }
	public override int VisionRange { get { return 18; } }
	public override int InputNodes { get { return 12; } }
	public override int InputAngles { get { return 6; } }


	public Prey(Controler controler, GameObject render, Vector2 position, float rotation) : base(controler, render, position, rotation) { }

	public Prey(Controler controler, GameObject render, Vector2 position, float rotation, Organism parent) : base(controler, render, position, rotation, parent) { }


	protected override float[] Movment()
	{
		float[] inputs = new float[InputNodes];
		float visionAngleSegment = visionAngle / (InputAngles - 1);

		for (int i = 0; i < InputAngles; i++)
		{
			float currentAngle = rotation - (visionAngle / 2) + (visionAngleSegment * i);

			RaycastHit2D hit = Physics2D.Raycast(position, Controler.RadTo2D(currentAngle), VisionRange, LayerMask.GetMask("Plant"));

			if (hit.collider != null)
				inputs[i] = Mathf.InverseLerp(VisionRange, 1, hit.distance);
			else
				inputs[i] = 0;
		}
		for (int i = 0; i < InputAngles; i++)
		{
			float currentAngle = rotation - (visionAngle / 2) + (visionAngleSegment * i);

			RaycastHit2D hit = Physics2D.Raycast(position, Controler.RadTo2D(currentAngle), VisionRange, LayerMask.GetMask("Predator"));

			if (hit.collider != null)
				inputs[i + InputAngles] = Mathf.InverseLerp(VisionRange, 1, hit.distance);
			else
				inputs[i + InputAngles] = 0;
		}

		return genome.Calculate(inputs);

		/*nearestPlant = null;
		Predator nearestPredator = null;

		foreach(Organism organism in organismsInRange)
		{
			if(organism is Plant)
			{
				if (nearestPlant == null)
					nearestPlant = (Plant)organism;

				else if (Vector2.Distance(position, organism.position) < Vector2.Distance(position, nearestPlant.position))
					nearestPlant = (Plant)organism;
			}

			else if (organism is Predator)
			{
				if (nearestPredator == null)
					nearestPredator = (Predator)organism;

				else if(Vector2.Distance(position, organism.position) < Vector2.Distance(position, nearestPredator.position))
					nearestPredator = (Predator)organism;
			}
		}

		organismsInRange = new List<Organism>();

		if (energy > controler.ReproductionRequirement)
			return Vector2.zero;

		if (nearestPlant != null)
		{
			return (nearestPlant.position - position).normalized;
		}
		else if (nearestPredator != null)
		{
			return (nearestPredator.position - position).normalized * -1;
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
		if (collider is Predator)
		{
			Die(DeathCauses.Predator);
		}
		else if (collider is Plant)
		{
			Plant plant = (Plant)collider;
			if (plant.growthTick <= controler.ticks - 1)
			{
				ticksSinceLastFood = controler.ticks;
				energy = Mathf.Clamp(controler.energyAnimalFood + energy, controler.energyMin, controler.energyMax);
			}
		}
	}
}
