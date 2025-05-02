Shader "Unlit/EfeitoFundo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Speed", Range(0, 5)) = 1.0
        _Distortion ("Distortion", Range(0, 1)) = 0.1
        _Color1 ("Color 1", Color) = (1,0,0,1)
        _Color2 ("Color 2", Color) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float _Distortion;
            float4 _Color1;
            float4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeOffset = _Time.y * _Speed;
                float distortionX = sin(i.uv.y * 10.0 + timeOffset) * _Distortion;
                float distortionY = cos(i.uv.x * 10.0 + timeOffset) * _Distortion;
                float2 uvDistorted = i.uv + float2(distortionX, distortionY);
                
                fixed4 texColor = tex2D(_MainTex, uvDistorted);
                
                float colorLerp = (sin(_Time.y * 2.0) + 1.0) * 0.5;
                fixed4 finalColor = lerp(_Color1, _Color2, colorLerp) * texColor;
                
                return finalColor;
            }
            ENDCG
        }
    }
}
