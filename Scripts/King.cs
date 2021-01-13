using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    private bool hasMoved = false; //Used to check if King has moved for Castle option (Toggled from PieceMoved)

    public override bool[,] PossibleMove(){
        bool[,] r = new bool[8,8]; //Array of possible moves
        ChessPiece c; //Used to check for blocking pieces
        int i, j;

        /* MOVE UP */
        i = xPosition - 1; //Start w/ UP-LEFT move
        j = yPosition + 1;
        if (yPosition != 7) { //If not at top of board
            for (int k = 0; k<3; k++) { //Loop covers all 3 moves (UP-LEFT, UP-MID, UP-RIGHT)
                if (i >= 0 && i < 8) { //If the move is within the board range
                    c = BoardController.Instance.ChessPieces[i, j];
                    if (c == null)//If the tile is empty
                        r[i,j] = true;//This is a valid move
                    else if (isWhite != c.isWhite) //If the tile contains an enemy
                        r[i,j] = true; //This is a valid move
                }
                i++;
            }
        }
        /* MOVE DOWN */
        i = xPosition - 1; //Start w/ DOWN-LEFT move
        j = yPosition - 1;
        if (yPosition != 0) { //If not at bottom of board
            for (int k = 0; k<3; k++){ //Loop covers all 3 moves (DOWN-LEFT, DOWN-MID, DOWN-RIGHT)
                if (i >= 0 && i < 8) { //If the move is within the board range
                    c = BoardController.Instance.ChessPieces[i, j];
                    if (c == null)//If the tile is empty
                        r[i,j] = true;//This is a valid move
                    else if (isWhite != c.isWhite) //If the tile contains an enemy
                        r[i,j] = true; //This is a valid move
                }
                i++;
            }
        }
        /* MOVE MID LEFT */
        if (xPosition != 0) { //If not at left-most tile
            c = BoardController.Instance.ChessPieces[xPosition - 1, yPosition]; //Check for blocking piece
            if (c == null) //The tile is empty
                r[xPosition - 1, yPosition] = true; //This is a valid move
            else if (isWhite != c.isWhite) //Tile contains an enemy
                r[xPosition - 1, yPosition] = true; //This is a valid move
        }
        /* MOVE MID RIGHT */
        if (xPosition != 7) { //If not at right-most tile
            c = BoardController.Instance.ChessPieces[xPosition + 1, yPosition]; //Check for blocking piece
            if (c == null) //The tile is empty
                r[xPosition + 1, yPosition] = true; //This is a valid move
            else if (isWhite != c.isWhite) //Tile contains an enemy
                r[xPosition + 1, yPosition] = true; //This is a valid move
        }

        /* Castle King  */ 
        if(!InCheck() && !hasMoved) { //If King is not in check and has not moved
            bool canCastleKS = true; //Initially true (King Side)
            bool canCastleQS = true; //Initially true (Queen Side)
            /* King Side */
            for (int k = 1; k < 3; k++) { //Check the 2 tiles right of King
                c = BoardController.Instance.ChessPieces[xPosition + k, yPosition]; //Check for blocking piece
                if (c != null) //The tile is blocked
                    canCastleKS = false; 
            }
            if (canCastleKS) { //Right 2 spaces clear
                c = BoardController.Instance.ChessPieces[xPosition + 3, yPosition]; //Check for Rook
                if(c != null && c.isWhite == isWhite && c.GetType() == typeof(Rook)) { //If we find a friendly Rook
                    if(c.HasMoved()) { //Check if Rook has been moved
                        canCastleKS = false; 
                    }
                } else {
                    canCastleKS = false;
                }
            }
            if (canCastleKS) {
                r[xPosition + 2, yPosition] = true;
            }
            /* Queen Side */
            for (int k = 1; k < 4; k++) { //Check the 3 tiles left of King
                c = BoardController.Instance.ChessPieces[xPosition - k, yPosition]; //Check for blocking piece
                if (c != null) //The tile is blocked
                    canCastleQS = false; 
            }
            if (canCastleQS) { //Left 3 spaces clear
                c = BoardController.Instance.ChessPieces[xPosition - 4, yPosition]; //Check for Rook
                if(c != null && c.isWhite == isWhite && c.GetType() == typeof(Rook)) { //If we find a friendly Rook
                    if(c.HasMoved()) { //Check if Rook has been moved
                        canCastleQS = false; 
                    }
                } else {
                    canCastleQS = false;
                }
            }
            if (canCastleQS) {
                r[xPosition - 2, yPosition] = true;
            }
        }

        /* Disable moves that will result in us in Check (Also works if we already are in check)*/
        //Loop over each possible move for King
        for(int h = 0;h < 8;h++){
            for(int k = 0;k < 8;k++){
                if (r[h,k]) {
                    //Save our current xPosition and yPosition because we will modify it for test below
                    int currentX = xPosition;
                    int currentY = yPosition;
                    //We make the move to test (only logically, not with transition)
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = null; //Set our current position to null, about to move
                    ChessPiece testC = BoardController.Instance.ChessPieces[h, k]; //Check for enemy piece on tile we are testing to move (test capture?)
                    SetPosition(h, k); //Must call SetPosition for InCheck here because we are moving the king (For InCheck to have right King location)
                    BoardController.Instance.ChessPieces[h, k] = this; //Make the test move
                    if(InCheck()) { //The King is still in check, move not possible
                        r[h, k] = false; //Not a valid move
                    }
                    SetPosition(currentX, currentY); //Set back correct xPosition and yPosition
                    //Set King back to its correct position
                    BoardController.Instance.ChessPieces[xPosition, yPosition] = this;
                    //Set test captured enemy back (if one)
                    if (testC != null) { //it must be an enemy
                        BoardController.Instance.ChessPieces[h, k] = testC;
                    } else { //There was not an enemy there
                        BoardController.Instance.ChessPieces[h, k] = null;
                    }
                }
            }
        }

        return r;
    }
    /* Called if our King has been moved (Disables option to castle)*/
    public override void PieceMoved(){
        if(!hasMoved) {
            hasMoved = true;
        }
    }

    public override bool InCheck(){
        ChessPiece c; //Used to check for targeting pieces
        int i, j;


        /* CHECK UP-LEFT-DIAGONAL ATTACK (Bishop, Queen, Pawn(Black attacking White)) */  //*********CHECK FOR KING CHECK HERE(When preparing to move King)**************
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i--; //Move up-left 1 tile
            j++;
            if (i < 0 || j >= 8) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition - 1 && j == yPosition + 1) { //On first up-left tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                        if (c.GetType() == typeof(Pawn) && !c.isWhite) { //Enemy Pawn
                            return true; //Check!
                        }
                    }
                    // if(c.GetType() == typeof(Pawn) && !c.isWhite && i == xPosition - 1 && j == yPosition + 1) { //Check for Pawn attack (Black Pawn)
                    //     return true; //Check!
                    // } 
                    if (c.GetType() == typeof(Bishop) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Bishop
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Rook, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK UP-RIGHT-DIAGONAL ATTACK (Bishop, Queen, Pawn(Black attacking White)) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i++; //Move up-right 1 tile
            j++;
            if (i >= 8 || j >= 8) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition + 1 && j == yPosition + 1) { //On first up-right tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                        if (c.GetType() == typeof(Pawn) && !c.isWhite) { //Enemy Pawn
                            return true; //Check!
                        }
                    }
                    // if(c.GetType() == typeof(Pawn) && !c.isWhite && i == xPosition + 1 && j == yPosition + 1) { //Check for Pawn attack (Black Pawn)
                    //     return true; //Check!
                    // } 
                    if (c.GetType() == typeof(Bishop) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Bishop
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Rook, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK DOWN-LEFT-DIAGONAL ATTACK (Bishop, Queen, Pawn(White attacking Black)) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i--; //Move down-left 1 tile
            j--;
            if (i < 0 || j < 0) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition - 1 && j == yPosition - 1) { //On first down-left tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                        if (c.GetType() == typeof(Pawn) && c.isWhite) { //Enemy Pawn
                            return true; //Check!
                        }
                    }
                    // if(c.GetType() == typeof(Pawn) && c.isWhite && i == xPosition - 1 && j == yPosition - 1) { //Check for Pawn attack (White Pawn)
                    //     return true; //Check!
                    // } 
                    if (c.GetType() == typeof(Bishop) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Bishop
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Rook, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK DOWN-RIGHT-DIAGONAL ATTACK (Bishop, Queen, Pawn(White attacking Black)) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            i++; //Move down-right 1 tile
            j--;
            if (i >= 8 || j < 0) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition + 1 && j == yPosition - 1) { //On first down-right tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                        if (c.GetType() == typeof(Pawn) && c.isWhite) { //Enemy Pawn
                            return true; //Check!
                        }
                    }
                    // if(c.GetType() == typeof(Pawn) && c.isWhite && i == xPosition + 1 && j == yPosition - 1) { //Check for Pawn attack (White Pawn)
                    //     return true; //Check!
                    // } 
                    if (c.GetType() == typeof(Bishop) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Bishop
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Rook, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK UP-MIDDLE ATTACK (Rook, Queen) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            //Move up 1 tile
            j++;
            if (j >= 8) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (j == yPosition + 1) { //On first forward tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                    }
                    if (c.GetType() == typeof(Rook) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Rook
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Bishop, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }

        /* CHECK DOWN-MIDDLE ATTACK (Rook, Queen) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            //Move down 1 tile
            j--;
            if (j < 0) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (j == yPosition - 1) { //On next tile behind King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                    }
                    if (c.GetType() == typeof(Rook) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Rook
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Bishop, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK MIDDLE-LEFT ATTACK (Rook, Queen) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            //Move left 1 tile
            i--;
            if (i < 0) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition - 1) { //On first left tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                    }
                    if (c.GetType() == typeof(Rook) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Rook
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Bishop, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK MIDDLE-RIGHT ATTACK (Rook, Queen) */
        i = xPosition; //Set to current x and y
        j = yPosition;
        while(true){
            //Move right 1 tile
            i++;
            if (i >= 8) //If outside of board range
                break;
            c = BoardController.Instance.ChessPieces[i, j]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) { //Tile contains enemy
                    if (i == xPosition + 1) { //On first right tile from King
                        if (c.GetType() == typeof(King)) { //Enemy King (Will not let us move here)
                            return true; //Check!
                        }
                    }
                    if (c.GetType() == typeof(Rook) || c.GetType() == typeof(Queen)) { //If the enemy piece is a Queen or Rook
                        return true; //Check!
                    } else { //If it is an enemy Pawn, Bishop, Knight blocking the lane
                        break;
                    }
                } else { //Tile Contains a friendly piece
                    break; //We get here before we find an enemy, no threat from this angle. (Path Blocked)
                }
        }
        /* CHECK KNIGHT ATTACK  */
        
        i = xPosition; //Set to current x and y
        j = yPosition;
        
        //UP-LEFT ATTACT
        if (i-1 >= 0 && i-1 < 8 && j+2 >=0 && j+2 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i-1, j+2]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //UP-RIGHT ATTACT
        if (i+1 >= 0 && i+1 < 8 && j+2 >=0 && j+2 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i+1, j+2]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //DOWN-LEFT ATTACT
        if (i-1 >= 0 && i-1 < 8 && j-2 >=0 && j-2 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i-1, j-2]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //DOWN-RIGHT ATTACT
        if (i+1 >= 0 && i+1 < 8 && j-2 >=0 && j-2 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i+1, j-2]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //LEFT-UP ATTACT
        if (i-2 >= 0 && i-2 < 8 && j+1 >=0 && j+1 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i-2, j+1]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //LEFT-DOWN ATTACT
        if (i-2 >= 0 && i-2 < 8 && j-1 >=0 && j-1 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i-2, j-1]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //RIGHT-UP ATTACT
        if (i+2 >= 0 && i+2 < 8 && j+1 >=0 && j+1 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i+2, j+1]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }
        //RIGHT-DOWN ATTACT
        if (i+2 >= 0 && i+2 < 8 && j-1 >=0 && j-1 < 8) { //If the move is whithin chessboard bounds
            c = BoardController.Instance.ChessPieces[i+2, j-1]; //Check for piece on tile
            if (c != null)//If tile contains piece
                if (isWhite != c.isWhite) //Tile contains enemy
                    if (c.GetType() == typeof(Knight)) //If the enemy piece is a Knight
                        return true; //In Check
        }

        return false;
    }
}
