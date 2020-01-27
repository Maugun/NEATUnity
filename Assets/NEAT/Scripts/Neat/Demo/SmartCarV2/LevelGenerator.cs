using NEAT.Demo.Tools;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace NEAT.Demo.SmartCarV2
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Configuration", order = 0)]
        [Header("High width or height values can take some time to generate, do not panic :)", order = 1)]
        [Range(0, 50)]
        public int _width = 10;
        [Range(0, 50)]
        public int _height = 10;
        [Header("To be a circuit width & height should be even")]
        public bool _isCircuit = true;
        public bool _addCheckpoints = true;

        [Header("Level Parts")]
        public List<GameObject> _forwardList;
        public List<GameObject> _cornerList;
        public List<GameObject> _startList;
        public List<GameObject> _endList;
        public GameObject _checkpoint;
        public Transform _levelParent;
        public float _tileSize = 10f;
        public bool _spawnOnlyFirstInList = false;

        [Header("End & Start Materials")]
        public Material _startMaterial;
        public Material _endMaterial;

        public Transform Spawn { get; set; }
        public Quaternion SpawnRotation { get; set; }

        private HamiltonianPath _hp;
        private Random _r;
        private Transform _checkpointsParent;
        private string _groundName = "Ground";
        private string _cpName = "CP_";
        private string _spawnName = "Spawn";
        private int _xMax;
        private int _yMax;

        private enum Direction
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            UNDEFINED
        }

        private enum Type
        {
            START,
            FORWARD,
            CORNER,
            END
        }

        private void Start()
        {
            // Init HP & Random
            _hp = new HamiltonianPath();
            _r = new Random();
            _xMax = _width - 1;
            _yMax = _height - 1;
        }

        public void GenerateLevel(int width, int height, bool isCircuit = true, bool addCheckpoints = true)
        {
            _width = width;
            _xMax = _width - 1;
            _height = height;
            _yMax = _height - 1;
            _isCircuit = isCircuit;
            _addCheckpoints = addCheckpoints;
            GenerateLevel();
        }

        public void GenerateLevel()
        {
            // Max x, y Value protection
            if (_xMax <= 0)
                _xMax = 1;
            if (_yMax <= 0)
                _yMax = 1;

            // Circuit protection
            int n = (_xMax + 1) * (_yMax + 1);
            if (_isCircuit && ((1 + (n % 2)) != 1))
            {
                Debug.Log("Impossible to do circuit, you should use even width & height !");
                return;
            }

            // Clean Level Parent
            foreach (Transform child in _levelParent)
                GameObject.Destroy(child.gameObject);

            // Reset Checkpoint
            Checkpoint._totalCp = 0;

            // Add Checkpoints parent
            if (_addCheckpoints)
                SpawnCheckpointParent();

            // Generate level path
            List<int[]> path = _hp.Generate(_xMax, _yMax, _isCircuit);

            // Generate Level
            StartTile(path);
            for (int i = 1; i < path.Count - 1; ++i)
                MiddleTile(path, i);
            EndTile(path);

            // Spawn
            SpawnSpawn(path);

            //Debug.Log(_hp.PathToString());
        }

        private void SpawnCheckpointParent()
        {
            GameObject obj = new GameObject();
            _checkpointsParent = Instantiate(obj, Vector3.zero, Quaternion.identity, _levelParent).transform;
            _checkpointsParent.name = "ChekpointsParent";
            Destroy(obj);
        }

        private void StartTile(List<int[]> path)
        {
            Direction dirNext = GetDirection(path[0], path[1]);
            Direction dirPrev = GetDirection(path[0], path[path.Count - 1]);
            Type type = Type.START;

            if (_isCircuit)
                type = GetType(dirPrev, dirNext);
            SpawnTile(path, 0, dirPrev, dirNext, type);
        }

        private void MiddleTile(List<int[]> path, int currentIndex)
        {
            Direction dirNext = GetDirection(path[currentIndex], path[currentIndex + 1]);
            Direction dirPrev = GetDirection(path[currentIndex], path[currentIndex - 1]);
            Type type = GetType(dirPrev, dirNext);
            SpawnTile(path, currentIndex, dirPrev, dirNext, type);
        }

        private void EndTile(List<int[]> path)
        {
            Direction dirPrev = GetDirection(path[path.Count - 1], path[path.Count - 2]);
            Direction dirNext = GetDirection(path[path.Count - 1], path[0]);
            Type type = Type.END;

            if (_isCircuit)
                type = GetType(dirPrev, dirNext);
            SpawnTile(path, path.Count - 1, dirPrev, dirNext, type);
            
        }

        private void SpawnTile(List<int[]> path, int index, Direction dirPrev, Direction dirNext, Type type)
        {
            // Spawn Tile
            GameObject obj = GetTileObj(type, index);
            float angle = GetAngle(dirPrev, dirNext, type);
            Vector3 spawnPosition = new Vector3(path[index][0] * _tileSize, 0f, path[index][1] * _tileSize);
            Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.up);
            GameObject tile = Instantiate(obj, spawnPosition, spawnRotation, _levelParent);

            // Change ground color for start & end tiles
            if (index == 0)
                tile.transform.Find(_groundName).GetComponent<MeshRenderer>().material = _startMaterial;
            else if (index == path.Count - 1)
                tile.transform.Find(_groundName).GetComponent<MeshRenderer>().material = _endMaterial;

            // Spawn Checkpoint
            if (_addCheckpoints)
                SpawnCheckPoint(path, index, dirNext, type);
        }

        private void SpawnCheckPoint(List<int[]> path, int index, Direction dirNext, Type type)
        {
            // Do not spawn CP for Type.END
            if (type == Type.END)
                return;

            // Position
            Vector3 spawnPosition = new Vector3(path[index][0] * _tileSize, 0f, path[index][1] * _tileSize);
            switch (dirNext)
            {
                case Direction.LEFT:
                    spawnPosition.z -= _tileSize / 2;
                    break;
                case Direction.UP:
                    spawnPosition.x -= _tileSize / 2;
                    break;
                case Direction.RIGHT:
                    spawnPosition.z += _tileSize / 2;
                    break;
                case Direction.DOWN:
                    spawnPosition.x += _tileSize / 2;
                    break;
            }

            // Angle
            float angle = 0f;
            if (dirNext == Direction.LEFT || dirNext == Direction.RIGHT)
                angle = 90f;
            Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.up);
            
            // Spawn
            GameObject checkpoint = Instantiate(_checkpoint, spawnPosition, spawnRotation, _checkpointsParent);
            checkpoint.transform.GetComponent<Checkpoint>().Id = index;
            Checkpoint._totalCp = Checkpoint._totalCp + 1;
            checkpoint.name = _cpName + index;
        }

        private void SpawnSpawn(List<int[]> path)
        {
            GameObject obj = new GameObject();
            Spawn = Instantiate(obj, new Vector3(path[0][0] * _tileSize, 0f, path[0][1] * _tileSize), Quaternion.identity, _levelParent).transform;
            Spawn.name = _spawnName;
            Destroy(obj);

            Direction nextDir = GetDirection(path[0], path[1]);
            float angle = 0f;
            switch (nextDir)
            {
                case Direction.LEFT:
                    angle = 180f;
                    break;
                case Direction.UP:
                    angle = 270f;
                    break;
                case Direction.DOWN:
                    angle = 90f;
                    break;
            }
            SpawnRotation = Quaternion.AngleAxis(angle, Vector3.up);
        }

        private GameObject GetTileObj(Type type, int index)
        {
            GameObject obj = null;
            switch (type)
            {
                case Type.START:
                    obj = GetTileInList(_startList, index);
                    break;
                case Type.FORWARD:
                    obj = GetTileInList(_forwardList, index);
                    break;
                case Type.CORNER:
                    obj = GetTileInList(_cornerList, index);
                    break;
                case Type.END:
                    obj = GetTileInList(_endList, index);
                    break;
            }
            return obj;
        }

        private GameObject GetTileInList(List<GameObject> list, int index)
        {
            GameObject obj = null;
            if (_spawnOnlyFirstInList || (_isCircuit && index == 0))
                obj = list[0];
            else
                obj = list[_r.Next(0, list.Count)];
            return obj;
        }

        private float GetAngle(Direction dirPrev, Direction dirNext, Type type)
        {
            float angle = 0f;
            if (type == Type.CORNER)
            {
                if ((dirPrev == Direction.UP && dirNext == Direction.LEFT) ||
                    (dirPrev == Direction.LEFT && dirNext == Direction.UP))
                    angle = 90f;
                else if ((dirPrev == Direction.UP && dirNext == Direction.RIGHT) ||
                    (dirPrev == Direction.RIGHT && dirNext == Direction.UP))
                    angle = 180f;
                else if ((dirPrev == Direction.DOWN && dirNext == Direction.RIGHT) ||
                    (dirPrev == Direction.RIGHT && dirNext == Direction.DOWN))
                    angle = 270f;
            }
            else if (type == Type.END)
            {
                switch (dirPrev)
                {
                    case Direction.LEFT:
                        angle = 90f;
                        break;
                    case Direction.UP:
                        angle = 180f;
                        break;
                    case Direction.RIGHT:
                        angle = 270f;
                        break;
                }
            }
            else
            {
                switch (dirNext)
                {
                    case Direction.RIGHT:
                        angle = 90f;
                        break;
                    case Direction.DOWN:
                        angle = 180f;
                        break;
                    case Direction.LEFT:
                        angle = 270f;
                        break;
                }
            }
            return angle;
        }

        private Type GetType(Direction prev, Direction next)
        {
            Type type = Type.CORNER;
            if ((prev == Direction.DOWN && next == Direction.UP) ||
                (prev == Direction.UP && next == Direction.DOWN) ||
                (prev == Direction.LEFT && next == Direction.RIGHT) ||
                (prev == Direction.RIGHT && next == Direction.LEFT))
                type = Type.FORWARD;
            return type;
        }

        private Direction GetDirection(int[] current, int[] neighbour)
        {
            Direction dir = Direction.UNDEFINED;

            if (current[0] == neighbour[0])
            {
                if (current[1] > neighbour[1])
                    dir = Direction.LEFT;
                else
                    dir = Direction.RIGHT;
            }
            else if (current[0] > neighbour[0])
                dir = Direction.UP;
            else
                dir = Direction.DOWN;

            return dir;
        }

        public string GetPathText()
        {
            return _hp.PathToString();
        }
    }
}
