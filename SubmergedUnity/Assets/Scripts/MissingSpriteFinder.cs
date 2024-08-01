using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissingSpriteFinder : MonoBehaviour
{
    public GameObject BaseObject;

    public List<Component> MissingSprites;
    
    [ContextMenu("FindSprites")]
    public void FindSprites()
    {
        var rends = BaseObject.GetComponentsInChildren<SpriteRenderer>();
        MissingSprites = rends.Where(s => !s.sprite).Select(t => (Component) t).ToList();
        
        var masks = BaseObject.GetComponentsInChildren<SpriteMask>();
        MissingSprites.AddRange(masks.Where(s => !s.sprite).Select(t => (Component) t).ToList());
    }
}
