using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Generation
{
    public class Genome
    {
        private readonly List<Gene> _genes = new ();
        public readonly List<Room> Rooms = new();
        public int Fitness;

        // CONSTRUCTOR
        public Genome(int numRooms)
        {
            foreach (GeneDescription description in Gene.GeneBlueprint.Keys)
            {
                _genes.Add(new Gene(description, numRooms));
            }
            
            EvaluateFitness();
        }
        
        public static Genome Cross(Genome a, Genome b)
        {
            Genome newG = new Genome(LevelGenerator.Instance.numRooms);

            for (int i = 0; i < a._genes.Count; i++)
            {
                // cross room expressions individually
                double probability;
                if (a._genes[i].Description is GeneDescription.RoomDimensions or GeneDescription.RoomWorldPosition)
                {
                    Debug.Log("Crossing separately");
                    for (int r = 0; r < a._genes[i].Expression.Count; r++)
                    {
                        probability = LevelGenerator.RandomGenerator.NextDouble();
                        if (probability < 0.45)
                        {
                            newG._genes[i].Expression[r] = a._genes[i].Expression[r];
                        }
                        else if (probability < 0.9)
                        {
                            newG._genes[i].Expression[r] = b._genes[i].Expression[r];
                        }
                        else
                        {
                            newG._genes[i] = new Gene(a._genes[i].Description, LevelGenerator.Instance.numRooms);
                        } 
                    }
                }
                else
                {
                    probability = LevelGenerator.RandomGenerator.NextDouble();
                    if (probability < 0.45)
                    {
                        newG._genes[i] = a._genes[i];
                    }
                    else if (probability < 0.9)
                    {
                        newG._genes[i] = b._genes[i];
                    }
                    else
                    {
                        newG._genes[i] = new Gene(a._genes[i].Description, LevelGenerator.Instance.numRooms);
                    } 
                }
            }
            
            newG.EvaluateFitness();
            return newG;
        }
        
        public void EvaluateFitness(bool debug=false)
        {
            Rooms.Clear();
            Fitness = 0;
            
            // proper room positioning
            
            // service room dims
            var serviceRoomDims = GetExpression(_genes, GeneDescription.ServiceRoomDimensions, 0);
            var serviceRoomPos = GetExpression(_genes, GeneDescription.ServiceRoomWorldPosition, 0);
            Rooms.Add(new Room
            (
                serviceRoomDims[GeneExpressionType.RoomDimX], serviceRoomDims[GeneExpressionType.RoomDimY],
                serviceRoomPos[GeneExpressionType.RoomPosX], serviceRoomPos[GeneExpressionType.RoomPosY], 
                0, true
            ));

            // circular level (last room connects to service room)
            var circularLevel = GetExpression(_genes, GeneDescription.CircularLevel, 0)[GeneExpressionType.CircularLevel];
            // total # of player spawns equal to # of players
            int totalSpawnPoints = 0;
            // total number of room divisions can't be > # of players
            int totalDivisions = 0;
            
            // rooms must be large enough to fit divisions
            int cannotFitDivisions = 0;
            
            // All unique workstations present at least once
            Dictionary<string, int> availableWorkstations = new Dictionary<string, int>()
            {
                {"Well", 0},
                {"ClayPit", 0},
                {"PottersWheel", 0},
                {"Table", 0},
                {"Trashcan", 0},
                {"Oven", 0},
                {"BlackRocksPit", 0},
                {"RedRocksPit", 0},
                {"GrindingTable", 0},
                {"PaintersTable", 0},
            };
            int totalWorkstations = 0;

            for (int room = 0; room < LevelGenerator.Instance.numRooms; room++)
            {
                // total # of player spawns equal to # of players
                int spawnPointsRoom = GetExpression(_genes, GeneDescription.PlayerSpawns, room)[GeneExpressionType.PlayerSpawns];
                totalSpawnPoints += spawnPointsRoom;

                var roomDims = GetExpression(_genes, GeneDescription.RoomDimensions, room);
                
                var roomPos = GetExpression(_genes, GeneDescription.RoomWorldPosition, room);

                Room current = new Room
                (
                    roomDims[GeneExpressionType.RoomDimX], roomDims[GeneExpressionType.RoomDimY],
                    roomPos[GeneExpressionType.RoomPosX], roomPos[GeneExpressionType.RoomPosY], 
                    spawnPointsRoom
                );

                Rooms.Add(current);
                
                int workstationsWell = GetExpression(_genes, GeneDescription.WorkstationWell, room)[GeneExpressionType.WorkstationWell];
                int workstationsClayPit = GetExpression(_genes, GeneDescription.WorkstationClayPit, room)[GeneExpressionType.WorkstationClayPit];
                int workstationsTable = GetExpression(_genes, GeneDescription.WorkstationTable, room)[GeneExpressionType.WorkstationTable];
                int workstationsTrashcan = GetExpression(_genes, GeneDescription.WorkstationTrashcan, room)[GeneExpressionType.WorkstationTrashcan];
                int workstationsPottersWheel = GetExpression(_genes, GeneDescription.WorkstationPottersWheel, room)[GeneExpressionType.WorkstationPottersWheel];
                int workstationsOven = GetExpression(_genes, GeneDescription.WorkstationOven, room)[GeneExpressionType.WorkstationOven];
                int workstationsBlackRocks = GetExpression(_genes, GeneDescription.WorkstationBlackRocksPit, room)[GeneExpressionType.WorkstationBlackRocksPit];
                int workstationsRedRocks = GetExpression(_genes, GeneDescription.WorkstationRedRocksPit, room)[GeneExpressionType.WorkstationRedRocksPit];
                int workstationsGrindingTable = GetExpression(_genes, GeneDescription.WorkstationGrindingTable, room)[GeneExpressionType.WorkstationGrindingTable];
                int workstationsPaintersTable = GetExpression(_genes, GeneDescription.WorkstationPaintersTable, room)[GeneExpressionType.WorkstationPaintersTable];

                current.NumWorkstations["Well"] = workstationsWell;
                current.NumWorkstations["ClayPit"] = workstationsClayPit;
                current.NumWorkstations["Table"] = workstationsTable;
                current.NumWorkstations["Trashcan"] = workstationsTrashcan;
                current.NumWorkstations["PottersWheel"] = workstationsPottersWheel;
                current.NumWorkstations["Oven"] = workstationsOven;
                current.NumWorkstations["BlackRocksPit"] = workstationsBlackRocks;
                current.NumWorkstations["RedRocksPit"] = workstationsRedRocks;
                current.NumWorkstations["GrindingTable"] = workstationsGrindingTable;
                current.NumWorkstations["PaintersTable"] = workstationsPaintersTable;
                
                totalWorkstations += workstationsWell + workstationsClayPit + workstationsTable +
                                        workstationsTrashcan + workstationsPottersWheel + workstationsOven +
                                        workstationsBlackRocks + workstationsRedRocks + workstationsGrindingTable +
                                        workstationsPaintersTable;

                int totalWorkstationsSize = workstationsWell * LevelGenerator.Instance.workbenchWell.GetComponent<GridObject>().GetSize() +
                                            workstationsClayPit * LevelGenerator.Instance.workbenchClayPit.GetComponent<GridObject>().GetSize() +
                                            workstationsTable * LevelGenerator.Instance.workbenchTable.GetComponent<GridObject>().GetSize() +
                                            workstationsTrashcan * LevelGenerator.Instance.workbenchTrashcan.GetComponent<GridObject>().GetSize() +
                                            workstationsPottersWheel * LevelGenerator.Instance.workbenchPottersWheel.GetComponent<GridObject>().GetSize() +
                                            workstationsOven * LevelGenerator.Instance.workbenchOven.GetComponent<GridObject>().GetSize() +
                                            workstationsBlackRocks * LevelGenerator.Instance.workbenchBlackRocks.GetComponent<GridObject>().GetSize() +
                                            workstationsRedRocks * LevelGenerator.Instance.workbenchRedRocks.GetComponent<GridObject>().GetSize() +
                                            workstationsGrindingTable * LevelGenerator.Instance.workbenchGrindingTable.GetComponent<GridObject>().GetSize() +
                                            workstationsPaintersTable * LevelGenerator.Instance.workbenchPaintersTable.GetComponent<GridObject>().GetSize();

                availableWorkstations["Well"] += workstationsWell;
                availableWorkstations["ClayPit"] += workstationsClayPit;
                availableWorkstations["PottersWheel"] += workstationsPottersWheel;
                availableWorkstations["Table"] += workstationsTable;
                availableWorkstations["Trashcan"] += workstationsTrashcan;
                availableWorkstations["Oven"] += workstationsOven;
                availableWorkstations["BlackRocksPit"] += workstationsBlackRocks;
                availableWorkstations["RedRocksPit"] += workstationsRedRocks;
                availableWorkstations["GrindingTable"] += workstationsGrindingTable;
                availableWorkstations["PaintersTable"] += workstationsPaintersTable;

                // >3x larger grid than space workstations use
                int roomSize = roomDims[GeneExpressionType.RoomDimX] * roomDims[GeneExpressionType.RoomDimY];

                if (roomSize < totalWorkstationsSize * 3)
                {
                    if (debug) Debug.Log("Roomsize not smaller than totalWSSize * 4");
                    Fitness += totalWorkstationsSize * 3 - roomSize;
                }

                // at elast 4 workstations per room
                if (totalWorkstations < 4)
                {
                    Fitness += 4-totalWorkstations;
                }
                
                // total number of room divisions can't be > # of players
                // for every additional player (1+) there can be 1 room division
                totalDivisions += GetExpression(_genes, GeneDescription.DivisionMode, room)[GeneExpressionType.DivisionMode] == 2 ? 1 : 0;

                if (GetExpression(_genes, GeneDescription.DivisionMode, room)[GeneExpressionType.DivisionMode] == 2 &&
                    roomDims[GeneExpressionType.RoomDimX] < 6)
                {
                    cannotFitDivisions++;
                }
            }

            // check room intersections
            for (int i = 0; i < Rooms.Count; i++)
            {
                Room room1 = Rooms[i];
                for (int j = 0; j < Rooms.Count; j++)
                {
                    if (i == j) continue;
                    Room room2 = Rooms[j];

                    // Source: https://www.geeksforgeeks.org/find-two-rectangles-overlap/
                    Vector2Int l1 = room1.Corners[0];
                    Vector2Int r1 = room1.Corners[1];
                
                    Vector2Int l2 = room2.Corners[0];
                    Vector2Int r2 = room2.Corners[1];
                    
                    // if rectangle has area 0, no overlap
                    if (l1.x == r1.x || l1.y == r1.y || r2.x == l2.x || l2.y == r2.y) continue;
   
                    // If one rectangle is on left side of other
                    if (l1.x > r2.x || l2.x > r1.x) continue;
 
                    // If one rectangle is above other
                    if (r1.y > l2.y || r2.y > l1.y) continue;

                    if (debug) Debug.Log("Rooms intersected!");
                    Fitness++;
                }
            }
            
            // room must be directly next to some other room
            for (int i = 0; i < Rooms.Count - (circularLevel == 1 && LevelGenerator.Instance.numRooms != 1 ? 0 : 1); i++)
            {
                int adjacentRooms = 0;
                
                Room room1 = Rooms[i];
                Room room2 = Rooms[(i+1)%Rooms.Count];
                
                Vector2Int l1 = room1.Corners[0];
                Vector2Int r1 = room1.Corners[1];
                
                Vector2Int l2 = room2.Corners[0];
                Vector2Int r2 = room2.Corners[1];
                    
                int overlapStartX = Math.Max(l1.x, l2.x) - room1.WorldX;
                int overlapEndX = Math.Min(r1.x, r2.x) - room1.WorldX;
                int overlapLengthX = Math.Max(0, overlapEndX - overlapStartX);
                    
                int overlapStartY = Math.Max(r1.y, r2.y) - room1.WorldY;
                int overlapEndY = Math.Min(l1.y, l2.y) - room1.WorldY;
                int overlapLengthY = Math.Max(0, overlapEndY - overlapStartY);
                    
                if (Math.Abs(l1.x - r2.x) == 1 && overlapLengthY >= 1)
                {
                    // west overlap
                    room1.AdjacentRoomOverlap.Add(new Tuple<int, int, int, Room>(1, overlapStartY, overlapEndY, room2));
                    
                    adjacentRooms++;
                }
                else if (Math.Abs(l1.y - r2.y) == 1 && overlapLengthX >= 1)
                {
                    // north overlap
                    room1.AdjacentRoomOverlap.Add(new Tuple<int, int, int, Room>(2, overlapStartX, overlapEndX, room2));
                    
                    adjacentRooms++;
                }
                else if (Math.Abs(r1.x - l2.x) == 1 && overlapLengthY >= 1)
                {
                    // east overlap
                    room1.AdjacentRoomOverlap.Add(new Tuple<int, int, int, Room>(3, overlapStartY, overlapEndY, room2));
                    
                    adjacentRooms++;
                }
                else if (Math.Abs(r1.y - l2.y) == 1 && overlapLengthX >= 1)
                {
                    // south overlap
                    room1.AdjacentRoomOverlap.Add(new Tuple<int, int, int, Room>(0, overlapStartX, overlapEndX, room2));
                    
                    adjacentRooms++;
                }

                if (adjacentRooms == 0)
                {
                    if (debug) Debug.Log("No adjacent room");
                    Fitness++;
                }
            }

            // total # of player spawns equal to # of players
            if (totalSpawnPoints != LevelGenerator.Instance.playerCount)
            {
                Fitness++;
                if (debug) Debug.Log("Wrong # spawns " + LevelGenerator.Instance.playerCount + " - "  + totalSpawnPoints);
            }
            
            // All unique workstations present at least once
            if (availableWorkstations["Well"] == 0 || availableWorkstations["ClayPit"] == 0 ||
                availableWorkstations["PottersWheel"] == 0 || availableWorkstations["Table"] == 0 ||
                availableWorkstations["Trashcan"] == 0 || availableWorkstations["Oven"] == 0 ||
                (LevelGenerator.Instance.difficulty == 2 && 
                (availableWorkstations["BlackRocksPit"] == 0 || availableWorkstations["RedRocksPit"] == 0 ||
                availableWorkstations["GrindingTable"] == 0 || availableWorkstations["PaintersTable"] == 0)))
            {
                Fitness++;
                if (debug) Debug.Log("Not all workstations present at least once");
            }
            
            // total number of room divisions can't be > # of players
            //if (totalDivisions > LevelGenerator.Instance.playerCount - 1)
            //{
            //    Fitness++;
            //    if (debug) Debug.Log("Total room divisions exceed limit for players.");
            //}
            
            // rooms must be large enough to fit divisions
            //if (cannotFitDivisions != 0)
            //{
            //    Fitness++;
            //    if (debug) Debug.Log("At least one room cannot fit selected divisions");
            //}

            
            if (LevelGenerator.Instance.difficulty == 1)
            {
                // easy mode has a circular level (2 rooms connect to service room)
                if (circularLevel == 0)
                {
                    if (debug) Debug.Log("Not a circular level in easy mode");
                    Fitness += 20;
                }

                // todo leave out painters table, red rocks, black rocks, grinding table
                // easy mode has no paint (painters table, red rocks, black rocks, grinding table can be left out
                Fitness += availableWorkstations["GrindingTable"];
                Fitness += availableWorkstations["PaintersTable"];
                Fitness += availableWorkstations["RedRocksPit"];
                Fitness += availableWorkstations["BlackRocksPit"];
            }
            else if (LevelGenerator.Instance.difficulty == 2)
            {
                // hard mode has no circular level (2 rooms connect to service room)
                if (circularLevel == 1)
                {
                    if (debug) Debug.Log("Circular level in hard mode");
                    Fitness += 20;
                }
                
                if (totalWorkstations > LevelGenerator.NumWorkstations * 2)
                {
                    if (debug) Debug.Log("Too many workstations for hard mode");
                    Fitness++;
                }
            }
        }
        
        public static Dictionary<GeneExpressionType, int> GetExpression(List<Gene> genes, GeneDescription description, int room)
        {
            foreach (var gene in genes.Where(gene => gene.Description == description))
            {
                return gene.Expression[room];
            }

            return null;
        }
        
        public override string ToString()
        {
            string ret = "-";
            foreach (var g in _genes)
            {
                ret += g + "-";
            }
            
            return ret;
        }
        
        public static Dictionary<int, Tuple<int, int>> DifficultyRanges = new()
        {
            {1, new Tuple<int, int>(12, 100)},
            {2, new Tuple<int, int>(6, 11)},
            {3, new Tuple<int, int>(0, 5)}
        };
    }
}