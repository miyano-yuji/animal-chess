// 参考: http://usapyon.cocolog-nifty.com/shogi/HowToMakeShogiProgram.html

using System;	// enumを使うのに必要?
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;	// textを使う

public class BoardManager : MonoBehaviour {

	public enum Koma {
		OutOfBoard = 64,
		Empty	= 0,
		Hiyoko	= 1,
		Kirin	= 2, 
		Zou		= 3,
		Lion	= 4,
		Promoted = 8,
		Niwatori = Promoted + Hiyoko,
		Enemy	= 16,
		eHiyoko	= Enemy + Hiyoko,
		eKirin	= Enemy + Kirin,
		eZou	= Enemy + Zou,
		eLion	= Enemy + Lion
	}
			
//	Koma[,] board = {{Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard},
//		{Koma.OutOfBoard, Koma.Empty, Koma.eLion, Koma.Empty, Koma.OutOfBoard},
//		{Koma.OutOfBoard, Koma.Empty, Koma.Empty, Koma.Empty, Koma.OutOfBoard},
//		{Koma.OutOfBoard, Koma.Empty, Koma.Empty, Koma.Empty, Koma.OutOfBoard},
//		{Koma.OutOfBoard, Koma.Empty, Koma.Lion, Koma.Empty, Koma.OutOfBoard},
//		{Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard}};	// 盤面 周りに盤の外を示すデータを入れておくために一回り大きい
//	int[,] hand = {{0, 1, 1, 1, 0}, {0, 1, 1, 1, 0}};	// hiyoko, kirin, zouを1個ずつ持っている状態 (持ち駒hand[0][x]が先手の駒の個数(x:1=Hiyoko, 2=Kirin,,,) hand[1][x]が後手)

	Koma[,] board = {{Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard},
		{Koma.OutOfBoard, Koma.eZou, Koma.eLion, Koma.eKirin, Koma.OutOfBoard},
		{Koma.OutOfBoard, Koma.Empty, Koma.eHiyoko, Koma.Empty, Koma.OutOfBoard},
		{Koma.OutOfBoard, Koma.Empty, Koma.Hiyoko, Koma.Empty, Koma.OutOfBoard},
		{Koma.OutOfBoard, Koma.Kirin, Koma.Lion, Koma.Zou, Koma.OutOfBoard},
		{Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard, Koma.OutOfBoard}};	// 盤面 周りに盤の外を示すデータを入れておくために一回り大きい
	int[,] hand = {{0, 0, 0, 0, 0}, {0, 0, 0, 0, 0}};	// 持ち駒hand[0][x]が先手の駒の個数(x:1=Hiyoko, 2=Kirin,,,) hand[1][x]が後手


	String[] komaStr = {"", "Hiyoko", "Kirin", "Zou", "Lion", "eHiyoko", "eKirin", "eZou", "eLion"};

	int koma;

	public struct Pos{
		public int dan;	// 段、筋
		public int suji;
	}

	public struct Te{
		public Pos from;
		public Pos to;			// どこからどこへ
		public Koma koma;		// どの駒が
		public bool promote;	// 成るか成らないか

		public void setTe(int _fromDan, int _fromSuji, int _toDan, int _toSuji, Koma _koma, bool _promote ){
			from.dan = _fromDan;
			from.suji = _fromSuji;
			to.dan = _toDan;
			to.suji = _toSuji;
			koma = _koma;
			promote = _promote;
		}
	}

	public Pos from = new Pos ();
	public Pos to = new Pos ();
	public Te te = new Te();

	public float scale = 300.0f;						// 1マスの大きさ	

	[SerializeField]
	Text player1Text, player2Text;

	public GameObject Hiyoko, Kirin, Zou, Lion, eHiyoko, eKirin, eZou, eLion;
	KomaController[] komas = new KomaController[8];

	[SerializeField]
	GameObject[] buttonsA, buttonsB, buttonsC;
	GameObject[,] buttons = new GameObject[4,5];	// 0列、0段目は使わないので1つ多めに用意

