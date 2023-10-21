using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class LightProbePlacement : EditorWindow {

	static float progress = 0.0f;
	static string current = "Hello";
	static bool working = false;

	float mergeDistance = 1;
	GameObject probeObject;
	bool disableMerging;

	[MenuItem ("Window/Generate Light Probes")]
	static void Init() {
		EditorWindow window = GetWindow (typeof(LightProbePlacement));
		window.Show ();
	}

	void PlaceProbes () {
		GameObject probe = probeObject;
		if(probe != null) {
			LightProbeGroup p = probe.GetComponent<LightProbeGroup>();

			if(p != null) {
					
				working = true;

				progress = 0.0f;
				current = "Triangulating navmesh...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);


				probe.transform.position = Vector3.zero;

				UnityEngine.AI.NavMeshTriangulation navMesh = UnityEngine.AI.NavMesh.CalculateTriangulation ();


				current = "Generating necessary lists...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				Vector3[] newProbes = navMesh.vertices;
				List<Vector3> probeList = new List<Vector3>(newProbes);
				List<ProbeGenPoint> probeGen = new List<ProbeGenPoint>();

				foreach(Vector3 pg in probeList) {
					probeGen.Add (new ProbeGenPoint(pg, false));
				}

				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				List<Vector3> mergedProbes = new List<Vector3>();

				int probeListLength = newProbes.Length;

				int done = 0;
				foreach(ProbeGenPoint pro in probeGen) {
					if(pro.used == false) {
						current = "Checking point at " + pro.point.ToString ();
						progress = (float)done / (float)probeListLength;
						EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
						List<Vector3> nearbyProbes = new List<Vector3>();
						nearbyProbes.Add (pro.point);
						pro.used = true;
						if (!disableMerging) {
							foreach(ProbeGenPoint pp in probeGen) {
								if(pp.used == false) {
									current = "Checking point at " + pro.point.ToString ();
									//EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
									if(Vector3.Distance (pp.point, pro.point) <= mergeDistance) {
										pp.used = true;
										nearbyProbes.Add (pp.point);
									}
								}
							}
						}

						Vector3 newProbe = new Vector3();
						foreach(Vector3 prooo in nearbyProbes) {
							newProbe += prooo;
						}
						newProbe /= nearbyProbes.ToArray ().Length;
						newProbe += Vector3.up;

						mergedProbes.Add (newProbe);
						done += 1;
						//Debug.Log ("Added probe at point " + newProbe.ToString ());
					}
				}

				/*for(int i=0; i<newProbes.Length; i++) {
					newProbes[i] = newProbes[i] + Vector3.up;
				}*/


				current = "Final steps...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
				
				p.probePositions = mergedProbes.ToArray ();
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				working = false;


			} else {
				EditorUtility.DisplayDialog("Error", "Probe object does not have a Light Probe Group attached to it", "OK");
			}

		} else {
			EditorUtility.DisplayDialog("Error", "Probe object not set", "OK");
		}
	}

	void OnGUI() {

		if(GUILayout.Button("Generate probes")) {
			PlaceProbes ();
		}
		mergeDistance = EditorGUILayout.FloatField ("Vector merge distance",mergeDistance);
		disableMerging = EditorGUILayout.Toggle ("Disable merging", disableMerging);
		probeObject = (GameObject)EditorGUILayout.ObjectField ("Probe GameObject" , probeObject, typeof(GameObject), true);
		EditorGUILayout.LabelField ("This script will automatically generate light probe positions based on the current navmesh.");
		EditorGUILayout.LabelField ("Please make sure that you have generated a navmesh before using the script.");
		EditorGUILayout.LabelField ("If your navmesh is very large or complex, try using 'Disable Merging' to tremendously speed up the process. Keep in mind this may produce more probes than necessary, which may negatively impact performance.");

		if(working) {
			EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
		} else {
			EditorUtility.ClearProgressBar ();
		}
	}

	void OnInspectorUpdate() {
		Repaint();
	}

}

public class ProbeGenPoint {

	public Vector3 point;
	public bool used = false;

	public ProbeGenPoint(Vector3 p, bool u) {
		point = p;
		used = u;
	}

}
