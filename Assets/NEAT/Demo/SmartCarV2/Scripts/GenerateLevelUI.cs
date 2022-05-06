﻿using NEAT.Demo.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NEAT.Demo.SmartCarV2
{
    public class GenerateLevelUI : MonoBehaviour
    {
        public CameraController cameraController;
        public NEATManager neatManager;
        public LevelGenerator levelGenerator;
        public TimeScale timeScale;
        public Canvas configurationCanvas;
        public Canvas levelCanvas;
        public Canvas carCanvas;
        public Canvas InGameCanvas;
        public List<GameObject> carList;

        private NEATConfig _config;
        private InputField _populationNbInput;
        private InputField _populationToKeepInput;
        private InputField _disabledConnectionInput;
        private InputField _addConnectionInput;
        private InputField _addNodeInput;
        private InputField _enableDisableInput;
        private InputField _weightMutationsInput;
        private InputField _perturbingProbabilityInput;
        private bool _timeAttack;
        private Text _genertionLogUI;
        public Text _timerUI;
        private BrainGraph _brainGraph;
        private RectTransform _brainRectTransform;
        private Image _speedOne;
        private Image _speedFive;
        private Image _speedTen;

        private void Awake()
        {
            configurationCanvas.gameObject.SetActive(true);
            levelCanvas.gameObject.SetActive(false);
            carCanvas.gameObject.SetActive(false);
            InGameCanvas.gameObject.SetActive(false);
            InitConfigurationCanvas();
            InitLevelCanvas();
            InitCarCanvas();
            InitGameCanvas();
        }

        #region Configuration
        public void HideConfigurationCanvas()
        {
            configurationCanvas.gameObject.SetActive(false);
            levelCanvas.gameObject.SetActive(true);
        }

        private void InitConfigurationCanvas()
        {
            // Copy Scriptable Object config
            _config = Instantiate(neatManager.config);
            neatManager.config = _config;

            _populationNbInput = configurationCanvas.transform.Find("PopulationInput").GetComponent<InputField>();
            _populationNbInput.text = _config.populationSize.ToString();
            _populationToKeepInput = configurationCanvas.transform.Find("PopulationToKeepInput").GetComponent<InputField>();
            _populationToKeepInput.text = _config.percentageToKeep.ToString();
            _config.bias = true;
            _config.addConnectionOnCreation = true;
            _config.crossover = true;
            _disabledConnectionInput = configurationCanvas.transform.Find("DisabledConnectionInput").GetComponent<InputField>();
            _disabledConnectionInput.text = (_config.disabledConnectionInheritChance * 1000).ToString();
            _config.genomeMutations = true;
            _addConnectionInput = configurationCanvas.transform.Find("AddConnectionInput").GetComponent<InputField>();
            _addConnectionInput.text = (_config.addConnectionRate * 1000).ToString();
            _addNodeInput = configurationCanvas.transform.Find("AddNodeInput").GetComponent<InputField>();
            _addNodeInput.text = (_config.addNodeRate * 1000).ToString();
            _enableDisableInput = configurationCanvas.transform.Find("EnableDisableInput").GetComponent<InputField>();
            _enableDisableInput.text = (_config.enableDisableRate * 1000).ToString();
            _weightMutationsInput = configurationCanvas.transform.Find("WeightMutationsInput").GetComponent<InputField>();
            _weightMutationsInput.text = (_config.mutationRate * 1000).ToString();
            _perturbingProbabilityInput = configurationCanvas.transform.Find("PerturbingProbabilityInput").GetComponent<InputField>();
            _perturbingProbabilityInput.text = (_config.perturbingProbability * 1000).ToString();
        }

        public void PopulationNumberChange()
        {
            try
            {
                int value = Int32.Parse(_populationNbInput.text);
                if (value < 2) value = 2;
                if (value > 200) value = 200;
                _config.populationSize = value;
            }
            catch { }
            _populationNbInput.text = _config.populationSize.ToString();
        }

        public void PopulationToKeepChange()
        {
            try
            {
                int value = Int32.Parse(_populationToKeepInput.text);
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                _config.percentageToKeep = value;
            }
            catch { }
            _populationToKeepInput.text = _config.percentageToKeep.ToString();
        }

        public void DisabledConnectionChange()
        {
            try
            {
                int value = Int32.Parse(_disabledConnectionInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.disabledConnectionInheritChance = value / 1000f;
            }
            catch { }
            _disabledConnectionInput.text = (_config.disabledConnectionInheritChance * 1000).ToString();
        }

        public void AddConnectionChange()
        {
            try
            {
                int value = Int32.Parse(_addConnectionInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.addConnectionRate = value / 1000f;
            }
            catch { }
            _addConnectionInput.text = (_config.addConnectionRate * 1000).ToString();
        }

        public void AddNodeChange()
        {
            try
            {
                int value = Int32.Parse(_addNodeInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.addNodeRate = value / 1000f;
            }
            catch { }
            _addNodeInput.text = (_config.addNodeRate * 1000).ToString();
        }

        public void EnableDisableChange()
        {
            try
            {
                int value = Int32.Parse(_enableDisableInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.enableDisableRate = value / 1000f;
            }
            catch { }
            _enableDisableInput.text = (_config.enableDisableRate * 1000).ToString();
        }

        public void WeightMutationsChange()
        {
            try
            {
                int value = Int32.Parse(_weightMutationsInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.mutationRate = value / 1000f;
            }
            catch { }
            _weightMutationsInput.text = (_config.mutationRate * 1000).ToString();
        }

        public void PerturbingProbabilityChange()
        {
            try
            {
                int value = Int32.Parse(_perturbingProbabilityInput.text);
                if (value < 0) value = 0;
                if (value > 1000) value = 1000;
                _config.perturbingProbability = value / 1000f;
            }
            catch { }
            _perturbingProbabilityInput.text = (_config.perturbingProbability * 1000).ToString();
        }

        public void HasBias() { _config.bias = _config.bias ? false : true; }

        public void HasBrainConnections() { _config.addConnectionOnCreation = _config.addConnectionOnCreation ? false : true; }

        public void HasCrossover() { _config.crossover = _config.crossover ? false : true; }

        public void HasGenomeMutations() { _config.genomeMutations = _config.genomeMutations ? false : true; }
        #endregion

        #region Level
        private void InitLevelCanvas()
        {
            levelGenerator.spawnOnlyFirstInList = false;
            levelGenerator.isCircuit = true;
        }

        public void HideLevelCanvas()
        {
            levelCanvas.gameObject.SetActive(false);
            carCanvas.gameObject.SetActive(true);
            neatManager.levelGenerator = levelGenerator;
            neatManager.spawn = levelGenerator.Spawn;
            neatManager.SetSpawnRotation(levelGenerator.SpawnRotation);
        }

        public void GenerateSmallLevel()
        {
            GenerateLevel(6, 6);
        }

        public void GenerateMediumLevel()
        {
            GenerateLevel(10, 10);
        }

        public void GenerateLargeLevel()
        {
            GenerateLevel(14, 14);
        }

        private void GenerateLevel(int w, int h)
        {
            levelGenerator.GenerateLevel(w, h, levelGenerator.isCircuit);
            cameraController.CenterCameraOnMap(levelGenerator.width - 1, levelGenerator.height - 1, levelGenerator.tileSize);
            levelCanvas.transform.Find("Next").gameObject.SetActive(true);
        }

        public void IsComplex() { levelGenerator.spawnOnlyFirstInList = levelGenerator.spawnOnlyFirstInList ? false : true; }

        public void IsCircuit() { levelGenerator.isCircuit = levelGenerator.isCircuit ? false : true; }
        #endregion

        #region Car
        private void InitCarCanvas()
        {
            _timeAttack = true;
        }

        public void SelectBasicCar()
        {
            SelectCar(1);
        }

        public void SelectComplexCar()
        {
            SelectCar(0);
        }

        private void SelectCar(int index)
        {
            neatManager.config.creaturePrefab = carList[index];
            _config.creaturePrefab.GetComponent<DemoCarController>().timeAttack = _timeAttack;
            carCanvas.gameObject.SetActive(false);
            InGameCanvas.gameObject.SetActive(true);
            neatManager.start = true;
        }

        public void IsTimeAttack() { _timeAttack = _timeAttack ? false : true; }
        #endregion

        #region InGame
        private void InitGameCanvas()
        {
            _genertionLogUI = InGameCanvas.transform.Find("LogText").GetComponent<Text>();
            _speedOne = InGameCanvas.transform.Find("SpeedOne").GetComponent<Image>();
            _speedFive = InGameCanvas.transform.Find("SpeedFive").GetComponent<Image>();
            _speedTen = InGameCanvas.transform.Find("SpeedTen").GetComponent<Image>();
            _timerUI = InGameCanvas.transform.Find("TimerTxt").GetComponent<Text>();
            _brainGraph = InGameCanvas.transform.Find("Brain").GetComponent<BrainGraph>();
            _brainRectTransform = _brainGraph.transform.GetComponent<RectTransform>();
            SpeedOne();

        }

        public void SpeedOne()
        {
            _speedOne.color = Color.green;
            _speedFive.color = Color.white;
            _speedTen.color = Color.white;
            timeScale.timeScale = 1f;
        }

        public void SpeedFive()
        {
            _speedOne.color = Color.white;
            _speedFive.color = Color.green;
            _speedTen.color = Color.white;
            timeScale.timeScale = 5f;
        }

        public void SpeedTen()
        {
            _speedOne.color = Color.white;
            _speedFive.color = Color.white;
            _speedTen.color = Color.green;
            timeScale.timeScale = 10f;
        }

        public void UpdateGenerationLog(string log)
        {
            _genertionLogUI.text = log;
        }

        public void UpdateTimer(float time)
        {
            _timerUI.text = time.ToString("F");
        }

        public void UpdateBrainGraphWithCurrentBest()
        {
            UpdateBrainGraph(CreatureNeuralNetwork.BestNN.NeuralNetwork);
        }

        public void UpdateBrainGraph(NeuralNetwork bestNN)
        {
            _brainRectTransform.rotation = Quaternion.AngleAxis(0f, Vector3.forward);
            _brainGraph.ClearGraph();
            _brainGraph.SetNeuralNetwork(bestNN);
            _brainGraph.CreateGraph();
            _brainRectTransform.rotation = Quaternion.AngleAxis(-90f, Vector3.forward);
        }
        #endregion
    }
}