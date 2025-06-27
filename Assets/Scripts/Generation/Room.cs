using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation
{
    public class Room
    {
        public int DimX, DimY;
        public int WorldX, WorldY;
        public List<Vector2Int> Corners = new();
        public int NumSpawns;

        public bool IsServiceRoom;
        public Dictionary<string, int> NumWorkstations = new ()
        {
            {"Well", 0},
            {"Trashcan", 0},
            {"Table", 0},
            {"PottersWheel", 0},
            {"Oven", 0},
            {"ClayPit", 0},
            {"BlackRocksPit", 0},
            {"RedRocksPit", 0},
            {"GrindingTable", 0},
            {"PaintersTable", 0}
        };

        // specifies the coordinates of the overlap area
        // List entries: wall orientation (south, west..), overlap coordinates x,y, other room
        public List<Tuple<int, int, int, Room>> AdjacentRoomOverlap = new();
        
        // wall orientation (south, west..), door position
        public List<Tuple<int, int>> AdjacentRoomDoorPositionsActive = new();
        public List<Tuple<int, int>> AdjacentRoomDoorPositionsPassive = new();

        public Room(int dimX, int dimY, int worldX, int worldY, int numSpawns, bool isServiceRoom=false)
        {
            DimX = dimX;
            DimY = dimY;
            WorldX = worldX;
            WorldY = worldY;
            NumSpawns = numSpawns;
            IsServiceRoom = isServiceRoom;
            // top left, bottom right
            Corners.Add(new Vector2Int(worldX, worldY+(dimY-1)));
            Corners.Add(new Vector2Int(worldX+(dimX-1), worldY));
        }
    }
}