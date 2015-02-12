using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

public class EnsembleAPI : MonoBehaviour {

	List<AudioClip> clips = new List<AudioClip>();

	IEnumerator LoadFile(string path)
	{
		WWW www = new WWW("file://" + path);
		Debug.Log("loading " + path);
		
		AudioClip clip = www.GetAudioClip(false);
		while(clip.loadState != AudioDataLoadState.Loaded)
			yield return www;
		
		print("done loading");
		clip.name = Path.GetFileName(path);
		clips.Add(clip);

		Debug.Log("We've got a clip! It's " + clip + " length:" + clip.length);

		// Start the first clip automatically
		if (clips.Count == 1) {
			AudioSource audiosource = Camera.main.GetComponent<AudioSource>();
			audiosource.clip = clip;
			audiosource.Play();
		}
	}

	// Use this for initialization
	void Start () {

		EnsembleConfig config = new EnsembleConfig();

		config.LoadJSON(Application.dataPath + "/StreamingAssets/" + "config.json");

		// Load music files
		//string[] files = Directory.GetFiles(Application.dataPath + "/StreamingAssets/Music", "*.ogg");
		string[] files = config.musicFiles;
		Debug.Log("Got " + files.Length + " files");
		for (int i = 0; i < files.Length; i++) {
			string file = files[i];
			StartCoroutine(LoadFile(Application.dataPath + "/StreamingAssets/" + file));
		}

		WebRequest request = HttpWebRequest.Create(config.compositionsEndpoint);
		WebResponse response = request.GetResponse();

		for (int i = 0; i < response.Headers.Count; i++) {
			string val = response.Headers.GetKey(i) + ":";
			foreach (string header in response.Headers.GetValues(i)) {
				val += header + ":";
			}
			Debug.Log (val);
		}

		using (StreamReader reader = new StreamReader(response.GetResponseStream())) {

			string contents = reader.ReadToEnd();
			Debug.Log(contents);

			SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(contents);
			Debug.Log("Count: " + node.Count);

			foreach (SimpleJSON.JSONNode child in node.Children) {
				Debug.Log("name:" + child["name"]);
				Debug.Log("tempo:" + child["tempo"].AsFloat);
				Debug.Log("created_by:" + child["created_by"]);
				Debug.Log("id:" + child["id"]);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
