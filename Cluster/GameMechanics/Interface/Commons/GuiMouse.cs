using OpenTK.Input;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class GuiMouse
    {
        private GuiMouse()
        {
        }

        static float _mouseX;
        static float _mouseY;
        static float _mouseZ;
        static float _mouseZSpeed;

        static bool _mouseHitLeft;
        static bool _mouseHitRight;
        static bool _mouseHitMiddle;
        static bool _mousePressLeft;
        static bool _mousePressRight;
        static bool _mousePressMiddle;

        public static void update()
        {
            _mouseHitLeft = GameWindow.mouse.IsButtonDown(MouseButton.Left) && !_mousePressLeft;
            _mouseHitRight = GameWindow.mouse.IsButtonDown(MouseButton.Right) && !_mousePressRight;
            _mouseHitMiddle = GameWindow.mouse.IsButtonDown(MouseButton.Middle) && !_mousePressMiddle;

            _mousePressLeft = GameWindow.mouse.IsButtonDown(MouseButton.Left);
            _mousePressRight = GameWindow.mouse.IsButtonDown(MouseButton.Right);
            _mousePressMiddle = GameWindow.mouse.IsButtonDown(MouseButton.Middle);

            _mouseX = GameWindow.mousePos.x;
            _mouseY = GameWindow.mousePos.y;
            _mouseZSpeed = GameWindow.mouse.WheelPrecise - _mouseZ;
            _mouseZ = GameWindow.mouse.WheelPrecise;
        }

        public static bool isMouseInRect(float x0, float y0, float width, float height)
        {
            return !(_mouseX < x0) &&
                   !(_mouseY < y0) &&
                   !(_mouseX > x0 + width) &&
                   !(_mouseY > y0 + height);
        }

        public static float mouseX
        {
            get { return _mouseX; }
        }

        public static float mouseY
        {
            get { return _mouseY; }
        }

        public static float mouseZ
        {
            get { return _mouseZ; }
        }

        public static float mouseZSpeed
        {
            get { return _mouseZSpeed; }
        }

        public static bool mouseHitLeft
        {
            get { return _mouseHitLeft; }
        }

        public static bool mouseHitRight
        {
            get { return _mouseHitRight; }
        }

        public static bool mouseHitMiddle
        {
            get { return _mouseHitMiddle; }
        }

        public static bool mousePressLeft
        {
            get { return _mousePressLeft; }
        }

        public static bool mousePressRight
        {
            get { return _mousePressRight; }
        }

        public static bool mousePressMiddle
        {
            get { return _mousePressMiddle; }
        }
    }
}