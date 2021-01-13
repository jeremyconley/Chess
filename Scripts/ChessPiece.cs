using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public int xPosition{set;get;}
    public int yPosition{set;get;}
    public bool isWhite;

    // float t;
    // Vector3 target;
    // float timeToReachTarget = 1.0f;

    public void SetPosition(int x, int y) {
        xPosition = x;
        yPosition = y;
    }

    // public void MovePiece(Vector3 target){
    //     this.target = target;
    // }

    // void Update(){
    //     t += Time.deltaTime/timeToReachTarget;
    //     transform.position = Vector3.Lerp(new Vector3(xPosition, 0, yPosition), new Vector3(target.x, 0, target.z), t);
    // }

    public virtual bool[,] PossibleMove() {
        return new bool[8,8];
    }

    public virtual bool InCheck(){ //Only used for King
        return false;
    }
    public virtual void PieceMoved(){} //Only used for King & Rook to check if Castle option is allowed
    public virtual bool HasMoved(){ return false; } //Only used in Rook to be accessible in King to check if it has been moved for Castle option
}
