using System.Data;

public static class Constants{
        public const float G = .1f; //6.67430e-11f; // Gravitational constant
        public const float MIN_INFLUENCE = .02f * G;
        public const int TARGET_FPS = 60;
        public const float MIN_CAPTURE_DISTANCE = 20F;
        public const int ORBIT_SLOW_PREDICT_TIME_SECONDS = 2;
        public const int FONT_SIZE = 30;
        public const float SHIP_ACCELERATION = 0.001f;
        public const int MIN_DISTANCE_TO_MOUSE = 100;
        public const int TURN_SECONDS = 10;
}