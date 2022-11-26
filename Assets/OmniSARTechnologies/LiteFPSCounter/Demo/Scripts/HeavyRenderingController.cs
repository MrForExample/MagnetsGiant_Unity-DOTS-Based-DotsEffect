///
// Heavy Rendering Controller
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OmniSARTechnologies.Helper;

namespace OmniSARTechnologies.LiteFPSCounter.Examples {
    public class HeavyRenderingController : MonoBehaviour {
        public Material heavyRenderingMaterial;

        [Range(0, 1)]
        public float complexity = 0.05f;

        public Text complexityText;
        public Slider complexitySlider;
        public Gradient complexityHeatMap;

        public float GetComplexity() {
            if (!heavyRenderingMaterial) {
                return 0.0f;
            }

            return Mathf.Clamp01(heavyRenderingMaterial.GetFloat("_Complexity"));
        }

        public void SetComplexity(float value) {
            complexity = value;

            if (heavyRenderingMaterial) {
                heavyRenderingMaterial.SetFloat("_Complexity", Mathf.Clamp01(complexity));
            }

            if (complexityText) {
                complexityText.text = string.Format(
                    "Material Complexity: {0}",
                    ColorHelper.ColorText(
                        (complexity * 100.0f).ToString("F2") + "%",
                        complexityHeatMap.Evaluate(complexity)
                    )
                );
            }
        }

        public void UpdateComplexity() {
            SetComplexity(complexitySlider ? complexitySlider.value : complexity);
        }

        private void Start() {
            UpdateComplexity();
        }

        private void OnValidate() {
            UpdateComplexity();
        }
    }
}