	TebanManager tebanManager;
	GameStatusManager gameStatusManager;

	public void tePrint(){
		if (te.from.suji != 0){						// 盤上の駒を動かした時
			Debug.Log (te.from.suji.ToString() + ", " + te.from.dan.ToString());
		}
		Debug.Log (te.to.suji.ToString() + ", " + te.to.dan.ToString());
		Debug.Log (komaStr [(byte)te.koma & ~(byte)Koma.Enemy]);
//		Debug.Log (te.koma);
		if (te.from.suji == 0) {
			Debug.Log ("打");
		} else if(te.promote == true){
			Debug.Log ("成");
		} else{
			Debug.Log(" ");
		}
	}

	public void Move(int teban, ref Te te){						// C#での参照渡しはref tebanは先手が0、後手が1

		Debug.Log ("Move te.from.dan : " + te.from.dan);
		Debug.Log ("Move te.from.suji : " + te.from.suji);
		Debug.Log ("Move te.to.dan : " + te.to.dan);
		Debug.Log ("Move te.to.suji : " + te.to.suji);

		Koma capture = board[te.to.dan, te.to.suji];					// 移動先にある駒は取られる

		if (te.from.suji != 0) {										// 盤上の駒を動かした場合
			board [te.from.dan, te.from.suji] = Koma.Empty;					// 元いた場所は空きに
		} else {														// 手駒を出した場合			
			hand [teban, (byte)te.koma & ~(byte)Koma.Enemy]--;				// 手駒が減る
		}

		if (te.promote) {												// 成った場合
			board [te.to.dan, te.to.suji] = te.koma | Koma.Promoted;		// ビットシフト(+8)して移動先に置かれる
		} else {														// 成らない場合
			if (teban == 0){												// 先手の場合
//				board [te.to.dan, te.to.suji] = te.koma;						// そのまま移動先に置かれる
				board [te.to.dan, te.to.suji] = te.koma & ~Koma.Enemy;						// そのまま移動先に置かれる
			}
			else {															// 後手の場合
				board [te.to.dan, te.to.suji] = te.koma | Koma.Enemy;			// ビットシフト(+16)して移動先に置かれる
			}
		}

		if (capture != Koma.Empty) {						// 相手の駒を取った場合
//			if (teban == 0) {
				hand [teban, (byte)capture & ~(byte)Koma.Enemy & ~(byte)Koma.Promoted]++;	//自分の手駒に加えられる(味方の駒になって成っている場合は戻る) ~->NOT
				// Hiyoko  = 0000 0001 -> 0000 0001 & 1110 1111 =  0000 0001 = Hiyoko
				// eHiyoko = 0001 0001 -> 0001 0001 & 1110 1111 =  0000 0001 = Hiyoko
				// Debug.Log ("hand" + ((byte)capture & ~(byte)Koma.Enemy & ~(byte)Koma.Promoted).ToString());
				Debug.Log ("hand[]" + (hand [teban, (byte)capture & ~(byte)Koma.Enemy & ~(byte)Koma.Promoted]).ToString());
				
//			} else {
//				hand [teban, (byte)capture | (byte)Koma.Enemy & ~(byte)Koma.Promoted]++;	//自分の手駒に加えられる(味方の駒になって成っている場合は戻る) ~->NOT
				// Hiyoko  = 0000 0001 -> 0000 0001 | 0001 0000 =  0001 0001 = eHiyoko 
				// eHiyoko = 0001 0001 -> 0001 0001 | 0001 0000 =  0001 0001 = eHiyoko
//				Debug.Log ("hand" + ((byte)capture | (byte)Koma.Enemy & ~(byte)Koma.Promoted).ToString());
//			}
			Debug.Log ("取った！" + ((byte)capture & ~(byte)Koma.Enemy & ~(byte)Koma.Promoted).ToString() );

		}

		if (hand [0, 4] == 1) {
			gameStatusManager.ChangePlayer1Won ();
		} else if (hand [1, 4] == 1) {
			gameStatusManager.ChangePlayer2Won ();
		}
	}

