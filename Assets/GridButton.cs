using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridButton : MonoBehaviour {

    public Button btn;
    public int x, y;

    public delegate void OnPress(int x, int y);
    public OnPress onPress;

	// Use this for initialization
	void Start () {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        onPress(x, y);
    }
}
