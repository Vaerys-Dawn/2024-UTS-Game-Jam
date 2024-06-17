using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public List<LevelLink> links = new List<LevelLink>();

    private void Awake()
    {
        links = new List<LevelLink>(FindObjectsOfType<LevelLink>());
        links.RemoveAll(l => l.gameObject.scene.buildIndex == -1);
    }

    public Transform getSpawnPoint(string linkID)
    {
        foreach (LevelLink link in links) {
            if (link.linkID == linkID) return link.transform;
        }
        return null;
    }
}
