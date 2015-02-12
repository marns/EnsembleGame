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
	}
	
	public string compositionsEndpoint = "http://soundserver.herokuapp.com/api/Compositions";

	public string[] musicFiles = {
		"test1.mp3",
		"test2.mp3",
	};

	public List<EffectsMap> effects;

	public void LoadJSON(string filename) {

		JSONNode node = null;
		using (StreamReader sr = new StreamReader(filename)) {
			// Load JSON config
			node = SimpleJSON.JSON.Parse(sr.ReadToEnd());
		}

		compositionsEndpoint = node["compositions_endpoint"];
		Debug.Log("compositions_endpoint:" + compositionsEndpoint);
		JSONArray audioArray = node["audio_files"].AsArray;
		musicFiles = new string[audioArray.Count];
		for (int i = 0; i < audioArray.Count; i++) {
			musicFiles[i] = audioArray[i].Value;
			Debug.Log("audio_files[" + i + "]:" + musicFiles[i]);
		}

		JSONArray effectsArray = node["effects_map"].AsArray;
		if (effectsArray != null) {
			effects = new List<EffectsMap>();
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
				effects.Add(effect);
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
