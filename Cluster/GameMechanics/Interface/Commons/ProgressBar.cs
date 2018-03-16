using System;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class ProgressBar : Button
    {
        private float _progress;
        private float _maxProgress;
        private int _anzahlFolgende;

        internal ProgressBar(float x = 0, float y = 0, float size = Properties.BUTTON_SIZE_DEFAULT) : base(x, y, size)
        {
            height = Properties.PROGRESS_BAR_HEIGHT;
            setProgress(0.0f, 1.0f);
        }

        public void setProgress(float progress, float maxProgress = -1)
        {
            _progress = progress;
            if (maxProgress > 0)
            {
                _maxProgress = maxProgress;
            }
        }

        private void setAnzahlFolgende(int anzahl = 0)
        {
            _anzahlFolgende = anzahl;
        }

        public new void updateState()
        {
            base.updateState();
            _progress = Math.Min(Math.Max(0, _progress), _maxProgress);
        }

        public new void render()
        {
            var percentage = getPercentage();
            Primitives.setColor(Properties.colorYes.r(), Properties.colorYes.g(), Properties.colorYes.b(), Properties.colorYes.a());
            Primitives.drawRect(x, y, width * percentage, height);
            
            Primitives.setColor(_color.r(), _color.g(), _color.b(), _color.a());
            Primitives.drawRect(x + width * percentage, y, width * (1.0f - percentage), height);
            if (_anzahlFolgende > 0)
            {
                Text.setTextSize(15.0f);
                Text.drawText(_anzahlFolgende.ToString(), x + width - 15.0f, y);
            }
        }

        private float getPercentage()
        {
            return _progress / _maxProgress;
        }
    }
}