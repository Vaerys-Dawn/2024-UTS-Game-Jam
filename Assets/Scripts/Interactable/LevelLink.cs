using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLink : InteractionReceiver
{
    [SerializeField] private int levelID;
    [SerializeField] public string linkID;

    private PlayerMovement movement;

    private void Awake()
    {
        if (linkID == null) return;
    }

    public override void InteractionUpdate()
    {
        // ignore
    }

    /// <summary>
    /// Processes the Level change on State update
    /// </summary>
    /// <param name="source">Trigger Source - Ignored</param>
    /// <param name="currentState">Trigger State</param>
    /// <param name="lastInteractionType">State Update Type - Ignored</param>
    /// <exception cref="Exception"></exception>
    public override void RecieveStateChange(AbstractInteractor source, bool currentState, InteractionType lastInteractionType)
    {
        
        // don't run if ids are not valid
        if (linkID == null) return;

        // only trigger if state is true
        if (!currentState) return;

        StartCoroutine(nameof(ChangeScene));
    }

    private IEnumerator ChangeScene()
    {
        movement = FindObjectOfType<PlayerMovement>();
        this.transform.parent = null;
        DontDestroyOnLoad(this);
        int lastSceneID = SceneManager.GetActiveScene().buildIndex;

        // pause time to process transition (maybe pop up loading screne while waiting)
        Time.timeScale = 0;
        // get current scene and new scene
        UnityEngine.AsyncOperation operation = SceneManager.LoadSceneAsync(levelID);
        while (!operation.isDone) yield return null;

        Scene next = SceneManager.GetActiveScene();

        // get level manager in new scene
        GameObject[] objects = next.GetRootGameObjects();
        Transform spawnPoint = null;
        bool managerFound = false;
        foreach (GameObject obj in objects)
        {
            // if level manager is found, get the spawn point that matches the link id
            LevelManager manager = obj.GetComponent<LevelManager>();
            managerFound = true;
            if (manager)
            {
                spawnPoint = manager.getSpawnPoint(linkID);
                break;
            }
        }
        // unload new scene restart time and throw error
        if (!managerFound)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(lastSceneID);
            movement.transform.position = this.transform.position;
            throw new Exception("LevelManager Not found in Scene: " + levelID);

        }
        else if (spawnPoint == null)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(lastSceneID);
            movement.transform.position = this.transform.position;
            throw new Exception("Spawnpoint Link: " + linkID + " not found in Scene: " + levelID);
        }
        else
        {
            Debug.Log(movement.transform.position);
            movement.transform.position = spawnPoint.position;
            Debug.Log(movement.transform.position);
            Debug.Log(spawnPoint.position);
        }
        // unload old Scene and restart time
        Time.timeScale = 1;
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Respawn.png");
    }

    private Scene? getScene(int buildIndex)
    {
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            Scene sc = SceneManager.GetSceneAt(i);
            if (sc.buildIndex == buildIndex) return sc;
        }
        return null;
    }

}
