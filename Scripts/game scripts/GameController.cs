﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.SceneManagement;
/// <summary>
/// Used to control alot of the game, create objects for rooms, player, minigames, characters, menus
/// </summary>
public class GameController : MonoBehaviour
{


    private Rigidbody2D mCharacter;
    private Vector3 roomLeftPos;
    private Vector3 roomRightPos;
    private Rigidbody2D leftPoint;
    private Rigidbody2D rightPoint;
    private Button useButton;
    private Button inventoryButton;
    private ConsoleInput consoleInput;
    private Sprite background;
    private Vector3 startPos;
    private Vector3 returnPos;
    private Vector3 nextRoomPos;
    private Player player;
    private List<Rooms> rooms;
    private Keypad keypad;
    private CanvasGroup positionCanvas;
    private Text positionText;
    private Text dialogueText;
    private ScrollText scrollText;
    private Inventory inventory;
    private LightsoffGame lightsoffGame;
    private Options options;
    private Pipegame pipegame;
    private Audio sound;
    private CanvasGroup key1Start;
    private CanvasGroup key1Sewer;
    private Button specialRoomExit;
    private Renderer specialRoomExitRend;
    private Button clogCleaner;
    private Button yellowkey;

    private bool use = false;
    private bool loop = false;//f2 menu loop
    private bool inventoryOpen = false;
    private bool sewersFirstEntry = true; //used to move the character to the right spot when entering sewers
    private bool usedClog = false; //check if the player has used clog remover on the toilet
    private bool pipeGameTextEvent1 = true; //text event flags for sewers & pipegame, so they won't loop forever
    private bool pipeGameTextEvent2 = true;
    private bool firstFlush = true;
    private bool hasClogcleaner = false;
    private bool hasYellowkey = false;

    List<GameObject> colliderList;
    GameObject room1;
    GameObject room2;
    GameObject room3;
    GameObject room4;
    GameObject room5;
    GameObject room6;
    GameObject room7;
    GameObject room8;
    GameObject room9;

    //inventory & item stuff
    RawImage itemSlot1;
    RawImage itemSlot2;
    RawImage itemSlot3;
    Item itemKeyYellow;
    Item itemKeyBlue;
    Item itemClogcleaner;

    string colliderName;

