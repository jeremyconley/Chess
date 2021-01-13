using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{

    public Text checkmateText;
    public Text stalemateText;
    public static UI Instance{set;get;} //Add for outside reference

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        HideText();
        
    }

    public void ShowCheckmate(){
        checkmateText.gameObject.SetActive(true);
    }
    public void HideText(){
        checkmateText.gameObject.SetActive(false);
        stalemateText.gameObject.SetActive(false);
    }
    public void ShowStalemate(){
        stalemateText.gameObject.SetActive(true);
    }
}