	// Use this for initialization
	void Start () {

		tebanManager = GameObject.Find ("TebanManager").GetComponent<TebanManager> ();
		tebanManager.isPlayer1Teban = true;
		gameStatusManager = GameObject.Find ("GameStatusManager").GetComponent<GameStatusManager> ();

		InitializeButtons ();

		komas [0] = Hiyoko.GetComponent<KomaController> ();
		komas [1] = Kirin.GetComponent<KomaController> ();
		komas [2] = Zou.GetComponent<KomaController> ();
		komas [3] = Lion.GetComponent<KomaController> ();
		komas [4] = eHiyoko.GetComponent<KomaController> ();
		komas [5] = eKirin.GetComponent<KomaController> ();
		komas [6] = eZou.GetComponent<KomaController> ();
		komas [7] = eLion.GetComponent<KomaController> ();

		Print ();

//		// 動かし方の例：Hiyoko(3,2)
//		te.from.dan = 0;
//		te.from.suji = 0;
//		te.to.dan = 3;
//		te.to.suji = 2;
//		te.koma = Koma.Hiyoko;
//		te.promote = false;
//		tePrint ();
//		Move (0, ref te);

	}
	
	// Update is called once per frame
	void Update () {

	}
		
	public void Print (){

		// 後手持ち駒
		Debug.Log ("後手持ち駒: ");
		for(koma=(int)Koma.Hiyoko; koma<=(int)Koma.Lion; koma++){
			if (hand [1, koma] == 1) {
				Debug.Log (komaStr [koma+4]);	//	敵駒名を表示するため+4した
			} else if (hand [1, koma] > 1) {
				Debug.Log (komaStr[koma] +","+ hand[1, koma]);
			}
		}

		Debug.Log ("        A     B     C");
		Debug.Log ("    -----------------");
		for(int dan = 1; dan <= 4; dan++){
			//			for(int suji = 1; suji <= 4; suji++){
			//				Debug.Log (board[dan, suji]);
			//			}
			Debug.Log ( dan + " | " + board[dan, 1] +", "+ board[dan, 2] +", "+ board[dan, 3] );
		}

		// 先手持ち駒
		Debug.Log ("先手持ち駒: ");
		for(koma=(int)Koma.Hiyoko; koma<=(int)Koma.Lion; koma++){
			if (hand [0, koma] == 1) {
				Debug.Log (komaStr [koma]);
			} else if (hand [0, koma] > 1) {
				Debug.Log (komaStr[koma] +","+ hand[0, koma]);
			}
		}

		KomaArrangement ();
	}

