using System;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class ProgressBar : Button
    {
        private float progress;
        private float maxProgress;
        private int anzahlFolgende;

        internal ProgressBar(float x = 0, float y = 0, float size = Properties.BUTTON_SIZE_DEFAULT) : base(x, y, size)
        {
            height = Properties.PROGRESS_BAR_HEIGHT;
            setProgress(0.0f, 1.0f);
        }

        public void setProgress(float progress, float maxProgress = -1)
        {
            this.progress = progress;
            if (maxProgress > 0)
            {
                this.maxProgress = maxProgress;
            }
        }

        private void setAnzahlFolgende(int anzahl = 0)
        {
            anzahlFolgende = anzahl;
        }

        public new void updateState()
        {
            base.updateState();
            progress = Math.Min(Math.Max(0, progress), maxProgress);
        }

        public new void render()
        {
            var percentage = getPercentage();
            Primitives.setColor(Properties.colorYes.r(), Properties.colorYes.g(), Properties.colorYes.b(), Properties.colorYes.a());
            Primitives.drawRect(x, y, width * percentage, height);
            
            Primitives.setColor(color.r(), color.g(), color.b(), color.a());
            Primitives.drawRect(x + width * percentage, y, width * (1.0f - percentage), height);
            if (anzahlFolgende > 0)
            {
                Text.setTextSize(15.0f);
                Text.drawText(anzahlFolgende.ToString(), x + width - 15.0f, y);
            }
        }

        private float getPercentage()
        {
            return progress / maxProgress;
        }
    }
}