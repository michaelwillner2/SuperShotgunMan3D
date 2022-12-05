Shader "Unlit/AffineTextureWarp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Lightmap("Texture", 2D) = "white" {}
		_Light("Light Level", Float) = 0.5
		_UnlitColor("Unlit Color", Color) = (0.0, 0.0, 0.0, 1.0)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                noperspective float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                noperspective float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _Lightmap;
			float4 _Lightmap_ST;
			float _Light;
			float4 _UnlitColor;

			half3 ObjectScale() {
				return half3(
					length(unity_ObjectToWorld._m00_m10_m20),
					length(unity_ObjectToWorld._m01_m11_m21),
					length(unity_ObjectToWorld._m02_m12_m22)
					);
			}

			half Average(half3 scale) {
				return (scale.x + scale.y + scale.z) / 3.0;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv = TRANSFORM_TEX(v.uv, _Lightmap);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture and lighting
				fixed4 col = tex2D(_MainTex, i.uv * Average(ObjectScale()));
				fixed4 unlitcol = col * _UnlitColor;
				fixed4 lightmapcol = tex2D(_Lightmap, i.uv * Average(ObjectScale()));
				lightmapcol = lerp(col, lightmapcol, lightmapcol.a);

				//interpolate between lit values by Light
				col = lerp(unlitcol, col, clamp(_Light * 2.0, 0.0, 1.0));
				col = lerp(col, lightmapcol, clamp((_Light - 0.5) * 2.0, 0.0, 1.0))

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
