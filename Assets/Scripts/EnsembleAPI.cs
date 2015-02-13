using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

using UnityEngine.Audio;

public class EnsembleAPI : MonoBehaviour {

	class NoteData {
		public string sensor;
		public float timestamp = 0;
		public float value = 0;
	}

	public AudioMixer audioMixer;
	public Animator animator;
	public Equalizer equalizer;

	public ParticleSystem[] fireworks;

	List<AudioClip> clips = new List<AudioClip>();
	List<NoteData> notes = new List<NoteData>();

	private float playbackTime = 0;
	private int currentNote = 0;

	private EnsembleConfig _config;

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

		_config = new EnsembleConfig();
		_config.LoadJSON(Application.streamingAssetsPath + "/config.json");

		// Load music files
		//string[] files = Directory.GetFiles(Application.dataPath + "/StreamingAssets/Music", "*.ogg");
		string[] files = _config.musicFiles;
		Debug.Log("Got " + files.Length + " files");
		//for (int i = 0; i < files.Length; i++) {

		// Load only the first file.
		if (files.Length > 0) {
			string file = files[0];
			StartCoroutine(LoadFile(Application.streamingAssetsPath + "/" + file));
		}

		Stream inputStream = null;
		if (!string.IsNullOrEmpty(_config.compositionFile)) {
			inputStream = new FileStream(Application.streamingAssetsPath + "/" + _config.compositionFile, FileMode.Open);
		} else {
			WebRequest request = HttpWebRequest.Create(_config.ensembleApi + "Compositions");
			WebResponse response = request.GetResponse();

			for (int i = 0; i < response.Headers.Count; i++) {
				string val = response.Headers.GetKey(i) + ":";
				foreach (string header in response.Headers.GetValues(i)) {
					val += header + ":";
				}
				Debug.Log (val);
			}

			int lastId = -1;
			using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
				
				string contents = reader.ReadToEnd();
				Debug.Log(contents);
				
				SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(contents);
				Debug.Log("Count: " + node.Count);
				
				foreach (SimpleJSON.JSONNode child in node.Children) {
					//Debug.Log("name:" + child["name"]);
					//Debug.Log("tempo:" + child["tempo"].AsFloat);
					//Debug.Log("created_by:" + child["created_by"]);
					Debug.Log("id:" + child["id"]);
					lastId = child["id"].AsInt;
				}
			}
			if (_config.compositionId < 0) {
				// Get latest ID
				Debug.Log("Using latest composition ID: " + lastId);
				_config.compositionId = lastId;
			}
			
			// Get notes from composition
			// Pull note data
			Debug.Log("GET NOTES! From composition: " + _config.compositionId);
			request = HttpWebRequest.Create(_config.ensembleApi + "Compositions/" + _config.compositionId + "/notes");
			response = request.GetResponse();
			
			for (int i = 0; i < response.Headers.Count; i++) {
				string val = response.Headers.GetKey(i) + ":";
				foreach (string header in response.Headers.GetValues(i)) {
					val += header + ":";
				}
				//Debug.Log (val);
			}
			inputStream = response.GetResponseStream();
		}
		
		using (StreamReader reader = new StreamReader(inputStream)) {
			
			string contents = reader.ReadToEnd();
			Debug.Log(contents);
			
			SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(contents);
			Debug.Log("Count: " + node.Count);

			notes = new List<NoteData>();
			float time = 0;
			foreach (SimpleJSON.JSONNode child in node.Children) {
				//Debug.Log("sensor_type:" + child["sensor_type"]);
				//Debug.Log("value:" + child["value"].AsFloat);
				//Debug.Log("time:" + child["time"].AsInt);

				if (child["time_float"] != null) {
					time = child["time_float"].AsFloat;
				}

				NoteData note = new NoteData() {
					sensor = child["sensor_type"],
					timestamp = time,
					value = child["value"].AsFloat,
				};
				time++;
				notes.Add(note);
			}
		}

		playbackTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
		playbackTime += Time.deltaTime;

		// Get data
		//float flange = Mathf.Sin(Time.time);
		//audioMixer.SetFloat("Flange.Rate", flange);

		while (currentNote < notes.Count - 1 && notes[currentNote].timestamp < playbackTime) {

			// Apply note transform
			EnsembleConfig.EffectsMap effect;
			if (_config.effects.TryGetValue(notes[currentNote].sensor, out effect)) {

				// Remap
				float value = effect.Transform(notes[currentNote].value);
				Debug.Log("SENSE REMAP: " + notes[currentNote].sensor + "(" + notes[currentNote].value + ")"
				          + " to " + effect.effectName + "(" + value + ")");

				// Special case for fireworks
				if (effect.effectName == "Fireworks") {
					foreach (ParticleSystem ps in fireworks) {
						//if (value > .5f)
							ps.Play();
					}
				}
				else
				{
					audioMixer.SetFloat(effect.effectName, value);

					// Special case for pitch: adjust animation playback speed
					if (effect.effectName == "Pitch") {
						animator.speed = value;
					}
				}

				// Adjust equalizer colors for visual feedback.
				// Goal: map one equalizer for each sensor
				equalizer.BlendColor(effect.id % equalizer.equalizers.Length, value);
			} else {
				Debug.LogWarning("No mapping for sensor " + notes[currentNote].sensor);
			}

			currentNote++;
		}
	}
}
