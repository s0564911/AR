using UnityEngine;

public class Intro : MonoBehaviour
{
    public Controller controller;

    private void OnMouseDown()
    {
        controller._blueBack.SetActive(false);
        gameObject.SetActive(false);
    }
}
