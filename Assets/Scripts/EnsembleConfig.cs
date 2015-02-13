using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

using SimpleJSON;

[Serializable]
public class EnsembleConfig {

	public class EffectsMap {

		public string effectName;
		public string sensorName;
		public float inputRangeLow;
		public float inputRangeHigh;
		public float outputRangeLow;
		public float outputRangeHigh;
		public int id;

		private static int _nextId = 0;

		public EffectsMap() {
			id = _nextId++;
		}

		public float Transform(float value) {
			// Remap into destination range
			float range = inputRangeHigh - inputRangeLow;
			float x = (value - inputRangeLow) / range;

			float outputRange = outputRangeHigh - outputRangeLow;
			return Mathf.Clamp((x*outputRange) + outputRangeLow, outputRangeLow, outputRangeHigh);
		}
	}
	
	public string ensembleApi = "http://soundserver.herokuapp.com/api/";
	public int compositionId = 1; // -1 for latest
	public string compositionFile = "";

	public string[] musicFiles = {
		"test1.mp3",
		"test2.mp3",
	};

	/// <summary>
	/// Maps sensor names to an effect transformation
	/// </summary>
	public Dictionary<string, EffectsMap> effects;

	public void LoadJSON(string filename) {

		JSONNode node = null;
		using (StreamReader sr = new StreamReader(filename)) {
			// Load JSON config
			node = SimpleJSON.JSON.Parse(sr.ReadToEnd());
		}

		ensembleApi = node["ensemble_api"];
		compositionId = node["composition_id"].AsInt;
		compositionFile = node["composition_file"];

		Debug.Log("ensemble_api:" + ensembleApi);
		JSONArray audioArray = node["audio_files"].AsArray;
		musicFiles = new string[audioArray.Count];
		for (int i = 0; i < audioArray.Count; i++) {
			musicFiles[i] = audioArray[i].Value;
			Debug.Log("audio_files[" + i + "]:" + musicFiles[i]);
		}

		JSONArray effectsArray = node["effects_map"].AsArray;
		if (effectsArray != null) {
			effects = new Dictionary<string, EffectsMap>();
			for (int i = 0; i < effectsArray.Count; i++) {
				JSONNode child = effectsArray[i];
				EffectsMap effect = new EffectsMap() {
					sensorName = child["sensor_name"],
					effectName = child["effect_name"],
					inputRangeLow = child["input_range_low"].AsFloat,
					inputRangeHigh = child["input_range_high"].AsFloat,
					outputRangeLow = child["output_range_low"].AsFloat,
					outputRangeHigh = child["output_range_high"].AsFloat
				};
				if (!string.IsNullOrEmpty(effect.sensorName)) {
					Debug.Log("Mapping " + effect.sensorName + " to " + effect.effectName);
					effects.Add(effect.sensorName, effect);
				}
			}
		}
	}

	public void SaveXml(string filename) {
		XmlSerializer serializer = new XmlSerializer(typeof(EnsembleConfig));

		using (XmlTextWriter xtw = new XmlTextWriter(filename, System.Text.Encoding.UTF8)) {

			xtw.Formatting = Formatting.Indented;

			serializer.Serialize(xtw, this);
		}
	}
}
