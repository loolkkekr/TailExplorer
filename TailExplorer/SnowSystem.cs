using UnityEngine;

namespace SceneExplorerMod
{
    public class SnowSystem
    {
        private class Snowflake
        {
            public float X;
            public float Y;
            public float Speed;
            public float Size;
            public float SwayOffset;
            public float SwaySpeed;
        }

        private List<Snowflake> _flakes = new List<Snowflake>();
        private int _maxFlakes = 150;
        private Texture2D _snowTex;

        public SnowSystem()
        {
            // Создаем текстуру снежинки
            _snowTex = new Texture2D(2, 2);
            _snowTex.SetPixel(0, 0, Color.white);
            _snowTex.SetPixel(1, 1, Color.white);
            _snowTex.SetPixel(0, 1, Color.white);
            _snowTex.SetPixel(1, 0, Color.white);
            _snowTex.Apply();

            for (int i = 0; i < _maxFlakes; i++)
            {
                ResetFlake(new Snowflake(), true);
            }
        }

        private void ResetFlake(Snowflake f, bool randomY = false)
        {
            // ИСПРАВЛЕНИЕ: Добавлено UnityEngine. перед Random
            f.X = UnityEngine.Random.Range(0, Screen.width);
            f.Y = randomY ? UnityEngine.Random.Range(0, Screen.height) : -10f;
            f.Speed = UnityEngine.Random.Range(20f, 60f);
            f.Size = UnityEngine.Random.Range(2f, 5f);
            f.SwayOffset = UnityEngine.Random.Range(0f, 100f);
            f.SwaySpeed = UnityEngine.Random.Range(0.5f, 2f);

            if (!_flakes.Contains(f)) _flakes.Add(f);
        }

        public void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float screenH = Screen.height;

            foreach (var f in _flakes)
            {
                f.Y += f.Speed * dt;

                float sway = Mathf.Sin(Time.unscaledTime * f.SwaySpeed + f.SwayOffset) * 20f * dt;
                f.X += sway;

                if (f.Y > screenH)
                {
                    ResetFlake(f);
                }
            }
        }

        public void Draw()
        {
            Color originalColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.8f);

            foreach (var f in _flakes)
            {
                GUI.DrawTexture(new Rect(f.X, f.Y, f.Size, f.Size), _snowTex);
            }

            GUI.color = originalColor;
        }
    }
}