    // Use this for initialization
    void Start()
    {
        //android settings
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        //find game objects
        mCharacter = GameObject.Find("MCharacter").GetComponent<Rigidbody2D>();
        useButton = GameObject.Find("useButton").GetComponent<Button>();
        positionCanvas = GameObject.Find("PositionCanvas").GetComponent<CanvasGroup>();
        positionText = GameObject.Find("PositionText").GetComponent<Text>();
        dialogueText = GameObject.Find("DialogueText").GetComponent<Text>();
        inventoryButton = GameObject.Find("InventoryButton").GetComponent<Button>();
        itemSlot1 = GameObject.Find("ItemSlot1").GetComponent<RawImage>();
        itemSlot2 = GameObject.Find("ItemSlot2").GetComponent<RawImage>();
        itemSlot3 = GameObject.Find("ItemSlot3").GetComponent<RawImage>();
        key1Start = GameObject.Find("Key1StartCanvas").GetComponent<CanvasGroup>();
        key1Sewer = GameObject.Find("Key1SewerCanvas").GetComponent<CanvasGroup>();
        specialRoomExit = GameObject.Find("SpecialRoomExitButton").GetComponent<Button>();
        clogCleaner = GameObject.Find("ClogCleaner").GetComponent<Button>();
        yellowkey = GameObject.Find("YellowKey").GetComponent<Button>();

        colliderList = new List<GameObject>();

        colliderList.Add(room1 = GameObject.Find("Room1"));
        colliderList.Add(room2 = GameObject.Find("Room2"));
        colliderList.Add(room3 = GameObject.Find("Room3"));
        colliderList.Add(room4 = GameObject.Find("Room4"));
        colliderList.Add(room5 = GameObject.Find("Room5"));
        colliderList.Add(room6 = GameObject.Find("Room6"));
        colliderList.Add(room7 = GameObject.Find("Room7"));
        colliderList.Add(room8 = GameObject.Find("Room8"));
        colliderList.Add(room9 = GameObject.Find("Room9"));
        
        foreach(GameObject obj in colliderList)
        {
            obj.SetActive(false);
        }

        //hide key in the sewers 
        key1Sewer.alpha = 0f;

        //hide special room exit button by default, only show it in the special rooms
        specialRoomExit.gameObject.SetActive(true);
        



        //add listeners
        useButton.onClick.AddListener(() => Use());
        inventoryButton.onClick.AddListener(() => ToggleInventory());
        specialRoomExit.onClick.AddListener(() => SpecialRoomExit());
        clogCleaner.onClick.AddListener(() => CollectCleaner());
        yellowkey.onClick.AddListener(() => CollectYellowKey());


        //create class objects
        rooms = new List<Rooms>();
        scrollText = new ScrollText();
        keypad = new Keypad(scrollText);
        inventory = new Inventory();
        lightsoffGame = new LightsoffGame();
        options = new Options();
        pipegame = new Pipegame();

        //send the games to ConsoleInput for the toggle commands
        consoleInput = new ConsoleInput(lightsoffGame, pipegame);
        
        itemKeyYellow = new Item("keyYellow", "item_key", itemSlot1);
        itemClogcleaner = new Item("clogcleaner", "clog_cleaner", itemSlot2);
        itemKeyBlue = new Item("keyBlue", "item_key2", itemSlot3);

        //create rooms and add to list
        CreateRooms();

        //setup previous and next rooms
        SetupRooms();


        //create player, give it a populated rooms list and the character
        player = new Player(rooms, mCharacter);
        //create audio object, pass the player to it because it uses some room locations as conditions
        sound = new Audio(player);

        //set a player for the consoleInput class, for some commands
        consoleInput.Player = player;

        // room positions for character
        startPos = new Vector3(-150, -54, 100);     
        nextRoomPos = new Vector3(-295, -54, 100);  
        returnPos = new Vector3(295, -54, 100);     



        //player.Position = startPos; // set player starting position
        player.SetScale(150); //set player default scale
        player.SetLocation(rooms[0]); //set player starting location

    }
    /// <summary>
    /// use button state, true = pressed
    /// </summary>
    void Use()
    {
        use = true;
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        //update collider name for trigger methods
        colliderName = collider.name;

        //check if player is in the start room
        if (player.LocationName == "start")
        {
            StartRoomTriggers();
        }

        //check if player is in the cell room
        else if(player.LocationName == "cellRoom")
        {
            CellRoomTriggers();
        }

        //check if player is in the exit room (fourth room)
        else if(player.LocationName == "exitRoom")
        {
            
            ExitRoomTriggers();
        }
        
        //fifth room securityDoor
        else if(player.LocationName == "securityDoor")
        {
            SecurityDoorTriggers();
        }

        //sixth room controlRoom
        else if(player.LocationName == "controlRoom")
        {
            ControlRoomTriggers();
        }

        //seventh room maintenanceRoom
        else if(player.LocationName == "maintenanceRoom")
        {
            MaintenanceRoomTriggers();
        }

        //ninth room sewers
        else if(player.LocationName == "sewers")
        {
            SewerTriggers();
        }
        
        
    }
    /// <summary>
    /// set using to false when you leave any trigger(just incase)
    /// more detailed explanation in OnTriggerEnter2D docs
    /// </summary>
    void OnTriggerExit2D()
    {
        use = false;
    }

    /// <summary>
    /// set using to false when you enter any trigger(just in case)
    /// if you click use where there isn't any interactable objects use will stay true, but right as you enter an object you want to interact with, it will set use to false
    /// so you don't accidentally pick up anything or start a game
    /// </summary>
    void OnTriggerEnter2D()
    {
        use = false;
    }

