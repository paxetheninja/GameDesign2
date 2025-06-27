using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Tutorial1Manager : NetworkBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> popUps;
    public List<GameObject> Workstations;
    public List<GameObject> players;
    public NetworkVariable<int> currentStep = new NetworkVariable<int>();
    public GamePhaseToggle gamePhase;
    public GameObject CustomerManager;

    public NetworkVariable<int> workstationsActive = new NetworkVariable<int>();
    private GameObject handleJoystick;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Character").ToList();

        foreach (GameObject workstation in Workstations)
        {
            workstation.SetActive(false);
            GridManager.Instance.gridObjects.Remove(workstation.GetComponent<GridObject>());
        }
        
     

        workstationsActive.OnValueChanged += (value, newValue) =>
        {
            if (newValue >= 0 && newValue <= Workstations.Count)
            {
                Workstations[newValue - 1].SetActive(true);
                GridManager.Instance.gridObjects.Add(Workstations[newValue-1].GetComponent<GridObject>());
            }
        };

        if (!IsServer)
            return;
        gamePhase.TogglePhaseServerRpc();

        GameObject touchButtons = GameObject.Find("TouchButtons"); 
        handleJoystick  = GameObject.Find("Handle");
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
                    case 26:
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

        if (players.Count == 0)
        {
            players = GameObject.FindGameObjectsWithTag("Character").ToList();
        }

        switch (currentStep.Value)
        {
            case 0:
            {
                if (Input.GetKeyDown(KeyCode.E) ) 
                {
                    //Accept first Popup with E
                    currentStep.Value++;
                  
                }

                break;
            }
            case 1:
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.D) || (handleJoystick != null && handleJoystick.transform.localPosition != Vector3.zero))
                {
                        //Walking around
                        Workstations[0].GetComponent<GridObject>().PlaceServerRpc();
                        currentStep.Value++;

                        workstationsActive.Value++;
                }

                break;
            }
            case 2:
            {
                if (players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand!= null &&
                                     x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>().type ==
                                     ComponentType.Clay))
                {
                    //Spawn the Clay Pit and take out atleast a clay
                    currentStep.Value++;
                }

                break;
            }
            case 3:
            {
                if (players.All(x => x.GetComponent<PlayerActionHandler>().GoInHand == null))
                {
                    //Lay Back all Clays
                    currentStep.Value++;
                }

                break;
            }
            case 4:
            {
                if (gamePhase.IsBuildingPhase)
                {
                    //switch to building phase
                    currentStep.Value++;
                }

                break;
            }
            case 5:
            {
                if (Workstations[0].GetComponent<GridObject>()._netPickedUp.Value)
                {
                    //picked up the pit
                    currentStep.Value++;
                }

                break;
            }
            case 6:
            {
                if (Workstations[0].transform.position != new Vector3(-0.5f, 0, 0.5f) && !Workstations[0].GetComponent<GridObject>()._netPickedUp.Value)
                {
                        //Lay it down on a new postion 

                        Workstations[1].GetComponent<GridObject>().PlaceServerRpc();

                        currentStep.Value++;
                        workstationsActive.Value++;
                }

                break;
            }
            case 7:
            {
                if (Workstations[1].GetComponent<GridObject>()._netPickedUp.Value)
                {
                    //move pickup table
                    currentStep.Value++;
                }

                break;
            }
            case 8:
            {
                if (Workstations[1].transform.position != new Vector3(-0.5f, 0, 0.5f)&& !Workstations[1].GetComponent<GridObject>()._netPickedUp.Value)
                {
                        // lay down table
                        
                        Workstations[2].GetComponent<GridObject>().PlaceServerRpc();

                        currentStep.Value++;
                        workstationsActive.Value++;
                }

                break;
            }
            case 9:
            {
                if (Workstations[2].GetComponent<GridObject>()._netPickedUp.Value)
                {
                    //Pick up Well 
                    currentStep.Value++;
                }

                break;
            }
            case 10:
            {
                if (Workstations[2].transform.rotation != Quaternion.identity)
                {
                    //Roatate the well :)
                    currentStep.Value++;
                }

                break;
            }
            case 11:
            {
                if (Workstations[2].transform.position != new Vector3(-0.5f, 0, 0.5f) && !Workstations[2].GetComponent<GridObject>()._netPickedUp.Value)
                {
                    //Roatate the well :)
                    Workstations[3].GetComponent<GridObject>().PlaceServerRpc();

                    currentStep.Value++;
                    workstationsActive.Value++;
                }

                break;
            }
            case 12:
            {
               if (Workstations[3].transform.position != new Vector3(-1.5f, 0, 0.5f) && !Workstations[3].GetComponent<GridObject>()._netPickedUp.Value)
                {
                    //Pick up Well 
                    currentStep.Value++;
                }

                break;
            }
            case 13:
            {
                if (!gamePhase.IsBuildingPhase)
                {   
                    //Exit building Phase
                    currentStep.Value++;
                }

                break;
            }
            case 14:
            {
                if (Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.Clay)
                {
                    //Take clay and lay it on Table
                    currentStep.Value++;
                }

                break;
            }
            case 15:
            {
                if (players.Any(x => x.GetComponent<PlayerActionHandler>().GoInHand != null &&
                                     x.GetComponent<PlayerActionHandler>().GoInHand.GetComponent<ComponentDescriptor>().type ==
                                     ComponentType.Water))
                {
                    //Go to the well and take some water
                    currentStep.Value++;
                }
                //Skip if user is to fast :)
                else if (Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable
                        .GetComponent<ComponentDescriptor>().type ==
                    ComponentType.WetClay)
                {
                    currentStep.Value += 2;
                }
                break;
            }
            case 16:
            {
                if (Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.WetClay)
                {
                    // Go to the table and cobine them
                    currentStep.Value++;
                }

                break;
            }
            case 17:
            {
                if (Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[1].GetComponent<ManufacturingWorkstation>().GoOnTable.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.KneadedClay)
                {
                    //Knead it
                    currentStep.Value++;
                    Workstations[4].GetComponent<GridObject>().PlaceServerRpc();
                    workstationsActive.Value++;
                    }

                break;
            }
            case 18:
            {
                if (Workstations[4].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[4].GetComponent<ManufacturingWorkstation>().GoOnTable.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.KneadedClay)
                {

                    //Place on Potters Wheel 
                    currentStep.Value++;
                }

                break;
            }
            case 19:
            {
                if (Workstations[4].GetComponent<ManufacturingWorkstation>().GoOnTable != null &&
                    Workstations[4].GetComponent<ManufacturingWorkstation>().GoOnTable.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateRaw)
                {

                        //Wheel add 
                        Workstations[5].GetComponent<GridObject>().PlaceServerRpc();
                        //Turn the wheel 
                        currentStep.Value++;
                        workstationsActive.Value++;

                }

                break;
            }
            case 20:
            {
                if (Workstations[5].GetComponent<Oven>().GoInOven != null &&
                    Workstations[5].GetComponent<Oven>().GoInOven.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateRaw)
                {

              

                    //Turn the wheel 
                    currentStep.Value++;

                }

                break;
            }
            case 21:
            {
                if (Workstations[5].GetComponent<Oven>().GoInOven != null &&
                    Workstations[5].GetComponent<Oven>().GoInOven.GetComponent<ComponentDescriptor>().type ==
                    ComponentType.PlateBaked)
                {


                    //Turn the wheel 
                    currentStep.Value++;
                    CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);

                    }

                break;
            }
            case 22:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 23:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null) 
                {
                    CustomerManager.GetComponent<CustomerManager>().SpawnNextCustomer(300);
                        currentStep.Value++;
                  
                }

                break;
            }
            case 24:
            {
                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer != null && CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer.GetComponent<CustomerProductLogic>().Order.Value != ComponentType.Unknown)
                {
                    currentStep.Value++;
                }

                break;
            }
            case 25:
            {

                if (CustomerManager.GetComponent<CustomerManager>().OrderSpots[0].CurrentCustomer == null)
                {
                    currentStep.Value++;

                }
                break;
            }
            case 26:
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
