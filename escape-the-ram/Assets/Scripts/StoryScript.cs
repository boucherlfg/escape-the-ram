using Bytes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScript : MonoBehaviour
{
    private static bool didStoryThisSession = false;
    [SerializeField]
    private List<GameObject> pages;
    private int index = 0;
    private IEnumerator Start()
    {
        yield return null;
        if (didStoryThisSession)
        {
            Time.timeScale = 1;
            didStoryThisSession = true;
            EventManager.Dispatch("IntroIsDone", null);
        }
    }
    public void HandleNextPage()
    {
        pages[index].SetActive(false);
        index++;
        if (index >= pages.Count)
        {
            Time.timeScale = 1;
            didStoryThisSession = true;
            EventManager.Dispatch("IntroIsDone", null);
            return;
        }
        pages[index].SetActive(true);
    }
}
