using UnityEngine;

namespace TextureEditing
{
    public class SpriteMerger
    {

        public static void Merge(Sprite[] spritesToMerge, int size, int knobSize, SpriteRenderer targetRenderer = null)
        {
            Resources.UnloadUnusedAssets();
            var newTexture = new Texture2D(spritesToMerge[0].texture.width, spritesToMerge[0].texture.height);

            for (int x = 0; x < newTexture.width; x++)
            for (int y = 0; y < newTexture.height; y++)
            {
                newTexture.SetPixel(x, y, new Color(0, 0, 0, 0));

            }


            for (int i = 0; i < spritesToMerge.Length; i++)
            for (int x = 0; x < spritesToMerge[i].texture.width; x++)
            for (int y = 0; y < spritesToMerge[i].texture.height; y++)
            {
                var color = spritesToMerge[i].texture.GetPixel(x, y).a == 0
                    ? newTexture.GetPixel(x, y)
                    : spritesToMerge[i].texture.GetPixel(x, y);
                newTexture.SetPixel(x, y, color);
            }

            newTexture.Apply();
            var finalSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height),
                new Vector2(0.5f, .5f));
            finalSprite.name = "newSprite";
            targetRenderer.sprite = finalSprite;

        }

        public static Texture2D MergePuzzleMaskTexture(Texture2D[] spritesToMerge, SpriteRenderer targetRenderer = null)
        {
            var newTexture = new Texture2D(spritesToMerge[0].width, spritesToMerge[0].height);

            for (int x = 0; x < newTexture.width; x++)
            for (int y = 0; y < newTexture.height; y++)
            {
                newTexture.SetPixel(x, y, new Color(0, 0, 0, 0));

            }


            for (int i = 0; i < spritesToMerge.Length; i++)
            for (int x = 0; x < spritesToMerge[i].width; x++)
            for (int y = 0; y < spritesToMerge[i].height; y++)
            {

                var color = spritesToMerge[i].GetPixel(x, y).a == 0
                    ? newTexture.GetPixel(x, y)
                    : spritesToMerge[i].GetPixel(x, y);
                newTexture.SetPixel(x, y, color);

            }

            newTexture.Apply();
            if (targetRenderer != null)
            {
                var finalSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height),
                    new Vector2(.5f, .5f));
                finalSprite.name = "newSprite";
                targetRenderer.sprite = finalSprite;
            }

            return newTexture;
        }

        public static Texture2D InsertTextureWithOffset(Texture2D original, Texture2D textureToInsert, Vector2 offset,
            SpriteRenderer targetRenderer = null)
        {
            var newTexture = new Texture2D(original.width, original.height);

            var colors = original.GetPixels();
            newTexture.SetPixels(colors);

            /*for (int x = 0; x < newTexture.width; x++)
                for (int y = 0; y < newTexture.height; y++)
                {
                    newTexture.SetPixel(x,y,new Color(0,0,0,0));
                }*/



            for (int x = 0; x < textureToInsert.width; x++)
            for (int y = 0; y < textureToInsert.height; y++)
            {
                var color = textureToInsert.GetPixel(x, y).a == 0
                    ? newTexture.GetPixel((int)offset.x + x, (int)offset.y + y)
                    : textureToInsert.GetPixel(x, y);
                newTexture.SetPixel(x + (int)offset.x, y + (int)offset.y, color);
            }

            newTexture.Apply();
            return newTexture;

        }

        public static Texture2D InsertMask(Texture2D original, Texture2D textureToInsert, Vector2 offset,
            SpriteRenderer targetRenderer = null)
        {
            var newTexture = new Texture2D(original.width, original.height);

            var colors = original.GetPixels();
            newTexture.SetPixels(colors);

            /*for (int x = 0; x < newTexture.width; x++)
                for (int y = 0; y < newTexture.height; y++)
                {
                    newTexture.SetPixel(x,y,new Color(0,0,0,0));
                }*/



            for (int x = 0; x < textureToInsert.width; x++)
            for (int y = 0; y < textureToInsert.height; y++)
            {
                var color = textureToInsert.GetPixel(x, y).a == 0
                    ? Color.clear
                    : Color.white;
                newTexture.SetPixel(x + (int)offset.x, y + (int)offset.y, color);
            }

            newTexture.Apply();
            return newTexture;

        }

        public static Texture2D InvertMask(Texture2D original)
        {
            Texture2D newTexture = new Texture2D(original.width, original.height);

            for (int x = 0; x < newTexture.width; x++)
            {
                for (int y = 0; y < newTexture.height; y++)
                {
                    var color = original.GetPixel(x, y).a == 0 ? Color.white : Color.clear;
                    newTexture.SetPixel(x, y, color);
                }
            }

            newTexture.Apply();
            return newTexture;
        }
    }
}