    /// <summary>
    /// open or close inventory
    /// </summary>
    void ToggleInventory()
    {
        if (inventoryOpen == false)
        {
            inventory.ShowInventory();
            inventoryOpen = true;
        }
        else
        {
            inventory.HideInventory();
            inventoryOpen = false;
        }
    }

    /// <summary>
    /// add clog cleaner to players inventory and hide the button
    /// </summary>
    void CollectCleaner()
    {
        inventory.AddItem(itemClogcleaner);
        clogCleaner.gameObject.SetActive(false);

    }

    /// <summary>
    /// add yellow key to players inventory and hide the button
    /// </summary>
    void CollectYellowKey()
    {
        inventory.AddItem(itemKeyYellow);
        yellowkey.gameObject.SetActive(false);
    }

    /// <summary>
    /// exit out of special rooms, like the cleaning closet or control room
    /// </summary>
    void SpecialRoomExit()
    {
        GotoMouse.Move = false;
        if(player.LocationName == "controlRoom")
        {
            player.Position = new Vector3(-145, mCharacter.position.y, 100);
        }

        if (player.LocationName == "cleaningCloset")
        {
            mCharacter.gameObject.SetActive(true);
            player.Position = new Vector3(3, mCharacter.position.y, 100);
        }

        player.SetLocation(player.PreviousLocation);
    }

    /// <summary>
    /// set up previous and last rooms for all rooms in the list
    /// </summary>
    private void SetupRooms()
    {
        int i = 0;

        foreach (Rooms room in rooms)
        {
            if (i >= 1)
            {
                room.PreviousRoom = rooms[i - 1];
            }
            if (i < (rooms.Count - 1))
            {
                room.NextRoom = rooms[i + 1];
            }
            i++;
        }

        //config special case rooms

        //room 5, next room is room 7 "maintenace room" and vice versa
        rooms[4].NextRoom = rooms[6];
        rooms[6].PreviousRoom = rooms[4];

        //room 6 "control room" doesn't have a next room, only the previous one, "security door"
        rooms[5].NextRoom = null;

        //room 7 "maintenance room" has 2 next rooms, sort of, it has two enterances
        rooms[6].NextRoom = null;

        //cleaning closet only has a previous room, "maintenance room"
        rooms[7].NextRoom = null;

        //set special room for cleaning closet and sewer
        rooms[7].PreviousRoom = rooms[6];
        rooms[8].PreviousRoom = rooms[6];
    }

    
    // Update is called once per frame
    void Update()
    {
        
        //debugging key commands
        CheckDebuggingKeyCommands();

        //check if we want to update dialogue text (is thread running)
        if(scrollText.ThreadStatus == false)
        {
            dialogueText.text = scrollText.Text;
        }
        
        //hide dialogue box if thread is finished scrolling
        if (scrollText.ThreadStatus == true)
        {
            scrollText.HideDialogue();
        }

        //debugging tool, loop is toggled true if you press F2
        //toggled in CheckDebuggingKeyCommands()
        if (loop)
        {
            positionCanvas.alpha = 1f; //set info textfield visible and print players coordinates
            //display player position, click to move loop statuses and if you have solved minigames or not
            positionText.text = "" + player.Position + GotoMouse.Move + GotoMouse.MenuOpen + "\nKeypad: " + Player.KeypadSolved + "\nLightsout: " + Player.LightsoutSolved + "\nPipegame: " + Player.PipegameSolved + "\nclicks: " + GotoMouse.numberOfClicks + "\nUse: " + use + "\nColl: " + colliderName + "\nLoc: " + player.LocationName;
        }

        //check if we need to change the room, based on character coordinates
        CheckRoomSwitch();

        //artificial boundary for the first and last rooms, so the character can't leave the screen
        ArtificialBoundaries();

        //play or stop room based sounds
        RoomSounds();
        
        //check if you have solved minigames and show dialogues based on that
        SolvedGames();

        //first room events
        StartRoomEvents();

        //cleaning closet events
        CleaningClosetEvents();

        //events for the sewer room
        SewerEvents();

        //enable special exit button for special case rooms
        EnableSpecialExitButton();

        //enable room specific colliders
        EnableColliders();

        //dont let the character go out of bounds in special rooms
        SpecialRoomBoundaries();

        

        //use = false; //leave as last entry, toggle use off if you didn't actually use anything
    }
    /// <summary>
    /// check if you are pressing any of the function keys
    /// </summary>
    private void CheckDebuggingKeyCommands()
    {
        if (Input.GetKey("f1"))
        {
            lightsoffGame.ShowGame();
        }

        if (Input.GetKeyDown("f2"))
        {
            loop = true; // used by position textfield loop
        }

        if (Input.GetKeyDown("f3"))
        {
            usedClog = true; //debug pipegame events
            Player.LightsoutSolved = true;
            Player.KeypadSolved = true;
        }

        if (Input.GetKeyDown("f4"))
        {
            inventory.AddItem(itemClogcleaner);
            inventory.AddItem(itemKeyBlue);
            inventory.AddItem(itemKeyYellow);
        }
        
    }

