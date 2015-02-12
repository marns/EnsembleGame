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
	
	public string compositionsEndpoint = "http://soundserver.herokuapp.com/api/Compositions";

	public string[] musicFiles = {
		"test1.mp3",
		"test2.mp3",
	};

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
	}

	public void SaveXml(string filename) {
		XmlSerializer serializer = new XmlSerializer(typeof(EnsembleConfig));

		using (XmlTextWriter xtw = new XmlTextWriter(filename, System.Text.Encoding.UTF8)) {

			xtw.Formatting = Formatting.Indented;

			serializer.Serialize(xtw, this);
		}
	}
}
