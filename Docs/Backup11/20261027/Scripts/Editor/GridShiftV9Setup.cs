using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class GridShiftV9Setup {
 [MenuItem("GridShift/9.0/Importar identidade visual")]
 public static void Import(){
 var paths=Directory.GetFiles("Assets/UI/GridShift","*.png",SearchOption.AllDirectories).Where(p=>!p.Contains("08_Previews")).ToArray();
 foreach(var path in paths){var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;
 var name=Path.GetFileNameWithoutExtension(path);bool panel=name.StartsWith("Painel")||name.StartsWith("Botao")||name.StartsWith("Aba")||name.StartsWith("Campo")||name.StartsWith("Quadro")||name.StartsWith("Etiqueta");t.spriteBorder=panel?new Vector4(32,24,32,24):name.StartsWith("Slider_")&&!name.Contains("Puxador")?new Vector4(10,6,10,6):Vector4.zero;t.SaveAndReimport();}
 var theme=AssetDatabase.LoadAssetAtPath<GridShiftUITheme>("Assets/Resources/GridShiftUITheme.asset");if(theme==null){theme=ScriptableObject.CreateInstance<GridShiftUITheme>();AssetDatabase.CreateAsset(theme,"Assets/Resources/GridShiftUITheme.asset");}theme.sprites=paths.Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray();EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();Directory.CreateDirectory("Docs/Validacao9");File.WriteAllText("Docs/Validacao9/importacao.txt","Sprites importados: "+theme.sprites.Length+"\nMissing: "+theme.sprites.Count(s=>s==null)+"\n");
 }
 public static void Batch(){Import();EditorApplication.Exit(0);}
}
