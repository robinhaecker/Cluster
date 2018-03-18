using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class MeshButton : Button
    {
        private Mesh _mesh;
        private float _red;
        private float _green;
        private float _blue;
        private Mesh _background;

        internal MeshButton(Mesh mesh, Mesh background, float x = 0, float y = 0,
            float size = Properties.BUTTON_SIZE_DEFAULT) : base(x, y, size)
        {
            _mesh = mesh;
            _background = background;
            _red = _green = _blue = 1.0f;
        }

        public MeshButton setColor(float r, float g, float b)
        {
            _red = r;
            _green = g;
            _blue = b;
            return this;
        }

        public override void render()
        {
            base.render();
            if (_mesh != null)
            {
                _mesh.deferred_drawFit(x, y, width, height, 1.0f, _red, _green, _blue);
            }

            if (_background != null)
            {
                _background.deferred_drawFit(x - 0.1f * width, y, width * 1.2f, height);
            }
        }
    }
}