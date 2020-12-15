Shader "Oof/GreyscaleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Scale ("Greyscale Strength", Float) = 0.0
		_Stage ("Stage", Float) = 0.0
		_Contrast ("Contrast", Float) = 1.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			float _Scale;
			float _Stage;
			float _Contrast;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				float a = col.a;
				col.rgb = lerp(col.rgb, (col.r + col.g + col.b / _Scale) * _Contrast, _Stage);
				col.a = a;
				return col;
            }
            ENDCG
        }
    }
}
