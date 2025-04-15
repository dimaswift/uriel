namespace Uriel.Domain
{
    [System.Serializable]
    public struct Source
    {
        public Vec2 position;
        public int frequency;
        public float amplitude;

        public static int Stride()
        {
            return sizeof(int) * 3 + sizeof(float);
        }
    }
}