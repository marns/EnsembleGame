using UnityEngine;
using System.Collections;

public class SoundProcessor : MonoBehaviour {

	public float rangeLow = 0;
	public float rangeHigh = 1;
	public float scale = 10;

	float[] samples = new float[1024];

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		AudioListener.GetSpectrumData(samples, 0, FFTWindow.Hamming);

		double total = 0;
		for (int i = (int)(rangeLow*samples.Length); i < (int)(rangeHigh*samples.Length); i++) {
			total += samples[i];
		}
		//total /= samples.Length;

		transform.localScale = new Vector3(transform.localScale.x, (float)total * scale, transform.localScale.z);
	}
}
