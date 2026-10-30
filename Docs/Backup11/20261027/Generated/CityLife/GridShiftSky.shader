Shader "GridShift/City Sky" {
 Properties { _Night("Night",Range(0,1))=0 }
 SubShader {
 Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct v2f {float4 position:SV_POSITION;float3 direction:TEXCOORD0;};
 float _Night;float4 _SunDirection;
 v2f vert(float4 vertex:POSITION){v2f o;o.position=UnityObjectToClipPos(vertex);o.direction=vertex.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float4 frag(v2f i):SV_Target {
 float3 d=normalize(i.direction);float height=saturate(d.y);
 float3 day=lerp(float3(.52,.65,.76),float3(.12,.31,.58),sqrt(height));
 float3 night=lerp(float3(.045,.075,.15),float3(.012,.025,.075),sqrt(height));
 float3 color=lerp(day,night,_Night);
 float2 stars=float2(atan2(d.x,d.z),asin(d.y))*160;
 float star=step(.997,hash(floor(stars)))*(1-smoothstep(.015,.08,length(frac(stars)-.5)));
 color+=star*float3(.65,.72,.9)*_Night*smoothstep(.08,.3,d.y);
 float moon=smoothstep(.9993,.9995,dot(d,normalize(float3(-.35,.5,-.7))));
 color+=moon*float3(.65,.73,.86)*_Night;
 float sun=smoothstep(.99975,.99985,dot(d,normalize(_SunDirection.xyz)));
 color+=sun*float3(1,.7,.32)*(1-_Night);
 return float4(color,1);
 }
 ENDCG
 }
 }
 Fallback Off
}
