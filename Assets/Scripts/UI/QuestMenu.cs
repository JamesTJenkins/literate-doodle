using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// If we want to go ham with quests we probably should switch this to using scriptableobjects instead
// and have better way of checking if quest conditions are met but this should do for now

public class QuestMenu : MonoBehaviour {
	[SerializeField] private Transform content;
	[SerializeField] private GameObject questTextPrefab;
	[SerializeField] private float showTime;

	private Dictionary<string, GameObject> questBoxes = new();
	private bool inCoroutine = false;

	private void Start() {
		PlayerEvents.showQuestsStart += StartShow;
		PlayerEvents.showQuestsStopped += TempShow;
		PlayerEvents.itemPickedUp += OnItemPickup;

		CreateQuest(Consts.Quests.BOOK_OF_DEATH, Consts.Quests.INITIAL_QUEST);
	}

	private void OnDestroy() {
		PlayerEvents.showQuestsStart -= StartShow;
		PlayerEvents.showQuestsStopped -= TempShow;
		PlayerEvents.itemPickedUp -= OnItemPickup;
	}

	private void StartShow() {
		content.gameObject.SetActive(true);
	}

	private void TempShow() {
		if (inCoroutine) {
			StopCoroutine(TempShowQuests());
		}

		StartCoroutine(TempShowQuests());
	}

	private IEnumerator TempShowQuests() {
		inCoroutine = true;

		yield return new WaitForSecondsRealtime(showTime);

		inCoroutine = false;
		content.gameObject.SetActive(false);
	}

	private void OnItemPickup(string itemName) {
		RemoveQuest(itemName);

		if (itemName == Consts.Quests.BOOK_OF_DEATH) {
			CreateQuest(Consts.Quests.ESCAPE_QUEST, Consts.Quests.ESCAPE_QUEST);
			PlayerEvents.OnEscapeEnabled();
		}
	}

	private void CreateQuest(string questKey, string text) {
		GameObject go = Instantiate(questTextPrefab, content);
		go.GetComponent<TMP_Text>().text = text;
		questBoxes.Add(questKey, go);
		StartShow();
		TempShow();
	}

	private void RemoveQuest(string questKey) {
		if (questBoxes.ContainsKey(questKey)) {
			GameObject go = questBoxes[questKey].gameObject;
			questBoxes.Remove(questKey);
			Destroy(go);
		}
	}
}