	void KomaArrangement(){	// 画面上の駒の配置

		// 盤上の駒の配置

		bool[] firstKoma = {true, true, true, true, true, true, true, true};	// 相手の駒を取ると同じ駒が2枚発生するためフラグで管理->もっといい感じに書く

		for (int dan = 1; dan <= 4; dan++) {
			for (int suji = 1; suji <= 3; suji++) {

                // Hiyoko & eHiyoko
                if (board[dan, suji] == Koma.Hiyoko && firstKoma[0])
                {

                    //Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x

                    if (komas[0].isPlayer1 && !komas[4].isPlayer1)
                    {   // P1: Hiyoko, P2: eHiyoko
                        Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン1-1");
                    }

                    else if (!komas[0].isPlayer1 && komas[4].isPlayer1)
                    {   // P1: eHiyoko, P2: Hiyoko
                        eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン2-1");
                    }

                    else if (komas[0].isPlayer1 && komas[4].isPlayer1)
                    {   // P1: Hiyoko, eHiyoko, P2: -
                        Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン3-1");
                        firstKoma[0] = false;
                    }

                    else if (!komas[0].isPlayer1 && !komas[4].isPlayer1)
                    {   // P1: -, P2: Hiyoko, eHiyoko
                        //Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン4-1 このパターンの可能性がない");
                    }
                }
                else if (board[dan, suji] == Koma.Hiyoko && !firstKoma[0])
                {
                    eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                }
                else if (board[dan, suji] == Koma.eHiyoko && firstKoma[4])
                {
                    //eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x

                    if (komas[0].isPlayer1 && !komas[4].isPlayer1)
                    {   // P1: Hiyoko, P2: eHiyoko
                        eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン1-2");
                    }

                    else if (!komas[0].isPlayer1 && komas[4].isPlayer1)
                    {   // P1: eHiyoko, P2: Hiyoko
                        Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン2-2");
                    }

                    else if (komas[0].isPlayer1 && komas[4].isPlayer1)
                    {   // P1: Hiyoko, eHiyoko, P2: -
                        //eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン3-2 このパターンの可能性がない");
                        //firstKoma[4] = false;
                    }

                    else if (!komas[0].isPlayer1 && !komas[4].isPlayer1)
                    {   // P1: -, P2: Hiyoko, eHiyoko
                        eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                        Debug.Log("パターン4-2");
                        firstKoma[4] = false;
                    }
                }
                else if (board[dan, suji] == Koma.eHiyoko && !firstKoma[4])
                {
                    Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                }  

                // Hiyoko
                //if (board[dan, suji] == Koma.Hiyoko && komas[0].isPlayer1 && !komas[4].isPlayer1)
                //{           // Player1がHiyoko1枚だけ持っているとき
                //    Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                //    Debug.Log("パターン1 Player1がHiyoko1枚(Player2:eHiyoko)");
                //}
                ////else if (board[dan, suji] == Koma.Hiyoko && !komas[0].isPlayer1 && komas[4].isPlayer1)
                ////{   // Player1がeHiyoko1枚だけ持っているとき
                ////    eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
                ////    Debug.Log("パターン2 Player1がeHiyoko1枚だけ持っているとき");
                ////}
                //else if (board[dan, suji] == Koma.Hiyoko && komas[0].isPlayer1 && komas[4].isPlayer1)
                //{   // Player1がHiyokoとeHiyokoの2枚を持っているとき
                //    if (firstKoma[0])
                //    {
                //        firstKoma[0] = false;
                //        Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                //    }
                //    else
                //    {
                //        firstKoma[0] = true;
                //        eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);  // suji, dan -> y, x
                //    }
                //    Debug.Log("パターン3 Player1がHiyokoとeHiyokoの2枚を持っているとき");
                //}
                //else if (board[dan, suji] == Koma.Hiyoko)
                //{   // Player1がHiyokoとeHiyokoの両方とも持っていないとき
                //    Debug.Log("パターン4 Player1がHiyokoとeHiyokoの両方とも持っていないとき");
                //    // eHiyoko
                //}
                //else if (board[dan, suji] == Koma.eHiyoko && komas[0].isPlayer1 && !komas[4].isPlayer1)
                //{           // Player1がHiyoko1枚だけ持っているとき
                //    eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);  // suji, dan -> y, x
                //    Debug.Log("パターン5");
                //}
                //else if (board[dan, suji] == Koma.eHiyoko && !komas[0].isPlayer1 && komas[4].isPlayer1)
                //{   // Player1がeHiyoko1枚だけ持っているとき
                //    eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                //    Debug.Log("パターン6 " + dan + "," + suji);
                //}
                //else if (board[dan, suji] == Koma.eHiyoko && komas[0].isPlayer1 && komas[4].isPlayer1)
                //{   // Player1がHiyokoとeHiyokoの2枚を持っているとき
                //    if (firstKoma[1])
                //    {
                //        firstKoma[1] = false;
                //        eHiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);  // suji, dan -> y, x
                //    }
                //    else
                //    {
                //        firstKoma[1] = true;
                //        Hiyoko.transform.localPosition = new Vector3(suji * scale, (5 - dan) * scale, 0);   // suji, dan -> y, x
                //    }
                //    Debug.Log("パターン7");
                //} else if (board[dan, suji] == Koma.eHiyoko){
                    //Debug.Log("パターン8");

				// Kirin
				//} else if (board [dan, suji] == Koma.Kirin && komas [1].isPlayer1 && !komas [5].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
                if (board [dan, suji] == Koma.Kirin && komas [1].isPlayer1 && !komas [5].isPlayer1) {            // Player1がHiyoko1枚だけ持っているとき
                    Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Kirin && !komas [1].isPlayer1 && komas [5].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Kirin && !komas [1].isPlayer1 && komas [5].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [2]) {
						firstKoma [2] = false;
						Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [2] = true;
						eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				// eKirin
				} else if (board [dan, suji] == Koma.eKirin && komas [1].isPlayer1 && !komas [5].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
					eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eKirin && !komas [1].isPlayer1 && komas [5].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eKirin && !komas [1].isPlayer1 && komas [5].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [3]) {
						firstKoma [3] = false;
						eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [3] = true;
						Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				// Zou
				} else if (board [dan, suji] == Koma.Zou && komas [2].isPlayer1 && !komas [6].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
					Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Zou && !komas [2].isPlayer1 && komas [6].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Zou && !komas [2].isPlayer1 && komas [6].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [4]) {
						firstKoma [5] = false;
						Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [4] = true;
						eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				// eZou
				} else if (board [dan, suji] == Koma.eZou && komas [2].isPlayer1 && !komas [6].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
					eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eZou && !komas [2].isPlayer1 && komas [6].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eZou && !komas [2].isPlayer1 && komas [6].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [5]) {
						firstKoma [5] = false;
						eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [5] = true;
						Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				// Lion
				} else if (board [dan, suji] == Koma.Lion && komas [3].isPlayer1 && !komas [7].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
					Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Lion && !komas [3].isPlayer1 && komas [7].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.Lion && !komas [3].isPlayer1 && komas [7].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [6]) {
						firstKoma [6] = false;
						Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [6] = true;
						eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				// eLion
				} else if (board [dan, suji] == Koma.eLion && komas [3].isPlayer1 && !komas [7].isPlayer1) {			// Player1がHiyoko1枚だけ持っているとき
					eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eLion && !komas [3].isPlayer1 && komas [7].isPlayer1) {	// Player1がeHiyoko1枚だけ持っているとき
					Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
				} else if (board [dan, suji] == Koma.eLion && !komas [3].isPlayer1 && komas [7].isPlayer1) {	// Player1がHiyokoとeHiyokoの2枚を持っているとき
					if (firstKoma [7]) {
						firstKoma [7] = false;
						eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					} else {
						firstKoma [7] = true;
						Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
					}
				}


//				if (board [dan, suji] == Koma.Hiyoko) {
//					if (!duplicate[0]) {	// ->2枚の駒の管理をもっといい感じに書く
//						Hiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						eHiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [0].isPlayer1 == komas [4].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[0] = true;
//					} else {
//						duplicate[0] = false;
//					}
//				} else if (board [dan, suji] == Koma.eHiyoko) {
//					if (!duplicate[1]) {
//						eHiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					} else {
//						Hiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					}
//					if (komas [0].isPlayer1 == komas [4].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[1] = true;
//					} else {
//						duplicate[1] = false;
//					}
//				} else if (board [dan, suji] == Koma.Kirin) {
////					Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[2]) {
//						Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [1].isPlayer1 == komas [5].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[2] = true;
//					} else {
//						duplicate[2] = false;
//					}
//				} else if (board [dan, suji] == Koma.eKirin) {
////					eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[3]) {
//						eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [1].isPlayer1 == komas [5].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[3] = true;
//					} else {
//						duplicate[3] = false;
//					}
//				} else if (board [dan, suji] == Koma.Zou) {
////					Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[3]) {
//						Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [2].isPlayer1 == komas [6].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[3] = true;
//					} else {
//						duplicate[3] = false;
//					}
//				} else if (board [dan, suji] == Koma.eZou) {
////					eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[3]) {
//						eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [2].isPlayer1 == komas [6].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[3] = true;
//					} else {
//						duplicate[3] = false;
//					}
//				} else if (board [dan, suji] == Koma.Lion) {
////					Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[4]) {
//						Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [3].isPlayer1 == komas [7].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[4] = true;
//					} else {
//						duplicate[4] = false;
//					}
//				} else if (board [dan, suji] == Koma.eLion) {
////					eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//					if (!duplicate[4]) {
//						eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					} else {
//						Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//					}
//					if (komas [3].isPlayer1 == komas [7].isPlayer1) {	// 同じ駒が2枚あれば
//						duplicate[4] = true;
//					} else {
//						duplicate[4] = false;
//					}
//				}

//				if (board [dan, suji].ToString() == komas[0].name) {
//					Hiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);	// suji, dan -> y, x
//				} else if (board [dan, suji].ToString() == komas[4].name) {
//					Debug.Log ("KomaArrangement, dan " + dan + " suji: " + suji + "eHiyoko");
//					Debug.Log ("komas[0] " + komas [0].isPlayer1);
//					Debug.Log ("komas[4] " + komas [4].isPlayer1);
//					Debug.Log ("komas[0].name" + komas[0].name);
//					Debug.Log ("komas[4].name" + komas[4].name);
//					eHiyoko.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[1].name) {
//					Kirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[5].name) {
//					eKirin.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[2].name) {
//					Zou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[6].name) {
//					eZou.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[3].name) {
//					Lion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				} else if (board [dan, suji].ToString() == komas[7].name) {
//					eLion.transform.localPosition = new Vector3 (suji * scale, (5 - dan) * scale, 0);
//				}

//				switch (board [dan, suji].ToString()){
//				case: 

			}
		}

        //画面上の手駒の配置 -> もっときれいに書く
        //Player1の手駒

        int count = 0;

        if (hand[0, 1] == 1)
        {   //Hiyokoを1枚持っているとき      
            eHiyoko.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eHiyoko.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[4].isPlayer1 = true;
            komas[4].ChangeKomaImage(); // Player1の画像に変更
            count++;
        }
        else if (hand[0, 1] > 1)
        {   //Hiyokoを2枚持っているとき              
            eHiyoko.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eHiyoko.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[4].isPlayer1 = true;
            komas[4].ChangeKomaImage(); // Player1の画像に変更
            count++;
            Hiyoko.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            count++;
        }

        if (hand[0, 2] == 1)
        {   //Kirinを1枚持っているとき   
            eKirin.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eKirin.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[5].isPlayer1 = true;
            komas[5].ChangeKomaImage(); // Player1の画像に変更
            count++;
        }
        else if (hand[0, 2] > 1)
        {   //Hiyokoを2枚持っているとき              
            eKirin.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eKirin.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[5].isPlayer1 = true;
            komas[5].ChangeKomaImage(); // Player1の画像に変更
            count++;
            Kirin.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            count++;
        }

        if (hand[0, 3] == 1)
        {   //Zouを1枚持っているとき 
            eZou.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eZou.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[6].isPlayer1 = true;
            komas[6].ChangeKomaImage(); // Player1の画像に変更
            count++;
        }
        else if (hand[0, 2] > 1)
        {   //Zouを2枚持っているとき             
            eZou.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eZou.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[6].isPlayer1 = true;
            komas[6].ChangeKomaImage(); // Player1の画像に変更
            count++;
            Zou.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            count++;
        }

        if (hand[0, 4] == 1)
        {   //Lionを1枚持っているとき    
            eLion.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eLion.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[7].isPlayer1 = true;
            komas[7].ChangeKomaImage(); // Player1の画像に変更
            count++;
        }
        else if (hand[0, 3] > 1)
        {   //Lionを2枚持っているとき                
            eLion.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            eLion.transform.localEulerAngles = new Vector3(0, 0, 0);
            komas[7].isPlayer1 = true;
            komas[7].ChangeKomaImage(); // Player1の画像に変更
            count++;
            Lion.transform.localPosition = new Vector3(4 * scale, (1 + count) * scale, 0);
            count++;
        }

        //Player2の手駒
        count = 0;

        if (hand[1, 1] == 1)
        {   //Hiyokoを1枚持っているとき      
            Hiyoko.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Hiyoko.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[0].isPlayer1 = false;
            komas[0].ChangeKomaImage(); // Player2の画像に変更
            count++;
        }
        else if (hand[1, 1] > 1)
        {   //Hiyokoを2枚持っているとき              
            Hiyoko.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Hiyoko.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[0].isPlayer1 = false;
            komas[0].ChangeKomaImage(); // Player2の画像に変更
            count++;
            eHiyoko.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            count++;
        }

        if (hand[1, 2] == 1)
        {   //Kirinを1枚持っているとき   
            Kirin.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Kirin.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[1].isPlayer1 = false;
            komas[1].ChangeKomaImage(); // Player2の画像に変更
            count++;
        }
        else if (hand[1, 2] > 1)
        {   //Hiyokoを2枚持っているとき              
            Kirin.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Kirin.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[1].isPlayer1 = false;
            komas[1].ChangeKomaImage(); // Player2の画像に変更
            count++;
            eKirin.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            count++;
        }

        if (hand[1, 3] == 1)
        {   //Zouを1枚持っているとき 
            Zou.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Zou.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[2].isPlayer1 = false;
            komas[2].ChangeKomaImage(); // Player2の画像に変更
            count++;
        }
        else if (hand[1, 2] > 1)
        {   //Zouを2枚持っているとき             
            Zou.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Zou.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[2].isPlayer1 = false;
            komas[2].ChangeKomaImage(); // Player2の画像に変更
            count++;
            eZou.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            count++;
        }

        if (hand[1, 4] == 1)
        {   //Lionを1枚持っているとき    
            Lion.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Lion.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[3].isPlayer1 = false;
            komas[3].ChangeKomaImage(); // Player2の画像に変更
            count++;
        }
        else if (hand[1, 3] > 1)
        {   //Lionを2枚持っているとき                
            Lion.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            Lion.transform.localEulerAngles = new Vector3(0, 0, 180);
            komas[3].isPlayer1 = false;
            komas[3].ChangeKomaImage(); // Player2の画像に変更
            count++;
            eLion.transform.localPosition = new Vector3(0 * scale, (4 - count) * scale, 0);
            count++;
        }


	}

