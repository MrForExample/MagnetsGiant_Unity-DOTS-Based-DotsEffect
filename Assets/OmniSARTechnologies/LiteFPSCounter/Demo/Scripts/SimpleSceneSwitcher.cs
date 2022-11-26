///
// Simple Scene Switcher
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace OmniSARTechnologies.LiteFPSCounter.Examples {
    public class SimpleSceneSwitcher : MonoBehaviour {
        public Text sceneNameText;

        private void Start() {
            UpdateSceneNameText();
        }

        public void ChangeActiveScene(int buildIndexOffset) {
            if (SceneManager.sceneCountInBuildSettings < 1) {
                return;
            }

            int newSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
            newSceneBuildIndex += buildIndexOffset;
            newSceneBuildIndex += SceneManager.sceneCountInBuildSettings << 10;
            newSceneBuildIndex %= SceneManager.sceneCountInBuildSettings;
            newSceneBuildIndex = Mathf.Clamp(newSceneBuildIndex, 0, SceneManager.sceneCountInBuildSettings - 1);

            SceneManager.LoadScene(newSceneBuildIndex);
            UpdateSceneNameText();
        }

        private void UpdateSceneNameText() {
            if (!sceneNameText) {
                return;
            }

            sceneNameText.text = SceneManager.GetActiveScene().name;
        }
    }
}
