using OpenTK;

namespace Pong
{
    internal class PaddleBasicCPU : Paddle
    {
        private readonly bool _cheating;

        public PaddleBasicCPU(Vector2d position, bool cheating) : base(position)
        {
            _cheating = cheating;
        }

        protected override Move GetMove(PongGame pongGame)
        {
            if (_cheating)
            {
                Position = new Vector2d(Position.X, pongGame.BallPosition.Y - PaddleHeight/2);
                return Move.Nothing;
            }

            var paddleMiddle = Position.Y + PaddleHeight / 2;

            if(paddleMiddle < pongGame.BallPosition.Y)
                return Move.Down;
            if (paddleMiddle > pongGame.BallPosition.Y)
                return Move.Up;

            return Move.Nothing;
        }
    }
}