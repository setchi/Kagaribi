﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;

public class Pattern : MonoBehaviour {
	static float popDepth = 200;
	
	static Vector3 RandomVectorFromRange(float min, float max) {
		return new Vector3(
			UnityEngine.Random.Range(min, max),
			UnityEngine.Random.Range(min, max),
			UnityEngine.Random.Range(min, max)
		);
	}

	static Vector3 DirectionFromDegree(float degree) {
		return Quaternion.AngleAxis(degree, Vector3.forward) * Vector3.up;
	}

	static void Repeat(int max, Action<int> action) {
		for (int i = 0; i < max; ++i)
			action(i);
	}


	public class Target {
		static float switchDelay = 1f;
		
		public static IEnumerator Circular(float r, float time, int resolution, int rotateDir) {
			yield return new WaitForSeconds(switchDelay);
			var counter = 0;
			var maxChain = 2;

			while (true) {
				Repeat (maxChain, chain => {
					var degree = 360f * ((float)counter / resolution);
					var direction = DirectionFromDegree(degree + chain * 2 * rotateDir);
					var basePos = direction * r;
					var pos = basePos + RandomVectorFromRange(-3, 3) / 100;
					pos.z = popDepth + chain * 1.5f;
					
					SquareGenerator.PopTarget(pos, Quaternion.identity);
				});
				
				counter = ++counter % resolution;
				
				yield return new WaitForSeconds((time * (1 / Time.timeScale)) / resolution);
			}
		}
		
		public static IEnumerator RandomPosition(float range, float interval, float ignoreRange = 0) {
			yield return new WaitForSeconds(switchDelay);
			int maxChain = 3;
			
			while (true) {
				Vector3 basePos;
				do {
					basePos = RandomVectorFromRange(-range, range);
				} while (Mathf.Abs(basePos.x) < ignoreRange || Mathf.Abs(basePos.y) < ignoreRange);
				
				Repeat (maxChain, chain => {
					var pos = basePos + RandomVectorFromRange(-3, 3) / 100;
					pos.z = popDepth + chain * 2f;

					SquareGenerator.PopTarget(pos, Quaternion.identity);
				});
				
				yield return new WaitForSeconds(interval);
			}
		}
	}


	public class Background {
		static float switchDelay = 0.5f;
		
		public static IEnumerator Circular(float r, float interval, int resolution, int rotateDir, int multiplicity, Tuple<Color, Color> colorPair) {
			yield return new WaitForSeconds(switchDelay);
			GameObject tailEnd = null;
			var counter = 0;
			
			while (true) {
				var tailEndZ = tailEnd == null ? popDepth - interval * 2 : tailEnd.transform.position.z;

				while ((tailEndZ += interval) < popDepth) {
					var percentage = (float)counter / resolution;
					var degree = 360f * percentage * rotateDir;
					
					Repeat (multiplicity, i => {
						var color = Color.Lerp(
							colorPair.Item1,
							colorPair.Item2,
							Mathf.PingPong(percentage * 5, 1)
						);

						var direction = DirectionFromDegree(degree + i * (360f / multiplicity));
						var pos = direction * r;
						pos.z = tailEndZ;
						tailEnd = SquareGenerator.PopBackground(pos, Quaternion.identity, color);
					});

					counter = ++counter % resolution;
				}
				
				yield return new WaitForEndOfFrame();
			}
		}
		
		public static IEnumerator RandomPositionAndScale(float min, float max, Tuple<Color, Color> colorPair) {
			yield return new WaitForSeconds(switchDelay);
			bool collerToggle = false;
			
			while (true) {
				Vector3 pos;
				do {
					pos = RandomVectorFromRange(min, max);
				} while (Mathf.Abs(pos.x) < 4 && Mathf.Abs(pos.y) < 4);
				
				pos.z = popDepth;
				var color = (collerToggle = !collerToggle) ? colorPair.Item1 : colorPair.Item2;
				var square = SquareGenerator.PopBackground(pos, Quaternion.identity, color);
				var scale = RandomVectorFromRange(0.5f, 2);
				square.transform.localScale = scale;
				
				yield return new WaitForSeconds(0.001f);
			}
		}
		
		public static IEnumerator Polygon(float r, float interval, int length, int multiplicity, Tuple<Color, Color> colorPair) {
			yield return new WaitForSeconds(switchDelay);
			GameObject tailEnd = null;
			var counter = 0;
			
			while (true) {
				var tailEndZ = tailEnd == null ? popDepth - interval * 2 : tailEnd.transform.position.z;

				while ((tailEndZ += interval) < popDepth) {
					var percentage = (float)counter / length;

					Repeat (multiplicity, i => {
						var color = Color.Lerp(
							colorPair.Item1,
							colorPair.Item2,
							Mathf.PingPong(percentage * multiplicity, 1)
						);

						var pos = Vector3.Lerp(
							DirectionFromDegree(i * (360f / multiplicity)),
							DirectionFromDegree((i + 2) * (360f / multiplicity)),
							percentage
						) * r;

						pos.z = tailEndZ;

						tailEnd = SquareGenerator.PopBackground(pos, Quaternion.identity, color);
						tailEnd.transform.localScale = Vector3.one * 0.5f;
					});

					counter = ++counter % length;
				}

				yield return new WaitForEndOfFrame();
			}
		}
	}
}
