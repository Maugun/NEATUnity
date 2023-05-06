using System;
using System.Collections.Generic;
using Random = System.Random;

namespace NEAT.Demo.Tools
{
    public class HamiltonianPath
    {
        private int _xMax;
        private int _yMax;
        private List<int[]> _path;
        private int _n;
        private Random _r;
        private bool _leftEnd;

        public HamiltonianPath(Random r)
        {
            _r = r;
        }

        public List<int[]> Generate(int xMax, int yMax, bool isCircuit)
        {
            _leftEnd = true;
            _xMax = xMax;
            _yMax = yMax;

            GeneratePath();
            if (isCircuit) GenerateCircuit();
            return _path;
        }

        private void GeneratePath()
        {
            _path = new List<int[]>();
            for (int i = 0; i < (_xMax + 1) * (_yMax + 1); ++i) _path.Add(null);
            _path[0] = new int[2] { _r.Next(0, _xMax + 1), _r.Next(0, _yMax + 1) };
            _n = 1;

            int nattempts = (int)(1 + 10.0 * (_xMax + 1) * (_yMax + 1) * Math.Pow(Math.Log(2f + (_xMax + 1) * (_yMax + 1)), 2));
            while (_n < (_xMax + 1) * (_yMax + 1))
            {
                for (int i = 0; i < nattempts; ++i) Backbite();
            }
        }

        private void GenerateCircuit()
        {
            int minDist = 1 + (_n % 2);
            while (Math.Abs(_path[_n - 1][0] - _path[0][0]) + Math.Abs(_path[_n - 1][1] - _path[0][1]) != minDist)
                Backbite();
        }

        private void Backbite()
        {
            int[] step = null;
            switch (_r.Next(0, 4))
            {
                case 0:
                    step = new int[2] { 1, 0 };
                    break;
                case 1:
                    step = new int[2] { -1, 0 };
                    break;
                case 2:
                    step = new int[2] { 0, 1 };
                    break;
                case 3:
                    step = new int[2] { 0, -1 };
                    break;
            }
            if (_r.Next(0, 2) == 0)
                BackbiteLeft(step);
            else
                BackbiteRight(step);
        }

        private void BackbiteLeft(int[] step)
        {
            int[] neighbour = new int[2] { _path[0][0] + step[0], _path[0][1] + step[1] };
            if (!InSublattice(neighbour[0], neighbour[1])) return;

            bool inPath = false;
            int j;
            for (j = 1; j < _n; j += 2)
            {
                if ((neighbour[0] == _path[j][0]) && (neighbour[1] == _path[j][1]))
                {
                    inPath = true;
                    break;
                }
            }

            if (inPath)
            {
                ReversePath(0, j - 1);
                return;
            }
            _leftEnd = !_leftEnd;
            ReversePath(0, _n - 1);
            _n++;
            _path[_n - 1] = neighbour;
        }

        private void BackbiteRight(int[] step)
        {
            int[] neighbour = new int[2] { _path[_n - 1][0] + step[0], _path[_n - 1][1] + step[1] };
            if (!InSublattice(neighbour[0], neighbour[1])) return;

            bool inPath = false;
            int j;
            for (j = _n - 2; j >= 0; j -= 2)
            {
                if ((neighbour[0] == _path[j][0]) && (neighbour[1] == _path[j][1]))
                {
                    inPath = true;
                    break;
                }
            }

            if (inPath)
            {
                ReversePath(j + 1, _n - 1);
                return;
            }
            _n++;
            _path[_n - 1] = neighbour;
        }

        private bool InSublattice(int x, int y)
        {
            if (x < 0 || x > _xMax || y < 0 || y > _yMax) return false;
            return true;
        }

        private void ReversePath(int i1, int i2)
        {
            int jlim = (i2 - i1 + 1) / 2;
            int[] temp;
            for (int j = 0; j < jlim; j++)
            {
                //faster to swap arrays directly
                temp = _path[i1 + j];
                _path[i1 + j] = _path[i2 - j];
                _path[i2 - j] = temp;
            }
        }

        public string PathToString()
        {
            string pathString = "[[" + _path[0][0] + "," + _path[0][1] + "]";
            for (int i = 1; i < _n; i++)
            {
                pathString += ",[" + _path[i][0] + "," + _path[i][1] + "]";
            }

            pathString += "]";
            return pathString;
        }
    }
}
