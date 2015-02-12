using UnityEngine;
using System.Collections;

public class Equalizer : MonoBehaviour {

	public GameObject[] equalizers;

	private Material[] cachedMaterials;

	void Start() {
		// Create a material copy for each equalizer, for future color adjustments.
		cachedMaterials = new Material[equalizers.Length];
	}

	public void BlendColor(int i, float value) {
		if (i < 0 || i >= equalizers.Length) {
			Debug.LogWarning("Equalizer out of range: " + i);
			return;
		}

		Material mat = cachedMaterials[i];
		if (!mat) {
			// Time for a copy
			mat = equalizers[i].GetComponent<Renderer>().material;
		}

		// Get a unit color and scale it by the minimum element to ensure enough intensity,
		// then modify it by the input value
		Vector3 color = Random.onUnitSphere;
		float maxVal = 0;
		if (color.x > color.y && color.x > color.z)
			maxVal = color.x;
		else if (color.y > color.x && color.y > color.z)
			maxVal = color.y;
		else
			maxVal = color.z;
		color *= (1f / maxVal);
		color *= value;

		mat.color = new Color(color.x, color.y, color.z, .5f);
	}
}
