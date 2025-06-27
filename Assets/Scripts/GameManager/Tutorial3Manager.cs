using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Tutorial3Manager : NetworkBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> popUps;
    public GameObject PaintersTable;
    public List<GameObject> Tables;
    public List<GameObject> GrindingTables;
    public List<GameObject> players;
    public GameObject Oven;
    public NetworkVariable<int> currentStep = new NetworkVariable<int>();
    public GameObject CustomerManager;

    void Start()
    {
        if(!IsServer)
            return;
        GamePhaseToggle.Instance.TogglePhaseServerRpc();
        GamePhaseToggle.Instance.DisableChangePhaseServerRpc();
        players = GameObject.FindGameObjectsWithTag("Character").ToList();
        GameObject touchButtons = GameObject.Find("TouchButtons");
        if (touchButtons != null)
        {
            var root = touchButtons.GetComponent<UIDocument>().rootVisualElement;
            Button pickupButton = root.Q<Button>("PickupButton");

            pickupButton.RegisterCallback<ClickEvent>((evt) =>
            {
                //All possible cases where E in play mode is expected
                switch (currentStep.Value)
                {
                    case 0:
                        currentStep.Value++;
                        break;
                    case 17:
                        currentStep.Value++;
                        FindAnyObjectByType<UIManager>().ExitLobby();
                        break;

                }
            });
        }
    }


    // Update is called once per frame
    void Update()
    {
      

        for (int i = 0; i < popUps.Count; i++)
        {
            if (i == currentStep.Value)
            {
                popUps[i].SetActive(true);
            }
            else
            {
                popUps[i].SetActive(false);
            }
        }

        if (!IsServer)
            return;

        players = GameObject.FindGameObjectsWithTag("Character").ToList();

        switch (currentStep.Value)
        {
            case 0:
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //Hello, it me again.
                    currentStep.Value++;
            
                }

                break;
            }
            case 1:
            {
                if (IsTypeInScene(ComponentType.PlateRaw))
                {
                    //Raw Plate -> place on the painters table
                    currentStep.Value++;
                }

                break;
            }
            case 2:
            {
                if (PaintersTable.GetComponent<PaintersTable>().GoOnTableVase != null &&
                    PaintersTable.GetComponent<PaintersTable>().GoOnTableVase
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateRaw)
                {
                    //Placed on painters Table ->  Get Red stones from the red
                    currentStep.Value++;
                }

                break;
            }
            case 3:
            {
                if (players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                                     x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>()
                                         .type == ComponentType.RedRocks)) 
                {
                    //Got Red Rocks -> place them on a grinding TAble
                    currentStep.Value++;
                }

                break;
            }
            case 4:
            {
                if (GrindingTables.Any(  x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    x.GetComponent<ManufacturingWorkstation>().GoOnTable
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.RedRocks))
                {
                    //place them on a grinding TAble -> interact to make Gravel
                        currentStep.Value++;
                }

                break;
            }
            case 5:
            {
                if (GrindingTables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                            x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                                .GetComponent<ComponentDescriptor>().type ==
                                            ComponentType.RedRocksGravel))
                {
                    //Red Rocks gravel -> get clay and put on table
                    currentStep.Value++;
                }

                break;
            }

            case 6:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                    x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                        .GetComponent<ComponentDescriptor>().type ==
                                    ComponentType.Clay))
                {
                    //get clay and put on table -> make wet
                        currentStep.Value++;
                }
                break;

            }
            case 7:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                   x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                       .GetComponent<ComponentDescriptor>().type ==
                                   ComponentType.WetClay))
                {
                    //make wet  -> make wetter 

                    currentStep.Value++;
                }

                break;
            }
            case 8:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                    x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                        .GetComponent<ComponentDescriptor>().type ==
                                    ComponentType.ClaySlip))
                {
                        //Clay Slip -> Redslip
                       
                        currentStep.Value++;
                }
                break;
            }
            case 9:
            {
                if (IsTypeInScene(ComponentType.RedSlip))
                {
                    //Red Slip -> painters table 
                    currentStep.Value++;
                }
                break;
            }
            case 10:
            {
                if (PaintersTable.GetComponent<PaintersTable>().GoOnTablePaint != null &&
                    PaintersTable.GetComponent<PaintersTable>().GoOnTablePaint
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.RedSlip)
                {
                    //Painters TAble -> painted Vase

                    currentStep.Value++;
                }
                break;
            }
            case 11:
            {
                if (PaintersTable.GetComponent<PaintersTable>().GoOnTableVase != null &&
                    PaintersTable.GetComponent<PaintersTable>().GoOnTableVase
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateRawRed)
                {
                    //Painted Vase ->Baked Painted Vase

                    currentStep.Value++;
                }
                break;
            }
            case 12:
            {
                if (Oven.GetComponent<Oven>().GoInOven != null &&
                    Oven.GetComponent<Oven>().GoInOven
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateBakedRed)
                {
                    //Baked paitned Vase -> You are finished. Place your things 

                    currentStep.Value++;
                    CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);
                    }
                break;
            }
            case 13:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 14:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null)
                {
                    CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);
                        currentStep.Value++;

                }

                break;
            }
            case 15:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 16:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null)
                {

                    currentStep.Value++;

                }

                break;
            }
            case 17:
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentStep.Value++;
                    FindAnyObjectByType<UIManager>().ExitLobby();
                }

                break;
            }
        }
    }

    public bool IsTypeInScene(ComponentType type)
    {
        return Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                               x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                   .GetComponent<ComponentDescriptor>().type == type)
               || players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                                   x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>()
                                       .type == type)
               || PaintersTable.GetComponent<PaintersTable>().GoOnTableVase != null &&
                 PaintersTable.GetComponent<PaintersTable>().GoOnTableVase
                                             .GetComponent<ComponentDescriptor>().type == type 
               || PaintersTable.GetComponent<PaintersTable>().GoOnTablePaint != null &&
               PaintersTable.GetComponent<PaintersTable>().GoOnTablePaint
                   .GetComponent<ComponentDescriptor>().type == type
                || GrindingTables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                              x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                                  .GetComponent<ComponentDescriptor>().type == type);
    }
}
