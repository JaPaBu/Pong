using OpenTK;

namespace Pong
{
    internal abstract class Paddle
    {
        public const double PaddleAcceleration = 5000;
        public const double PaddleMaxSpeed = 20000;
        public const double PaddleDamping = 5;
        public const double PaddleWidth = 20;
        public const double PaddleHeight = 140;

        protected enum Move
        {
            Nothing,
            Up,
            Down
        }

        public Vector2d Position { get; set; }
        public Vector2d Velocity { get; set; }

        private readonly Vector2d _startPosition;

        protected Paddle(Vector2d position)
        {
            Position = position;
            _startPosition = position;
            Velocity = Vector2d.Zero;
        }

        protected abstract Move GetMove(PongGame pongGame);

        internal void Update(PongGame pongGame, Paddle opponent, double delta)
        {
            var move = GetMove(pongGame);

            if(move == Move.Down)
                Velocity += new Vector2d(0, +1) * 0.5 * PaddleAcceleration * delta;
            else if(move == Move.Up)
                Velocity += new Vector2d(0, -1) * 0.5 * PaddleAcceleration * delta;

            if (Velocity.LengthSquared > PaddleMaxSpeed * PaddleMaxSpeed)
                Velocity = Velocity.Normalized() * PaddleMaxSpeed;


            Position += Velocity * delta;

            if (Position.Y < -PaddleHeight / 2)
            {
                Position = new Vector2d(Position.X, -PaddleHeight / 2);
                Velocity = Vector2d.Zero;
            }


            if (Position.Y > 720 - PaddleHeight / 2)
            {
                Position = new Vector2d(Position.X, 720 - PaddleHeight / 2);
                Velocity = Vector2d.Zero;
            }


            //Apply dampening
            Velocity -= Velocity * PaddleDamping * delta;
            if(Velocity.LengthSquared < 100)
                Velocity = Vector2d.Zero;
        }

        public virtual void Reset(PongGame pongGame, bool won, bool gameExiting)
        {
            Position = _startPosition;
        }

        public virtual void HitBall()
        {
        }
    }
}