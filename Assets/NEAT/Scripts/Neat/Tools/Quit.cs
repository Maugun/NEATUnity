using UnityEngine;

public class Quit : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey("escape")) Application.Quit();
    }
}
