using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance{set;get;} //Add for outside reference
    public bool[,] AllowedMoves{set;get;} //Array of playable moves for selected piece

    //Multidiemensional array w/ get&set
    public ChessPiece[,] ChessPieces{set;get;}
    public ChessPiece selectedChessPiece{set;get;} //currently selected piece

    //Tile size and offset, current selected tile (mouse hover)
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;
    private int selectionX = -1;
    private int selectionY = -1;

    public bool whiteInCheck = false; //Test if either team is in check
    public bool blackInCheck = false;

    public List<GameObject> chessPiecePrefabs; //List of piece prefabs
    private List<GameObject> activeChessPieces;

    private Quaternion orientation = Quaternion.Euler(0,90,0); //Set initial piece rotation

    public bool isWhiteTurn = true; //toggle turn control

    public int moveCount{set;get;}

    //toggle material for selected chess piece
        // private Material previousMat; 
        // public Material selectedMat;

    public int[] EnPassantMove{set;get;} //array for possible enPassant move, make accessible to Pawn

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        SpawnAllPieces();
    }

    // Update is called once per frame
    void Update()
    {
        //Check for currently selected tile
        UpdateSelection();
        // DrawChessboard();

        if (Input.GetMouseButtonDown (0)) { //Check for input
            HandleInput();
        }
    }
    /*
    Check for user events
    */
    private void HandleInput(){
        if (selectionX >= 0 && selectionY >= 0) { //If we clck AND we are selecting a tile
                if (selectedChessPiece == null) { //If we have no piece currently selected
                    //Select piece
                    SelectChessPiece(selectionX, selectionY);
                } else { //We have a piece selected
                    //Move piece
                    MoveChessPiece(selectionX, selectionY);
                }
            }
    }
    /* 
       Handle piece selection from HandleInput
    */
    private void SelectChessPiece(int x, int y){
        /* Check if it is player's turn */
        if (isWhiteTurn && ChessAI.Instance.isWhite || !isWhiteTurn && !ChessAI.Instance.isWhite) {
            Debug.Log("I'm Thinking....");
            return; //Selecting during AI's move
        }
        
        if (ChessPieces[x,y] == null) //If we select a tile without a chess piece
            return;
        if (ChessPieces[x,y].isWhite != isWhiteTurn) //Selected other opponents piece
            return;

        //Check to see which moves are allowed
        AllowedMoves = ChessPieces[x,y].PossibleMove();

        bool atLeast1Move = false; //Check to see if selected piece can move
        for(int i = 0; i < 8;i++) {
            for(int j = 0; j < 8;j++) {
                if (AllowedMoves[i, j])
                    atLeast1Move = true;
            }
        }
        if (!atLeast1Move) //If piece is unable to move
            return; //We do not select it

        selectedChessPiece = ChessPieces[x,y]; //We have selected our piece
        
        BoardHighlighter.Instance.HighlightAllowedMoves(AllowedMoves); //Show available moves for selected piece
        
    }
    /* 
       Move selected chesspiece to selected tile x, y (from HandleInput)
    */
    public void MoveChessPiece(int x, int y){ //Public so we can access from AI script

        string moveSound = "move1"; //Control the type of sound from the move
        
        if (AllowedMoves[x,y]) { //Check to see if move is allowed

            ChessPiece c = ChessPieces[x, y]; //Check if there is enemy piece on selected tile
            if (c != null && c.isWhite != isWhiteTurn) {//If there is enemy
                //Capture the piece
                activeChessPieces.Remove(c.gameObject);
                Destroy(c.gameObject);

                moveSound = "capture1"; //Set sound to capture
            }

            //Move is an enPassant move
            if (selectedChessPiece.GetType() == typeof(Pawn)) { //If we are moving a Pawn
                if(x == EnPassantMove[0] && y == EnPassantMove[1]) {
                    if (isWhiteTurn) { ////White capturing Black
                        //Get for piece behind current piece
                        c = ChessPieces[x,y-1];
                    } else { //Black capturing White
                        //Get for piece ahead current piece
                        c = ChessPieces[x,y+1];
                    }
                    activeChessPieces.Remove(c.gameObject); //Capture piece
                    Destroy(c.gameObject);

                    moveSound = "capture1"; //Set sound to capture
                }
            }

            //Check if move creates possible enPassant move for opponent
            EnPassantMove[0] = -1; //Reset on every turn (Move is only possible immediately after Pawn move)
            EnPassantMove[1] = -1;
            if (selectedChessPiece.GetType() == typeof(Pawn)) { //If we are moving a Pawn

                if (y == 7 ){ //Pawn Promotion (White)
                    activeChessPieces.Remove(selectedChessPiece.gameObject); //Remove the Pawn
                    Destroy(selectedChessPiece.gameObject);
                    SpawnChessPiece(1, x, y); //Add the Queen (White Queen prefab (1))
                    selectedChessPiece = ChessPieces[x, y];
                } else if(y == 0) { //Pawn Promotion (Black)
                    activeChessPieces.Remove(selectedChessPiece.gameObject); //Remove the Pawn
                    Destroy(selectedChessPiece.gameObject);
                    SpawnChessPiece(7, x, y); //Add the Queen (Black Queen prefab (7))
                }

                if (selectedChessPiece.yPosition == 1 && y == 3) {//(White Team) If it is the Pawn's first move, and it is moving 2 spaces
                    EnPassantMove[0] = x; //Add the coordiates of the possible move (example: Pawn to A4[0,3] -> EnPassantMove = A3[0, 2])
                    EnPassantMove[1] = y-1;
                } else if (selectedChessPiece.yPosition == 6 && y == 4) {//(Black Team)  If it is the Pawn's first move, and it is moving 2 spaces
                    EnPassantMove[0] = x; //Add the coordiates of the possible move (example: Pawn to A5[0,4] -> EnPassantMove = A6[0, 5])
                    EnPassantMove[1] = y+1;
                }
            }

            /* Check if player is moving a King (For Castleing availability) */
            if (selectedChessPiece.GetType() == typeof(King)) { //If we are moving a King
                int[] kingCoordinates = new int[2]; //Get King Coordinates (Which we know is the selected piece)
                kingCoordinates[0] = selectedChessPiece.xPosition;
                kingCoordinates[1] = selectedChessPiece.yPosition;
                ChessPieces[kingCoordinates[0], kingCoordinates[1]].PieceMoved();

                /* Check if Castleing (By testing if King is moving 2 spaces horizontally, only allowed when castleing) */
                if(selectedChessPiece.xPosition + 2 == x) { //King Side
                    //Get King Side Rook
                    ChessPiece rook = ChessPieces[selectedChessPiece.xPosition + 3, selectedChessPiece.yPosition]; //If we are running this, we know it must be here
                    //Move the Rook
                    ChessPieces[rook.xPosition, rook.yPosition] = null; //Set current rook tile to null, about to move
                    rook.transform.position = GetTileCenter(selectedChessPiece.xPosition + 1, y); //Move the Rook 1 Tile right of where King CURRENTLY is
                    rook.SetPosition(selectedChessPiece.xPosition + 1, y);
                    ChessPieces[selectedChessPiece.xPosition + 1, y] = rook; //Add back to array with correct postion
                    moveSound = "castle1"; //Set sound to castle
                } else if (selectedChessPiece.xPosition - 2 == x) { //Queen Side
                    //Get Queen Side Rook
                    ChessPiece rook = ChessPieces[selectedChessPiece.xPosition - 4, selectedChessPiece.yPosition]; //If we are running this, we know it must be here
                    //Move the Rook
                    ChessPieces[rook.xPosition, rook.yPosition] = null; //Set current rook tile to null, about to move
                    rook.transform.position = GetTileCenter(selectedChessPiece.xPosition - 1, y); //Move the Rook 1 Tile right of where King CURRENTLY is
                    rook.SetPosition(selectedChessPiece.xPosition - 1, y);
                    ChessPieces[selectedChessPiece.xPosition - 1, y] = rook; //Add back to array with correct postion
                    moveSound = "castle1"; //Set sound to castle
                }
            }

            /* Check if player is moving a Rook (For Castleing availability) */
            if (selectedChessPiece.GetType() == typeof(Rook)) { //If we are moving a Rook
                int[] rookCoordinates = new int[2]; //Get Rook Coordinates (Which we know is the selected piece)
                rookCoordinates[0] = selectedChessPiece.xPosition;
                rookCoordinates[1] = selectedChessPiece.yPosition;
                ChessPieces[rookCoordinates[0], rookCoordinates[1]].PieceMoved();
            }

            /* Move the selected piece */
            ChessPieces[selectedChessPiece.xPosition, selectedChessPiece.yPosition] = null; //Set current tile to null, about to move
            selectedChessPiece.transform.position = GetTileCenter(x, y); //Move the piece to selected tile
            selectedChessPiece.SetPosition(x,y);
            ChessPieces[x, y] = selectedChessPiece; //Add back to array with correct postion

            /* Check if move puts opponent in check */
            int[] blackKingCoordinates = LocateKing(1); //Look for Check on Black
            if(ChessPieces[blackKingCoordinates[0], blackKingCoordinates[1]].InCheck()) {
                blackInCheck = true;
                moveSound = "check1"; //Set sound to check
                /* Look for Checkmate */
                bool canMove = false;
                for(int i = 0; i < 8; i++) { //Loop over all pieces
                    for(int j = 0; j < 8; j++) {
                        ChessPiece p = ChessPieces[i, j];
                        if (p != null && !p.isWhite) { //We found a friendly piece
                            bool [,] r = p.PossibleMove();
                            for(int h = 0; h < 8; h++) { //Loop over possible moves of friendly piece
                                for(int k = 0; k < 8; k++) {
                                    if (r[h,k]) { //If we find a valid move
                                        canMove = true; //Can't be mate
                                    }
                                }
                            }
                        }
                    }
                }
                if (!canMove) {
                    Checkmate();
                    return;
                }
            } else {
                blackInCheck = false;
            }
            int[] whiteKingCoordinates = LocateKing(0); //Look for Check on White
            if(ChessPieces[whiteKingCoordinates[0], whiteKingCoordinates[1]].InCheck()) {
                whiteInCheck = true;
                moveSound = "check1"; //Set sound to check
                /* Look for Checkmate */
                bool canMove = false;
                for(int i = 0; i < 8; i++) { //Loop over all pieces
                    for(int j = 0; j < 8; j++) {
                        ChessPiece p = ChessPieces[i, j];
                        if (p != null && p.isWhite) { //We found a friendly piece
                            bool [,] r = p.PossibleMove();
                            for(int h = 0; h < 8; h++) { //Loop over possible moves of friendly piece
                                for(int k = 0; k < 8; k++) {
                                    if (r[h,k]) { //If we find a valid move
                                        canMove = true; //Can't be mate
                                    }
                                }
                            }
                        }
                    }
                }
    
                if (!canMove) {
                    Checkmate();
                    return;
                }
            } else {
                whiteInCheck = false;
            }
            isWhiteTurn = !isWhiteTurn; //Toggle turns

            if (ChessAI.Instance.isWhite && isWhiteTurn || !ChessAI.Instance.isWhite && !isWhiteTurn) { //Check if it is the AI's turn
                Invoke("AIMove", 0.5f);
            }
        }

        selectedChessPiece = null; //Unselect the piece
        BoardHighlighter.Instance.HideHighlights(); //Hide allowable moves

        SoundManager.PlaySound(moveSound); //Play sound effect
    }

    //Called when it is the AI's turn (Created seperate function because we are calling with 'invoke')
    private void AIMove() {
        ChessAI.Instance.CalculateMoves();
    }

    /*
        Locate the Kings (To test whether InCheck or not)
        Takes int for team, 0 = White, 1 = Black
        Return int array, [x,y] coordinates
    */
    public int[] LocateKing(int t) { 
        int[] coordinates = new int[2];
        ChessPiece c; //Create seperate piece for testing. Idk why yet...
        for (int i = 0; i < 8; i++) { //Check X values
            for (int j = 0; j < 8; j++) { //Check Y values
                c = ChessPieces[i, j]; //Set to current tile
                if (c != null && c.GetType() == typeof(King)) { //Check if there is a piece, and if it is a King
                    if(t == 0 && c.isWhite){
                        coordinates[0] = i; //White King Coordinates
                        coordinates[1] = j;
                    } else if (t == 1 && !c.isWhite) {
                        coordinates[0] = i; //Black King Coordinates
                        coordinates[1] = j;
                    }
                }
            }
        }
        return coordinates; //Return King Coordinates
    }
    /* Checkmate! */
    public void Checkmate(){
        UI.Instance.ShowCheckmate();
        BoardHighlighter.Instance.HideHighlights(); //Hide allowable moves
        Invoke("RestartGame", 1.5f);
    }
    /* Stalemate! */
    public void Stalemate(){
        UI.Instance.ShowStalemate();
        BoardHighlighter.Instance.HideHighlights(); //Hide allowable moves
        Invoke("RestartGame", 1.5f);
    }

    /* 
        For testing...
    */
    private void DrawChessboard() {
        Vector3 widthLine = Vector3.right * 8; //8 units width
        Vector3 heightLine = Vector3.forward * 8; //8 units height

        for(int i = 0; i <= 8; i++) { //Draw lines
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);
            for(int j = 0; j <= 8; j++) {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        //Draw "X" on the selected tile, 2 lines
        if (selectionX >= 0 && selectionY >= 0) { //We have a tile selected
            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX, //Start point
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1) //End point (1 unit long)
            );
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX, //Start point
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1) //End point (1 unit long)
            );
        }
    }
    /* Restart Game */
    public void RestartGame(){
        //Remove all pieces
        ChessPiece c;
        for (int i = 0; i < 8; i++) { //Check X values
            for (int j = 0; j < 8; j++) { //Check Y values
                c = ChessPieces[i, j]; //Set to current tile
                if (c != null) { //Check if there is a piece
                    activeChessPieces.Remove(c.gameObject);
                    Destroy(c.gameObject);
                }
            }
        }
        selectedChessPiece = null; //Unselect the piece
        BoardHighlighter.Instance.HideHighlights(); //Hide allowable moves
        isWhiteTurn = true;
        moveCount = 0; 
        UI.Instance.HideText();      
        SpawnAllPieces(); //Reset pieces
    }
    /* Spawn Chesspiece from set ChessPiecePrefab list 
        Add spawned piece to activeChessPieces list
    */
    private void SpawnChessPiece(int index, int x, int y) {
        GameObject obj = Instantiate(chessPiecePrefabs[index], GetTileCenter(x,y), orientation) as GameObject;
        obj.transform.SetParent(transform);
        // obj.transform.Rotate(-90, 0, 0, Space.Self); //Spawns laying down?
        ChessPieces[x,y] = obj.GetComponent<ChessPiece>(); //Add obj to ChessPieces array
        ChessPieces[x,y].SetPosition(x, y); //Call set position so piece knows its current position
        activeChessPieces.Add(obj);
    }
    /* Spawns all pieces.
        Called once at beginning of game */
    private void SpawnAllPieces(){
        activeChessPieces = new List<GameObject>(); //initiate
        ChessPieces = new ChessPiece[8,8];
        EnPassantMove= new int[2]{-1,-1}; //Initially set to [-1,-1]

        //Spawn White team
        SpawnChessPiece(0, 4,0); //King
        SpawnChessPiece(1, 3,0); //Queen
        SpawnChessPiece(2, 7,0); //Rook1_KS
        SpawnChessPiece(2, 0,0); //Rook2_QS
        SpawnChessPiece(3, 5,0); //Bishop1_KS
        SpawnChessPiece(3, 2,0); //Bishop2_QS
        SpawnChessPiece(4, 6,0); //Knight1_KS
        SpawnChessPiece(4, 1,0); //Knight2_QS
        for (int i=0; i<8;i++){
            SpawnChessPiece(5, i,1);//Pawns
        }

        //Spawn Black team
        SpawnChessPiece(6, 4,7); //King
        SpawnChessPiece(7, 3,7); //Queen
        SpawnChessPiece(8, 7,7); //Rook1_KS
        SpawnChessPiece(8, 0,7); //Rook2_QS
        SpawnChessPiece(9, 5,7); //Bishop1_KS
        SpawnChessPiece(9, 2,7); //Bishop2_QS
        SpawnChessPiece(10, 6,7); //Knight1_KS
        SpawnChessPiece(10, 1,7); //Knight2_QS
        for (int i=0; i<8;i++){
            SpawnChessPiece(11, i,6);//Pawns
        }
    }
    /* Update to check mouse haver selection */
    private void UpdateSelection(){
        if(!Camera.main)
            return;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit,25.0f,LayerMask.GetMask("Chessplane"))) {
            //If we hit our "Chessplane" box collider w/ mouseposition 
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        } else {
            //Not currently selecting a tile
            selectionX = -1;
            selectionY = -1;
        }
    }
    /* 
        Handle movement and piece spawn with offset
    */
    public Vector3 GetTileCenter(int x, int y) {
        Vector3 origin = Vector3.zero; //Set origin to zero
        //If we select A3, x=0, y=2
        origin.x += (TILE_SIZE * x) + TILE_OFFSET; //origin.x += (1.0 * 0) + 0.5 = 0.5 (middle of A)
        origin.z += (TILE_SIZE * y) + TILE_OFFSET; //origin.z += (1.0 * 2) + 0.5 = 2.5 (middle of 3)
        origin.y -= 0.4f;
        return origin;
    }
    /* 
        Declare winner, restart game. Respawn pieces.
    */
    private void EndGame(){
        if (isWhiteTurn)
            Debug.Log("White Team Wins");
        else
            Debug.Log("Black Team Wins");

        foreach (GameObject obj in activeChessPieces) //Clear all pieces
        {   
            Destroy(obj);
        }

        //Restart Game
        isWhiteTurn = true;
        BoardHighlighter.Instance.HideHighlights();
        SpawnAllPieces();

    } 
}
