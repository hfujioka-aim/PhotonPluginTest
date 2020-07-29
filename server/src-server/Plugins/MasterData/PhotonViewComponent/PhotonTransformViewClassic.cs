namespace MasterData
{
    public class PhotonTransformViewClassic: PhotonViewComponent
    {
        public PhotonTransformViewPositionModel PositionModel { get; set; }

        public PhotonTransformViewRotationModel RotationModel { get; set; }

        public PhotonTransformViewScaleModel ScaleModel { get; set; }
    }

    public class PhotonTransformViewPositionModel
    {
        public bool SynchronizeEnabled { get; set; }

        public bool TeleportEnabled { get; set; }

        public float TeleportIfDistanceGreaterThan { get; set; }

        public InterpolateOptions InterpolateOption { get; set; }

        public float InterpolateMoveTowardsSpeed { get; set; }

        public float InterpolateLerpSpeed { get; set; }

        public ExtrapolateOptions ExtrapolateOption { get; set; }

        public float ExtrapolateSpeed { get; set; }

        public bool ExtrapolateIncludingRoundTripTime { get; set; }

        public int ExtrapolateNumberOfStoredPositions { get; set; }

        public enum InterpolateOptions
        {
            Disabled,
            FixedSpeed,
            EstimatedSpeed,
            SynchronizeValues,
            Lerp
        }

        public enum ExtrapolateOptions
        {
            Disabled,
            SynchronizeValues,
            EstimateSpeedAndTurn,
            FixedSpeed,
        }
    }

    public class PhotonTransformViewRotationModel
    {
        public bool SynchronizeEnabled { get; set; }

        public InterpolateOptions InterpolateOption { get; set; }

        public float InterpolateRotateTowardsSpeed { get; set; }

        public float InterpolateLerpSpeed { get; set; }

        public enum InterpolateOptions
        {
            Disabled,
            RotateTowards,
            Lerp,
        }
    }

    public class PhotonTransformViewScaleModel
    {
        public bool SynchronizeEnabled { get; set; }

        public InterpolateOptions InterpolateOption { get; set; }

        public float InterpolateMoveTowardsSpeed { get; set; }

        public float InterpolateLerpSpeed { get; set; }

        public enum InterpolateOptions
        {
            Disabled,
            MoveTowards,
            Lerp,
        }
    }
}
