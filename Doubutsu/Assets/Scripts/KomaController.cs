using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KomaController : MonoBehaviour {

//	[SerializeField]
//	GameObject[] buttonGameobjecs;	// 周囲のボタンをGameobjectとして取得 (Gameobjectとして取得することでSetActiveが使えるため)
//	Button[] moveButtons;	// 要素数不定で宣言

	[SerializeField]
	bool FR, F, FL, R, L, BR, B, BL;	// Inspector上で指定する駒の動ける範囲
	bool[] movableArea = new bool[8];	// 駒の動ける範囲を入れる配列

	public bool isPlayer1;				// Player1かどうか
	int direction;						// 駒の進む方向

	public bool isSelected = false;			// 駒が選択中かどうか

	bool isInHand = false;				// 手駒になっているかどうか

	BoardManager boardManager;
	string gameobjectName;				// スクリプトがアタッチされたゲームオブジェクト名(駒の名前)
//	KomaController koma;
	TebanManager tebanManager;

	Image image;
	public Sprite imagePlayer1, imagePlayer2;
//	public Sprite imagePlayer1Nari, imagePlayer2Nari;

	void Start () {

		gameobjectName = gameObject.name;

		boardManager = GameObject.Find("BoardManager").GetComponent<BoardManager>();
		tebanManager = GameObject.Find ("TebanManager").GetComponent<TebanManager> ();

		// 画像設定
		image = GetComponent<Image> ();
		if (isPlayer1) {
			
			image.sprite = imagePlayer1;
		} else {
			image.sprite = imagePlayer2;
		}

		movableArea [0] = FR;
		movableArea [1] = F;
		movableArea [2] = FL;
		movableArea [3] = R;
		movableArea [4] = L;
		movableArea [5] = BR;
		movableArea [6] = B;
		movableArea [7] = BL;

//		// 駒の進む方向を決定
//		if (isPlayer1) {
//			direction = -1;
//		} else {
//			direction = 1;
//		}

	}

	/// 自分自身をクリックした時の処理
	public void OnClickSelf() {

		if (isPlayer1 == tebanManager.isPlayer1Teban) {

			// 駒の進む方向を決定
			if (isPlayer1) {
				direction = -1;
			} else {
				direction = 1;
			}

			Debug.Log ("Button click!");

			int posSuji = (int)(transform.position.x / boardManager.scale);	//盤の座標をプログラムの座標に変換 suji, dan -> y, x
			int posDan = -(int)(transform.position.y / boardManager.scale) + 5;	//盤の座標をプログラムの座標に変換 suji, dan -> y, x

			Debug.Log ("posDan " + posDan);		// suji, dan -> y, x
			Debug.Log ("posSuji " + posSuji);	// suji, dan -> y, x

			// 画面上の処理
			boardManager.DeactivateButtonAll ();	// ボタンを全部消す

			// 内部の処理
			boardManager.te.koma = boardManager.CheckKomaName (gameObject.name.ToString ());	// 駒を指定

			if (!isSelected) {	// 駒が未選択の場合
				isSelected = true;
				boardManager.ResetFlagOtherThanSelf (gameObject);	// 他の駒のisSelectedをfalseにする

				//盤上の駒の場合
				if (1 <= posDan && posDan <= 4 && 1 <= posSuji && posSuji <= 3) {
					Debug.Log ("盤上の駒");

					boardManager.te.from.dan = posDan;
					boardManager.te.from.suji = posSuji;

					// 駒の動ける範囲を確認
					for (int i = 0; i < movableArea.Length; i++) {
						if (movableArea [i] == true) {
							switch (i) {
							case 0:
								if (boardManager.IsMovable (isPlayer1, posDan + 1 * direction, posSuji - 1 * direction)) {	// suji, dan -> y, x
									boardManager.ActivateButton (posDan + 1 * direction, posSuji - 1 * direction);	// suji, dan -> y, x
								}
								break;
							case 1:
								if (boardManager.IsMovable (isPlayer1, posDan + 1 * direction, posSuji)) {
									boardManager.ActivateButton (posDan + 1 * direction, posSuji);
								}
								break;
							case 2:
								if (boardManager.IsMovable (isPlayer1, posDan + 1 * direction, posSuji + 1 * direction)) {
									boardManager.ActivateButton (posDan + 1 * direction, posSuji + 1 * direction);
								}
								break;
							case 3:
								if (boardManager.IsMovable (isPlayer1, posDan, posSuji - 1 * direction)) {
									boardManager.ActivateButton (posDan, posSuji - 1 * direction);
								}
								break;
							case 4:
								if (boardManager.IsMovable (isPlayer1, posDan, posSuji + 1 * direction)) {
									boardManager.ActivateButton (posDan, posSuji + 1 * direction);
								}
								break;
							case 5:
								if (boardManager.IsMovable (isPlayer1, posDan - 1 * direction, posSuji - 1 * direction)) {
									boardManager.ActivateButton (posDan - 1 * direction, posSuji - 1 * direction);
								}
								break;
							case 6:
								if (boardManager.IsMovable (isPlayer1, posDan - 1 * direction, posSuji)) {
									boardManager.ActivateButton (posDan - 1 * direction, posSuji);
								}
								break;
							case 7:
								if (boardManager.IsMovable (isPlayer1, posDan - 1 * direction, posSuji + 1 * direction)) {
									boardManager.ActivateButton (posDan - 1 * direction, posSuji + 1 * direction);
								}
								break;
							default:
								Debug.Log ("Initial Pos Error");
								break;
							}
						}
					}
				} else { // 手駒の場合
					boardManager.te.from.dan = 0;
					boardManager.te.from.suji = 0;
					boardManager.ActivateButtonInEmpty ();	// 開いているマスのボタンを有効にする
				}

			} else {	// 駒が選択済の場合
				isSelected = false;
			}
		}
	}

	public void ResetSelectedFlag(){
		isSelected = false;
	}

	public void ChangeKomaImage(){
		if (isPlayer1) {
			image.sprite = imagePlayer1;
		} else {
			image.sprite = imagePlayer2;
		}
	}

//	public void ChangeKomaImageToNari(){
//		if (isPlayer1) {
//			spriteRenderer.sprite = imagePlayer1Nari;
//		} else {
//			spriteRenderer.sprite = imagePlayer2Nari;
//		}
//	}
}
