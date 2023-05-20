using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    public class HUDController : MonoBehaviour
    {
        public Text uiHealth;
        public Slider uiHealthSlider;
        [Space(10)]
        public MeshRenderer HealthMeshFill;
        public TextMeshPro HealthTextMesh;
        [Space(20)]


        public Transform DamageCrosshairHolder;



        void Start()
        {
            
        }


     


    }
}
