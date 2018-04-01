using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenTK;

namespace Pong
{
    internal class PaddleAntonAI : Paddle
    {
        private class Person : IComparable<Person>
        {
            public readonly double[] Weights;
            public double Fitness;

            public Person(double[] weights, double fitness)
            {
                Weights = weights;
                Fitness = fitness;
            }

            public int CompareTo(Person other)
                => other.Fitness.CompareTo(Fitness);
        }

        private static readonly Random Random = new Random();

        private const double MutationChance = 0.15;
        private const double MutationAmplitude = 0.25;

        private readonly int _weightsCount;
        private readonly List<double[]> _layers = new List<double[]>();
        private readonly List<Person> _persons = new List<Person>();

        private int _currentPersonIndex;
        private Person CurrentPerson => _persons[_currentPersonIndex];
        private int _hitBallCounter;
        private int _wonCounter;
        
        private readonly string _name;
        private string FileName => _name + ".bin";

        public PaddleAntonAI(Vector2d position, string name) : base(position)
        {
            _name = name;
            _layers.Add(new double[6]);
            _layers.Add(new double[5]);
            _layers.Add(new double[4]);
            _layers.Add(new double[3]);

            _weightsCount = 0;
            for (var i = 1; i < _layers.Count; i++)
                _weightsCount += _layers[i - 1].Length * _layers[i].Length;
            for (var i = 1; i < _layers.Count; i++)
                _weightsCount += _layers[i].Length;

            for (var i = 0; i < 10; i++)
                _persons.Add(CreatePerson());
            Load();
        }

        private static double Map(double value, double inMin, double inMax, double outMin, double outMax)
            => outMin + (outMax - outMin) * (value - inMin) / (inMax - inMin);

        private static double Normalize(double value, double inMin, double inMax)
            => Map(value, inMin, inMax, -1, +1);

        private static double Sigmoid(double value)
            => 1 / Math.Sqrt(1 + value*value);

        // private static double Sigmoid(double value)
        // {
        //     if (value > 0)
        //         return 1;
        //     return -1;
        // }

        private static double NextGaussian(double mean, double stdDev)
        {
            var u1 = 1.0 - Random.NextDouble();
            var u2 = 1.0 - Random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        private Person CreatePerson()
        {
            var weights = new double[_weightsCount];

            for (var i = 0; i < weights.Length; i++)
                weights[i] = Random.NextDouble() * 2 - 1;

            return new Person(weights, 0);
        }

        private void PopulateFirstLayer(PongGame pongGame)
        {
            var layer = _layers[0];
            
            //Self y-position
            layer[0] = Normalize(Position.Y, -PaddleHeight/2, 720 - PaddleHeight/2);

            //Self y-speed
            layer[1] = Normalize(Velocity.Y, -PaddleMaxSpeed, +PaddleMaxSpeed);

            //Distance to the ball
            layer[2] = Normalize(Math.Abs(pongGame.BallPosition.X - Position.X - PaddleWidth / 2), 0, 1280 + PongGame.BallRadius);

            //Ball y-position
            layer[3] = Normalize(pongGame.BallPosition.Y, -PongGame.BallRadius, 720 + PongGame.BallRadius);

            //Ball y-speed
            layer[4] = Normalize(pongGame.BallVelocity.Y, -PongGame.MaxBallSpeedY, +PongGame.MaxBallSpeedY);

            //Ball x-speed towards paddle
            layer[5] = Normalize(pongGame.BallVelocity.X, -PongGame.MaxBallSpeedX, +PongGame.MaxBallSpeedX);
            if(Position.X < pongGame.BallPosition.X) layer[5] = -layer[5];
        }

        private void MutatePerson(Person source, Person dest)
        {
            for (var i = 0; i < source.Weights.Length; i++)
                if(MutationChance > Random.NextDouble())
                    dest.Weights[i] = source.Weights[i] + (Random.NextDouble()-0.5)*MutationAmplitude*2;
                else
                    dest.Weights[i] = source.Weights[i];
        }

        private int _writeCounter;
        private int _saveCounter;
        private void MutatePersons()
        {
            _persons.Sort();

            _writeCounter++;
            if (_writeCounter >= 100)
            {
                Console.WriteLine($"[{_name}] Dumping top 5 persons");
                for (var i = 0; i < 5; i++)
                    Console.WriteLine(_persons[i].Fitness);
                Console.WriteLine("---------------------------------");
                _writeCounter = 0;

                _bestHitBallCounter = 0;
            }

            _saveCounter++;
            if (_saveCounter >= 1000)
            {
                Save();
                _saveCounter = 0;
            }

            for (var i = 0; i < 5; i++)
                MutatePerson(_persons[i], _persons[5 + i]);
        }

        private Thread _saveThread;
        private void Save()
        {
            if (_saveThread == null || !_saveThread.IsAlive)
            {
                Console.WriteLine($"[{_name}] Saving progress...");
                _saveThread = new Thread(() =>
                {
                    using (var fileStream = File.OpenWrite(FileName))
                    using (var binaryWriter = new BinaryWriter(fileStream))
                    {
                        foreach (var person in _persons)
                        {
                            var weightsBytes = new byte[person.Weights.Length * sizeof(double)];
                            Buffer.BlockCopy(person.Weights, 0, weightsBytes, 0, weightsBytes.Length);
                            binaryWriter.Write(weightsBytes);
                        }
                    }
                });
                _saveThread.Start();
            }
            else
                Console.WriteLine($"[{_name}] Error: Could not save because save thread is stuck");
        }

        private void Load()
        {
            if (File.Exists(FileName))
            {
                Console.WriteLine($"[{_name}] Loading from save...");
                using (var fileStream = File.OpenRead(FileName))
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    foreach (var person in _persons)
                    {
                        var weightsBytes = binaryReader.ReadBytes(person.Weights.Length * sizeof(double));
                        Buffer.BlockCopy(weightsBytes, 0, person.Weights, 0, weightsBytes.Length);
                    }
                }
            }
            else Console.WriteLine($"[{_name}] No save file found starting from scratch");
        }

