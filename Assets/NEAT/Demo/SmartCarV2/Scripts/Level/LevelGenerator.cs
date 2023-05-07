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
        public int width = 10;
        [Range(0, 50)]
        public int height = 10;
        [Header("To be a circuit width & height should be even")]
        public bool isCircuit = true;
        public bool addCheckpoints = true;

        [Header("Level Parts")]
        public List<GameObject> forwardList;
        public List<GameObject> cornerList;
        public List<GameObject> startList;
        public List<GameObject> endList;
        public GameObject checkpoint;
        public Transform levelParent;
        public float tileSize = 10f;
        public bool spawnOnlyFirstInList = false;

        [Header("End & Start Materials")]
        public Material startMaterial;
        public Material endMaterial;

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
        private Level _currentLevel;

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
            // Init Random & HP
            _r = new Random();
            _hp = new HamiltonianPath(_r);
            _xMax = width - 1;
            _yMax = height - 1;
        }

        public void GenerateLevel(int width, int height, bool isCircuit = true, bool addCheckpoints = true)
        {
            this.width = width;
            _xMax = this.width - 1;
            this.height = height;
            _yMax = this.height - 1;
            this.isCircuit = isCircuit;
            this.addCheckpoints = addCheckpoints;
            GenerateLevel();
        }

        public void LoadLevel(Level level)
        {
            _currentLevel = level;
            isCircuit = _currentLevel.isCircuit;
            spawnOnlyFirstInList = _currentLevel.spawnOnlyFirstInList;
            _xMax = _currentLevel.xMax;
            _yMax = _currentLevel.yMax;
            width = _xMax + 1;
            height = _yMax + 1;
            InitLevel();
            SpawnLevel(_currentLevel.path, true);
        }

        public void GenerateLevel()
        {
            // Max x, y Value protection
            if (_xMax <= 0) _xMax = 1;
            if (_yMax <= 0) _yMax = 1;

            // Circuit protection
            int n = (_xMax + 1) * (_yMax + 1);
            if (isCircuit && ((1 + (n % 2)) != 1))
            {
                Debug.Log("Impossible to do circuit, you should use even width & height !");
                return;
            }

            _currentLevel = new Level(_xMax, _yMax, isCircuit, spawnOnlyFirstInList);
            InitLevel();

            List<int[]> path = GenerateLevelPath();
            SpawnLevel(path);

            //Debug.Log(_hp.PathToString());
        }

        private void InitLevel()
        {
            // Clean Level Parent
            foreach (Transform child in levelParent) GameObject.Destroy(child.gameObject);

            // Reset Checkpoint
            Checkpoint.totalCp = 0;

            // Add Checkpoints parent
            if (addCheckpoints) SpawnCheckpointParent();
        }

        private List<int[]> GenerateLevelPath()
        {
            List<int[]> path = _hp.Generate(_xMax, _yMax, isCircuit);
            _currentLevel.path = path;

            return path;
        }

        private void SpawnLevel(List<int[]> path, bool isLoaded = false)
        {
            StartTile(path, isLoaded);
            for (int i = 1; i < path.Count - 1; ++i) MiddleTile(path, i, isLoaded);
            EndTile(path, isLoaded);

            // Spawn Object
            SpawnSpawn(path);
        }

        private void SpawnCheckpointParent()
        {
            GameObject obj = new GameObject();
            _checkpointsParent = Instantiate(obj, Vector3.zero, Quaternion.identity, levelParent).transform;
            _checkpointsParent.name = "ChekpointsParent";
            Destroy(obj);
        }

        private void StartTile(List<int[]> path, bool isLoaded = false)
        {
            Direction dirNext = GetDirection(path[0], path[1]);
            Direction dirPrev = GetDirection(path[0], path[path.Count - 1]);
            Type type = Type.START;

            if (isCircuit) type = GetType(dirPrev, dirNext);
            SpawnTile(path, 0, dirPrev, dirNext, type, isLoaded);
        }

        private void MiddleTile(List<int[]> path, int currentIndex, bool isLoaded = false)
        {
            Direction dirNext = GetDirection(path[currentIndex], path[currentIndex + 1]);
            Direction dirPrev = GetDirection(path[currentIndex], path[currentIndex - 1]);
            Type type = GetType(dirPrev, dirNext);
            SpawnTile(path, currentIndex, dirPrev, dirNext, type, isLoaded);
        }

        private void EndTile(List<int[]> path, bool isLoaded = false)
        {
            Direction dirPrev = GetDirection(path[path.Count - 1], path[path.Count - 2]);
            Direction dirNext = GetDirection(path[path.Count - 1], path[0]);
            Type type = Type.END;

            if (isCircuit) type = GetType(dirPrev, dirNext);
            SpawnTile(path, path.Count - 1, dirPrev, dirNext, type, isLoaded);

        }

        private void SpawnTile(List<int[]> path, int index, Direction dirPrev, Direction dirNext, Type type, bool isLoaded = false)
        {
            // Spawn Tile
            GameObject obj = GetTileObj(type, index, isLoaded);
            float angle = GetAngle(dirPrev, dirNext, type);
            Vector3 spawnPosition = new Vector3(path[index][0] * tileSize, 0f, path[index][1] * tileSize);
            Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.up);
            GameObject tile = Instantiate(obj, spawnPosition, spawnRotation, levelParent);

            // Change ground color for start & end tiles
            if (index == 0)
                tile.transform.Find(_groundName).GetComponent<MeshRenderer>().material = startMaterial;
            else if (index == path.Count - 1)
                tile.transform.Find(_groundName).GetComponent<MeshRenderer>().material = endMaterial;

            // Spawn Checkpoint
            if (addCheckpoints) SpawnCheckPoint(path, index, dirNext, type);
        }

        private void SpawnCheckPoint(List<int[]> path, int index, Direction dirNext, Type type)
        {
            // Do not spawn CP for Type.END
            if (type == Type.END)
                return;

            // Position
            Vector3 spawnPosition = new Vector3(path[index][0] * tileSize, 0f, path[index][1] * tileSize);
            switch (dirNext)
            {
                case Direction.LEFT:
                    spawnPosition.z -= tileSize / 2;
                    break;
                case Direction.UP:
                    spawnPosition.x -= tileSize / 2;
                    break;
                case Direction.RIGHT:
                    spawnPosition.z += tileSize / 2;
                    break;
                case Direction.DOWN:
                    spawnPosition.x += tileSize / 2;
                    break;
            }

            // Angle
            float angle = dirNext == Direction.LEFT || dirNext == Direction.RIGHT ? 90f : 0f;
            Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.up);

            // Spawn
            GameObject checkpoint = Instantiate(this.checkpoint, spawnPosition, spawnRotation, _checkpointsParent);
            checkpoint.transform.GetComponent<Checkpoint>().Id = index;
            Checkpoint.totalCp = Checkpoint.totalCp + 1;
            checkpoint.name = _cpName + index;
        }

        private void SpawnSpawn(List<int[]> path)
        {
            GameObject obj = new GameObject();
            Spawn = Instantiate(obj, new Vector3(path[0][0] * tileSize, 0.1f, path[0][1] * tileSize), Quaternion.identity, levelParent).transform;
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

        private GameObject GetTileObj(Type type, int index, bool isLoaded = false)
        {
            List<GameObject> list = null;

            if (type == Type.FORWARD) list = forwardList;
            else if (type == Type.CORNER) list = cornerList;
            else if (type == Type.START) list = startList;
            else if (type == Type.END) list = endList;

            return list != null ? GetTileInList(list, index, isLoaded) : null;
        }

        private GameObject GetTileInList(List<GameObject> list, int index, bool isLoaded = false)
        {
            if (isLoaded) return list[_currentLevel.tileIndexes[index]];

            int tileIndex = (spawnOnlyFirstInList || (isCircuit && index == 0)) ? 0 : _r.Next(0, list.Count);
            _currentLevel.tileIndexes.Add(index, tileIndex);
            return list[tileIndex];
        }

        private float GetAngle(Direction dirPrev, Direction dirNext, Type type)
        {
            if (type == Type.CORNER)
            {
                if ((dirPrev == Direction.UP && dirNext == Direction.LEFT) ||
                    (dirPrev == Direction.LEFT && dirNext == Direction.UP))
                    return 90f;
                if ((dirPrev == Direction.UP && dirNext == Direction.RIGHT) ||
                    (dirPrev == Direction.RIGHT && dirNext == Direction.UP))
                    return 180f;
                if ((dirPrev == Direction.DOWN && dirNext == Direction.RIGHT) ||
                    (dirPrev == Direction.RIGHT && dirNext == Direction.DOWN))
                    return 270f;
                return 0f;
            }
            if (type == Type.END)
            {
                if (dirPrev == Direction.LEFT) return 90f;
                if (dirPrev == Direction.UP) return 180f;
                if (dirPrev == Direction.RIGHT) return 270f;
                return 0f;
            }
            if (dirNext == Direction.RIGHT) return 90f;
            if (dirNext == Direction.DOWN) return 180f;
            if (dirNext == Direction.LEFT) return 270f;
            return 0f;
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
            if (current[0] == neighbour[0]) return current[1] > neighbour[1] ? Direction.LEFT : Direction.RIGHT;
            return current[0] > neighbour[0] ? Direction.UP : Direction.DOWN;
        }

        public Level GetCurrentLevel()
        {
            return _currentLevel;
        }

        public string GetPathText()
        {
            return _hp.PathToString();
        }
    }
}
