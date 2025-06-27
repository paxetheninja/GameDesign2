using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentRecipesManager : MonoBehaviour
{

    [SerializeField] private GameObject clayPrefab;
    [SerializeField] private GameObject wetClayPrefab;
    [SerializeField] private GameObject waterPrefab;

    [SerializeField] private GameObject kneadedClayPrefab;
    [SerializeField] private GameObject kneadedClay2Prefab;
    [SerializeField] private GameObject kneadedClay3Prefab;

    #region PlatePrefabs
    [SerializeField] private GameObject plateRawPrefab;
    [SerializeField] private GameObject plateBakedPrefab;
    [SerializeField] private GameObject plateRawRedPrefab;
    [SerializeField] private GameObject plateBakedRedPrefab;
    [SerializeField] private GameObject plateRawBlackPrefab;
    [SerializeField] private GameObject plateBakedBlackPrefab;

    [SerializeField] private GameObject plate1HandleRawPrefab;
    [SerializeField] private GameObject plate1HandleBakedPrefab;
    [SerializeField] private GameObject plate1HandleRawRedPrefab;
    [SerializeField] private GameObject plate1HandleBakedRedPrefab;
    [SerializeField] private GameObject plate1HandleRawBlackPrefab;
    [SerializeField] private GameObject plate1HandleBakedBlackPrefab;

    [SerializeField] private GameObject plate2HandleRawPrefab;
    [SerializeField] private GameObject plate2HandleBakedPrefab;
    [SerializeField] private GameObject plate2HandleRawRedPrefab;
    [SerializeField] private GameObject plate2HandleBakedRedPrefab;
    [SerializeField] private GameObject plate2HandleRawBlackPrefab;
    [SerializeField] private GameObject plate2HandleBakedBlackPrefab;
    #endregion

    #region MediumVases
    [SerializeField] private GameObject mediumVaseRawPrefab;
    [SerializeField] private GameObject mediumVaseRawBlackPrefab;
    [SerializeField] private GameObject mediumVaseRawRedPrefab;

    [SerializeField] private GameObject mediumVaseBakedPrefab;
    [SerializeField] private GameObject mediumVaseBakedBlackPrefab;
    [SerializeField] private GameObject mediumVaseBakedRedPrefab;

    [SerializeField] private GameObject mediumVase1HandleRawPrefab;
    [SerializeField] private GameObject mediumVase1HandleRawBlackPrefab;
    [SerializeField] private GameObject mediumVase1HandleRawRedPrefab;

    [SerializeField] private GameObject mediumVase1HandleBakedPrefab;
    [SerializeField] private GameObject mediumVase1HandleBakedBlackPrefab;
    [SerializeField] private GameObject mediumVase1HandleBakedRedPrefab;

    [SerializeField] private GameObject mediumVase2HandleRawPrefab;
    [SerializeField] private GameObject mediumVase2HandleRawBlackPrefab;
    [SerializeField] private GameObject mediumVase2HandleRawRedPrefab;

    [SerializeField] private GameObject mediumVase2HandleBakedPrefab;
    [SerializeField] private GameObject mediumVase2HandleBakedBlackPrefab;
    [SerializeField] private GameObject mediumVase2HandleBakedRedPrefab;


    [SerializeField] private GameObject mediumVase3HandleRawPrefab;
    [SerializeField] private GameObject mediumVase3HandleRawBlackPrefab;
    [SerializeField] private GameObject mediumVase3HandleRawRedPrefab;

    [SerializeField] private GameObject mediumVase3HandleBakedPrefab;
    [SerializeField] private GameObject mediumVase3HandleBakedBlackPrefab;
    [SerializeField] private GameObject mediumVase3HandleBakedRedPrefab;



    #endregion

    #region BigVases
    [SerializeField] private GameObject bigVaseRawPrefab;
    [SerializeField] private GameObject bigVaseRawBlackPrefab;
    [SerializeField] private GameObject bigVaseRawRedPrefab;

    [SerializeField] private GameObject bigVaseBakedPrefab;
    [SerializeField] private GameObject bigVaseBakedBlackPrefab;
    [SerializeField] private GameObject bigVaseBakedRedPrefab;

    [SerializeField] private GameObject bigVase1HandleRawPrefab;
    [SerializeField] private GameObject bigVase1HandleRawBlackPrefab;
    [SerializeField] private GameObject bigVase1HandleRawRedPrefab;

    [SerializeField] private GameObject bigVase1HandleBakedPrefab;
    [SerializeField] private GameObject bigVase1HandleBakedBlackPrefab;
    [SerializeField] private GameObject bigVase1HandleBakedRedPrefab;

    [SerializeField] private GameObject bigVase2HandleRawPrefab;
    [SerializeField] private GameObject bigVase2HandleRawBlackPrefab;
    [SerializeField] private GameObject bigVase2HandleRawRedPrefab;

    [SerializeField] private GameObject bigVase2HandleBakedPrefab;
    [SerializeField] private GameObject bigVase2HandleBakedBlackPrefab;
    [SerializeField] private GameObject bigVase2HandleBakedRedPrefab;

    [SerializeField] private GameObject bigVase3HandleRawPrefab;
    [SerializeField] private GameObject bigVase3HandleRawBlackPrefab;
    [SerializeField] private GameObject bigVase3HandleRawRedPrefab;

    [SerializeField] private GameObject bigVase3HandleBakedPrefab;
    [SerializeField] private GameObject bigVase3HandleBakedBlackPrefab;
    [SerializeField] private GameObject bigVase3HandleBakedRedPrefab;
    #endregion

    [SerializeField] private GameObject handle3Prefab;
    [SerializeField] private GameObject handle2Prefab;
    [SerializeField] private GameObject handle1Prefab;

    [SerializeField] private GameObject claySlipPrefab;
    [SerializeField] private GameObject redSlipPrefab;
    [SerializeField] private GameObject blackSlipPrefab;

    [SerializeField] private GameObject redRocksPrefab;
    [SerializeField] private GameObject redRocksGravelPrefab;
    [SerializeField] private GameObject blackRocksPrefab;
    [SerializeField] private GameObject blackRocksGravelPrefab;

    private Dictionary<Tuple<ComponentType, ComponentType>, ComponentType> combinationRecipes = new();
    private Dictionary<Tuple<WorkstationType, ComponentType>, Tuple<float, ComponentType>> interactionRecipes = new();
    private Dictionary<Tuple<WorkstationType, ComponentType, ComponentType>, Tuple<float, ComponentType>> twoBaseInteractionRecipes = new();
    private Dictionary<Tuple<ComponentType, ComponentType>, Tuple<ComponentType, ComponentType>> pickupStackingItems = new();


    public static ComponentRecipesManager Instance;

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

        DontDestroyOnLoad(gameObject);

        #region CombinationDescriptions
        //Basic Components
        CreateCombinationRecipe(ComponentType.Clay, ComponentType.Water, ComponentType.WetClay);
        CreateCombinationRecipe(ComponentType.WetClay, ComponentType.Water, ComponentType.ClaySlip);
        CreateCombinationRecipe(ComponentType.ClaySlip, ComponentType.RedRocksGravel, ComponentType.RedSlip);
        CreateCombinationRecipe(ComponentType.ClaySlip, ComponentType.BlackRocksGravel, ComponentType.BlackSlip);

        //Plates
        CreateCombinationRecipe(ComponentType.PlateRaw, ComponentType.Handle1, ComponentType.Plate1HandleRaw);
        CreateCombinationRecipe(ComponentType.PlateRaw, ComponentType.Handle2, ComponentType.Plate2HandleRaw);

        //Medium Vases 
        CreateCombinationRecipe(ComponentType.MediumVaseRaw, ComponentType.Handle1, ComponentType.MediumVase1HandleRaw);
        CreateCombinationRecipe(ComponentType.MediumVaseRaw, ComponentType.Handle2, ComponentType.MediumVase2HandleRaw);
        CreateCombinationRecipe(ComponentType.MediumVaseRaw, ComponentType.Handle3, ComponentType.MediumVase3HandleRaw);

        //Big Vases
        CreateCombinationRecipe(ComponentType.BigVaseRaw, ComponentType.Handle1, ComponentType.BigVase1HandleRaw);
        CreateCombinationRecipe(ComponentType.BigVaseRaw, ComponentType.Handle2, ComponentType.BigVase2HandleRaw);
        CreateCombinationRecipe(ComponentType.BigVaseRaw, ComponentType.Handle3, ComponentType.BigVase3HandleRaw);
        #endregion

        #region InteractionDescriptions
        //Table
        CreateInteractionRecipe(WorkstationType.Table, ComponentType.WetClay, 2, ComponentType.KneadedClay);
        CreateInteractionRecipe(WorkstationType.Table, ComponentType.KneadedClay, 2, ComponentType.Handle3);

        //Grinding Table
        CreateInteractionRecipe(WorkstationType.GrindingTable, ComponentType.RedRocks, 3, ComponentType.RedRocksGravel);
        CreateInteractionRecipe(WorkstationType.GrindingTable, ComponentType.BlackRocks, 3, ComponentType.BlackRocksGravel);

        //Potters Wheel
        CreateInteractionRecipe(WorkstationType.Potterswheel, ComponentType.KneadedClay, 2, ComponentType.PlateRaw);
        CreateInteractionRecipe(WorkstationType.Potterswheel, ComponentType.KneadedClay2, 4, ComponentType.MediumVaseRaw);
        CreateInteractionRecipe(WorkstationType.Potterswheel, ComponentType.KneadedClay3, 8, ComponentType.BigVaseRaw);

        #region OvenInteractionDescriptions
        //Plates
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.PlateRaw, 4, ComponentType.PlateBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.PlateRawBlack, 4, ComponentType.PlateBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.PlateRawRed, 4, ComponentType.PlateBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate1HandleRaw, 4, ComponentType.Plate1HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate1HandleRawBlack, 4, ComponentType.Plate1HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate1HandleRawRed, 4, ComponentType.Plate1HandleBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate2HandleRaw, 4, ComponentType.Plate2HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate2HandleRawBlack, 4, ComponentType.Plate2HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.Plate2HandleRawRed, 4, ComponentType.Plate2HandleBakedRed);

        //Medium Vases
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVaseRaw, 6, ComponentType.MediumVaseBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVaseRawBlack, 6, ComponentType.MediumVaseBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVaseRawRed, 6, ComponentType.MediumVaseBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase1HandleRaw, 6, ComponentType.MediumVase1HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase1HandleRawBlack, 6, ComponentType.MediumVase1HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase1HandleRawRed, 6, ComponentType.MediumVase1HandleBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase2HandleRaw, 6, ComponentType.MediumVase2HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase2HandleRawBlack, 6, ComponentType.MediumVase2HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase2HandleRawRed, 6, ComponentType.MediumVase2HandleBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase3HandleRaw, 6, ComponentType.MediumVase3HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase3HandleRawBlack, 6, ComponentType.MediumVase3HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.MediumVase3HandleRawRed, 6, ComponentType.MediumVase3HandleBakedRed);

        //Big Vases
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVaseRaw, 8, ComponentType.BigVaseBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVaseRawBlack, 8, ComponentType.BigVaseBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVaseRawRed, 8, ComponentType.BigVaseBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase1HandleRaw, 8, ComponentType.BigVase1HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase1HandleRawBlack, 8, ComponentType.BigVase1HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase1HandleRawRed, 8, ComponentType.BigVase1HandleBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase2HandleRaw, 8, ComponentType.BigVase2HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase2HandleRawBlack, 8, ComponentType.BigVase2HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase2HandleRawRed, 8, ComponentType.BigVase2HandleBakedRed);

        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase3HandleRaw, 8, ComponentType.BigVase3HandleBaked);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase3HandleRawBlack, 8, ComponentType.BigVase3HandleBakedBlack);
        CreateInteractionRecipe(WorkstationType.Oven, ComponentType.BigVase3HandleRawRed, 8, ComponentType.BigVase3HandleBakedRed);
        #endregion
        #endregion

        #region StackingDescriptions
        //Stacking for Handles
        CreatePickupStackingItem(ComponentType.Handle1, ComponentType.Handle1, ComponentType.Unknown, ComponentType.Handle2);
        CreatePickupStackingItem(ComponentType.Handle1, ComponentType.Handle2, ComponentType.Unknown, ComponentType.Handle3);
        CreatePickupStackingItem(ComponentType.Handle2, ComponentType.Handle1, ComponentType.Handle1, ComponentType.Handle2);
        CreatePickupStackingItem(ComponentType.Handle2, ComponentType.Unknown, ComponentType.Handle1, ComponentType.Handle1);
        CreatePickupStackingItem(ComponentType.Handle3, ComponentType.Unknown, ComponentType.Handle2, ComponentType.Handle1);

        //Fillup for Handles
        CreatePickupStackingItem(ComponentType.Handle1, ComponentType.Handle3, ComponentType.Handle3, ComponentType.Handle1);
        CreatePickupStackingItem(ComponentType.Handle2, ComponentType.Handle3, ComponentType.Handle3, ComponentType.Handle2);
        CreatePickupStackingItem(ComponentType.Handle2, ComponentType.Handle2, ComponentType.Handle3, ComponentType.Handle1);

        //Stacking for Clay
        CreatePickupStackingItem(ComponentType.KneadedClay, ComponentType.KneadedClay, ComponentType.Unknown, ComponentType.KneadedClay2);
        CreatePickupStackingItem(ComponentType.KneadedClay, ComponentType.KneadedClay2, ComponentType.Unknown, ComponentType.KneadedClay3);
        CreatePickupStackingItem(ComponentType.KneadedClay2, ComponentType.KneadedClay, ComponentType.KneadedClay, ComponentType.KneadedClay2);
        CreatePickupStackingItem(ComponentType.KneadedClay2, ComponentType.Unknown, ComponentType.KneadedClay, ComponentType.KneadedClay);
        CreatePickupStackingItem(ComponentType.KneadedClay3, ComponentType.Unknown, ComponentType.KneadedClay2, ComponentType.KneadedClay);

        //Fillup for Clay
        CreatePickupStackingItem(ComponentType.KneadedClay, ComponentType.KneadedClay3, ComponentType.KneadedClay3, ComponentType.KneadedClay);
        CreatePickupStackingItem(ComponentType.KneadedClay2, ComponentType.KneadedClay3, ComponentType.KneadedClay3, ComponentType.KneadedClay2);
        CreatePickupStackingItem(ComponentType.KneadedClay2, ComponentType.KneadedClay2, ComponentType.KneadedClay3, ComponentType.KneadedClay);
        #endregion

        #region TwoBaseInteractionDescriptions
        //Plates
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRaw, ComponentType.RedSlip, 2, ComponentType.PlateRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRaw, ComponentType.BlackSlip, 2, ComponentType.PlateRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRawRed, ComponentType.BlackSlip, 2, ComponentType.PlateRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRawBlack, ComponentType.RedSlip, 2, ComponentType.PlateRawRed);
        
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRawBlack, ComponentType.BlackSlip, 2, ComponentType.PlateRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.PlateRawRed, ComponentType.RedSlip, 2, ComponentType.PlateRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRaw, ComponentType.RedSlip, 2, ComponentType.Plate1HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRaw, ComponentType.BlackSlip, 2, ComponentType.Plate1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRawRed, ComponentType.BlackSlip, 2, ComponentType.Plate1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRawBlack, ComponentType.RedSlip, 2, ComponentType.Plate1HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRawBlack, ComponentType.BlackSlip, 2, ComponentType.Plate1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate1HandleRawRed, ComponentType.RedSlip, 2, ComponentType.Plate1HandleRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRaw, ComponentType.RedSlip, 2, ComponentType.Plate2HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRaw, ComponentType.BlackSlip, 2, ComponentType.Plate2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRawRed, ComponentType.BlackSlip, 2, ComponentType.Plate2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRawBlack, ComponentType.RedSlip, 2, ComponentType.Plate2HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRawBlack, ComponentType.BlackSlip, 2, ComponentType.Plate2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.Plate2HandleRawRed, ComponentType.RedSlip, 2, ComponentType.Plate2HandleRawRed);


        //Medium Vases
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRaw, ComponentType.RedSlip, 3, ComponentType.MediumVaseRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRaw, ComponentType.BlackSlip, 3, ComponentType.MediumVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRawRed, ComponentType.BlackSlip, 3, ComponentType.MediumVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRawBlack, ComponentType.RedSlip, 3, ComponentType.MediumVaseRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRawBlack, ComponentType.BlackSlip, 3, ComponentType.MediumVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVaseRawRed, ComponentType.RedSlip, 3, ComponentType.MediumVaseRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRaw, ComponentType.RedSlip, 3, ComponentType.MediumVase1HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRaw, ComponentType.BlackSlip, 3, ComponentType.MediumVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.MediumVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.MediumVase1HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.MediumVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase1HandleRawRed, ComponentType.RedSlip, 3, ComponentType.MediumVase1HandleRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRaw, ComponentType.RedSlip, 3, ComponentType.MediumVase2HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRaw, ComponentType.BlackSlip, 3, ComponentType.MediumVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.MediumVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.MediumVase2HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.MediumVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase2HandleRawRed, ComponentType.RedSlip, 3, ComponentType.MediumVase2HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRaw, ComponentType.RedSlip, 3, ComponentType.MediumVase3HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRaw, ComponentType.BlackSlip, 3, ComponentType.MediumVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.MediumVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.MediumVase3HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.MediumVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.MediumVase3HandleRawRed, ComponentType.RedSlip, 3, ComponentType.MediumVase3HandleRawRed);


        //Medium Vases
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRaw, ComponentType.RedSlip, 3, ComponentType.BigVaseRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRaw, ComponentType.BlackSlip, 3, ComponentType.BigVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRawRed, ComponentType.BlackSlip, 3, ComponentType.BigVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRawBlack, ComponentType.RedSlip, 3, ComponentType.BigVaseRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRawBlack, ComponentType.BlackSlip, 3, ComponentType.BigVaseRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVaseRawRed, ComponentType.RedSlip, 3, ComponentType.BigVaseRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRaw, ComponentType.RedSlip, 3, ComponentType.BigVase1HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRaw, ComponentType.BlackSlip, 3, ComponentType.BigVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.BigVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.BigVase1HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.BigVase1HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase1HandleRawRed, ComponentType.RedSlip, 3, ComponentType.BigVase1HandleRawRed);


        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRaw, ComponentType.RedSlip, 3, ComponentType.BigVase2HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRaw, ComponentType.BlackSlip, 3, ComponentType.BigVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.BigVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.BigVase2HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.BigVase2HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase2HandleRawRed, ComponentType.RedSlip, 3, ComponentType.BigVase2HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRaw, ComponentType.RedSlip, 3, ComponentType.BigVase3HandleRawRed);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRaw, ComponentType.BlackSlip, 3, ComponentType.BigVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRawRed, ComponentType.BlackSlip, 3, ComponentType.BigVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRawBlack, ComponentType.RedSlip, 3, ComponentType.BigVase3HandleRawRed);

        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRawBlack, ComponentType.BlackSlip, 3, ComponentType.BigVase3HandleRawBlack);
        CreateTwoBaseInteractionRecipe(WorkstationType.PaintersTable, ComponentType.BigVase3HandleRawRed, ComponentType.RedSlip, 3, ComponentType.BigVase3HandleRawRed);
        #endregion


    }

    private void CreateCombinationRecipe(ComponentType type1, ComponentType type2, ComponentType result)
    {
        if ((int)type1 > (int)type2)
            (type2, type1) = (type1, type2);

        combinationRecipes[new Tuple<ComponentType, ComponentType>(type1, type2)] = result;
    }

    private void CreateInteractionRecipe(WorkstationType workstationType, ComponentType componentType, float duration, ComponentType result)
    {
        interactionRecipes[new Tuple<WorkstationType, ComponentType>(workstationType, componentType)] =
            new Tuple<float, ComponentType>(duration, result);
    }

    private void CreatePickupStackingItem(ComponentType typeOnWorkstation, ComponentType typeInHands, ComponentType typeOnWorkstationAfterwards, ComponentType typeInHandsAfterwards)
    {
        pickupStackingItems[new Tuple<ComponentType, ComponentType>(typeOnWorkstation, typeInHands)] =
            new Tuple<ComponentType, ComponentType>(typeOnWorkstationAfterwards, typeInHandsAfterwards);
    }

    private void CreateTwoBaseInteractionRecipe(WorkstationType workstationType, ComponentType type1, ComponentType type2, float duration, ComponentType result)
    {
        if ((int)type1 > (int)type2)
            (type2, type1) = (type1, type2);

        twoBaseInteractionRecipes[new Tuple<WorkstationType, ComponentType, ComponentType>(workstationType, type1, type2)] =
           new Tuple<float, ComponentType>(duration, result);
    }



    public GameObject GetPrefabOfComponentType(ComponentType type)
    {
        if (type == ComponentType.Unknown)
            return null;

        switch (type)
        {
            case ComponentType.Clay:
                return clayPrefab;
            case ComponentType.WetClay:
                return wetClayPrefab;
            case ComponentType.Water:
                return waterPrefab;
            case ComponentType.KneadedClay:
                return kneadedClayPrefab;
            case ComponentType.KneadedClay2:
                return kneadedClay2Prefab;
            case ComponentType.KneadedClay3:
                return kneadedClay3Prefab;
            case ComponentType.PlateRaw:
                return plateRawPrefab;
            case ComponentType.PlateBaked:
                return plateBakedPrefab;
            case ComponentType.Handle3:
                return handle3Prefab;
            case ComponentType.Handle2:
                return handle2Prefab;
            case ComponentType.Handle1:
                return handle1Prefab;
            case ComponentType.BlackRocks:
                return blackRocksPrefab;
            case ComponentType.BlackRocksGravel:
                return blackRocksGravelPrefab;
            case ComponentType.RedRocks:
                return redRocksPrefab;
            case ComponentType.RedRocksGravel:
                return redRocksGravelPrefab;
            case ComponentType.BlackSlip:
                return blackSlipPrefab;
            case ComponentType.RedSlip:
                return redSlipPrefab;
            case ComponentType.ClaySlip:
                return claySlipPrefab;
            case ComponentType.PlateRawRed:
                return plateRawRedPrefab;
            case ComponentType.PlateRawBlack:
                return plateRawBlackPrefab;
            case ComponentType.PlateBakedRed:
                return plateBakedRedPrefab;
            case ComponentType.PlateBakedBlack:
                return plateBakedBlackPrefab;
            case ComponentType.Unknown:
                return null;
            case ComponentType.Plate1HandleRaw:
                return plate1HandleRawPrefab;
            case ComponentType.Plate1HandleRawRed:
                return plate1HandleRawRedPrefab;
            case ComponentType.Plate1HandleRawBlack:
                return plate1HandleRawBlackPrefab;
            case ComponentType.Plate2HandleRaw:
                return plate2HandleRawPrefab;
            case ComponentType.Plate2HandleRawRed:
                return plate2HandleRawRedPrefab;
            case ComponentType.Plate2HandleRawBlack:
                return plate2HandleRawBlackPrefab;
            case ComponentType.Plate1HandleBaked:
                return plate1HandleBakedPrefab;
            case ComponentType.Plate1HandleBakedRed:
                return plate1HandleBakedRedPrefab;
            case ComponentType.Plate1HandleBakedBlack:
                return plate1HandleBakedBlackPrefab;
            case ComponentType.Plate2HandleBaked:
                return plate2HandleBakedPrefab;
            case ComponentType.Plate2HandleBakedRed:
                return plate2HandleBakedRedPrefab;
            case ComponentType.Plate2HandleBakedBlack:
                return plate2HandleBakedBlackPrefab;
            case ComponentType.MediumVaseRaw:
                return mediumVaseRawPrefab;
            case ComponentType.MediumVaseRawRed:
                return mediumVaseRawRedPrefab;
            case ComponentType.MediumVaseRawBlack:
                return mediumVaseRawBlackPrefab;
            case ComponentType.MediumVase1HandleRaw:
                return mediumVase1HandleRawPrefab;
            case ComponentType.MediumVase1HandleRawRed:
                return mediumVase1HandleRawRedPrefab;
            case ComponentType.MediumVase1HandleRawBlack:
                return mediumVase1HandleRawBlackPrefab;
            case ComponentType.MediumVase2HandleRaw:
                return mediumVase2HandleRawPrefab;
            case ComponentType.MediumVase2HandleRawRed:
                return mediumVase2HandleRawRedPrefab;
            case ComponentType.MediumVase2HandleRawBlack:
                return mediumVase2HandleRawBlackPrefab;
            case ComponentType.MediumVase3HandleRaw:
                return mediumVase3HandleRawPrefab;
            case ComponentType.MediumVase3HandleRawRed:
                return mediumVase3HandleRawRedPrefab;
            case ComponentType.MediumVase3HandleRawBlack:
                return mediumVase3HandleRawBlackPrefab;
            case ComponentType.BigVaseRaw:
                return bigVaseRawPrefab;
            case ComponentType.BigVaseRawRed:
                return bigVaseRawRedPrefab;
            case ComponentType.BigVaseRawBlack:
                return bigVaseRawBlackPrefab;
            case ComponentType.BigVase1HandleRaw:
                return bigVase1HandleRawPrefab;
            case ComponentType.BigVase1HandleRawRed:
                return bigVase1HandleRawRedPrefab;
            case ComponentType.BigVase1HandleRawBlack:
                return bigVase1HandleRawBlackPrefab;
            case ComponentType.BigVase2HandleRaw:
                return bigVase2HandleRawPrefab;
            case ComponentType.BigVase2HandleRawRed:
                return bigVase2HandleRawRedPrefab;
            case ComponentType.BigVase2HandleRawBlack:
                return bigVase2HandleRawBlackPrefab;
            case ComponentType.BigVase3HandleRaw:
                return bigVase3HandleRawPrefab;
            case ComponentType.BigVase3HandleRawRed:
                return bigVase3HandleRawRedPrefab;
            case ComponentType.BigVase3HandleRawBlack:
                return bigVase3HandleRawBlackPrefab;
            case ComponentType.MediumVaseBaked:
                return mediumVaseBakedPrefab;
            case ComponentType.MediumVaseBakedRed:
                return mediumVaseBakedRedPrefab;
            case ComponentType.MediumVaseBakedBlack:
                return mediumVaseBakedBlackPrefab;
            case ComponentType.MediumVase1HandleBaked:
                return mediumVase1HandleBakedPrefab;
            case ComponentType.MediumVase1HandleBakedRed:
                return mediumVase1HandleBakedRedPrefab;
            case ComponentType.MediumVase1HandleBakedBlack:
                return mediumVase1HandleBakedBlackPrefab;
            case ComponentType.MediumVase2HandleBaked:
                return mediumVase2HandleBakedPrefab;
            case ComponentType.MediumVase2HandleBakedRed:
                return mediumVase2HandleBakedRedPrefab;
            case ComponentType.MediumVase2HandleBakedBlack:
                return mediumVase2HandleBakedBlackPrefab;
            case ComponentType.MediumVase3HandleBaked:
                return mediumVase3HandleBakedPrefab;
            case ComponentType.MediumVase3HandleBakedRed:
                return mediumVase3HandleBakedRedPrefab;
            case ComponentType.MediumVase3HandleBakedBlack:
                return mediumVase3HandleBakedBlackPrefab;
            case ComponentType.BigVaseBaked:
                return bigVaseBakedPrefab;
            case ComponentType.BigVaseBakedRed:
                return bigVaseBakedRedPrefab;
            case ComponentType.BigVaseBakedBlack:
                return bigVaseBakedBlackPrefab;
            case ComponentType.BigVase1HandleBaked:
                return bigVase1HandleBakedPrefab;
            case ComponentType.BigVase1HandleBakedRed:
                return bigVase1HandleBakedRedPrefab;
            case ComponentType.BigVase1HandleBakedBlack:
                return bigVase1HandleBakedBlackPrefab;
            case ComponentType.BigVase2HandleBaked:
                return bigVase2HandleBakedPrefab;
            case ComponentType.BigVase2HandleBakedRed:
                return bigVase2HandleBakedRedPrefab;
            case ComponentType.BigVase2HandleBakedBlack:
                return bigVase2HandleBakedBlackPrefab;
            case ComponentType.BigVase3HandleBaked:
                return bigVase3HandleBakedPrefab;
            case ComponentType.BigVase3HandleBakedRed:
                return bigVase3HandleBakedRedPrefab;
            case ComponentType.BigVase3HandleBakedBlack:
                return bigVase3HandleBakedBlackPrefab;
            default:
                Debug.LogError($"Enum {type} does not have a prefab assigned.");
                return null;
        }
    }

    public ComponentType GetRecipeFor(ComponentType type1, ComponentType type2)
    {
        // Order the two components
        if ((int)type1 > (int)type2)
            (type2, type1) = (type1, type2);


        if (combinationRecipes.TryGetValue(new Tuple<ComponentType, ComponentType>(type1, type2), out var result))
            return result;
        else
            return ComponentType.Unknown;
    }

    public Tuple<float, ComponentType> GetInteractionRecipeFor(WorkstationType workstationType, ComponentType componentType)
    {
        if (interactionRecipes.TryGetValue(new Tuple<WorkstationType, ComponentType>(workstationType, componentType), out var result))
            return result;
        else
            return null;
    }

    public Tuple<float, ComponentType> GetTwoBaseInteractionRecipeFor(WorkstationType workstationType,ComponentType type1, ComponentType type2)
    {
        // Order the two components
        if ((int)type1 > (int)type2)
            (type2, type1) = (type1, type2);


        if (twoBaseInteractionRecipes.TryGetValue(new Tuple<WorkstationType, ComponentType, ComponentType>(workstationType,type1, type2), out var result))
            return result;
        else
            return null;
    }



    /// <returns>Tuple of: typeOnWorkstationAfterwards, typeInHandsAfterwards</returns>
    public Tuple<ComponentType, ComponentType> GetPickupDefinitionFor(ComponentType typeOnWorkstation,
        ComponentType typeInHands)
    {
        if (pickupStackingItems.TryGetValue(new Tuple<ComponentType, ComponentType>(typeOnWorkstation, typeInHands),
                out var result))
            return result;
        else
            return null;
    }


    public GameObject GetPrefabOfCombination(ComponentType type1, ComponentType type2)
    {
        return GetPrefabOfComponentType(GetRecipeFor(type1, type2));
    }
}
