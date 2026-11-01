  using System.IO;
  using UnityEditor;
  using UnityEngine;

  public static class ExportSprite                                                                                                                                                                        
  {                                                                                                                                                                                                       
      [MenuItem("Tools/Export Selected Sprite")]                                                                                                                                                          
      static void Export()                                                                                                                                                                                
      {                                                                                                                                                                                                   
          var sprite = Selection.activeObject as Sprite;                                                                                                                                                  
          if (sprite == null) { Debug.LogWarning("Select a Sprite"); return; }                                                                                                                            
                                                                                                                                                                                                          
          var tex = sprite.texture;                                                                                                                                                                       
          var rt = RenderTexture.GetTemporary(tex.width, tex.height);                                                                                                                                     
          Graphics.Blit(tex, rt);                                                                                                                                                                         
          var prev = RenderTexture.active;                                                                                                                                                                
          RenderTexture.active = rt;                                                                                                                                                                      
          var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);                                                                                                                   
          copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);                                                                                                                                   
          copy.Apply();                                                                                                                                                                                   
          RenderTexture.active = prev;                                                                                                                                                                    
          RenderTexture.ReleaseTemporary(rt);                                                                                                                                                             
                                                                                                                                                                                                          
          File.WriteAllBytes("Assets/" + sprite.name + ".png", copy.EncodeToPNG());                                                                                                                       
          AssetDatabase.Refresh();                                                                                                                                                                        
      }                                                                                                                                                                                                   
  }