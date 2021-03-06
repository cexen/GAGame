﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AMtest : MonoBehaviour {
    int[] score;
    GameObject[] scoreobject;
    public GameObject element, kao;
    GeneManager.ViewParam vp = GeneManager.viewParam; // UI表示用のグローバルパラメタの参照（短いあだ名をつけてるだけ）
    public AMElement mcamera; // cameraが予約語っぽいのでmaincameraの意

    //遺伝子の配列情報（-1,0,1を適当な色に置き換えておく）
    int[][] colorarray;
    // 親の数
    int parentNum;
    const int TURN = 20;

    GameObject[] geneObject;

    //親となる遺伝子の（子を作る）順番を記した配列
    int[] fatherarray,motherarray;

    GameObject father,mother,child;


    // Use this for initialization
    IEnumerator Start () {
        // 親の数を求める
        Debug.Log(GeneManager.param.selectionMode);
        if (GeneManager.param.selectionMode == 0) parentNum = System.Math.Min(36, GeneManager.param.playerNum);
        else if (GeneManager.param.selectionMode == 1) parentNum = 10;
        else parentNum = (int)(GeneManager.param.playerNum * 0.2);
        // 偶数にしておく
        if (parentNum % 2 == 1) parentNum++;
        // input
        score = new int[parentNum];
        for (int i = 0; i < parentNum; i++)
        {
            score[i] = Random.Range(0, 50);
        }
        System.Array.Sort(score);
        System.Array.Reverse(score);
        GeneManager.Player[] players = new GeneManager.Player[GeneManager.param.playerNum];
        GeneManager.players.CopyTo(players, 0);
		System.Array.Sort (players, 
			delegate(GeneManager.Player p1, GeneManager.Player p2) {
				return p2.score.CompareTo (p1.score);
			}
		);
        colorarray = new int[parentNum][];
        for (int i = 0; i < parentNum; i++) {
            int div = GeneManager.param.playFrame/30;
            colorarray[i] = new int[30];
            for (int j = 0; j < 30; j++)
            {
                //colorarray[i][j] = Random.Range(-1, 1+1);
                colorarray[i][j] = players[i].gene[j*div];
            }
        }
        //親の遺伝子集団の作成(並べるだけ)
        geneObject = new GameObject[parentNum];
        for (int i = 0; i < parentNum/2; i++)
        {
            geneObject[i] = new GameObject();
            geneObject[i] = Instantiate(element, new Vector3 (0, 0, i*15), Quaternion.identity) as GameObject;
            StartCoroutine(geneObject[i].GetComponent<AMGroup>().move(new Vector3(0, 0, (i-10)*15), 0f));
            geneObject[i].GetComponent<AMGroup>().setColor(colorarray[i]);
            geneObject[i].GetComponent<AMGroup> ().setScore (score[i]);
        }
        for (int i=parentNum/2; i<parentNum; i++){
            geneObject[i] = new GameObject();
            geneObject[i] = Instantiate(element, new Vector3 (320, 0, (i-parentNum/2)*15), Quaternion.identity) as GameObject;
            StartCoroutine(geneObject[i].GetComponent<AMGroup>().move(new Vector3(320, 0, (i-10-parentNum/2)*15), 0f));
            geneObject[i].GetComponent<AMGroup> ().setScore (score[i]);
            geneObject[i].GetComponent<AMGroup> ().setColor (colorarray [i]);
        }
        fatherarray = new int[TURN];
        motherarray = new int[TURN];
        for (int i = 0; i < TURN; i++) motherarray[i] = Random.Range(0, parentNum);
        for (int i = 0; i < TURN; i++) fatherarray[i] = Random.Range(0, parentNum);
        //交叉の開始
        yield return StartCoroutine ("cross");
        // カメラ移動テスト
        //yield return StartCoroutine(mcamera.move(mcamera.transform.position + new Vector3(0, 0, -500), 3f/vp.playSpeed));
        yield return new WaitForSeconds(3f/vp.playSpeed); // ちょっと待ってからSCに遷移
        SceneManager.LoadScene("SakeruCheese");
    }
    private IEnumerator cross ()
    {
        //親の移動
        for (int i = 0; i < TURN; i++)
        {
            if (fatherarray[i] == motherarray[i]) motherarray[i] = (fatherarray[i]+1)%parentNum;
            father = geneObject[fatherarray[i]];
            mother = geneObject[motherarray[i]];
            Vector3 fatherDestination;
            Vector3 motherDestination;
            Vector3 faceDestination;
            if (i < TURN/2)
            {
                fatherDestination = new Vector3(0, 0, -200 - i * 15);
                motherDestination = new Vector3(0, 0, -170 - i * 15);
            } else
            {
                fatherDestination = new Vector3(320, 0, -200 - (i-TURN/2) * 15);
                motherDestination = new Vector3(320, 0, -170 - (i-TURN/2) * 15);
            }
            faceDestination = (fatherDestination + motherDestination) / 2;
            yield return StartCoroutine(father.GetComponent<AMGroup>().move(fatherDestination, 0.5f / vp.playSpeed));
            yield return StartCoroutine(mother.GetComponent<AMGroup>().move(motherDestination, 0.5f/vp.playSpeed));
            // 突然変位用配列
            int[] vv = new int[30];
            for (int j = 0; j < 30; j++)
            {
                if (Random.Range(0.0f, 1.0f) < GeneManager.param.mutationRate) vv[j] = 1;
                else vv[j] = 0;
            }
            if (GeneManager.param.crossingMode == 0)
            {
                //交叉ポイント
                int r = Random.Range(1, 29);
                GameObject go1 = father.GetComponent<AMGroup>().getSegment(0, r);
                GameObject go2 = mother.GetComponent<AMGroup>().getSegment(r + 1, 30 - 1);
                GameObject face = Instantiate(kao, faceDestination, Quaternion.identity) as GameObject;
                yield return StartCoroutine(go1.GetComponent<AMGenePieces>().move(new Vector3(12, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                yield return StartCoroutine(go2.GetComponent<AMGenePieces>().move(new Vector3(12 + (r+1) * 10, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                GameObject[] gogo = { go1, go2 };
                yield return StartCoroutine(blink(gogo, vv, 0.5f/vp.playSpeed));
                colorChange(gogo, vv);
                //yield return StartCoroutine(fadeOut(face, gogo));
            }
            else if (GeneManager.param.crossingMode == 1)
            {
                // 交叉ポイント
                int r = Random.Range(2, 27), q = Random.Range(r + 1, 29);
                GameObject go1 = father.GetComponent<AMGroup>().getSegment(0, r);
                GameObject go2 = mother.GetComponent<AMGroup>().getSegment(r + 1, q);
                GameObject go3 = father.GetComponent<AMGroup>().getSegment(q + 1, 30 - 1);
                GameObject face = Instantiate(kao, faceDestination, Quaternion.identity) as GameObject;
                yield return StartCoroutine(go1.GetComponent<AMGenePieces>().move(new Vector3(12, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                yield return StartCoroutine(go2.GetComponent<AMGenePieces>().move(new Vector3(12 + (r+1) * 10, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                yield return StartCoroutine(go3.GetComponent<AMGenePieces>().move(new Vector3(12 + (q+1) * 10, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                GameObject[] gogo = { go1, go2, go3 };
                yield return StartCoroutine(blink(gogo, vv, 0.5f/vp.playSpeed));
                colorChange(gogo, vv);
                //yield return StartCoroutine(fadeOut(face, gogo));
            }
            else if (GeneManager.param.crossingMode == 2)
            {
                int sz = 0;
                int[] vs = new int[6];
                vs[0] = -1;
                for (int j = 1; j <= 5; j++)
                {
                    vs[j] = Random.Range(vs[j - 1]+1, 30);
                    sz++;
                    if (vs[j] == 29) break;
                }
                vs[sz] = 29;
                GameObject[] go = new GameObject[sz];
                for (int j = 0; j < sz; j++)
                {
                    if (j % 2 == 0)
                    {
                        go[j] = father.GetComponent<AMGroup>().getSegment(vs[j]+1, vs[j + 1]);
                    }
                    else
                    {
                        go[j] = mother.GetComponent<AMGroup>().getSegment(vs[j]+1, vs[j + 1]);
                    }
                }
                GameObject face = Instantiate(kao, faceDestination, Quaternion.identity) as GameObject;
                for (int j = 0; j < sz; j++)
                {
                    yield return StartCoroutine(go[j].GetComponent<AMGenePieces>().move(new Vector3(12+(vs[j]+1)*10, 0, 0) + faceDestination, 0.2f/vp.playSpeed));
                }
                yield return StartCoroutine(blink(go, vv, 0.5f/vp.playSpeed));
                colorChange(go, vv);
                //yield return StartCoroutine(fadeOut(face, go));
            }
            if (fatherarray[i] < parentNum/2)
            {
                yield return StartCoroutine(father.GetComponent<AMGroup>().move(new Vector3(0, 0, (fatherarray[i]-10) * 15), 0f));
            }
            else
            {
                yield return StartCoroutine(father.GetComponent<AMGroup>().move(new Vector3(320, 0, (fatherarray[i] - 10 - parentNum/2) * 15), 0f));
            }
            if (motherarray[i] < parentNum/2)
            {
                yield return StartCoroutine(mother.GetComponent<AMGroup>().move(new Vector3(0, 0, (motherarray[i] - 10) * 15), 0f));
            }
            else
            {
                yield return StartCoroutine(mother.GetComponent<AMGroup>().move(new Vector3(320, 0, (motherarray[i] - 10 - parentNum/2) * 15), 0f));
            }
        }
    }
    private void colorChange(GameObject[] gps, int[] vs)
    {

        int length = gps.Length;
        int now = 0;
        for (int i = 0; i < length; i++)
        {
            int len = gps[i].GetComponent<AMGenePieces>().Length();
            for (int j = 0; j < len; j++)
            {
                if (vs[now++] == 1)
                {
                    int random = Random.Range(0, 3);
                    if (random == 0) gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().material.color = Color.red;
                    else if (random == 1) gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().material.color = Color.green;
                    else gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().material.color = Color.blue;
                }
            }
        }
    }
    // vs は 30 要素の配列で 0 or 1
    // gps は 点滅する可能性のあるオブジェクト
    private IEnumerator blink(GameObject[] gps, int[] vs, float t)
    {
        float remainTime = t;
        int length = gps.Length;
        while (true)
        {
            if (remainTime < 0)
            {
                for (int i = 0; i < length; i++)
                {
                    int len = gps[i].GetComponent<AMGenePieces>().Length();
                    for (int j = 0; j < len; j++)
                    {
                        gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().enabled = true;
                    }
                }
                break;
            }
            int now = 0;
            for (int i = 0; i < length; i++)
            {
                int len = gps[i].GetComponent<AMGenePieces>().Length();
                for (int j = 0; j < len; j++)
                {
                    if (vs[now++] == 1) gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().enabled = !gps[i].GetComponent<AMGenePieces>().get(j).GetComponent<Renderer>().enabled;
                }
            }
            remainTime -= 0.05f / vp.playSpeed;
            yield return new WaitForSeconds(0.05f / vp.playSpeed);
        }
        yield break;
    }
    // 交差専用
    // 同時に複数の GenePieces と頭を自然消滅させる
    private IEnumerator fadeOut(GameObject face, GameObject[] gps)
    {
        float fadeTime = 0.2f / vp.playSpeed;
        float currentRemainTime = fadeTime;
        float interval = AMCommon.interval;
        int sz = gps.Length;
        float buffer = 0f; // getIntervalに渡すやつ
        while (true)
        {
            currentRemainTime -= interval;
            // Debug.Log(currentRemainTime);
            if (currentRemainTime <= 0f)
            {
                Destroy(face);
                for (int i = 0; i < sz; i++)
                {
                    gps[i].GetComponent<AMGenePieces>().delete();
                    Destroy(gps[i]);
                }
                yield break;
            }
            float alpha = currentRemainTime / fadeTime;
            face.GetComponent<AMElement>().setAlpha(alpha);
            for (int i = 0; i < sz; i++) gps[i].GetComponent<AMGenePieces>().setAlpha(alpha);
            yield return new WaitForSeconds(AMCommon.getInterval(interval, ref buffer));
        }
    }

    public void Skip()
    {
        SceneManager.LoadScene("SakeruCheese");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            SceneManager.LoadScene("GameMain");
        }
    }
}
