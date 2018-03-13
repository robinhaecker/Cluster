using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class Properties
    {
        public static readonly Vec4 colorDefault = new Vec4(0.25f, 0.25f, 0.25f, 0.85f);
        public static readonly Vec4 colorHighlight = new Vec4(0.35f, 0.35f, 0.35f, 0.65f);
        public static readonly Vec4 colorYes = new Vec4(0.5f, 1.0f, 0.5f, 0.5f);
        public static readonly Vec4 colorMaybe = new Vec4(1.0f, 0.55f, 0.15f, 0.5f);
        public static readonly Vec4 colorNo = new Vec4(1.0f, 0.25f, 0.25f, 0.5f);

        public const float BUTTON_SIZE_DEFAULT = 50.0f;
        public const float BUTTON_SIZE_LARGE = 100.0f;
        public const float PROGRESS_BAR_HEIGHT = 10.0f;
        
    }
}