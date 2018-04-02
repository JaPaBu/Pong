using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Pong
{
    internal class Program
    {
        private static Paddle _paddle1;
        private static Paddle _paddle2;
        private static PongGame _pongGame;
        private static GameWindow _gameWindow;
        private static SpriteVertexArrayObject _rectVao;
        private static Shader _shader;

        private static void Main(string[] args)
        {
            var paddle1StartPos = new Vector2d(0, (720 - Paddle.PaddleHeight) / 2);
            var paddle2StartPos = new Vector2d(1280 - Paddle.PaddleWidth, (720 - Paddle.PaddleHeight) / 2);

            //_paddle1 = new PaddleHuman(paddle1StartPos, PaddleHuman.ControlScheme.WASD);
            //_paddle2 = new PaddleHuman(paddle2StartPos, PaddleHuman.ControlScheme.ArrowKeys);

            //_paddle1 = new PaddleBasicCPU(paddle1StartPos, false);
            //_paddle2 = new PaddleBasicCPU(paddle2StartPos, false);

            _paddle1 = new PaddleAI(paddle1StartPos, "player1");
            _paddle2 = new PaddleAI(paddle2StartPos, "player2");

            _pongGame = new PongGame(_paddle1, _paddle2);

            _gameWindow = new GameWindow(1280, 720);
            _gameWindow.Load += _gameWindow_Load;
            _gameWindow.UpdateFrame += GameWindow_UpdateFrame;
            _gameWindow.RenderFrame += GameWindow_RenderFrame;
            _gameWindow.Resize += GameWindowOnResize;
            _gameWindow.Closing += GameWindowOnClosing;
            _gameWindow.KeyDown += GameWindowOnKeyDown;
            _gameWindow.KeyUp += GameWindowOnKeyUp;

            _gameWindow.Run();
        }

        
        private static void GameWindowOnKeyDown(object sender, KeyboardKeyEventArgs e) => KeyboardState.Keys.Add(e.Key);

        private static void GameWindowOnKeyUp(object sender, KeyboardKeyEventArgs e) => KeyboardState.Keys.Remove(e.Key);

        private static void GameWindowOnClosing(object sender, CancelEventArgs cancelEventArgs) => _pongGame.GameExiting();

        private static void GameWindowOnResize(object sender, EventArgs eventArgs) => GL.Viewport(_gameWindow.ClientRectangle);

        private static void _gameWindow_Load(object sender, EventArgs e)
        {
            _rectVao = new SpriteVertexArrayObject();
            _rectVao.Add(new Vector2(-1, +1), Vector2.Zero, Vector3.Zero);
            _rectVao.Add(new Vector2(+1, +1), Vector2.Zero, Vector3.Zero);
            _rectVao.Add(new Vector2(-1, -1), Vector2.Zero, Vector3.Zero);
            _rectVao.Add(new Vector2(+1, -1), Vector2.Zero, Vector3.Zero);
            _rectVao.AddFace(new uint[] {0, 2, 1, 1, 2, 3});
            _rectVao.Upload();

            _shader = new Shader("resources\\shader");
        }

        private static void GameWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            var delta = e.Time;
            _pongGame.Update(delta);
        }

        private static void GameWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Bind();
            GL.Uniform2(0, Paddle.PaddleWidth, Paddle.PaddleHeight);
            GL.Uniform2(1, _paddle1.Position.X, _paddle1.Position.Y);
            GL.Uniform2(2, _paddle2.Position.X, _paddle2.Position.Y);
            GL.Uniform3(3, _pongGame.BallPosition.X, _pongGame.BallPosition.Y, PongGame.BallRadius);
            _rectVao.Draw();

            _gameWindow.SwapBuffers();
        }
    }
}