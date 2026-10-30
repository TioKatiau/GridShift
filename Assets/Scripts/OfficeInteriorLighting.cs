using UnityEngine;
// Local artificial lights remain independent of the city's solar clock.
public sealed class OfficeInteriorLighting:MonoBehaviour {
 public Light[] fixtures;
 [Min(0)]public float intensity=5;
 public Color color=new Color(1,.91f,.77f);
 public float range=5;
 public LightShadows shadows=LightShadows.Hard;
 void OnEnable(){Apply();}
 void OnValidate(){Apply();}
 public void Apply(){if(fixtures==null)return;foreach(var light in fixtures)if(light!=null){light.enabled=true;light.type=LightType.Spot;light.spotAngle=110;light.innerSpotAngle=70;light.intensity=intensity;light.color=color;light.range=range;light.shadows=shadows;}}
}
