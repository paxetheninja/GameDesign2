using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class Tutorial2Manager : NetworkBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> popUps;
    public List<GameObject> Workstations;
    public List<GameObject> Tables;
    public List<GameObject> players;
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
                    case 13:
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
                if (Tables.FindAll(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                        x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                         .GetComponent<ComponentDescriptor>().type ==
                                     ComponentType.KneadedClay).Count >= 2)
                {
                   //Two kneaded clay
                    currentStep.Value++;
                }

                break;
            }
            case 2:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                       x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                            .GetComponent<ComponentDescriptor>().type ==
                                        ComponentType.KneadedClay2) || players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                        x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>().type ==
                        ComponentType.KneadedClay2))
                {
                    //Two kneaded clay
                    currentStep.Value++;
                }

                break;
            }
            case 3:
            {
                if (Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.KneadedClay2 )
                {
                    //Two kneaded clay
                    currentStep.Value++;
                }

                break;
            }
            case 4:
            {
                if (Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.MediumVaseRaw)
                {
                    //Two kneaded clay
                    currentStep.Value++;
                }

                break;
            }
            case 5:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                    x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                        .GetComponent<ComponentDescriptor>().type ==
                                    ComponentType.Handle3))
                {
                    //Make handle
                    currentStep.Value++;
                }

                break;
            }

            case 6:
            {
                if (players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                                     x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>().type ==
                                     ComponentType.Handle2))
                {
                    //Make handle
                    currentStep.Value++;
                }
                break;

            }
            case 7:
            {
                if (Tables.Any(x => x.GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                                    x.GetComponent<ManufacturingWorkstation>().GoOnTable
                                        .GetComponent<ComponentDescriptor>().type ==
                                    ComponentType.MediumVase2HandleRaw)
                    || (Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[0].GetComponent<ManufacturingWorkstation>().GoOnTable
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.MediumVase2HandleRaw
                    ) || players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                    x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.MediumVase2HandleRaw))
                {
                    //Make handle
                    currentStep.Value++;
                }

                break;
            }
            case 8:
            {
                if (Workstations[1].GetComponent<Oven>().GoInOven != null &&
                    Workstations[1].GetComponent<Oven>().GoInOven
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.MediumVase2HandleBaked)
                {
                        //Make handle
                       
                        currentStep.Value++;
                        CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);
                }
                break;
            }
            case 9:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 10:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null)
                {
                    CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);
                        currentStep.Value++;

                }

                break;
            }
            case 11:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 12:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null)
                {
               
                    currentStep.Value++;

                }

                break;
            }
            case 13:
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
}
