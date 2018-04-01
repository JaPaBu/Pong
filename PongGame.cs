using System;
using OpenTK;
using OpenTK.Input;

namespace Pong
{
    internal class PongGame
    {
        private static readonly Random Random = new Random();

        public const double BallRadius = 6;
        public const double BallSpeedX = 400;
        public const double MaxBallSpeedY = BallSpeedX * 2.7;
        public const double MaxBallSpeedX = BallSpeedX * 4;
        public const double BallPaddleFriction = 0.3;
        public const double RandomBallSpin = 25;
        public const double BallHitAcceleration = 5;

        public static double GameSpeed = 1;

        private readonly Paddle _paddle1;
        private readonly Paddle _paddle2;

        public PongGame(Paddle paddle1, Paddle paddle2)
        {
            _paddle1 = paddle1;
            _paddle2 = paddle2;

            ResetBall(null, null);
        }

        public Vector2d BallPosition { get; private set; }
        public Vector2d BallVelocity { get; private set; }


        private MouseState _oldMouseState;
        public void Update(double delta)
        {
            var wholeStepTime = delta * GameSpeed;
            var wholeStepBallLength = wholeStepTime * BallSpeedX;

            var stepBallLength = Math.Min(wholeStepBallLength, BallRadius);
            var steps = wholeStepBallLength / stepBallLength;
            var stepTime = wholeStepTime / steps;

            for (var i = 0; i < steps; i++)
            {
                if (i + 1 >= steps)
                    stepTime = wholeStepTime - stepTime * i;

                _paddle1.Update(this, _paddle2, stepTime);
                _paddle2.Update(this, _paddle1, stepTime);
                UpdateBall(stepTime);
            }

            var mouseState = Mouse.GetState();
            GameSpeed += mouseState.WheelPrecise - _oldMouseState.WheelPrecise;
            GameSpeed = Math.Max(GameSpeed, 1);

            _oldMouseState = mouseState;

            //Doesnt work with dotnetcore
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Key.Number1)) GameSpeed = 1;
            if (keyboardState.IsKeyDown(Key.Number2)) GameSpeed = 15000;
        }

        private void UpdateBall(double delta)
        {
            if(BallVelocity.Y > MaxBallSpeedY)
                BallVelocity = new Vector2d(BallVelocity.X, MaxBallSpeedY);
            if(BallVelocity.Y < -MaxBallSpeedY)
                BallVelocity = new Vector2d(BallVelocity.X, -MaxBallSpeedY);

            BallPosition += BallVelocity * delta;
            PaddleCollision(_paddle1);
            PaddleCollision(_paddle2);

            if (BallPosition.Y < BallRadius)
                BallVelocity = new Vector2d(BallVelocity.X, +Math.Abs(BallVelocity.Y));
            else if(BallPosition.Y > 720 - BallRadius)
                BallVelocity = new Vector2d(BallVelocity.X, -Math.Abs(BallVelocity.Y));


            if (BallPosition.X < -BallRadius)
                ResetBall(_paddle2, _paddle1);
             if (BallPosition.X > 1280 + BallRadius)
                 ResetBall(_paddle1, _paddle2);
        }

        private void PaddleCollision(Paddle paddle)
        {
            //For all 4 sides
            //Vertical sides
            if (BallPosition.Y > paddle.Position.Y && BallPosition.Y < paddle.Position.Y + Paddle.PaddleHeight)
            {
                //Left side
                if (BallPosition.X < paddle.Position.X)
                {
                    var distance = Math.Abs(BallPosition.X - paddle.Position.X);
                    if (distance < BallRadius)
                    {
                        BallVelocity = new Vector2d(-Math.Abs(BallVelocity.X) - BallHitAcceleration,
                            BallVelocity.Y + paddle.Velocity.Y * BallPaddleFriction + (Random.NextDouble()-0.5)*RandomBallSpin);
                        paddle.HitBall();
                    }
                }

                //Right side
                if (BallPosition.X > paddle.Position.X + Paddle.PaddleWidth)
                {
                    var distance = Math.Abs(BallPosition.X - paddle.Position.X - Paddle.PaddleWidth);
                    if (distance < BallRadius)
                    {
                        BallVelocity = new Vector2d(+Math.Abs(BallVelocity.X) + BallHitAcceleration,
                            BallVelocity.Y + paddle.Velocity.Y * BallPaddleFriction + (Random.NextDouble() - 0.5) * RandomBallSpin);
                        paddle.HitBall();
                    }
                }
            }
        }

        public void GameExiting()
        {
            _paddle1.Reset(this, false, true);
            _paddle2.Reset(this, false, true);
        }

        private void ResetBall(Paddle winner, Paddle loser)
        {
            winner?.Reset(this, true, false);
            loser?.Reset(this, false, false);


            BallPosition = new Vector2d(640, 360);
            BallVelocity = new Vector2d(Math.Sign(Random.NextDouble() - 0.5) * BallSpeedX, Math.Sign(Random.NextDouble()-0.5) * (Random.NextDouble()*100 + 50));
        }
    }
}