    /// <summary>
    /// check if character wants to change rooms (=> is at the edge of the screen)
    /// </summary>
    private void CheckRoomSwitch()
    {
        //enter previous room if you are at the left side of the screen
        if (mCharacter.position.x <= -305)
        {
            GotoMouse.Move = false;//set click to move loop to false;

            if (player.PreviousLocation != null)//check if previous room exists
            {
                player.Position = returnPos; //sprite position in the room
                player.SetLocation(player.PreviousLocation); //actual room location
            }
        }

        //enter next room if you are at the right side of the screen
        if (mCharacter.position.x >= 305)
        {
            GotoMouse.Move = false;//set click to move loop to false

            if (player.NextLocation != null) //check if next room exists
            {
                player.Position = nextRoomPos;//sprite position in the room
                player.SetLocation(player.NextLocation);//actual room location
            }
        }
    }

    /// <summary>
    /// create artificial boundaries, i.e if nextroom is null dont let the character go any further
    /// </summary>
    private void ArtificialBoundaries()
    {
        //exclude sewers, special case room - see SewerFunctions()
        if (!(player.LocationName == "sewers"))
        {
            if (player.Location.NextRoom == null && mCharacter.position.x > 300)
            {
                GotoMouse.Move = false;
                player.Position = new Vector3(299, -54, 100);
                scrollText.Text = "I can't go that way";
                scrollText.StartScrolling();
            }

            if (player.Location.PreviousRoom == null && mCharacter.position.x < -300)
            {
                GotoMouse.Move = false;
                player.Position = new Vector3(-299, -54, 100);
                scrollText.Text = "I can't go that way";
                scrollText.StartScrolling();
            }
        }
    }

    /// <summary>
    /// Events for securityDoor room, room 5
    /// </summary>
    private void SecurityDoorTriggers()
    {
        //keypad minigame toggling  -- check if character is in the right position, and which room you are in
        if (colliderName == "Keypad" && use)
        {
            keypad.Togglekeypad();
        }

        //check if keypad is solved and you want to enter "control room"
        if (colliderName == "Room5Door" && use)
        {
            if (Player.KeypadSolved == true)
            {
                player.SetLocation(rooms[5]);
                sound.ElectricDoorOpenAudio();
            }
            else if (Player.KeypadSolved == false)
            {
                scrollText.Text = "It's locked.";
                scrollText.StartScrolling();
            }

        }
        use = false;
    }

