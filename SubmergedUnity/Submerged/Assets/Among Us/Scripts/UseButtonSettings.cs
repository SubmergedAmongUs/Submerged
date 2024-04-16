using UnityEngine;

[CreateAssetMenu]
public class UseButtonSettings : ScriptableObject
{
	public ImageNames ButtonType;
	public Sprite Image;
	public StringNames Text;
	public Material FontMaterial;
}
