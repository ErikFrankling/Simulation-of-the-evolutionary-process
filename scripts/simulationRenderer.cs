using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationRenderer : MonoBehaviour
{
	private string[] data;

	private Dictionary<int, GameObject> gameObjects = new();

	public GameObject predatorPrefab;
	public GameObject preyPrefab;
	public GameObject plantPrefab;

	// Start is called before the first frame update
	void Start()
	{
		data = File.ReadAllText(Controler.pathData).Split('*');
	}
	int i = 0;
	// Update is called once per frame
	void Update()
	{
		string frame = data[i];
		i++;
		if (frame.Contains(Controler.collumnsData))
		{
			foreach (string line in frame.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Skip(2))
			{
				if (line == Controler.collumnsData)
					continue;

				Dictionary<Controler.DataCategories, string> dictionary;

				try
				{
					dictionary = Enumerable.Range(0, Enum.GetNames(typeof(Controler.DataCategories)).Length).ToDictionary(i => (Controler.DataCategories)i, i => line.Split(';')[i]);
				}
				catch (IndexOutOfRangeException)
				{
					continue;
				}

				int id = int.Parse(dictionary[Controler.DataCategories.id]);

				if (gameObjects.ContainsKey(id))
				{
					if (bool.Parse(dictionary[Controler.DataCategories.isDead]))
					{
						Destroy(gameObjects[id]);
						gameObjects.Remove(id);
					}
					else if (!(dictionary[Controler.DataCategories.type] == "Plant"))
					{
						gameObjects[id].transform.position = new Vector2(float.Parse(dictionary[Controler.DataCategories.x]), float.Parse(dictionary[Controler.DataCategories.y]));
						//gameObjects[id].transform.rotation = Quaternion;
					}
				}
				else
				{
					if (dictionary[Controler.DataCategories.type] == "Predator")
						gameObjects.Add(id, Instantiate(predatorPrefab, new Vector2(float.Parse(dictionary[Controler.DataCategories.x]), float.Parse(dictionary[Controler.DataCategories.y])), Quaternion.Euler(Vector3.zero)));
					if (dictionary[Controler.DataCategories.type] == "Prey")
						gameObjects.Add(id, Instantiate(preyPrefab, new Vector2(float.Parse(dictionary[Controler.DataCategories.x]), float.Parse(dictionary[Controler.DataCategories.y])), Quaternion.Euler(Vector3.zero)));
					if (dictionary[Controler.DataCategories.type] == "Plant")
						gameObjects.Add(id, Instantiate(plantPrefab, new Vector2(float.Parse(dictionary[Controler.DataCategories.x]), float.Parse(dictionary[Controler.DataCategories.y])), Quaternion.Euler(Vector3.zero)));
				}
			}
		}
		else
		{

		}

	}
}
