Shader "Custom/Noise Shader" {
	Properties{
	  _MainTex("Texture", 2D) = "white" {}
	  _Level("Level", Range(-1,1)) = 0.5
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert vertex:vert
		  struct Input {
			  float2 uv_MainTex;
		  };
		  float _Level;
		  
		  float rand(half2 uv) {
			  return frac(sin(dot(uv, float2(100, 100))) * 100);
		  }
		  
		  void vert(inout appdata_full v) {
			  float3 vv = v.vertex.xyz;
			  v.vertex.xyz += v.normal * _Level;
		  }
		  sampler2D _MainTex;
		  void surf(Input IN, inout SurfaceOutput o) {
			  o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		  }
		  ENDCG
	  }
		  Fallback "Diffuse"
}