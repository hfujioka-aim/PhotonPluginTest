namespace MasterData
{
    public class PhotonAnimatorView: PhotonViewComponent
    {
        public SynchronizedLayer[] SynchronizeLayers { get; set; }

        public SynchronizedParameter[] SynchronizeParameters { get; set; }

        public enum ParameterType
        {
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9,
        }

        public enum SynchronizeType
        {
            Disabled = 0,
            Discrete = 1,
            Continuous = 2,
        }

        public class SynchronizedLayer
        {
            public SynchronizeType SynchronizeType { get; set; }

            public int LayerIndex { get; set; }
        }

        public class SynchronizedParameter
        {
            public ParameterType Type { get; set; }

            public SynchronizeType SynchronizeType { get; set; }

            public string Name { get; set; }
        }
    }
}