    /// <summary>
    /// play or stop room based sounds
    /// </summary>
    private void RoomSounds()
    {
        //play room sounds
        if (player.Location.RoomName != "controlRoom" || player.Location.RoomName != "securityDoor")
        {
            sound.AmbientAudio();
        }

        if (player.Location.RoomName == "start")
        {
            sound.WaterdropAudio();
        }
        if (player.Location.RoomName == "cellRoom")
        {
            sound.WaterdropLowAudio();
        }
        if (player.Location.RoomName == "securityDoor")
        {
            sound.ControlRoomAudio();
        }
    
        
        
        if (player.Location.RoomName == "largeCellRoom")
        {

            sound.PrisoncellDoorAudio();
            
        }
        sound.stopAudio();

    }
    /// <summary>
    /// 
    /// check if you have solved minigames and show a dialogue based on that
    /// </summary>
    private void SolvedGames()
    {
        //check if keypad has been solved and show a dialogue
        if (keypad.Solved)
        {
            keypad.Solved = false; // set it to false so it won't run again, Player keeps track of solved also
            StartCoroutine(keypad.WaitForSecs(1.5f));
            scrollText.Text = "Access granted.";
            scrollText.StartScrolling();
        }

        //check if lightsoffGame has been solved, show a dialogue
        if (lightsoffGame.Solved)
        {
            lightsoffGame.Solved = false;
            StartCoroutine(lightsoffGame.WaitForSecs(1.5f));
            scrollText.Text = "Solved lights out game(PH).";
            scrollText.StartScrolling();
        }


    }

    /// <summary>
    /// create rooms and add them to the room list
    /// </summary>
    private void CreateRooms()
    {
        //create rooms and add to list
        rooms.Add(new Rooms("start", "room1"));
        rooms.Add(new Rooms("cellRoom", "room2"));
        rooms.Add(new Rooms("largeCellRoom", "room3"));
        rooms.Add(new Rooms("exitRoom", "room4"));
        rooms.Add(new Rooms("securityDoor", "room5"));
        rooms.Add(new Rooms("controlRoom", "room6"));
        rooms.Add(new Rooms("maintenanceRoom", "room7"));
        rooms.Add(new Rooms("cleaningCloset", "room8"));
        rooms.Add(new Rooms("sewers", "room9"));
    }

    /// <summary>
    /// handle maintenance room events
    /// </summary>
    private void MaintenanceRoomTriggers()
    {
        //check if you are at the cleaning room door in maintenanceroom
        if (colliderName == "Room7CleaningDoor" && use)
        {
            //check if you have completed the lightsout game
            if(Player.LightsoutSolved == true)
            {
                player.SetLocation(rooms[7]);
            }
            else//if not display a dialogue
            {
                scrollText.Text = "It's locked.";
                scrollText.StartScrolling();
            }
            use = false;
        }

        //check if you are at the sewer door
        if(colliderName == "Room7SewerDoor" && use)
        {
            player.SetLocation(rooms[8]);
            use = false;
        }
    }

    /// <summary>
    /// handle sewer room events
    /// </summary>
    private void SewerTriggers()
    {

        //see if you want to leave (use stairs)
        if(colliderName == "Room9Stairs" && use)
        {
            player.SetLocation(player.PreviousLocation);
            use = false;
            player.Position = new Vector3(200, -54, 100);
        }

        //pickup the key
        if(colliderName == "Room9Key" && use)
        {
            //add key to inventory if it's shown and hide it in the world
            if(!(key1Sewer.alpha == 0f))
            {
                inventory.AddItem(itemKeyBlue); //already in the list check is in inventory addItem method
                key1Sewer.alpha = 0f;
            }
            
            use = false;
        }
    }

    /// <summary>
    /// sewer events, character position and pipegame 
    /// </summary>
    private void SewerEvents()
    {
        if (player.LocationName == "sewers")
        {
            //position the character slightly lower if you just entered
            if (sewersFirstEntry)
            {
                player.Position = new Vector3(-195, -102, 100);
                sewersFirstEntry = false;
            }
            

            //show the game if you are in the sewers
            pipegame.ShowGame();

            //dont interact with the pipes if you already won the game
            if (Player.PipegameSolved == true)
            {
                pipegame.SetUninteractable();
            }
        }
        else
        {
            pipegame.CloseGame();
            sewersFirstEntry = true; //set first entry back to true when you leave
        }

        //dialogue stuff after you complete the game
        if (Player.PipegameSolved == true)
        {
            //has player used clog remover?
            if (usedClog)
            {
                // see if the text event has triggered already
                if (pipeGameTextEvent1)
                {
                    scrollText.Text = "That's the key from my cell!";
                    scrollText.StartScrolling();
                    pipeGameTextEvent1 = false;
                    key1Sewer.alpha = 1f;
                }
            }
            else
            {
                //see if the text event has triggered already
                if (pipeGameTextEvent2)
                {
                    scrollText.Text = "I think the pipes are clogged, nothing is coming out.";
                    scrollText.StartScrolling();
                    pipeGameTextEvent2 = false;
                }
            }
        }
    }

