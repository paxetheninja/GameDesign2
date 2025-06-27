using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace Generation
{
    public class LevelGenerator : NetworkBehaviour
    {
        [SerializeField] private GameObject loadingBar;
        [SerializeField] private CustomerManager customerManager;
        private GeneratingLevelLoadingBarScript _loadingBarScript;
        
        public Genome Result;

        private const int PopulationSize = 200;
        private const int MaxGenerations = 100;
        private const float EliteRatio = 0.1f;
        private const float TopPopulation = 0.3f;
        
        public int playerCount;

        public static Random RandomGenerator = new ();
        public static LevelGenerator Instance;
        
        private int _currentGeneration;
        private readonly NetworkVariable<int> _netCurrentGeneration = new();
        private readonly NetworkVariable<bool> _netGenerationFinished = new();

        public int numRooms;
        public const int NumWorkstations = 10;
        public int difficulty;
        private List<Genome> _population = new ();
        
        public GameObject floorRoomPrefab;
        public GameObject floorServiceRoomPrefab;
        public GameObject wallTile;
        public GameObject topLevel;

        public GameObject grid;
        public GameObject workbenchWell;
        public GameObject workbenchClayPit;
        public GameObject workbenchTable;
        public GameObject workbenchTrashcan;
        public GameObject workbenchPottersWheel;
        public GameObject workbenchOven;
        public GameObject workbenchBlackRocks;
        public GameObject workbenchRedRocks;
        public GameObject workbenchGrindingTable;
        public GameObject workbenchPaintersTable;
    
        public GameObject door;

        public List<Vector3Int> playersSpawns = new();

        public UIManager uiManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
            } 
            else 
            { 
                Instance = this;
            }
            uiManager = FindObjectOfType<UIManager>();

        }

        private void Start()
        {
            _loadingBarScript = loadingBar.GetComponent<GeneratingLevelLoadingBarScript>();

            if (IsServer)
            {
                GenerateServerRpc(NetworkManager.ConnectedClients.Count, PersistentInfoHolder.Instance.Difficulty, PersistentInfoHolder.Instance.Seed);
            }
        }

        void Update()
        {
            if (IsServer)
            {
                _netCurrentGeneration.Value = _currentGeneration;
            }
            else
            {
                _currentGeneration = _netCurrentGeneration.Value;
                if (_netGenerationFinished.Value)
                {
                    loadingBar.SetActive(false);
                }
                else
                {
                    _loadingBarScript.SetScale((float) _currentGeneration / MaxGenerations);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = true)]
        private void GenerateServerRpc(int _playerCount, int _difficulty, int seed)
        {
            playersSpawns.Clear();
            Result = null;
            playerCount = _playerCount;
            difficulty = _difficulty;
            _currentGeneration = 1;

            if (seed == 0)
            {
                Random rnd = new Random();
                seed = rnd.Next();
            }

            Debug.Log("Seed=" + seed);
            RandomGenerator = new Random(seed);

            numRooms = RandomGenerator.Next(1, playerCount+1);
            numRooms = Math.Clamp(numRooms, 1, 3);

            StartCoroutine(GenerateCoroutine());
        }

        private IEnumerator GenerateCoroutine()
        {
            while (Result == null)
            {
                yield return GeneticAlgorithm();
                Debug.Log(_population[0].Fitness);
                _population[0].EvaluateFitness(true);

                numRooms = Math.Max(numRooms - 1, 1);
            }

            GenerateLevel();

            _netGenerationFinished.Value = true;
            customerManager.Difficulty = difficulty == 1 ? 5 : 10;
            GameplayManager.Instance.StartGame();
            uiManager.SetLoadingProgress(1.0f);
            yield return new WaitForSeconds(1);
            loadingBar.SetActive(false);
            uiManager.SetLoadingProgress(1.0f);
        }

        private IEnumerator GeneticAlgorithm()
        { 
            _population.Clear();
            _currentGeneration = 0;
            for (int i = 0; i < PopulationSize; i++)
            {
                Genome g = new Genome(numRooms);
                _population.Add(g);
            }
            
            while (_currentGeneration <= MaxGenerations)
            {
                _loadingBarScript.SetScale((float) _currentGeneration / MaxGenerations);
                UpdateLoadingBarClientRpc(_currentGeneration / (MaxGenerations / 0.85f));
                yield return true;
                
                _population = _population.OrderBy(o => o.Fitness).ToList();
                
                if (_population[0].Fitness <= 0)
                {
                    _currentGeneration = MaxGenerations;
                    _loadingBarScript.SetScale(1.0f);
                    UpdateLoadingBarClientRpc(1.0f);
                    Result = _population[0];
                    yield break;
                }
                
                float numElite = PopulationSize * EliteRatio;
                List<Genome> newGeneration = new();
                newGeneration.AddRange(_population.Take((int) numElite));
                
                float numGenerate = PopulationSize * (1f - EliteRatio);
                for (int i = 0; i < (int) numGenerate; i++)
                {
                    Genome g1 = _population[RandomGenerator.Next((int)(PopulationSize * TopPopulation))];
                    Genome g2 = _population[RandomGenerator.Next((int)(PopulationSize * TopPopulation))];

                    Genome cross = Genome.Cross(g1, g2);
                    newGeneration.Add(cross);
                }

                foreach (var g in _population)
                {
                    g.EvaluateFitness();
                }
                _population = newGeneration;
                _currentGeneration++;
            }
        }

        [ClientRpc]
        private void UpdateLoadingBarClientRpc(float fraction)
        {
            uiManager.SetLoadingProgress(fraction);
        }
        
        private void GenerateLevel()
        {
            // generate door positions
            foreach (var room in Result.Rooms)
            {
                foreach (var adj in room.AdjacentRoomOverlap)
                {
                    int doorPosition = RandomGenerator.Next(adj.Item2, adj.Item3);
                    int doorPositionWorld = adj.Item1 % 2 == 0 ? room.WorldX + doorPosition : room.WorldY + doorPosition;
                    room.AdjacentRoomDoorPositionsActive.Add(new Tuple<int, int>(adj.Item1, doorPositionWorld));
                    adj.Item4.AdjacentRoomDoorPositionsPassive.Add(new Tuple<int, int>((adj.Item1+2)%4, doorPositionWorld));
                }
            }
            
            foreach (var room in Result.Rooms)
            {
                var roomObj = Instantiate(room.IsServiceRoom ? floorServiceRoomPrefab : floorRoomPrefab);
                roomObj.GetComponent<NetworkObject>().Spawn(true);
                roomObj.transform.localScale = new Vector3(room.DimX, 0.01f, room.DimY);
                roomObj.transform.position = new Vector3(room.WorldX + (float)room.DimX / 2, -0.01f,
                    room.WorldY + (float)room.DimY / 2);
                roomObj.transform.SetParent(topLevel.transform);

                var gridObj = Instantiate(grid, topLevel.transform);
                gridObj.transform.position = new Vector3(room.WorldX + (float)room.DimX / 2, 0,
                    room.WorldY + (float)room.DimY / 2);
                var gridScript = gridObj.GetComponent<Grid>();
                gridScript.Init(new Vector3Int(room.DimX, 1, room.DimY), new Vector3(1, 1, 1));
                gridObj.GetComponent<NetworkObject>().Spawn(true);
                gridObj.transform.SetParent(topLevel.transform);
                
                for (int j = 0; j < room.NumWorkstations["Well"]; j++)
                {
                    var ws = Instantiate(workbenchWell, gridObj.transform);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["ClayPit"]; j++)
                {
                    var ws = Instantiate(workbenchClayPit, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["Table"]; j++)
                {
                    var ws = Instantiate(workbenchTable, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["Trashcan"]; j++)
                {
                    var ws = Instantiate(workbenchTrashcan, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["PottersWheel"]; j++)
                {
                    var ws = Instantiate(workbenchPottersWheel, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["Oven"]; j++)
                {
                    var ws = Instantiate(workbenchOven, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["BlackRocksPit"]; j++)
                {
                    var ws = Instantiate(workbenchBlackRocks, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["RedRocksPit"]; j++)
                {
                    var ws = Instantiate(workbenchRedRocks, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["GrindingTable"]; j++)
                {
                    var ws = Instantiate(workbenchGrindingTable, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }
                
                for (int j = 0; j < room.NumWorkstations["PaintersTable"]; j++)
                {
                    var ws = Instantiate(workbenchPaintersTable, gridObj.transform);
                    int randomX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                    int randomY = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                    ws.transform.position = new Vector3(randomX, 1, randomY);
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);
                }

                Dictionary<int, List<int>> doorPositions = new (); // mapping from wallIdx (south, west...) to list of door positions
                doorPositions.Add(0, new ());
                doorPositions.Add(1, new ());
                doorPositions.Add(2, new ());
                doorPositions.Add(3, new ());

                Debug.Log("ADJACENT ROOM POSITIONS ACTIVE SIZE: " + room.AdjacentRoomOverlap.Count);
                
                // spawn doors and save door positions
                foreach (var adj in room.AdjacentRoomDoorPositionsActive)
                {
                    var ws = Instantiate(door);;

                    var doorPositionWorld = adj.Item2;
                    if (adj.Item1 == 0)
                    {
                        doorPositions[0].Add(doorPositionWorld);
                        ws.transform.position = new Vector3(doorPositionWorld+0.5f, 0, room.WorldY);
                    }
                    else if (adj.Item1 == 1)
                    {
                        doorPositions[1].Add(doorPositionWorld);
                        ws.transform.position = new Vector3(room.WorldX, 0, doorPositionWorld+0.5f);
                        ws.transform.Rotate(Vector3.up, 90);
                    }
                    else if (adj.Item1 == 2)
                    {
                        doorPositions[2].Add(doorPositionWorld);
                        ws.transform.position = new Vector3(doorPositionWorld+0.5f, 0, room.WorldY+room.DimY);
                    }
                    else if (adj.Item1 == 3)
                    {
                        doorPositions[3].Add(doorPositionWorld);
                        ws.transform.position = new Vector3(room.WorldX+room.DimX, 0, doorPositionWorld+0.5f);
                        ws.transform.Rotate(Vector3.up, 270);
                    }
                    
                    ws.GetComponent<NetworkObject>().Spawn(true);
                    ws.transform.SetParent(gridObj.transform);

                    if (room.IsServiceRoom) GridManager.Instance.grids.Remove(gridScript);
                }

                if (room.IsServiceRoom) continue;
                
                // add door positions of adjacent rooms that lead to current room
                foreach (var adjPassive in room.AdjacentRoomDoorPositionsPassive)
                {
                    doorPositions[adjPassive.Item1].Add(adjPassive.Item2);
                }

                // spawn north and south walls
                for (int i = 0; i < room.DimX; i++)
                {
                    if (!doorPositions[0].Contains(room.WorldX+i)) // spawn a wall tile if there is no door
                    {
                        GameObject w1 = Instantiate(wallTile, topLevel.transform);
                        w1.transform.position = new Vector3(room.WorldX + i * gridScript.size.x + gridScript.size.x / 2,
                            1,
                            room.WorldY);
                        w1.GetComponent<NetworkObject>().Spawn(true);
                        w1.transform.SetParent(topLevel.transform);
                    }

                    if (!doorPositions[2].Contains(room.WorldX+i)) // spawn a wall tile if there is no door
                    {
                        GameObject w2 = Instantiate(wallTile, topLevel.transform);
                        w2.transform.position = new Vector3(room.WorldX + i * gridScript.size.x + gridScript.size.x / 2,
                            1,
                            room.WorldY + room.DimY);
                        w2.GetComponent<NetworkObject>().Spawn(true);
                        w2.transform.SetParent(topLevel.transform);
                    }
                }
                
                // spawn east and west walls
                for (int i = 0; i < room.DimY; i++)
                {
                    if (!doorPositions[1].Contains(room.WorldY + i)) // spawn a wall tile if there is no door
                    {
                        GameObject w1 = Instantiate(wallTile, topLevel.transform);
                        w1.transform.position = new Vector3(room.WorldX,
                            1,
                            room.WorldY + i * gridScript.size.y + gridScript.size.y / 2);
                        w1.transform.Rotate(Vector3.up, 90);
                        w1.GetComponent<NetworkObject>().Spawn(true);
                        w1.transform.SetParent(topLevel.transform);
                    }

                    if (!doorPositions[3].Contains(room.WorldY + i)) // spawn a wall tile if there is no door
                    {
                        GameObject w2 = Instantiate(wallTile, topLevel.transform);
                        w2.transform.position = new Vector3(room.WorldX + room.DimX,
                            1,
                            room.WorldY + i * gridScript.size.y + gridScript.size.y / 2);
                        w2.transform.Rotate(Vector3.up, 90);
                        w2.GetComponent<NetworkObject>().Spawn(true);
                        w2.transform.SetParent(topLevel.transform);
                    }
                }
            }
            
            foreach (var room in Result.Rooms)
            {
                // calculate random player spawns (TODO: do this such that player cant spawn inside object)
                for (int i = 0; i < room.NumSpawns; i++)
                {
                    while (true)
                    {
                        int x = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                        int z = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                        if (Instance.playersSpawns.Any(s => s.x == x && s.z == z)) continue;
                    
                        Instance.playersSpawns.Add(new Vector3Int(x, 0, z));
                        break;
                    }
                }

                // direction -> queueStartPosition, queueExitPosition, queueExitDirection
                Dictionary<int, Tuple<Vector3Int, Vector3Int, Vector3>> customerPositionInfo = new()
                {
                    {0, new Tuple<Vector3Int, Vector3Int, Vector3>(new Vector3Int(room.WorldX+1, 0, room.WorldY), 
                        new Vector3Int(room.WorldX+room.DimX-1, 0, room.WorldY), Vector3.back)},
                    {1, new Tuple<Vector3Int, Vector3Int, Vector3>(new Vector3Int(room.WorldX, 0, room.WorldY+room.DimY-2), 
                        new Vector3Int(room.WorldX, 0, room.WorldY), Vector3.left)},
                    {2, new Tuple<Vector3Int, Vector3Int, Vector3>(new Vector3Int(room.WorldX+room.DimX-2, 0, room.WorldY+room.DimY-1),
                        new Vector3Int(room.WorldX, 0, room.WorldY+room.DimY-1), Vector3.forward)},
                    {3, new Tuple<Vector3Int, Vector3Int, Vector3>(new Vector3Int(room.WorldX+room.DimX-1, 0, room.WorldY+1), 
                        new Vector3Int(room.WorldX+room.DimX-1, 0, room.WorldY+room.DimY-1), Vector3.right)},
                }; 
                
                // calculate customer spawns
                if (room.IsServiceRoom)
                {
                    CustomerManager customerManager =
                        GameObject.Find("CustomerManager").GetComponent<CustomerManager>();

                    List<Vector3Int> orderSpots = new();
                    Vector3Int queuePos = new Vector3Int();
                    Vector3Int exitPos = new Vector3Int();
                    Vector3 exitDirection = new Vector3();

                    // recalculate collider mesh for raycasting
                    Physics.autoSimulation = false;
                    Physics.Simulate(Time.fixedDeltaTime);  
                    Physics.autoSimulation = true;
                    
                    // determine obstacle free direction
                    int freeDir = 5;
                    for (int i = 0; i < 4; i++)
                    {
                        Tuple<Vector3Int, Vector3Int, Vector3> customerPosInfo = customerPositionInfo[i];
                        queuePos = customerPosInfo.Item1;
                        exitPos = customerPosInfo.Item2;
                        exitDirection = customerPosInfo.Item3;

                        bool foundObstacle = false;
                        Vector3 connection = exitPos - queuePos;
                        connection = connection.normalized;
                        for (int rayId = 0; rayId < (i % 2 == 0 ? room.DimX : room.DimY); rayId++)
                        {
                            bool raycastHit = Physics.Raycast(new Vector3(queuePos.x+0.5f, 0.5f, queuePos.z+0.5f) - connection + connection * rayId, exitDirection, out _, 200, LayerMask.GetMask("Wall"));
                            if (!raycastHit) continue;
                            foundObstacle = true;
                            break;
                        }

                        if (foundObstacle) continue;
                        freeDir = i;
                        break;
                    }
                    
                    Debug.Assert(freeDir is >= 0 and < 4);
                    
                    customerManager.OrderSpots.Clear();
                    
                    // determine order spots
                    for (int i = 0; i < 3; i++)
                    {
                        int posX = queuePos.x, posZ = queuePos.z;
                        while ((posX == queuePos.x && posZ == queuePos.z)
                               || (posX == exitPos.x && posZ == exitPos.z)
                               || orderSpots.Any(a => a.Equals(new Vector3Int(posX, 0, posZ))))
                        {
                            posX = RandomGenerator.Next(room.WorldX, room.WorldX + room.DimX);
                            posZ = RandomGenerator.Next(room.WorldY, room.WorldY + room.DimY);
                        }

                        int rotY = RandomGenerator.Next(0, 361);

                        orderSpots.Add(new Vector3Int(posX, 0, posZ));
                        customerManager.OrderSpots.Add(
                            new OrderSpot
                                { Position = new Vector3(posX+0.5f, 0, posZ+0.5f), Rotation = Quaternion.Euler(0, rotY, 0) });
                    }

                    customerManager.exitPosition =
                        new Vector3(exitPos.x + 0.5f, 0, exitPos.z + 0.5f) + exitDirection * 30;
                    customerManager.queueFirstPosition = new Vector3(queuePos.x+0.5f, 0, queuePos.z+0.5f);
                    customerManager.spawnPosition = 
                        new Vector3(queuePos.x + 0.5f, 0, queuePos.z + 0.5f) + exitDirection * 15;

                    customerManager.queueFirstFacingRotation = Quaternion.LookRotation(-exitDirection, Vector3.up);
                }
            }
            
            // place player objects
            int playerIdIdx = 0;
            foreach (var spawnPoint in playersSpawns)
            {
                if (playerIdIdx >= NetworkManager.Singleton.ConnectedClientsList.Count) continue;
                
                ulong clientId = NetworkManager.Singleton.ConnectedClientsList[playerIdIdx].ClientId;

                PlacePlayerObjectClientRpc(clientId, spawnPoint + new Vector3(0.5f, 0.1f, 0.5f));
                
                playerIdIdx++;
            }
        }

        [ClientRpc]
        private void PlacePlayerObjectClientRpc(ulong clientId, Vector3 position)
        {
            if (NetworkManager.Singleton.LocalClient.ClientId != clientId) return;

            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = position;

            Debug.Log("Placed player: " + NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.name + " at " + position);
        }
    }
}