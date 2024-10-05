using System.Data;

public static class Constants{
        public const float G = .001f; //6.67430e-11f; // Gravitational constant
        public const float MIN_INFLUENCE = .01f * G;
        public const int TARGET_FPS = 60;
        public const float MIN_CAPTURE_DISTANCE = 20F;
        public const int ORBIT_SLOW_PREDICT_TIME_SECONDS = 2;
}