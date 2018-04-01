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
            var keyboardState = Keyboard.GetState();

            if (_controlScheme == ControlScheme.WASD)
            {
                if (keyboardState.IsKeyDown(Key.W)) return Move.Up;
                if (keyboardState.IsKeyDown(Key.S)) return Move.Down;
            }
            else if (_controlScheme == ControlScheme.ArrowKeys)
            {
                if (keyboardState.IsKeyDown(Key.Up)) return Move.Up;
                if (keyboardState.IsKeyDown(Key.Down)) return Move.Down;
            }

            return Move.Nothing;
        }
    }
}