    /// <summary>
    /// time wasting dialogue for the second room
    /// </summary>
    private void CellRoomTriggers()
    {
        //check if you are near door 1
        if (colliderName == "Room2Door1" && use)
        {
            scrollText.Text = "It's empty.";
            scrollText.StartScrolling();
        }
        //check if you are near door 2
        else if (colliderName == "Room2Door2" && use)
        {
            scrollText.Text = "I think I saw something...                                     \n ...nevermind.";
            scrollText.StartScrolling();
        }
        //check if you are near door 3
        else if (colliderName == "Room2Door3" && use)
        {
            scrollText.Text = "Nothing of value.";
            scrollText.StartScrolling();
        }
        use = false; //toggle use last
    }

    /// <summary>
    /// show or hide key number 1 based on room location and toilet events
    /// </summary>
    private void StartRoomEvents()
    {
        //show or hide key depending on location
        if (player.LocationName == "start")
        {
            if (firstFlush)
            {
                key1Start.alpha = 1f;
            }

        }
        else
        {
            key1Start.alpha = 0f;
        }
    }

    /// <summary>
    /// Trigger events for the first room based on collision and use
    /// </summary>
    private void StartRoomTriggers()
    {

        //search players inventory for clog cleaner
        List<Item> items = inventory.GetItems();
        bool hasCleaner = false;
        foreach (Item item in items)
        {
            if (item.Name == "clogcleaner")
            {
                hasCleaner = true;
            }
        }

        //check if you are near the toilet and presssed use
        if (colliderName == "ToiletCollider" && use)
        {
            //check if you have triggered first use and that you don't have the cleaner yet
            if (!firstFlush && !hasCleaner) 
            {
                scrollText.Text = "I think the pipes might be clogged.";
                scrollText.StartScrolling();
            }

            //check if first use has been triggered
            if (firstFlush)
            {
                scrollText.Text = "The key fell down into the toilet.             \nGreat.";
                scrollText.StartScrolling();
                firstFlush = false;
                key1Start.alpha = 0f;
            }

            //check if the player has clogcleaner and that first use has been triggered
            if (hasCleaner && !firstFlush)
            {
                scrollText.Text = "Used the clog cleaner.";
                scrollText.StartScrolling();
                usedClog = true;
            }
        }
        use = false; //toggle use last
    }

    /// <summary>
    /// exit rooms events & dialogue
    /// </summary>
    private void ExitRoomTriggers()
    {
        if(player.LocationName == "exitRoom")
        {
            //check how many keys you have
            int keyCount = 0;
            List<Item> items = inventory.GetItems();
            foreach(Item item in items)
            {
                if(item.Name == "keyYellow" || item.Name == "keyBlue")
                {
                    keyCount++;
                }
            }
            //check if you are at the door
            if(colliderName == "Room4LargeDoor" && use)
            {
                if (keyCount == 0)
                {
                    scrollText.Text = "The door is locked, there are two keyholes";
                    scrollText.StartScrolling();
                }
                else if(keyCount == 1)
                {
                    scrollText.Text = "It needs a second key.";
                    scrollText.StartScrolling();
                }
                else if(keyCount == 2)
                {
                    Debug.Log("you win the game!");
                    SceneManager.LoadSceneAsync("Credits");
                }
            }
            use = false;
        }
    }

