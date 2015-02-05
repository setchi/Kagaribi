﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RankingGUI : MonoBehaviour {
	public GameObject rankingPanel;
	public GameObject rankingTable;
	public GameObject rankRecordPrefab;
	public InputField nameInputField;
	public Text messageText;

	List<GameObject> rankingTableContents = new List<GameObject>();

	void Awake () {
		Hide();

		var localData = LocalData.Read();
		// ローカルにユーザ情報が無ければ新規ID取得
		if (localData.playerInfo == null) {
			FetchPlayerId();
		}
	}
	
	void Retry(float waitTime, Action action) { StartCoroutine(StartRetry(waitTime, action)); }
	IEnumerator StartRetry(float waitTime, Action action) {
		yield return new WaitForSeconds(waitTime);
		action();
	}

	void FetchPlayerId() {
		API.CreatePlayerId(playerInfo => {
			var localData = LocalData.Read();
			localData.playerInfo = playerInfo;
			LocalData.Write(localData);

		}, www => Retry(3f, () => FetchPlayerId()));
	}

	void FetchRanking() {
		API.FetchRanking(records => {
			// テーブルの内容をリセット
			rankingTableContents.ForEach(obj => DestroyObject(obj));
			rankingTableContents.Clear();

			DispRanking(records);
		}, www => Retry(5f, () => FetchRanking()));
	}

	void DispRanking(JsonModel.Record[] records) {
		var playerInfo = LocalData.Read().playerInfo;
		var selfPlayerId = playerInfo == null ? "" : playerInfo.id;

		foreach (var elem in records.Select((record, index) => new {record, index})) {
			var record = (GameObject) Instantiate(rankRecordPrefab);
			record.transform.SetParent(rankingTable.transform);
			record.transform.localScale = Vector3.one;
			record.GetComponent<RankRecord>().Set(elem.index + 1, elem.record, selfPlayerId);
			rankingTableContents.Add(record);
		}
	}

	public void Show() {
		FetchRanking();
		nameInputField.text = LocalData.Read().playerInfo.name;
		messageText.text = "";
		rankingPanel.transform.localPosition = Vector3.one;
	}

	public void Hide() {
		rankingPanel.transform.localPosition = new Vector3(0, -100000, 0);
	}

	public void OnClickNameChangeButton() {
		var name = nameInputField.text;

		var maxLength = 8;
		if (name == "" || name.Length > maxLength) {
			messageText.text = "名前は1~" + maxLength.ToString() + "文字で入力してください";
			return;
		}

		var playerInfo = LocalData.Read().playerInfo;
		if (playerInfo == null) {
			messageText.text = "通信環境の良い場所でもう一度お試しください";
			return;
		}

		playerInfo.name = name;
		UpdatePlayerName(playerInfo);
	}
	
	void UpdatePlayerName(JsonModel.PlayerInfo playerInfo) {
		API.UpdatePlayerName(playerInfo, () => {
			messageText.text = "名前を変更しました";
			
			var localData = LocalData.Read();
			localData.playerInfo = playerInfo;
			LocalData.Write(localData);

			// ランキングテーブル再取得
			FetchRanking();
			
		}, www => {
			messageText.text = "通信に失敗しました";
		});
	}
}