	// 駒の名前を返す(文字列->Koma.xxxのenum形式)
	public Koma CheckKomaName(string _komaName){
		if (_komaName == "Hiyoko") {
			return Koma.Hiyoko;
		} else if (_komaName == "eHiyoko") {
			return Koma.eHiyoko;
		} else if (_komaName == "Kirin") {
			return Koma.Kirin;
		} else if (_komaName == "eKirin") {
			return Koma.eKirin;
		} else if (_komaName == "Zou") {
			return Koma.Zou;
		} else if (_komaName == "eZou") {
			return Koma.eZou;
		} else if (_komaName == "Lion") {
			return Koma.Lion;
		} else if (_komaName == "eLion") {
			return Koma.eLion;
		} else {
			Debug.Log ("Wrong argument value");
			return Koma.OutOfBoard;
		}
	}
		

	// 指定した駒が動けるかを返す
	public bool IsMovable(bool _isPlayer1, int _danButton, int _sujiButton){	// コマが動けるかどうかを返す(空きマス、敵コマがいるマス、範囲内のマス）
//		Debug.Log("_danButton" + _danButton);
//		Debug.Log("_sujiButton" + _sujiButton);


		if (_isPlayer1) {	//Player1の場合
			if ( board [_danButton, _sujiButton] == Koma.Empty
				|| ((byte)board [_danButton, _sujiButton] > (byte)Koma.Enemy && (byte)board [_danButton, _sujiButton] < (byte)Koma.OutOfBoard)) {	//空きマスか、敵コマがいるマス、範囲外でないマスなら進める
				Debug.Log("plyr1 (byte)board [_danButton, _sujiButton]" + (byte)board [_danButton, _sujiButton]);
				return true;	//移動可能
			} else {
				return false;	//移動不可
			}
		} else { //Player2の場合
			if ( (byte)board [_danButton, _sujiButton] < (byte)Koma.Enemy){	//空きマスか、敵コマがいるマス、範囲外でないマスなら進める
				Debug.Log("plyr2 (byte)board [_danButton, _sujiButton]" + (byte)board [_danButton, _sujiButton]);
				return true;	//移動可能
			} else {
				return false;	//移動不可
			}
		}
	}

