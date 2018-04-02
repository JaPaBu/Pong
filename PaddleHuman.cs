using OpenTK;
using OpenTK.Input;

namespace Pong
{
    internal class PaddleHuman : Paddle
    {
        public enum ControlScheme
        {
            WASD,
            ArrowKeys
        }

        private readonly ControlScheme _controlScheme;

        public PaddleHuman(Vector2d position, ControlScheme controlScheme) : base(position)
        {
            _controlScheme = controlScheme;
        }

        protected override Move GetMove(PongGame pongGame)
        {            
            if (_controlScheme == ControlScheme.WASD)
            {
                if (KeyboardState.IsKeyDown(Key.W)) return Move.Up;
                if (KeyboardState.IsKeyDown(Key.S)) return Move.Down;
            }
            else if (_controlScheme == ControlScheme.ArrowKeys)
            {
                if (KeyboardState.IsKeyDown(Key.Up)) return Move.Up;
                if (KeyboardState.IsKeyDown(Key.Down)) return Move.Down;
            }

            return Move.Nothing;
        }
    }
}
