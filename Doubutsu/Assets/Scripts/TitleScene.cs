using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour {

	void Start () {
		Invoke("ChangeScene", 2.0f);	// 指定秒数後に説明画面に
	}

	void ChangeScene(){	// 説明画面に遷移
		SceneManager.LoadScene("Main");
		Debug.Log ("Move to Introduction Scene");
	}
}
