using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour {

//	[SerializeField]
//	Image panel, player1Won, player2Won;
	[SerializeField]
	GameObject panel, player1Won, player2Won;

	// Use this for initialization
	void Start () {
		Clear ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangePlayer1Won (){
		panel.SetActive (true);
		player1Won.SetActive (true);
	}

	public void ChangePlayer2Won (){
		panel.SetActive (true);
		player2Won.SetActive (true);
	}

	public void Clear (){
		panel.SetActive (false);
		player1Won.SetActive (false);
		player2Won.SetActive (false);
	}
}
