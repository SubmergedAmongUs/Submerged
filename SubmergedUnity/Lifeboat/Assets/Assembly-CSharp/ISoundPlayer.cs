using System;
using UnityEngine;

public interface ISoundPlayer
{
	string Name { get; set; }

	AudioSource Player { get; set; }

	void Update(float dt);
}
