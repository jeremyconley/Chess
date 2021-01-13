using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlighter : MonoBehaviour
{
    public static BoardHighlighter Instance{set;get;} //Add for outside reference

    public GameObject moveHighlightPrefab; //Hightlight for possible move
    public GameObject captureHighlightPrefab; //Highlight for possible capture
    private List<GameObject> moveHighlights; //List of all move highlight prefabs
    private List<GameObject> captureHighlights; //List of all capture highlight prefabs

    private void Start(){
        Instance = this;
        moveHighlights = new List<GameObject>(); //Initialize
        captureHighlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(bool enemyOnTile){
        if (enemyOnTile) {
            GameObject enemyHighlight = captureHighlights.Find(g => !g.activeSelf); //Find inactive enemy highlight prefab
            if (enemyHighlight == null) { //If none found
                enemyHighlight = Instantiate(captureHighlightPrefab); //Create one
                captureHighlights.Add(enemyHighlight); //Add to list
            }
            return enemyHighlight;
        }
        //No enemy, empty tile
        GameObject obj = moveHighlights.Find(g => !g.activeSelf); //Find inactive prefab
        if (obj == null) { //If none found
            obj = Instantiate(moveHighlightPrefab); //Create one
            moveHighlights.Add(obj); //Add to list
        }
        return obj;
    }
    /* Highlight all possible moves for selected piece */
    public void HighlightAllowedMoves(bool[,] moves) {
        for(int i = 0;i < 8;i++){
            for(int j = 0;j < 8;j++){
                if (moves[i,j]) {
                    GameObject obj; //Our highlight obj
                    ChessPiece c = BoardController.Instance.ChessPieces[i,j]; //Check for enemy piece on tile
                    if (c != null) { //Piece on tile, must be an enemy (else would not be possible move)
                        obj = GetHighlightObject(true); //Capture highlight
                    } else {
                        obj = GetHighlightObject(false); //Move highlight
                    } 
                    obj.SetActive(true);
                    obj.transform.position = new Vector3 (i+0.5f,0,j+0.5f); //+ 0.5 for offset
                }
            }
        }
    }
    public void HideHighlights(){
        foreach(GameObject obj in moveHighlights)
            obj.SetActive(false);
        foreach(GameObject obj in captureHighlights)
            obj.SetActive(false);
    }
}
