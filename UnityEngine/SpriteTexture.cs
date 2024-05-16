namespace UnityEngine
{
    public class SpriteTexturePack : ScriptableObject
    {
        private Sprite sprite;
        private Texture2D texture;
        public Sprite Sprite => sprite;
        public Texture2D Texture => texture;

        public SpriteTexturePack(Sprite sprite, Texture2D texture)
        {
            this.sprite = sprite;
            this.texture = texture;
        }

        public void SetTexture(Texture2D texture) => this.texture = texture;

        public void SetSprite(Sprite sprite) => this.sprite = sprite;
    }
}
