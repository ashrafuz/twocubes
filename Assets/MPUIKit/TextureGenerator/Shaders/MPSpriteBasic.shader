// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "MPUI/Sprite Basic"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _TextureSize("Texture Size", Vector) = (1, 1, 1, 1)

        _Width ("Width", float) = 100
        _Height ("Height", float) = 100
        _PixelWorldScale ("Pixel world scale", Range(0.01, 5)) = 1

        _Radius ("Radius", Vector) = (0, 0, 0, 0)
        _LineWeight ("Line Weight", float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "2D_SDF.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            #pragma multi_compile_local _ BASE_IMAGE
            #pragma multi_compile_local _ PROCEDURAL_CUT
            
            #pragma multi_compile_local _ CIRCLE 
            #pragma multi_compile_local _ TRIANGLE 
            #pragma multi_compile_local _ RECTANGLE 
            #pragma multi_compile_local _ PENTAGON 
            #pragma multi_compile_local _ HEXAGON 
            #pragma multi_compile_local _ OCTAGON
            
            #pragma multi_compile_local _ OUTLINE
            #pragma multi_compile_local _ ROUNDED_CORNERS
            #pragma multi_compile_local SOLID GRADIENT_LINEAR GRADIENT_RADIAL

            struct appdata_t {
                float4 vertex: POSITION;
                float4 color: COLOR;
                float2 texcoord: TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex: SV_POSITION;
                fixed4 color: COLOR;
                float2 texcoord: TEXCOORD0;
                float4 worldPosition: TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSize;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _Width;
            float _Height;
            float4 _RectangleCornerRadius;
            float _CircleRadius;
            half _PixelWorldScale;
            half _LineWeight;
            
            #if PENTAGON
            float4 _PentagonRectangleRadius;
            float _PentagonTriangleRadius;
            float _PentagonTriangleSize;
            #endif
            
            #if TRIANGLE
            float3 _TriangleRadius;
            
            #endif
            
            
            #if GRADIENT_LINEAR || GRADIENT_RADIAL
                half interpolationType;
                half colorsLength;
                half alphasLength;
                half4 colors[8];
                half4 alphas[8];
                half gradientRot;
            #endif

            #if GRADIENT_LINEAR || GRADIENT_RADIAL
                float4 SampleGradient(float Time) {
                    float3 color = colors[0].rgb;
                    [unroll]
                    for (int c = 1; c < 8; c++)
                    {
                        float colorPos = saturate((Time - colors[c-1].w) / (colors[c].w - colors[c-1].w)) * step(c, colorsLength-1);
                        color = lerp(color, colors[c].rgb, lerp(colorPos, step(0.01, colorPos), interpolationType));
                    }
                    
                    float alpha = alphas[0].x;
                    [unroll]
                    for (int a = 1; a < 8; a++)
                    {
                        float alphaPos = saturate((Time - alphas[a-1].y) / (alphas[a].y - alphas[a-1].y)) * step(a, alphasLength-1);
                        alpha = lerp(alpha, alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), interpolationType));
                    }
                    return float4(color, alpha);
                }
            #endif
            
            #if RECTANGLE
            half rectanlgeScene(half2 _texcoord){
                // 1. Draw Main Rectangle
                half rect = rectanlge(_texcoord - half2(_Width/2.0, _Height/2.0), _Width, _Height);
      
                half4 radius = _RectangleCornerRadius;
                
                half cornerCircle = circle(_texcoord - radius.xx , radius.x);
                rect = _texcoord.x < radius.x && _texcoord.y < radius.x ? cornerCircle : rect;
                cornerCircle = circle(_texcoord - half2(_Width - radius.y, radius.y), radius.y);
                rect = _texcoord.x > _Width- radius.y && _texcoord.y < radius.y ? cornerCircle : rect;
                cornerCircle = circle(_texcoord - (half2(_Width, _Height)-radius.zz), radius.z);
                rect = _texcoord.x > _Width- radius.z && _texcoord.y > _Height - radius.z ? cornerCircle : rect;
                cornerCircle = circle(_texcoord - half2(radius.w, _Height - radius.w), radius.w);
                rect = _texcoord.x < radius.w && _texcoord.y > _Height - radius.w ? cornerCircle : rect;

                return rect;
            }
            #endif
            
            #if CIRCLE
            half circleScene(half2 _texcoord){
                half sdf = circle(_texcoord - half2(_Width/2, _Height/2), _CircleRadius);
                return sdf;
            }
            #endif
            
            #if TRIANGLE
            half triangleScene(half2 _texcoord){
                half sdf = sdTriangleIsosceles(_texcoord-half2(_Width/2, _Height), half2(_Width/2, -_Height));
                
                _TriangleRadius = max(_TriangleRadius, half3(0.001, 0.001, 0.001));
                // Left Corner
                half halfWidth = _Width/2;
                half m = _Height/halfWidth;
                half d = sqrt(1+m*m);
                half c = 0;
                half k = -_TriangleRadius.x * d + c;
                half x = (_TriangleRadius.x - k)/m;
                half2 circlePivot = half2(x, _TriangleRadius.x);
                half cornerCircle = circle(_texcoord - circlePivot, _TriangleRadius.x);
                x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m); 
                half y = m*x + c;
                half fy = map(_texcoord.x, x, circlePivot.x, y, circlePivot.y);
                sdf = _texcoord.y < fy && _texcoord.x < circlePivot.x ? cornerCircle : sdf;
                
                // Right Corner
                m = -m; c = 2 * _Height;
                k = -_TriangleRadius.y * d + c;
                x = (_TriangleRadius.y - k)/m;
                circlePivot = half2(x, _TriangleRadius.y);
                cornerCircle = circle(_texcoord - circlePivot, _TriangleRadius.y);
                x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m); y = m*x + c;
                fy = map(_texcoord.x, circlePivot.x, x, circlePivot.y, y);
                sdf = _texcoord.x > circlePivot.x && _texcoord.y < fy ? cornerCircle : sdf;
                
                //Top Corner
                k = -_TriangleRadius.z * sqrt(1+m*m) + c;
                y = m*(_Width/2)+k;
                circlePivot = half2(halfWidth, y);
                cornerCircle = circle(_texcoord - circlePivot, _TriangleRadius.z);
                x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m); y = m*x + c;
                fy = map(_texcoord.x, _Width-x, x, -1, 1);
                fy = lerp(circlePivot.y,y, abs(fy));
                sdf = _texcoord.y > fy ? cornerCircle : sdf;
                
                return sdf;
            }
            #endif
            
            #if PENTAGON
            half pentagonScene(half2 _texcoord){
                 // solid pentagon
                 half baseRect = rectanlge(_texcoord - half2(_Width/2.0, _Height/2.0), _Width, _Height);
                 half scale = _Height/_PentagonTriangleSize;
                 half rhombus = sdRhombus(_texcoord - half2(_Width/2, _PentagonTriangleSize * scale), half2(_Width/2, _PentagonTriangleSize) * scale);
                 half sdfPentagon = sdfDifference(baseRect, sdfDifference(baseRect, rhombus));
                 
                 // Bottom rounded corner
                 _PentagonTriangleRadius = max(_PentagonTriangleRadius, 0.001);
                 half halfWidth = _Width/2;
                 half m = -_PentagonTriangleSize/halfWidth; 
                 half d = sqrt(1 + m*m);
                 half c = _PentagonTriangleSize;
                 half k = _PentagonTriangleRadius * d + _PentagonTriangleSize;
                 
                 half2 circlePivot = half2(halfWidth, m * halfWidth+k);
                 half cornerCircle = circle(_texcoord - circlePivot, _PentagonTriangleRadius);
                 half x = x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m);
                 half y = m*x + c;
                 half fy = map(_texcoord.x, x, _Width-x, -1, 1);
                 fy = lerp(_PentagonTriangleRadius, y, abs(fy));
                 sdfPentagon = _texcoord.y<fy ? cornerCircle : sdfPentagon; 
                 
                 // Mid Left rounded corner
                 k = _PentagonRectangleRadius.w * d + _PentagonTriangleSize;
                 circlePivot = half2(_PentagonRectangleRadius.w, m*_PentagonRectangleRadius.w+k);
                 cornerCircle = circle(_texcoord - circlePivot, _PentagonRectangleRadius.w);
                 x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m); y = m*x + c;
                 fy = map(_texcoord.x, x, circlePivot.x, y, circlePivot.y);
                 sdfPentagon = _texcoord.y > fy && _texcoord.y < circlePivot.y ? cornerCircle : sdfPentagon;
                 
                 // Mid Right rounded corner
                 m = -m; k = _PentagonRectangleRadius.z * d - _PentagonTriangleSize;
                 circlePivot = half2(_Width-_PentagonRectangleRadius.z, m*(_Width-_PentagonRectangleRadius.z)+k);
                 cornerCircle = circle(_texcoord - circlePivot, _PentagonRectangleRadius.z);
                 x = (circlePivot.y+ circlePivot.x/m-c)/(m+1/m); y = m*x + c;
                 fy = map(_texcoord.x, circlePivot.x, x, circlePivot.y, y);
                 sdfPentagon = _texcoord.y > fy && _texcoord.y < circlePivot.y ? cornerCircle : sdfPentagon;
                 
                 // Top rounded corners
                 cornerCircle = circle(_texcoord - half2(_PentagonRectangleRadius.x, _Height-_PentagonRectangleRadius.x), _PentagonRectangleRadius.x);
                 bool mask = _texcoord.x < _PentagonRectangleRadius.x && _texcoord.y > _Height - _PentagonRectangleRadius.x;
                 sdfPentagon = mask ? cornerCircle : sdfPentagon;
                 cornerCircle = circle(_texcoord - half2(_Width - _PentagonRectangleRadius.y, _Height-_PentagonRectangleRadius.y), _PentagonRectangleRadius.y);
                 mask = _texcoord.x > _Width - _PentagonRectangleRadius.y && _texcoord.y > _Height - _PentagonRectangleRadius.y;
                 sdfPentagon = mask ? cornerCircle : sdfPentagon;
                 
                 return sdfPentagon;
            }
            #endif

            v2f vert(appdata_t v) {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord * float2(_Width, _Height);
                #ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN): SV_Target {
            
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 color = IN.color;
                
                half2 texcoord = ((IN.texcoord - _TextureSize.zw) / half2(_Width, _Height)) * _TextureSize.xy;
                #if BASE_IMAGE
                    color = (tex2D(_MainTex, texcoord) + _TextureSampleAdd) * IN.color;
                #endif
                
                #if GRADIENT_LINEAR || GRADIENT_RADIAL
                    #if GRADIENT_LINEAR
                        half4 grad = SampleGradient(texcoord.x);
                        color *= grad;
                    #else
                        half fac = saturate(length(texcoord - float2(.5, .5)));
                        half4 grad = SampleGradient(clamp(fac, 0, 1));
                        color *= grad;
                    #endif
                #endif
                

                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xz, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                #if PROCEDURAL_CUT
                    half sdfData = 0;
                    #if RECTANGLE
                        sdfData = rectanlgeScene(IN.texcoord);
                    #elif CIRCLE
                        sdfData = circleScene(IN.texcoord);
                    #elif PENTAGON
                        sdfData = pentagonScene(IN.texcoord);
                    #elif TRIANGLE
                        sdfData = triangleScene(IN.texcoord);
                    #endif
                    
                    #if OUTLINE
                        sdfData = sampleSdfStrip(sdfData, _LineWeight, _PixelWorldScale);
                    #else
                        sdfData = sampleSdf(sdfData, _PixelWorldScale);
                    #endif
                    color.a *= sdfData;
                    //color = fixed4(sdfData, sdfData, sdfData, 1);
                #endif
                return fixed4(color);
            }
            ENDCG
            
        }
    }
}