	// ボタンの初期化
	void InitializeButtons(){
		// buttons[1,1]~[4,3]にbuttonを格納([0,x]、[x,0]はnull)
		for(int i = 0; i < buttonsA.Length; i++){
			buttons[1, i+1] = buttonsA[i];
		}
		for(int i = 0; i < buttonsB.Length; i++){
			buttons[2, i+1] = buttonsB[i];
		}
		for(int i = 0; i < buttonsC.Length; i++){
			buttons[3, i+1] = buttonsC[i];
		}

		for (int i = 1; i < buttons.GetLength (0); i++) {
			for (int j = 1; j < buttons.GetLength (1); j++) {
				DeactivateButton (j, i);	// suji, dan -> y, x
			}
		}
	}
		
	// 自分自身以外の全ての駒のisSelectedフラグのクリア
	public void ResetFlagOtherThanSelf(GameObject _obj){
		// isSelectedフラグのクリア
		foreach(KomaController i in komas){
			if (_obj.name.ToString() != i.name.ToString()) {
				i.isSelected = false;
			}
		}
	}

	// 全ての駒のisSelectedフラグのクリア
	public void ResetFlagAll(){
		foreach(KomaController i in komas){
			i.isSelected = false;
		}
	}

	// ボタンの有効化
	public void ActivateButton(int _suji, int _dan){	// suji, dan -> y, x
		if (!buttons [_dan, _suji].activeSelf) {
			buttons [_dan, _suji].SetActive (true);
		} else {
			buttons [_dan, _suji].SetActive (false);
		}
	}

