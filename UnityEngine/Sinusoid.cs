namespace UnityEngine
{
    public static class Sinusoid
    {
        private static int position = 0;
        private static int samplerate = 44100;
        private static float frequency = 440;

        private static AudioClip clip;
        public static AudioClip Clip
        {
            get
            {
                if (clip == null)
                    clip = AudioClip.Create("Sinusoid", samplerate * 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
                return clip;
            }
        }

        private static void OnAudioRead(float[] data)
        {
            int count = 0;
            while (count < data.Length)
            {
                data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate);
                position++;
                count++;
            }
        }

        private static void OnAudioSetPosition(int newPosition)
        {
            position = newPosition;
        }
    }
}