        private int PopulateNextLayer(int layerIndex, int weightOffset)
        {
            var lastLayer = _layers[layerIndex - 1];
            var layer = _layers[layerIndex];

            for (var i = 0; i < layer.Length; i++)
                layer[i] = Sigmoid(lastLayer.Sum(value => value * CurrentPerson.Weights[weightOffset++]) + CurrentPerson.Weights[weightOffset++]);

            return weightOffset;
        }

        protected override Move GetMove(PongGame pongGame)
        {
            PopulateFirstLayer(pongGame);

            var weightCounter = 0;
            for (var i = 1; i < _layers.Count; i++)
                weightCounter = PopulateNextLayer(i, weightCounter);


            var lastLayer = _layers.Last();

            var tuples = new[]
            {
                (confidence: lastLayer[0], move: Move.Nothing),
                (confidence: lastLayer[1], move: Move.Up),
                (confidence: lastLayer[2], move: Move.Down)
            };

            Array.Sort(tuples, (v0, v1) => Math.Sign(v1.confidence - v0.confidence));
            var first = tuples.First();
            return first.move;
        }

        public override void Reset(PongGame pongGame, bool won, bool gameExiting)
        {
            base.Reset(pongGame, won, gameExiting);

            if (gameExiting)
            {
                Save();
                return;
            }

            if (won)
            {
                _wonCounter++;
                return;
            }

            var ballDistance = Math.Abs(pongGame.BallPosition.Y - (Position.Y + PaddleHeight / 2));
            CurrentPerson.Fitness = _hitBallCounter * 50 - ballDistance/2 + _wonCounter*0;
            //CurrentPerson.Fitness = _hitBallCounter;
            _hitBallCounter = 0;
            _wonCounter = 0;

            _currentPersonIndex++;

            if (_currentPersonIndex < _persons.Count) return;

            MutatePersons();
            _currentPersonIndex = 0;
        }

        private int _bestHitBallCounter;
        public override void HitBall()
        {
            _hitBallCounter++;
            if (_hitBallCounter % 25 == 0 && _hitBallCounter > _bestHitBallCounter)
            {
                Console.WriteLine($"[{_name}] Hit ball counter: " + _hitBallCounter);
                _bestHitBallCounter = _hitBallCounter;
            }
        }
    }
}