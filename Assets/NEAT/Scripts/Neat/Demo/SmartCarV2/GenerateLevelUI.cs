using NEAT.Demo.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NEAT.Demo.SmartCarV2
{
    public class GenerateLevelUI : MonoBehaviour
    {
        public CameraController _cameraController;
        public NEATManager _neatManager;
        public LevelGenerator _levelGenerator;
        public TimeScale _timeScale;
        public Canvas _levelCanvas;
        public Canvas _carCanvas;
        public Canvas _InGameCanvas;
        public Text _genertionLogUI;
        public Text _currentSpeedUI;
        public List<GameObject> _carList;

        private void Start()
        {
            _carCanvas.gameObject.SetActive(false);
            _InGameCanvas.gameObject.SetActive(false);
            _levelCanvas.gameObject.SetActive(true);
            _levelGenerator._spawnOnlyFirstInList = false;
            SpeedOne();
        }

        #region Level
        public void HideLevelCanvas()
        {
            _levelCanvas.gameObject.SetActive(false);
            _carCanvas.gameObject.SetActive(true);
            _neatManager._levelGenerator = _levelGenerator;
            _neatManager._spawn = _levelGenerator.Spawn;
            _neatManager.SetSpawnRotation(_levelGenerator.SpawnRotation);
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
            _levelGenerator.GenerateLevel(w, h);
            _cameraController.CenterCameraOnMap(_levelGenerator._width - 1, _levelGenerator._height - 1, _levelGenerator._tileSize);
            _levelCanvas.transform.Find("Next").gameObject.SetActive(true);
        }

        public void IsComplex()
        {
            if (_levelGenerator._spawnOnlyFirstInList)
                _levelGenerator._spawnOnlyFirstInList = false;
            else
                _levelGenerator._spawnOnlyFirstInList = true;
        }
        #endregion

        #region Car
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
            _neatManager._config._creaturePrefab = _carList[index];
            _carCanvas.gameObject.SetActive(false);
            _InGameCanvas.gameObject.SetActive(true);
            _neatManager._start = true;
        }
        #endregion

        #region InGame
        public void SpeedOne()
        {
            Speed(1f);
        }

        public void SpeedFive()
        {
            Speed(5f);
        }

        public void SpeedTen()
        {
            Speed(10f);
        }

        public void Speed(float speed)
        {
            _timeScale._timeScale = speed;
            _currentSpeedUI.text = "Current: " + speed;
        }

        public void UpdateGenerationLog(string log)
        {
            _genertionLogUI.text = log;
        }
        #endregion
    }
}
