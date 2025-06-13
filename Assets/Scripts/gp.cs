using UnityEngine;

public class gp : MonoBehaviour
{
    public GameObject profilePanel;

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
    }

    public void CloseProfile()
    {
        profilePanel.SetActive(false);
    }
}
