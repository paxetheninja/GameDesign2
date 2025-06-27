using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation
{
    public enum GeneDescription
    {
        CircularLevel,
        ServiceRoomDimensions,
        ServiceRoomWorldPosition,
        DivisionMode,
        RoomDimensions,
        RoomWorldPosition,
        PlayerSpawns,
        WorkstationWell,
        WorkstationClayPit,
        WorkstationTable,
        WorkstationTrashcan,
        WorkstationPottersWheel,
        WorkstationOven,
        WorkstationBlackRocksPit,
        WorkstationGrindingTable,
        WorkstationPaintersTable,
        WorkstationRedRocksPit
    }

    public enum GeneExpressionType
    {
        CircularLevel,
        DivisionMode,
        RoomDimX,
        RoomDimY,
        RoomPosX,
        RoomPosY,
        PlayerSpawns,
        WorkstationWell,
        WorkstationClayPit,
        WorkstationTable,
        WorkstationTrashcan,
        WorkstationPottersWheel,
        WorkstationOven,
        WorkstationBlackRocksPit,
        WorkstationGrindingTable,
        WorkstationPaintersTable,
        WorkstationRedRocksPit
    }
    
    public class Gene
    {
        public static Dictionary<GeneDescription, List<GeneExpressionType>> GeneBlueprint = new()
        {
            {GeneDescription.CircularLevel, new List<GeneExpressionType> { GeneExpressionType.CircularLevel }},
            {GeneDescription.ServiceRoomDimensions, new List<GeneExpressionType> { GeneExpressionType.RoomDimX , GeneExpressionType.RoomDimY}},
            {GeneDescription.ServiceRoomWorldPosition, new List<GeneExpressionType> { GeneExpressionType.RoomPosX , GeneExpressionType.RoomPosY}},
            {GeneDescription.DivisionMode, new List<GeneExpressionType> { GeneExpressionType.DivisionMode }},
            {GeneDescription.RoomDimensions, new List<GeneExpressionType> { GeneExpressionType.RoomDimX , GeneExpressionType.RoomDimY}},
            {GeneDescription.RoomWorldPosition, new List<GeneExpressionType> { GeneExpressionType.RoomPosX , GeneExpressionType.RoomPosY}},
            {GeneDescription.PlayerSpawns, new List<GeneExpressionType> { GeneExpressionType.PlayerSpawns }},
            {GeneDescription.WorkstationWell, new List<GeneExpressionType> { GeneExpressionType.WorkstationWell }},
            {GeneDescription.WorkstationClayPit, new List<GeneExpressionType> { GeneExpressionType.WorkstationClayPit }},
            {GeneDescription.WorkstationTable, new List<GeneExpressionType> { GeneExpressionType.WorkstationTable }},
            {GeneDescription.WorkstationTrashcan, new List<GeneExpressionType> { GeneExpressionType.WorkstationTrashcan }},
            {GeneDescription.WorkstationPottersWheel, new List<GeneExpressionType> { GeneExpressionType.WorkstationPottersWheel }},
            {GeneDescription.WorkstationOven, new List<GeneExpressionType> { GeneExpressionType.WorkstationOven }},
            {GeneDescription.WorkstationBlackRocksPit, new List<GeneExpressionType> { GeneExpressionType.WorkstationBlackRocksPit }},
            {GeneDescription.WorkstationRedRocksPit, new List<GeneExpressionType> { GeneExpressionType.WorkstationRedRocksPit }},
            {GeneDescription.WorkstationGrindingTable, new List<GeneExpressionType> { GeneExpressionType.WorkstationGrindingTable }},
            {GeneDescription.WorkstationPaintersTable, new List<GeneExpressionType> { GeneExpressionType.WorkstationPaintersTable }}
        };
        
        public static Dictionary<GeneExpressionType, List<int>> GeneExpressions = new()
        {
            // first 2 values define the range, last defines the multiplier (to get multiples of X)
            {GeneExpressionType.CircularLevel, new List<int> {0, 1, 1}},
            {GeneExpressionType.DivisionMode, new List<int> {1, 2, 1}},
            {GeneExpressionType.RoomDimX, new List<int> {2, 6, 2}},
            {GeneExpressionType.RoomDimY, new List<int> {2, 6, 2}},
            {GeneExpressionType.RoomPosX, new List<int> {-12, 12, 2}},
            {GeneExpressionType.RoomPosY, new List<int> {-12, 12, 2}},
            {GeneExpressionType.PlayerSpawns, new List<int> {0, 4, 1}},
            {GeneExpressionType.WorkstationWell, new List<int> {0, 1, 1}},
            {GeneExpressionType.WorkstationClayPit, new List<int> {0, 2, 1}},
            {GeneExpressionType.WorkstationTable, new List<int> {0, 2, 1}},
            {GeneExpressionType.WorkstationTrashcan, new List<int> {0, 1, 1}},
            {GeneExpressionType.WorkstationPottersWheel, new List<int> {0, 2, 1}},
            {GeneExpressionType.WorkstationOven, new List<int> {0, 1, 1}},
            {GeneExpressionType.WorkstationBlackRocksPit, new List<int> {0, 1, 1}},
            {GeneExpressionType.WorkstationRedRocksPit, new List<int> {0, 1, 1}},
            {GeneExpressionType.WorkstationGrindingTable, new List<int> {0, 2, 1}},
            {GeneExpressionType.WorkstationPaintersTable, new List<int> {0, 2, 1}},
        };

        public GeneDescription Description;

        // list contains rooms and dictionary mapping from expression type to value
        // feature expression can be N-dimensional (for example Room sizes)
        public List<Dictionary<GeneExpressionType, int>> Expression = new ();

        public Gene(GeneDescription description, int rooms)
        {
            Description = description;

            for (int i = 0; i < rooms; i++)
            {
                Expression.Add(new Dictionary<GeneExpressionType, int>());

                foreach (var eType in GeneBlueprint[description])
                {
                    int randomExpr = LevelGenerator.RandomGenerator.Next(GeneExpressions[eType][0], GeneExpressions[eType][1]+1) * GeneExpressions[eType][2];
                    Expression[i].Add(eType, randomExpr);
                }
            }
        }

        public override string ToString()
        {
            string s = "[";
            foreach (var r in Expression)
            {
                s += "(" + string.Join(",", r.Values) + ")";
            }

            s += "]";

            return s;
        }
    }
}