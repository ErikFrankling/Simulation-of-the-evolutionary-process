using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class Controler : MonoBehaviour
{
	private List<Organism> organisms = new();

	public bool render = true;

	public int size = 1000;
	public int maxOrganisms;

	public bool randomReplicationLocation;

	public int tickTime;

	public float speed = 1;

	public float visionRange = 10;

	public int turnChance = 10;

	public int startingPredators = 5;
	public int startingPrey = 5;

	public int ticks = 0;
	public int endTicks = 600;

	public float energyMax = 100;
	public float energyMin = 0;
	public float energyAnimalMoveCost = 1;
	public float energyAnimalFood = 20;
	public float energyStart = 19;

	public int MinAge = 100;
	public int DeatChance = 25;

	public int ReproductionChance = 5; 
	public int ReproductionRequirement = 50;
	public int ReproductionCost = 20;

	public float mutationRate = 0.05f;

	public GameObject preyPrefab;
	public GameObject predatorPrefab;
	public GameObject plantPrefab;

	public GameObject linePrefab;
	public GameObject panelPrefab;

	public Canvas canvas;

	public float baseEnergyCost;

	public static string pathData => Application.dataPath + "dataTest1.csv";
	public static string pathTotal => Application.dataPath + "totalTest1.csv";
	public static string pathDebug => Application.dataPath + "Debug.csv";

	public float plantGrowthPerTick;

	private Plant[] plants;

	public Organism selected;

	public float turnSpeed;

	public float startingPlants;

	public GameObject edgePrefab;

	public bool renderGenome2;

	// Start is called before the first frame update
	void Start()
	{
		for (int i = 0; i < 50; i++)
		{
			panels.Add(Instantiate(panelPrefab));
			panels[i].transform.position = new Vector2(10000, 10000);

		}
		for (int i = 0; i < 200; i++)
		{
			lines.Add(Instantiate(linePrefab));
			lines[i].SetActive(false);
		}

		GameObject right = Instantiate(edgePrefab, new Vector2(size / 2f + 1, 0), Quaternion.identity);
		right.transform.localScale = new Vector3(0.5f, size + 2, 1);
		right.GetComponent<CollisionDetector>().colliderObject = new Edge();

		GameObject left = Instantiate(edgePrefab, new Vector2(-size / 2f - 1, 0), Quaternion.identity);
		left.transform.localScale = new Vector3(0.5f, size + 2, 1);
		left.GetComponent<CollisionDetector>().colliderObject = new Edge();

		GameObject top = Instantiate(edgePrefab, new Vector2(0, size / 2f + 1), Quaternion.identity);
		top.transform.localScale = new Vector3(size + 2, 0.5f, 1);
		top.GetComponent<CollisionDetector>().colliderObject = new Edge();

		GameObject bot = Instantiate(edgePrefab, new Vector2(0, -size / 2f - 1), Quaternion.identity);
		bot.transform.localScale = new Vector3(size + 2, 0.5f, 1);
		bot.GetComponent<CollisionDetector>().colliderObject = new Edge();

		plants = new Plant[size * size];

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				plants[x * size + y] = new Plant(x - size / 2, y - size / 2, this);
			}
		}

		for (int i = 0; i < startingPredators; i++)
		{
			organisms.Add(new Predator(this, Instantiate(predatorPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f)));
		}
		for (int i = 0; i < startingPrey; i++)
		{
			organisms.Add(new Prey(this, Instantiate(preyPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f)));
		}
		if (renderGenome)
		{
			selected = organisms[UnityEngine.Random.Range(0, organisms.Count)];
			selected.genome.RenderGenome(canvas, ref lines, ref panels);
		}
		
		//File.CreateText(pathData).Close();
		File.CreateText(pathTotal).Close();

		using StreamWriter swTotal = File.CreateText(pathTotal);
		{
			Prey prey = new Prey(this, Instantiate(preyPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f));
			Predator predator = new Predator(this, Instantiate(predatorPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f));
			swTotal.WriteLine("Size;Speed;StartingPredators;StartingPrey;EnergyMax;EnergyMin;EnergyMoveCost;EnergyFood;EnergyStart;ReproductionRequirement;ReproductionCost;BaseEnergyCost;PlantGrowthPerTick;TurnSpeed;StartingPlants;SPeciationFreshhold;AdddNodeMutationRate;AddConnectionMutationRate;WeightMutationRate;RemoveConnectionRate;PreyVisionAngel;PreyVisionAngels;PreyVisionRange;PreyInputNodes;PredatorVisionAngel;PredatorVisionAngels;PredatorVisionRange;PredatorInputNodes");
			object[] variables = { size, speed, startingPredators, startingPrey, energyMax, energyMin, energyAnimalMoveCost, energyAnimalFood, energyStart, ReproductionRequirement, ReproductionCost, baseEnergyCost, plantGrowthPerTick, turnSpeed, startingPlants, speciationFreshhold, organisms[0].AddNodeMutationRate, organisms[0].AddConnectionMutationRate - organisms[0].ConnectionWeightMutationRate, organisms[0].ConnectionWeightMutationRate - organisms[0].AddNodeMutationRate, organisms[0].RemoveConnectionMutationRate - organisms[0].AddConnectionMutationRate, prey.visionAngle * Mathf.Rad2Deg, prey.InputAngles, prey.VisionRange, prey.InputNodes, predator.visionAngle * Mathf.Rad2Deg, predator.InputAngles, predator.VisionRange, predator.InputNodes };
			swTotal.WriteLine(string.Join(';', variables));
			swTotal.WriteLine(collumnsTotal);
			Destroy(prey.render);
			Destroy(predator.render);
		}
		//File.CreateText(Controler.pathDebug).Close();
		Outdata();
	}

	int topConnectionsCount = 0;
	public double turnRate = 0;
	private double turnRateTemp = 0;

	public double inputAvredge = 0;

	// Update is called once per frame
	void Update()
	{
		//Debug.Log((inputAvredge / organisms.Count) / organisms[0].InputNodes);
		inputAvredge = 0;
		//turnRateTemp = turnRate;
		//turnRate = 0;
		//foreach (Species species in speciesListPrey.Values)
		//{
		//	if (species.reference == null)
		//		species.newRefrence(ref speciesListPrey, this);
		//}

		//foreach (Species species in speciesListPredator.Values)
		//{
		//	if (species.reference == null)
		//		species.newRefrence(ref speciesListPredator, this);
		//}
		float rotation = 0;
		int count = 0;

		GetComponent<Camera>().enabled = render;

		/*if (ticks <= endTicks && !simulationDead)
			return;*/

		/*foreach (Species species in speciesListPrey)
		{
			if (species.genomes.Count == 0)
				Debug.Log("zero");
			if (species.genomes.Count > 1)
				Debug.Log("one" + species.genomes.Count);
		}*/

		Outdata();

		for (int i = 0; i < organisms.Count; i++)
		{
			Organism organism = organisms[i];

			if (organism.isDead)
			{
				if (renderGenome && selected == organism)
				{
					selected = organisms[UnityEngine.Random.Range(0, organisms.Count)];
					selected.genome.RenderGenome(canvas, ref lines, ref panels);
				}

				if (organism is Prey)
				{
					speciesListPrey[organism.genome.species].Genomes.Remove(organism.genome);

					if (speciesListPrey[organism.genome.species].Genomes.Count == 0)
					{
						speciesListPrey.Remove(organism.genome.species);

					}
					else if (speciesListPrey[organism.genome.species].reference == organism.genome)
					{
						speciesListPrey[organism.genome.species].newRefrence(ref speciesListPrey, this);
					}

				}
				else
				{
					speciesListPredator[organism.genome.species].Genomes.Remove(organism.genome);

					if (speciesListPredator[organism.genome.species].Genomes.Count == 0)
					{
						speciesListPredator.Remove(organism.genome.species);
					}
					else if (speciesListPredator[organism.genome.species].reference == organism.genome)
						speciesListPredator[organism.genome.species].newRefrence(ref speciesListPredator, this);
				}

				Destroy(organism.render);

				organisms.RemoveAt(i);

				positions.Remove(organism.id);
			}
			else
			{
				var position = organism.position;
				organism.Update();
			}
		}
		//int connections = 0;

		/*foreach (Organism organism in organisms)
		{
			if (organism.genome.connectionGenes.Count != 0)
			{
				foreach(Genome.connectionGene connection in organism.genome.connectionGenes)
				{
					Debug.Log(organism.InputNodes + " " + organism.OutputsNodes + " " + organism.genome.nodeGenes.Keys.Max() + " " + connection.outNode);
				}
			}

			connections += organism.genome.connectionGenes.Count;

			if (!positions.ContainsKey(organism.id))
			{
				positions.Add(organism.id, organism.position);
				continue;
			}

			if ((organism.position - positions[organism.id]).magnitude > speed)
			{
				Debug.Log((organism.position - positions[organism.id]).magnitude);
			}

			positions[organism.id] = organism.position;

		}*/

		//Debug.Log(connections);
		//Debug.Log((rotation / count));

		//Growing Plants
		int grownPlants = 0;

		if (plantGrowthPerTick < 1)
		{
			if (UnityEngine.Random.Range(0f, 1f) < plantGrowthPerTick)
				grownPlants = 1;
		}
		else
			grownPlants = (int)plantGrowthPerTick;


		for (int j = 0; j < grownPlants && j < plants.Length; j++)
		{
			Plant plant = plants[UnityEngine.Random.Range(0, size * size)];

			if (plant.growth)
				grownPlants++;
			else
				plant.Grow();
		}

		if (renderGenome &&(-size/2 > selected.position.x || selected.position.x > size / 2 || selected.position.y < -size / 2 || selected.position.y > size / 2))
		{
			selected = organisms[UnityEngine.Random.Range(0, organisms.Count)];
			selected.genome.RenderGenome(canvas, ref lines, ref panels);
		}

		ticks++;
		
		if (renderGenome && ticks % 500 == 0)
		{
			selected = organisms[UnityEngine.Random.Range(0, organisms.Count)];
			selected.genome.RenderGenome(canvas, ref lines, ref panels);
		}

		Thread.Sleep(tickTime);
		//selected.genome.RenderGenome(canvas, ref lines, ref panels);
		//Debug.Log(turnRate / organisms.Count);

		//if (turnRate / organisms.Count - turnRateTemp / organisms.Count > 0.01)
			//Debug.Log(turnRateTemp / organisms.Count);

		foreach (Species species in speciesListPrey.Values)
		{
			if (species.reference == null)
			{
				species.reference = species.genomes[0];
			}
		}

		foreach (Species species in speciesListPredator.Values)
		{
			if (species.reference == null)
			{
				species.reference = species.genomes[0];

			}
		}
	}

	public bool renderGenome = false;

	Dictionary<int, Vector2> positions = new();

	private List<GameObject> lines = new();
	private List<GameObject> panels = new();

	public static Vector2 OnUnitCircle()
	{
		float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
		return new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)).normalized;
	}

	private Dictionary<int, Species> speciesListPrey = new();
	private Dictionary<int, Species> speciesListPredator = new();
	public double speciationConstant;
	public double speciationFreshhold;

	public int CalculateSpecies(Genome genome)
	{
		foreach (Species species in speciesListPrey.Values)
		{
			if (species.reference == null)
			{
				species.reference = species.genomes[0];
			}
		}

		foreach (Species species in speciesListPredator.Values)
		{
			if (species.reference == null)
			{
				species.reference = species.genomes[0];

			}
		}
		ref Dictionary<int, Species> speciesList = ref speciesListPrey;

		if (genome.organism is Predator)
			speciesList = ref speciesListPredator;

		(double, Species) lowestDis = (double.MaxValue, null);

		foreach (Species species in speciesList.Values)
		{
			double tempDis = Genome.Distance(genome, species.reference);
			if (lowestDis.Item1 > tempDis && tempDis < speciationFreshhold)
				lowestDis = (tempDis, species);
		}
		
		if (lowestDis.Item2 != null)
		{
			speciesList[lowestDis.Item2.id].Genomes.Add(genome);
			return lowestDis.Item2.id;
		}

		double dis;

		var l = speciesList.Keys.ToList();
		l.Sort();
		int id = speciesList.Count == 0 ? 0 : l.Last() + 1;
		
		speciesList.Add(id, new Species(new List<Genome>(), genome, id));
		speciesList.Last().Value.Genomes.Add(genome);

		if (speciesListPrey.Count > preyCount && preyCount > 0)
		{
			Debug.Log(3);
		}

		foreach (Species species in speciesList.Values)
		{

			for (int i = 0; i < species.Genomes.Count; i++)
			{
				Genome genome2 = species.Genomes[i];
				dis = Genome.Distance(genome2, genome);
				if (dis < speciationFreshhold)
				{
					double disToCurrent = Genome.Distance(genome2, species.reference);

					if (dis < disToCurrent)
					{
						speciesList[species.id].Genomes.Remove(genome2);

						/*if (speciesList[species.id].Genomes.Count == 0)
						{
							speciesList.Remove(species.id);
						}
						else if (speciesList[species.id].reference == genome2)
						{
							speciesList[species.id].newRefrence(ref speciesList, this);
						}*/

						speciesList.Last().Value.Genomes.Add(genome2);
						genome2.species = speciesList.Last().Key;
					}
				}
			}
		}

		return speciesList.Last().Key;
	}

	public bool Reproduce(Organism parent)
	{
		if (organisms.Count > maxOrganisms)
			return false;




		Dictionary<int, Species> speciesList = speciesListPrey;
		int count = preyCount;

		if (parent is Predator)
		{
			speciesList = speciesListPredator;
			count = predatorsCount;
		}
		/*Debug.Log(speciesListPrey.Count);
		Debug.Log(speciesList[parent.genome.species].genomes.Count);
		Debug.Log(count);
		Debug.Log(Math.Pow(Math.E, (((double)speciesList[parent.genome.species].genomes.Count / (double)count) / (1d / (double)speciesList.Count)) - 1d) * speciationConstant);*/
		/*if (!(UnityEngine.Random.Range(0f, (float)(Math.Pow(Math.E, (((double)speciesList[parent.genome.species].Genomes.Count / (double)count) / (1d / (double)speciesList.Count)) - 1d) * speciationConstant)) <= 1))
		{
			Debug.Log(speciesListPrey.Count);
			Debug.Log(speciesList[parent.genome.species].genomes.Count);
			Debug.Log(count);
			Debug.Log(Math.Pow(Math.E, (((double)speciesList[parent.genome.species].genomes.Count / (double)count) / (1d / (double)speciesList.Count)) - 1d));
			Debug.Log(2);
			return false; 
		}*/

		Organism organism;

		if(randomReplicationLocation)
		{
			if (parent is Predator)
			{
				organism = new Predator(this, Instantiate(predatorPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f), parent);
			}
			else
			{
				organism = new Prey(this, Instantiate(preyPrefab), new Vector2(UnityEngine.Random.Range(size / 2f, -size / 2f), UnityEngine.Random.Range(size / 2f, -size / 2f)), Mathf.PI * UnityEngine.Random.Range(-1f, 1f), parent);
			}
		}
		else
		{
			if (parent is Predator)
			{
				organism = new Predator(this, Instantiate(predatorPrefab), parent.position + OnUnitCircle(), Mathf.PI * UnityEngine.Random.Range(-1f, 1f), parent);
			}
			else
			{
				organism = new Prey(this, Instantiate(preyPrefab), parent.position + OnUnitCircle(), Mathf.PI * UnityEngine.Random.Range(-1f, 1f), parent);
			}
		}

		if (organism.genome.connectionGenes.Count < organism.genome.generation)
		{
			//Debug.Log(organism.genome.connectionGenes.Count);
		}
		parent.children++;
		organisms.Add(organism);



		return true;
	}

	private List<Genome.connectionGene> innovations = new();

	public int InnovationNumber(Genome.connectionGene connection)
	{
		for (int i = 0; i < innovations.Count; i++)
		{
			if (connection.isEqual(innovations[i]))
				return i;
		}

		innovations.Add(connection);
		return innovations.Count;
	}

	public static string collumnsData = "id;type;x;y;isDead;rotation;geneSpeed";
	public static string collumnsTotal = "tick;PreyPopulation;PredatorPopulation;PlantPopulation;PreyDeathCausePredator;PreyDeathCauseStarvation" +
										";PreyDeathCauseAge;PreyDeathCauseEdge;PredatorDeathCauseStarvation;PredatorDeathCauseAge;PredatorDeathCauseEdge;" +
										"PlantDeathCausePrey;PreyAvredgeGenomeSize;PredatorAvredgeGenomeSize;PreyTimeSinceLastFood;PredatorTimeSinceLastFood;" + 
										"PreyAvredgeLifeSpan;PredatorAvredgeLifeSpan;PreyChildren;PredatorChildren;PreyDeath;PredatorDeath";

	public static string[] collumnNamesData = collumnsData.Split(";");
	public static string[] collumnNamesTotal = collumnsTotal.Split(";");

	private int id = 0;
	public int GetId()
	{
		id++;
		return id;
	}

	/*public enum TotalCategories { tick, Preys, Predators, Plants, PreyDeathCausePredator, PreyDeathCauseStarvation, 
								PreyDeathCauseAge, PredatorDeathCauseStarvation, PredatorDeathCauseAge, 
								PlantDeathCauseAge, PlantDeathCausePrey, PredatorGeneSpeedAverage, PreyGeneSpeedAverage
	};*/

	public enum DataCategories{ id, type, x, y, isDead, rotationX, rotationY, geneSpeed };

	public int plantPopulation;

	private void Outdata()
	{
		Dictionary<string, float> totalDictionary = Enumerable.Range(0, collumnNamesTotal.Length).ToDictionary(i => collumnNamesTotal[i], i => 0f);

		//using StreamWriter swData = File.AppendText(pathData);
		
		//swData.WriteLine("*" + ticks + "*");
		//swData.WriteLine(collumnsData);

		foreach (Organism organism in organisms)
		{
			/*Dictionary<string, string> lineDictionary = Enumerable.Range(0, collumnNamesData.Length).ToDictionary(i => collumnNamesData[i], i => "");

			lineDictionary["id"] = organism.id.ToString();
			lineDictionary["type"] = organism.GetType().ToString();
			lineDictionary["x"] = organism.position.x.ToString();
			lineDictionary["y"] = organism.position.y.ToString();
			lineDictionary["isDead"] = organism.isDead.ToString();

			lineDictionary["rotation"] = organism.rotation.ToString();
			lineDictionary["geneSpeed"] = organism.geneSpeed.ToString();*/

			//swData.WriteLine(string.Join(';', lineDictionary.Values));

			if (organism.isDead)
			{
				totalDictionary[organism.GetType() + "DeathCause" + organism.deathCause]++;
				totalDictionary[organism.GetType() + "Children"] += organism.children;
				totalDictionary[organism.GetType() + "AvredgeLifeSpan"] += ticks - organism.startingTick;
				totalDictionary[organism.GetType() + "Death"]++;
			}
			else
			{
				if(organism.ticksSinceLastFood != -1)
					totalDictionary[organism.GetType() + "TimeSinceLastFood"] += ticks - organism.ticksSinceLastFood;

				totalDictionary[organism.GetType() + "AvredgeGenomeSize"] += organism.genome.size;
				totalDictionary[organism.GetType() + "Population"]++;
			}
		}
		totalDictionary["PlantPopulation"] = plantPopulation;
		totalDictionary["tick"] = ticks;

		CallculateAvredge("Prey", ref totalDictionary);
		CallculateAvredge("Predator", ref totalDictionary);


		using StreamWriter swTotal = File.AppendText(pathTotal);
		{
			swTotal.WriteLine(string.Join(';', totalDictionary.Values));
		}

		/*if (tickDictionary["Prey"] == 0 || tickDictionary["Predator"] == 0)
			simulationDead = true;*/

		predatorsCount = (int)totalDictionary["PredatorPopulation"];
		preyCount = (int)totalDictionary["PreyPopulation"];
	}

	private void CallculateAvredge(string type, ref Dictionary<string, float> totalDictionary)
	{
		totalDictionary[type + "AvredgeGenomeSize"] /= totalDictionary[type + "Population"];
		totalDictionary[type + "TimeSinceLastFood"] /= totalDictionary[type + "Population"];
		totalDictionary[type + "AvredgeLifeSpan"] /= totalDictionary[type + "Death"];

	}

	//private bool simulationDead = false;

	private int predatorsCount = 0;
	private int preyCount = 0;

	public static Vector2 RadTo2D(float angle)
	{
		return new Vector2(Mathf.Cos(Mathf.PI / 2 - angle), Mathf.Sin(Mathf.PI / 2 - angle));
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(selected.position, 1.5f);

		Gizmos.color = Color.white;
		float visionAngleSegment = selected.visionAngle / (selected.InputAngles - 1);
		
		for (int i = 0; i < selected.InputAngles; i++)
		{
			float currentAngle = selected.rotation - (selected.visionAngle / 2) + (visionAngleSegment * i);
			Gizmos.DrawLine(selected.position, selected.position + RadTo2D(currentAngle) * selected.VisionRange);
		}
	}
}