	// 空きマスのボタンの有効化
	public void ActivateButtonInEmpty(){
		for(int i = 0; i < board.GetLength (0); i++){
			for (int j = 0; j < board.GetLength (1); j++) {
				if (board [i, j] == Koma.Empty) {
					ActivateButton (i, j);
				}
			}
		}
	}

	// ボタンの無効化
	public void DeactivateButton(int _suji, int _dan){	// suji, dan -> y, x
		buttons [_dan, _suji].SetActive (false);
	}

	// すべてのボタンの無効化
	public void DeactivateButtonAll(){
		for (int i = 1; i < buttons.GetLength (0); i++) {
			for (int j = 1; j < buttons.GetLength (1); j++) {
				DeactivateButton (j, i);	// suji, dan -> y, x
			}
		}
	}

	/// 移動ボタンをクリックした時の処理
	public void OnClickMoveButton(GameObject btn) {

		int posToSuji = (int)(btn.transform.position.x / scale) ;	//盤の座標をプログラムの座標に変換 suji, dan -> y, x
		int posToDan = -(int)(btn.transform.position.y / scale) + 5;	//盤の座標をプログラムの座標に変換 suji, dan -> y, x

		Debug.Log ("posToDan " + posToDan);		// suji, dan -> y, x
		Debug.Log ("posToSuji " + posToSuji);	// suji, dan -> y, x
			
		te.to.dan = posToDan;
		te.to.suji = posToSuji;

		te.promote = false;	//あとで分岐させる！！！！
		tePrint ();

        // KomaArrengementではなくここでUnity上の駒も移動したほうがいい？

		Move (tebanManager.isPlayer1Teban ? 0 : 1, ref te);
		tebanManager.isPlayer1Teban = !tebanManager.isPlayer1Teban;

		DeactivateButtonAll ();
		ResetFlagAll ();

		Print ();
	}

}
