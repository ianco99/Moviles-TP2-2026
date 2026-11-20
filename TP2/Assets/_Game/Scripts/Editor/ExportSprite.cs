  using System.IO;
  using UnityEditor;
  using UnityEngine;

  public static class ExportSprite                                                                                                                                                                        
  {                                                                                                                                                                                                       
      [MenuItem("Tools/Export Selected Sprite")]                                                                                                                                                          
      static void Export()                                                                                                                                                                                
      {                                                                                                                                                                                                   
          Sprite sprite = Selection.activeObject as Sprite;                                                                                                                                                  
          if (sprite == null) { Debug.LogWarning("Select a Sprite"); return; }                                                                                                                            
                                                                                                                                                                                                          
          Texture2D texture = sprite.texture;                                                                                                                                                                       
          RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);                                                                                                                                     
          Graphics.Blit(texture, renderTexture);                                                                                                                                                                         
          RenderTexture previousActive = RenderTexture.active;                                                                                                                                                                
          RenderTexture.active = renderTexture;                                                                                                                                                                      
          Texture2D copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);                                                                                                                   
          copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);                                                                                                                                   
          copy.Apply();                                                                                                                                                                                   
          RenderTexture.active = previousActive;                                                                                                                                                                    
          RenderTexture.ReleaseTemporary(renderTexture);                                                                                                                                                             
                                                                                                                                                                                                          
          File.WriteAllBytes("Assets/" + sprite.name + ".png", copy.EncodeToPNG());                                                                                                                       
          AssetDatabase.Refresh();                                                                                                                                                                        
      }                                                                                                                                                                                                   
  }