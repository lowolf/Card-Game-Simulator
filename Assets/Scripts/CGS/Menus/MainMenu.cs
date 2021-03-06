﻿using System.Collections.Generic;
using System.Linq;
using CardGameDef;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CGS.Menus
{
    public class MainMenu : MonoBehaviour
    {
        public const int MainMenuSceneIndex = 1;
        public const int PlayModeSceneIndex = 2;
        public const int DeckEditorSceneIndex = 3;
        public const int OptionsMenuSceneIndex = 4;
        public const string VersionMessage = "Ver. ";

        public List<GameObject> buttons;
        public GameObject quitButton;
        public Text versionText;

        void Start()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        quitButton.SetActive(false);
#endif
            versionText.text = VersionMessage + Application.version;

            if (CardGameManager.Instance.Discovery.running)
                CardGameManager.Instance.Discovery.StopBroadcast();
            CardGameManager.Instance.Discovery.HasReceivedBroadcast = false;
            CardGameManager.Instance.Discovery.SearchForHost();
        }

        void Update()
        {
            if (!Input.anyKeyDown || CardGameManager.TopMenuCanvas != null)
                return;

            if (Input.GetKeyDown(Inputs.BluetoothReturn))
                EventSystem.current.currentSelectedGameObject?.GetComponent<Button>()?.onClick?.Invoke();
            else if (Input.GetButtonDown(Inputs.Horizontal))
            {
                if (Input.GetAxis(Inputs.Horizontal) < 0)
                    CardGameManager.Instance.SelectLeft();
                else
                    CardGameManager.Instance.SelectRight();
            }
            else if (Input.GetButtonDown(Inputs.Vertical) && !buttons.Contains(EventSystem.current.currentSelectedGameObject))
                EventSystem.current.SetSelectedGameObject(buttons[1].gameObject);
            else if (Input.GetButtonDown(Inputs.Sort))
                ShowGameSelectionMenu();
            else if (Input.GetButtonDown(Inputs.New))
                StartGame();
            else if (Input.GetButtonDown(Inputs.Load))
                JoinGame();
            else if (Input.GetButtonDown(Inputs.Save))
                EditDeck();
            else if (Input.GetButtonDown(Inputs.Filter))
                ShowOptions();
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(Inputs.Cancel))
                Quit();
        }

        public void ShowGameSelectionMenu()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            CardGameManager.Instance.Selector.Show();
        }

        public void StartGame()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            if (CardGameManager.Instance.Discovery.running)
                CardGameManager.Instance.Discovery.StopBroadcast();
            CardGameManager.Instance.Discovery.HasReceivedBroadcast = false;
            SceneManager.LoadScene(PlayModeSceneIndex);
        }

        public void PlayGame()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            SceneManager.LoadScene(PlayModeSceneIndex);
        }

        public void JoinGame()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            CardGameManager.Instance.Discovery.HasReceivedBroadcast = true;
            SceneManager.LoadScene(PlayModeSceneIndex);
        }

        public void EditDeck()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            SceneManager.LoadScene(DeckEditorSceneIndex);
        }

        public void ShowOptions()
        {
            if (Time.timeSinceLevelLoad < 0.1)
                return;
            SceneManager.LoadScene(OptionsMenuSceneIndex);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WSA
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
        Application.Quit();
#endif
        }
    }
}
