using UnityEngine;

public class demoGUI : MonoBehaviour {

    void OnGUI () {
        float w = Screen.width;
        float h = Screen.height;
        float buttonHeight = h * 0.2f;
        float buttonWidth = w * 0.35f;
        float buttonMarginLR = w * 0.1f;
        float buttonMarginTB = h * 0.4f;

        if (GUI.Button(new Rect(buttonMarginLR, buttonMarginTB, buttonWidth, buttonHeight), "Server")) {
            gameObject.AddComponent<demoPeer>().isServer = true;
        }

        if (GUI.Button(new Rect(buttonMarginLR + buttonWidth + buttonMarginLR, buttonMarginTB, buttonWidth, buttonHeight), "Client")) {
            gameObject.AddComponent<demoPeer>().isServer = false;
        }
    }
}
