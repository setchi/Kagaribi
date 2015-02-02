﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;

public class PlayerLife : MonoBehaviour {
	public PlayerMovement playerMovement;
	public HealthUI healthUI;
	public int startingHealth = 5;

	int currentLife;
	float invincibleTime = 1.0f;
	bool isInvincible = false;
	bool isDead_ = false;
	public bool isDead { get { return isDead_; } }

	void Awake() {
		currentLife = startingHealth;
	}

	public void TakeDamage() {
		if (isInvincible || isDead) {
			return;
		}
		StartCoroutine(StartInvincibleTime());

		currentLife--;
		healthUI.SetLife(currentLife);

		if (currentLife <= 0) {
			Death();
		}
	}

	void Death() {
		isDead_ = true;
		playerMovement.enabled = false;
	}

	IEnumerator StartInvincibleTime() {
		isInvincible = true;
		yield return new WaitForSeconds(invincibleTime);
		isInvincible = false;
	}
}
