namespace HandTrackingModule
{
    public enum Gesture
    {
        None,
        MiddleFinger,
        ThumbsUp,
        Fist,
        OpenPalm,
        Ok,
        Metal,
        WebShooter,
        Number1,
        Number2,
        Number3
    }

    public enum Orientation
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    
    public enum HandType
    {
        Right,
        Left
    }

    public enum ReceiveType
    {
        Landmarks,
        Orientation,
        Gesture
    }
}