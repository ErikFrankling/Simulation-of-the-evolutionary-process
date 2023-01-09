using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class Species
{
	public List<Genome> genomes;

	public List<Genome> Genomes 
	{
		get 
		{
			//genomes.Last().organism.render.GetComponent<SpriteRenderer>().color = color;

			return genomes;
		}
		set
		{
			genomes = value;

			//genomes.Last().organism.render.GetComponent<SpriteRenderer>().color = color;
		}
	}
	public Genome reference;
	public int id = 0;
	public Color color;

	public Species(List<Genome> genomes, Genome reference, int id)
	{
		this.Genomes = genomes;
		if (reference == null)
			Debug.Log(1);
		else
			this.reference = reference;
		this.id = id;
		color = Random.ColorHSV();

		foreach (Genome genome in genomes)
			genome.organism.render.GetComponent<SpriteRenderer>().color = color;
	}

	public void newRefrence(ref Dictionary<int, Species> speciesList, Controler controler)
	{
		if (genomes.Count <= 1 && reference == Genomes[0])
		{
			speciesList.Remove(id);
			return;
		}

		(double, Genome) lowestDis = (double.MaxValue, null);
		foreach (Genome genome in Genomes)
		{
			if (genome == reference)
				continue;

			double tempDis = Genome.Distance(genome, reference);
			if (lowestDis.Item1 > tempDis)
				lowestDis = (tempDis, genome);
		}

		if (lowestDis.Item2 == null)
			reference = Genomes[0];

		reference = lowestDis.Item2;

		foreach (Genome genome in Genomes)
		{
			if (Genome.Distance(reference, genome) > controler.speciationFreshhold)
			{
				genome.species = controler.CalculateSpecies(genome);
			}
		}

		double dis;

		foreach (Species species in speciesList.Values)
		{
			if (species == this)
				continue;

			foreach (Genome genome2 in species.Genomes)
			{
				dis = Genome.Distance(genome2, reference);
				if (dis < controler.speciationFreshhold)
				{
					double disToCurrent = Genome.Distance(genome2, species.reference);

					if (dis < disToCurrent)
					{
						genome2.species = id;
						speciesList[species.id].Genomes.Remove(genome2);

						if (speciesList[species.id].Genomes.Count == 0)
						{
							speciesList.Remove(species.id);
						}
						else if (speciesList[species.id].reference == genome2)
						{
							speciesList[species.id].newRefrence(ref speciesList, controler);
						}

						speciesList[id].Genomes.Add(genome2);
					}
				}
			}
		}
	}
}
