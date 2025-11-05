Shader "Custom/FogOfWar"
{
    Properties
    {
        _MainTex ("Base (built-in)", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0,0,0,1)
        _PlayerPos ("Player Viewport Pos", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius (viewport)", Float) = 0.2
        _Feather ("Feather (viewport)", Float) = 0.08
        _Darkness ("Darkness (0-1)", Range(0,1)) = 0.85
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _FogColor;
            float4 _PlayerPos; // x = viewport x, y = viewport y
            float _Radius;
            float _Feather;
            float _Darkness;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = tex2D(_MainTex, uv);

                // distance in viewport space (0..1)
                float2 diff = uv - _PlayerPos.xy;
                float dist = length(diff);

                // compute smooth mask: 0 inside radius, 1 outside
                float mask = smoothstep(_Radius, _Radius + _Feather, dist);

                // blend fog color over scene
                fixed4 fogColor = _FogColor;
                fogColor.a = saturate(_Darkness); // how strong the fog is
                // multiply scene by (1-mask) then lerp to fogColor
                fixed4 outCol = lerp(col, lerp(col * (1.0 - fogColor.a), fogColor, fogColor.a), mask);

                // optionally darken more outside:
                outCol.rgb = lerp(outCol.rgb, outCol.rgb * (1.0 - _Darkness), mask);

                return outCol;
            }
            ENDCG
        }
    }
}