    /// <summary>
    /// handle control room events
    /// </summary>
    private void ControlRoomTriggers()
    {
        
        //check if you are near the table and want to play lightsout
        if(colliderName == "LightsoutGame" && use)
        {
            use = false;
            lightsoffGame.ShowGame();
        }
        
    }

    /// <summary>
    /// handle cleaning closet events
    /// </summary>
    private void CleaningClosetEvents()
    {
        //check if player is in the cleaning closet
        if (player.LocationName == "cleaningCloset")
        {
            //check if player has clogcleaner in his inventory, then hide or show it in the world
            
            List<Item> items = inventory.GetItems();
            foreach(Item item in items)
            {
                if(item.Name == "clogcleaner")
                {
                    hasClogcleaner = true;
                }

                if(item.Name == "item_key")
                {
                    hasYellowkey = true;
                }
            }
            //check if player has the clog cleaner and hide or show the button
            if (hasClogcleaner)
            {
                clogCleaner.gameObject.SetActive(false);
            }
            else
            {
                clogCleaner.gameObject.SetActive(true);
            }

            //check if player has the yellow key and hide or show the button
            if (hasYellowkey)
            {
                yellowkey.gameObject.SetActive(false);
            }
            else
            {
                yellowkey.gameObject.SetActive(true);
            }
            
            //special case room, disable character here
            mCharacter.gameObject.SetActive(false);
        }
        else
        {
            clogCleaner.gameObject.SetActive(false);
            yellowkey.gameObject.SetActive(false);
            mCharacter.gameObject.SetActive(true);
        }
        
    }
    
    /// <summary>
    /// enable specialexitbutton for special case rooms
    /// </summary>
    private void EnableSpecialExitButton()
    {
        if(player.LocationName == "controlRoom" || player.LocationName == "cleaningCloset")
        {
            specialRoomExit.gameObject.SetActive(true);
        }
        else
        {
            specialRoomExit.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// enable room specific colliders
    /// </summary>
    private void EnableColliders()
    {
        bool check;
        //check if you are in room 1
        check = (player.LocationName == "start") ? true : false;
        colliderList[0].SetActive(check);

        //room 2
        check = (player.LocationName == "cellRoom") ? true : false;
        colliderList[1].SetActive(check);

        //room 3
        check = (player.LocationName == "largeCellRoom") ? true : false;
        colliderList[2].SetActive(check);

        //room 4
        check = (player.LocationName == "exitRoom") ? true : false;
        colliderList[3].SetActive(check);

        //room 5
        check = (player.LocationName == "securityDoor") ? true : false;
        colliderList[4].SetActive(check);

        //room 6
        check = (player.LocationName == "controlRoom") ? true : false;
        colliderList[5].SetActive(check);

        //room 7
        check = (player.LocationName == "maintenanceRoom") ? true : false;
        colliderList[6].SetActive(check);

        //room 8
        check = (player.LocationName == "cleaningCloset") ? true : false;
        colliderList[7].SetActive(check);

        //room 9
        check = (player.LocationName == "sewers") ? true : false;
        colliderList[8].SetActive(check);
    }

    /// <summary>
    /// create boundaries for special rooms
    /// </summary>
    private void SpecialRoomBoundaries()
    {
        //create artificial boundaries for sewer room
        if(player.LocationName == "sewers")
        {
            //don't let the character go out of bounds
            if (mCharacter.position.x > 300)
            {
                GotoMouse.Move = false;
                player.Position = new Vector3(299, -102, 100);
            }
            //don't let the character go out of bounds
            if (mCharacter.position.x < -300)
            {
                GotoMouse.Move = false;
                player.Position = new Vector3(-299, -102, 100);
            }
        }

        //dont let character exit the normal way in controlRoom
        if(player.LocationName == "controlRoom")
        {
            if (mCharacter.position.x < -295)
            {
                GotoMouse.Move = false;
                player.Position = new Vector3(-290, -54, 100);
                scrollText.Text = "I can't go that way";
                scrollText.StartScrolling();
            }
        }
    }
}
