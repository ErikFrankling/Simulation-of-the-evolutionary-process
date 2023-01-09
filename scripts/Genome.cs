using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class Genome
{
	public class connectionGene
	{
		public connectionGene(int inNode, int outNode, float weight)
		{
			this.inNode = inNode;
			this.outNode = outNode;
			this.weight = weight;
		}

		public int inNode;
		public int outNode;
		public float weight;

		public bool isEqual(connectionGene connection)
		{
			if (connection.inNode == inNode && connection.outNode == outNode)
				return true;
			else
				return false;
		}

		public (bool, connectionGene) Exists(List<connectionGene> connections)
		{
			foreach (connectionGene connectionGene in connections)
			{
				if (isEqual(connectionGene))
					return (true, connectionGene);
			}

			return (false, null);
		}
	}

	public class nodeGene
	{
		public nodeGene(int id, nodeTypes type, List<connectionGene> connectionGenes)
		{
			this.id = id;
			this.type = type;
			depth = 0;
			this.connectionGenes = connectionGenes;
		}

		public List<connectionGene> connectionGenes;

		public int id;
		public nodeTypes type;
		public int depth;
	}

	public enum nodeTypes { input, output, hidden, bias }

	public Dictionary<int, nodeGene> nodeGenes;
	public List<connectionGene> connectionGenes = new();

	public float size { get { return (nodeGenes.Count + connectionGenes.Count * 0.5f) / (organism.InputNodes + organism.OutputsNodes + organism.BiasNodes); } }

	public Organism organism;

	public int species = -1;

	public int generation;
	private int mutations;

	public double biasAvredgeGenome;

	//new Genome
	public Genome(Organism organism)
	{
		generation = 0;
		mutations = 0;

		this.organism = organism;

		nodeGenes = new();

		List<connectionGene> newConnectionGenes = new();

		for (int i = 0; i < organism.InputNodes; i++)
		{
			newConnectionGenes = new();

			/*for (int j = 0; j < organism.OutputsNodes; j++)
			{
				connectionGene connection = new connectionGene(i, organism.InputNodes + j, UnityEngine.Random.Range(-1f, 1f));
				newConnectionGenes.Add(connection);
				connectionGenes.Add(connection);
			}*/

			nodeGenes.Add(i, new nodeGene(i, nodeTypes.input, newConnectionGenes));
		}

		newConnectionGenes = new();

		for (int j = 0; j < organism.OutputsNodes; j++)
		{
			connectionGene connection = new connectionGene(organism.InputNodes, organism.InputNodes + organism.BiasNodes + j, 0);
			newConnectionGenes.Add(connection);
			connectionGenes.Add(connection);
		}
		
		nodeGenes.Add(organism.InputNodes, new nodeGene(organism.InputNodes, nodeTypes.bias, newConnectionGenes));

		for (int i = 0; i < organism.OutputsNodes; i++)
		{
			nodeGenes.Add(organism.InputNodes + organism.BiasNodes + i, new nodeGene(organism.InputNodes + organism.BiasNodes + i, nodeTypes.output, null));
		}
		int connectionsCount = connectionGenes.Count;


		/*bool depthSearch = false;
		int random = UnityEngine.Random.Range(0, 5);

		switch (random)
		{
			case 0:
				depthSearch = AddNodeMutation();
				break;
			case 1:
				depthSearch = EnableMutation();
				break;
			case 2:
				depthSearch = AddConnectionMutation();
				break;
			case 3:
				depthSearch = NodeBiasMutation();
				break;
			case 4:
				depthSearch = ConnectionWeightMutation();
				break;
		}
		
		if (!depthSearch)
			NodeDepthSearch();*/

		float random = UnityEngine.Random.Range(0f, organism.RemoveConnectionMutationRate);

		bool mutation = false;
		mutationH = new();
		if (random <= organism.AddNodeMutationRate)
		{
			mutations++;
			mutationH.Add(1);
			mutation = AddNodeMutation();
		}
		else if (random <= organism.ConnectionWeightMutationRate)
		{
			mutations++;
			mutationH.Add(2);
			mutation = ConnectionWeightMutation();
		}
		else if (random <= organism.AddConnectionMutationRate)
		{
			mutations++;
			mutationH.Add(3);
			mutation = AddConnectionMutation();
		}
		else if (random <= organism.RemoveConnectionMutationRate)
		{
			mutations++;
			mutationH.Add(4);
			mutation = RemoveConnectionMutation();
		}
		
		if (!mutation)
			NodeDepthSearch();

		species = organism.controler.CalculateSpecies(this);
	}

	List<int> mutationH;
	//new Genome from asexual reproduction
	public Genome(Organism organism, Genome parent)
	{
		int connectionsCount = parent.connectionGenes.Count;
		int[] mutationHTemp = parent.mutationH.ToArray();
		mutationH = mutationHTemp.ToList();
		generation = parent.generation + 1;
		mutations = parent.mutations;

		this.organism = organism;
		nodeGenes = new();
		connectionGenes = new();
		for (int s = 0; s < parent.nodeGenes.Count; s++)
		{
			nodeGene node = parent.nodeGenes.Values.ToArray()[s];
			List<connectionGene> newConnectionGenes;
			if (node.connectionGenes != null)
			{
				newConnectionGenes = new();
				foreach (connectionGene connection in node.connectionGenes)
				{
					newConnectionGenes.Add(new connectionGene(connection.inNode, connection.outNode, connection.weight));
				}
			}
			else
				newConnectionGenes = null;
			nodeGenes.Add(parent.nodeGenes.Keys.ToArray()[s], new nodeGene(node.id, node.type, newConnectionGenes));
		}
		foreach (connectionGene connection in parent.connectionGenes)
		{
			connectionGenes.Add(new connectionGene(connection.inNode, connection.outNode, connection.weight));
		}

		Mutate();
		NodeDepthSearch();

		int maxDepth = 0;

		foreach (nodeGene node in nodeGenes.Values)
		{
			if (maxDepth < node.depth)
			{
				maxDepth = node.depth;
			}
		}

		species = organism.controler.CalculateSpecies(this);
		int i = 0;
		foreach (List<nodeGene> layer in layerList)
		{
			foreach (nodeGene node in layer)
			{
				if (node.depth != i)
				{
					NodeDepthSearch();
				}
			}
			i++;
		}

		for (int j = 0; j < nodeGenes.Count; j++)
		{
			nodeGene node = nodeGenes.Values.ToArray()[j];
			int key = nodeGenes.Keys.ToArray()[j];

			if (node.id != key)
			{
				nodeGenes.Remove(key);
				nodeGenes.Add(node.id, node);
			}
		}

		//for (int k = 0; k < connectionGenes.Count; k++)
		//{
		//	if (!nodeGenes[connectionGenes[k].inNode].connectionGenes.Contains(connectionGenes[k]))
		//	{
		//		nodeGenes[connectionGenes[k].inNode].connectionGenes.Add(connectionGenes[k]);
		//	}
		//}

		int total = 0;
		foreach (var layer in layerList)
		{
			total += layer.Count;
			if (layer.Count == 0)
			{
				//NodeDepthSearch();
			}
		}
		if (total != nodeGenes.Count)
		{
			NodeDepthSearch();
		}
		NodeDepthSearch();

		foreach (nodeGene node in nodeGenes.Values)
		{
			if (node.connectionGenes == null)
				continue;
			foreach (connectionGene connectionGene in node.connectionGenes)
			{
				if (!nodeGenes.Keys.Contains(connectionGene.outNode))
				{
					Debug.Log(1);
				}
			}
		}
		if (organism.InputNodes + organism.OutputsNodes + organism.BiasNodes < nodeGenes.Count)
		{
			Organism selected = organism.controler.selected;
			if (!(selected.genome.nodeGenes.Count > selected.InputNodes + selected.OutputsNodes + selected.BiasNodes))
			{
				organism.controler.selected = organism;
			}
			//Debug.Log(nodeGenes.Count);
		}
	}


	//new Genome from sexual reproduction
	/*public Genome(Organism organism, Genome parent1, Genome parent2)
	{
		this.organism = organism;

		nodeGenes = parent1.nodeGenes; //Enumerable.Range(0, organism.InputNodes + organism.OutputsNodes).ToDictionary(i => parent1.nodeGenes[i].id, i => parent1.nodeGenes[i]);
		connectionGenes = parent1.connectionGenes; //parent1.connectionGenes.GetRange(0, organism.InputNodes + organism.OutputsNodes);

		for (int i = organism.InputNodes + organism.OutputsNodes; i < parent2.nodeGenes.Count; i++)
		{
			int key2 = parent2.nodeGenes.Keys.ToArray()[i];

			if (!parent1.nodeGenes.Keys.Contains(key2))
			{
				nodeGene node2 = parent2.nodeGenes[key2];
				nodeGenes.Add(node2.id, node2);
			}
		}

		foreach (connectionGene connection in parent2.connectionGenes)
		{
			var exsists = connection.Exsists(connectionGenes);

			if (!exsists.Item1)
			{
				connectionGenes.Add(connection);
				nodeGenes[connection.inNode].connectionGenes.Add(connection);
			}
			else if (!connection.enabled ^ !exsists.Item2.enabled)
			{
				if (connection.enabled)
					exsists.Item2 = connection;
				else
					exsists.Item2.enabled = true;
			}
		}

		species = organism.controler.CalculateSpecies(this);
		NodeDepthSearch();
	}*/

	public static double Distance(Genome genome1, Genome genome2)
	{
		if (genome1.connectionGenes.Count == 0 && genome2.connectionGenes.Count == 0)
			return 0;

		int disjoint = 0;
		double weightDifference = 0;

		int matchingConnections = 0;

		foreach (connectionGene connection in genome1.connectionGenes)
		{
			nodeGene temp;

			if (!genome2.nodeGenes.TryGetValue(connection.inNode, out temp))
			{
				disjoint++;
				continue;
			}

			var exsists = connection.Exists(temp.connectionGenes);

			if (exsists.Item1)
			{
				weightDifference += Math.Abs(connection.weight - exsists.Item2.weight);

				matchingConnections++;
			}
			else
			{
				disjoint++;
			}
		}

		disjoint += genome2.connectionGenes.Count - matchingConnections;

		//Debug.Log(disjoint / (matchingConnections + disjoint) * 10/* * constant */ + (weightDifference / matchingConnections) /* * constant */ + (biasDifference / matchingNodes) * 10/* * constant */);
		return disjoint / ((matchingConnections + disjoint) == 0 ? 1 : (matchingConnections + disjoint) * 10) /* * constant */ + (matchingConnections == 0 ? (weightDifference / matchingConnections) : 0) /* * constant */;
	}

	float randomtemp;

	private bool Mutate()
	{
		float random = UnityEngine.Random.Range(0f, 100f);
		randomtemp = random;

		if (random <= organism.AddNodeMutationRate)
		{
			mutationH.Add(1);
			mutations++;
			return AddNodeMutation();
		}
		else if (random <= organism.ConnectionWeightMutationRate)
		{
			mutationH.Add(2);
			mutations++;
			return ConnectionWeightMutation();
		}
		else if (random <= organism.AddConnectionMutationRate)
		{
			mutationH.Add(3);
			mutations++;
			return AddConnectionMutation();
		}
		else if (random <= organism.RemoveConnectionMutationRate)
		{
			mutationH.Add(4);
			mutations++;
			return RemoveConnectionMutation();
		}
		else
		{
			return false;
		}
	}

	private bool AddNodeMutation()
	{
		if (connectionGenes.Count == 0)
			return false;

		connectionGene connection = connectionGenes[UnityEngine.Random.Range(0, connectionGenes.Count)];

		int innovationNumber = organism.controler.InnovationNumber(connection) + organism.InputNodes + organism.OutputsNodes + organism.BiasNodes - 1;

		if (nodeGenes.Keys.Contains(innovationNumber))
			return false;

		nodeGene node = new nodeGene(innovationNumber, nodeTypes.hidden, new List<connectionGene>());

		connectionGene outConnection = new connectionGene(node.id, connection.outNode, 1);
		node.connectionGenes.Add(outConnection);
		connectionGenes.Add(outConnection);

		connectionGene inConnection = new connectionGene(connection.inNode, node.id, connection.weight);
		nodeGenes[connection.inNode].connectionGenes.Add(inConnection);
		connectionGenes.Add(inConnection);

		nodeGenes.Add(node.id, node);

		nodeGenes[connection.inNode].connectionGenes.Remove(connection);
		connectionGenes.Remove(connection);

		NodeDepthSearch();
		return true;
	}

	private bool ConnectionWeightMutation()
	{
		if (connectionGenes.Count == 0)
			return false;
		connectionGene connection = connectionGenes[UnityEngine.Random.Range(0, connectionGenes.Count)];

		connection.weight = Math.Clamp(organism.ConnectionWeightMutationChange * UnityEngine.Random.Range(-1f, 1f) + connection.weight, -1f, 1f);

		return false;

	}

	private bool AddConnectionMutation()
	{
		int connectionsCount = connectionGenes.Count;

		int inNode = nodeGenes.Keys.ToArray()[UnityEngine.Random.Range(0, nodeGenes.Count)];
		int outNode = nodeGenes.Keys.ToArray()[UnityEngine.Random.Range(organism.InputNodes + organism.BiasNodes, nodeGenes.Count)];

		if (!nodeGenes.Keys.Contains(outNode) || !nodeGenes.Keys.Contains(inNode) || inNode == outNode || nodeGenes[inNode].type == nodeTypes.output || nodeGenes[outNode].type == nodeTypes.input || nodeGenes[outNode].type == nodeTypes.bias)
			return false;

		connectionGene connection = new connectionGene(inNode, outNode, UnityEngine.Random.Range(-1f, 1f) * organism.ConnectionWeightMutationChange);

		if (connection.Exists(connectionGenes).Item1 || ConnectionLoopCheck(connection))
			return false;

		else
		{
			nodeGenes[connection.inNode].connectionGenes.Add(connection);
			connectionGenes.Add(connection);

			NodeDepthSearch();

			return true;
		}

	}

	private bool RemoveConnectionMutation()
	{
		if (connectionGenes.Count != 0)
		{
			connectionGene connection = connectionGenes[UnityEngine.Random.Range(0, connectionGenes.Count)];

			nodeGenes[connection.inNode].connectionGenes.Remove(connection);
			connectionGenes.Remove(connection);
		}
		/*else if (nodeGenes.Count != 0)
		{
			nodeGene node = nodeGenes[nodeGenes.Keys.ToList()[UnityEngine.Random.Range(0, nodeGenes.Count)]];
			if (node.type == nodeTypes.hidden)
				nodeGenes.Remove(node.id);
			else
				return false;
		}*/
		else
			return false;

		RemoveUnnesecaryStructure();
		NodeDepthSearch();
		return true;
	}

	private void RemoveUnnesecaryStructure()
	{
		List<nodeGene> visited = new();

		for (int i = 0; i < organism.InputNodes + organism.BiasNodes; i++)
		{
			DFSRemoveUnnesecaryStructure(nodeGenes[i], visited);
		}

		for (int i = 0; i < nodeGenes.Count; i++)
		{
			nodeGene node = nodeGenes.Values.ToArray()[i];
			if (!visited.Contains(node) && node.type != nodeTypes.output)
			{
				foreach (connectionGene connection in node.connectionGenes)
				{
					connectionGenes.Remove(connection);
				}
				node.connectionGenes.Clear();
				nodeGenes.Remove(node.id);
			}
		}

		foreach (connectionGene connection in connectionGenes)
		{
			if (!nodeGenes.ContainsKey(connection.outNode) || !nodeGenes.ContainsKey(connection.inNode))
			{
				connectionGenes.Remove(connection);
				nodeGenes[connection.inNode].connectionGenes.Remove(connection);	
			}
		}
	}

	private bool DFSRemoveUnnesecaryStructure(nodeGene node, List<nodeGene> visited)
	{
		if (visited.Contains(node) || node.type == nodeTypes.output)
			return false;

		visited.Add(node);

		if (node.connectionGenes.Count > 0)
		{
			for (int i = 0; i < node.connectionGenes.Count; i++)
			{
				connectionGene connection = node.connectionGenes[i];

				if (DFSRemoveUnnesecaryStructure(nodeGenes[connection.outNode], visited))
				{
					connectionGenes.Remove(connection);
					node.connectionGenes.Remove(connection);
					i--;
				}
			}
		}

		if (node.type != nodeTypes.input && node.type != nodeTypes.bias && node.connectionGenes.Count == 0)
		{
			nodeGenes.Remove(node.id);
			return true;
		}
		else
		{
			return false;
		}
	}

	private bool ConnectionLoopCheck(connectionGene startingConnection)
	{
		List<nodeGene> visited = new();

		visited.Add(nodeGenes[startingConnection.inNode]);

		return DFSCheckrecursive(nodeGenes[startingConnection.outNode], visited);
	}

	private bool DFSCheckrecursive(nodeGene node, List<nodeGene> visited)
	{
		for (int j = 0; j < nodeGenes.Count; j++)
		{
			nodeGene nodeTest = nodeGenes.Values.ToArray()[j];
			int key = nodeGenes.Keys.ToArray()[j];

			if (nodeTest.id != key)
			{
				nodeGenes.Remove(key);
				nodeGenes.Add(nodeTest.id, nodeTest);
			}
		}

		if (node.type == nodeTypes.output)
			return false;

		if (visited.Contains(node))
			return true;

		foreach (connectionGene connection in node.connectionGenes)
		{
			return DFSCheckrecursive(nodeGenes[connection.outNode], visited);
		}
		
		return false;
	}

	private List<List<nodeGene>> layerList;

	private void NodeDepthSearch()
	{

		layerList = new();

		List<nodeGene> visited = new();

		for (int i = 0; i < organism.InputNodes + organism.BiasNodes; i++)
		{
			DFSRecursive(nodeGenes[i], 0, visited);
		}

		int maxDepth = 0;

		foreach(nodeGene node in nodeGenes.Values)
		{
			if (maxDepth < node.depth)
			{
				maxDepth = node.depth;
			}
			//Debug.Log(node.depth + " max depth " + maxDepth + " type " + node.type + " id " + organism.id);

		}

		for (int i = 0; i < (maxDepth + 1); i++)
		{
			layerList.Add(new List<nodeGene>());

			//Debug.Log((maxDepth + 1) + " " + i + " layers " + layerList.Count);
		}

		foreach (nodeGene node in nodeGenes.Values)
		{
			layerList[node.depth].Add(node);
		}
		//Debug.Log(nodeGenes.Values.ToArray()[organism.InputNodes + organism.OutputsNodes - 1].depth + " id " + organism.id);
	}

	private void DFSRecursive(nodeGene node, int depth, List<nodeGene> visited)
	{
		if (visited.Contains(node) && node.depth < depth)
			return;
		
		node.depth = depth;

		depth++;

		visited.Add(node);

		if (node.type == nodeTypes.output)
			return;

		for (int i = 0; i < node.connectionGenes.Count; i++)
		{
			connectionGene connection = node.connectionGenes[i];

			//if (connection.inNode != node.id)
			//{
			//	node.connectionGenes.Remove(connection);
			//	continue;
			//}
			//if (!connectionGenes.Contains(connection))
			//	connectionGenes.Add(connection);
			DFSRecursive(nodeGenes[connection.outNode], depth, visited);
		}
	}


	public float[] Calculate(float[] inputs)
	{

		//if (!done)
		//{
		//int total = 0;
		//foreach (var layer in layerList)
		//{
		//	total += layer.Count;
		//}
		//if (total != nodeGenes.Count)
		//{
		//	//NodeDepthSearch();
		//}

		//	int maxDepth = 0;

		//	foreach (nodeGene node in nodeGenes.Values)
		//	{
		//		if (maxDepth < node.depth)
		//		{
		//			maxDepth = node.depth;
		//		}
		//	}

		//	if (maxDepth >= layerList.Count)
		//	{
		//		NodeDepthSearch();
		//	}
		//}

		//foreach (nodeGene node in nodeGenes.Values)
		//{
		//	if (!nodeGenes.ContainsKey[node.id])
		//}

		//done = true;

		Dictionary<int, float> activations = new Dictionary<int, float>(nodeGenes.Count);
		activations = nodeGenes.Keys.ToDictionary(i => i, i => new float());

		for (int i = 0; i < organism.InputNodes; i++)
		{
			activations[i] = inputs[i];

			foreach (connectionGene connection in nodeGenes[i].connectionGenes)
			{
				activations[connection.outNode] += inputs[i] * connection.weight;
			}
		}

		activations[organism.InputNodes] = 1;

		foreach (connectionGene connection in nodeGenes[organism.InputNodes].connectionGenes)
		{
			activations[connection.outNode] += connection.weight;
		}

		for (int i = 1; i < layerList.Count; i++)
		{
			for (int j = 0; j < layerList[i].Count; j++)
			{
				nodeGene node = layerList[i][j];

				float acti = (float)Math.Tanh((double)activations[node.id] * 0.5d);
				if (acti > 1)
					Debug.Log(1);
				activations[node.id] = acti;

				if (node.type == nodeTypes.output)
					continue;

				foreach (connectionGene connection in node.connectionGenes)
				{
					activations[connection.outNode] += acti * connection.weight;
				}
			}
		}


		float[] output = new float[organism.OutputsNodes];

		for (int i = organism.InputNodes + organism.BiasNodes; i < organism.OutputsNodes + organism.InputNodes + organism.BiasNodes; i++)
		{
			if (activations[i] > 1)
				Debug.Log(1);

			//if (outputTemp != null && Math.Abs(activations[i] - outputTemp[i - organism.InputNodes - organism.BiasNodes]) > 0.01f && outputTemp[i - organism.InputNodes - organism.BiasNodes] != 0)
			//{
			//	Debug.Log(Math.Abs(activations[i] - outputTemp[i - organism.InputNodes - organism.BiasNodes]));
			//}
			//if (nodeGenes[i].depth == 0)
			//{
			//	double acti = Math.Tanh(nodeGenes[i].bias * 0.5d);
			//	activations[nodeGenes[i].id] = acti;
			//}
			output[i - organism.InputNodes - organism.BiasNodes] = activations[i];
		}

		return output;
	}

	public Dictionary<int, double> tempActivations;

	public void RenderGenome(Canvas canvas, ref List<GameObject> lines, ref List<GameObject> panels)
	{
		if (!organism.controler.renderGenome2)
			return;
		int realWidth = (int)canvas.GetComponent<RectTransform>().rect.width;//organism.controler.gameObject.GetComponent<Camera>().scaledPixelWidth;
		int realHeight = (int)canvas.GetComponent<RectTransform>().rect.height;//organism.controler.gameObject.GetComponent<Camera>().scaledPixelHeight;

		int width = (int)(realWidth * 0.8);
		int height = (int)(realHeight * 0.8);

		int marginHeight = (int)(realWidth * 0.1);
		int marginWidth = (int)(realHeight * 0.1);

		int maxDepth = 0;

		foreach (nodeGene node in nodeGenes.Values)
		{
			if (maxDepth < node.depth)
			{
				maxDepth = node.depth;
			}
			//Debug.Log(node.depth + " max depth " + maxDepth + " type " + node.type + " id " + organism.id);

		}
		int depthWidth = maxDepth == 0 ? 1 : width / maxDepth;

		/*int biggestLayer = 0;

		foreach (List<nodeGene> layer in layerList)
		{
			 if (layer.Count > biggestLayer)
				biggestLayer = layer.Count;
		}*/

		//int j = panels.Count;

		//int temp = Mathf.Abs(panels.Count - nodeGenes.Count);

		//for (int i = 0; i < temp; i++)
		//{
		//	if (panels.Count < nodeGenes.Count)
		//	{
		//		//Debug.Log(panels.Count + " " + nodeGenes.Count + " " + Mathf.Abs(panels.Count - nodeGenes.Count));

		//		//Debug.Log("här");
		//		panels.Add(GameObject.Instantiate(organism.controler.panelPrefab, canvas.transform));
		//	}
		//	else if (j > nodeGenes.Count)
		//	{
		//		panels[j - 1].SetActive(false);
		//		j--;
		//	}
		//}
		foreach (GameObject panel in panels)
			panel.transform.position = new Vector2(10000, 10000);
		List<GameObject> tempPanels = panels;
		Dictionary<int, GameObject> nodes = nodeGenes.Keys.ToDictionary(i => i, i => tempPanels[i]);

		int j = 0;

		foreach(nodeGene node in nodeGenes.Values)
		{
			GameObject panel = panels[j];
			panel.SetActive(true);
			j++;

			//Debug.Log(layerList.Count + " depth " + node.depth + " " + node.type.ToString() + " id " + organism.id);
			List<nodeGene> layer = layerList[node.depth];
			Vector3 position = new Vector3(node.depth * depthWidth + marginWidth, layer.IndexOf(node) * height / layer.Count + marginHeight, 1);

			RectTransform transform = panel.GetComponent<RectTransform>();
			transform.SetAsFirstSibling();
			transform.anchoredPosition = position;
			panel.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("{0}\nid: {1}\nacti: {2}\ndepth: {3}", node.type, node.id, Mathf.Round(tempActivations == null ? 0 : (float)tempActivations[node.id] * 100f) / 100f, node.depth);

		}

		//j = lines.Count;

		//int temp = Mathf.Abs(lines.Count - connectionGenes.Count);
		///*if(nodeGenes.Count > 6)
		//{
		//	Debug.Log("line " + temp + " " + nodeGenes.Count + " " + connectionGenes.Count);
		//}*/
		//for (int i = 0; i < temp; i++)
		//{
		//	//Debug.Log("line1");

		//	if (lines.Count < connectionGenes.Count)
		//	{
		//		//Debug.Log("line");
		//		lines.Add(GameObject.Instantiate(organism.controler.linePrefab));
		//	}
		//	else if (j > connectionGenes.Count)
		//	{
		//		lines[j - 1].SetActive(false);
		//		j--;
		//	}
		//}

		foreach (GameObject line in lines)
			line.SetActive(false);

		for (int l = 0; l < connectionGenes.Count; l++)
		{
			connectionGene connection = connectionGenes[l];
			GameObject line = lines[l];

			line.SetActive(true);
			
			LineRenderer render = line.GetComponent<LineRenderer>();
			//Debug.Log(connection.inNode + " connection " + connection.outNode);
			Vector3[] positions = { nodes[connection.inNode].GetComponent<RectTransform>().position, nodes[connection.outNode].GetComponent<RectTransform>().position };
			render.SetPositions(positions);
			
			Gradient gradient = new Gradient();
			Color color;

			if (connection.weight < 0)
				color = Color.Lerp(Color.blue, Color.gray, (float)connection.weight + 1f);
			else
				color = Color.Lerp(Color.gray, Color.red, (float)connection.weight);
			
			gradient.SetKeys(
				new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
				new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
			);

			render.colorGradient = gradient;

			line.GetComponent<RectTransform>().SetAsLastSibling();

			RectTransform[] comps = line.GetComponentsInChildren<RectTransform>();
			foreach (RectTransform comp in comps)
			{
				if (comp.gameObject.GetInstanceID() != line.GetInstanceID())
				{
					comp.localPosition = new Vector2(positions[0].x + (positions[1].x - positions[0].x), positions[0].y + (positions[1].y - positions[0].y));
				}
			}

			line.GetComponentInChildren<TextMeshProUGUI>().text = "weight: " + Mathf.Round((float)connection.weight * 100f) / 100f;
		}
	}
}
