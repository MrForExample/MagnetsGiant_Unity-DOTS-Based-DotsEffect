Shader "OmniSAR Technologies/Heavy Rendering Test" {
	Properties {
        _Complexity ("Complexity (This may cripple your machine)", Range(0, 1)) = 0.01
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
        ZWrite On

		CGPROGRAM

		#pragma surface surf Standard
        #pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

        uniform float _Complexity;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = 1;

            // do some useless heavy stuff
            // also, add some sloppy coding to help the cause
            float4 k = 0.5;
            float4 t = 0.5;
            float4 s = 0.0;
            for (int i = 0; i < (int)(_Complexity * 10000000.0); i++) {
                k = (float)i / (float)10000 * t;
                t = (float)i / (float)10000 * k;
                s = (k * t) * (k * t);
                o.Albedo.r = 0.5 + sin(_Time * 23) / 2;
                o.Albedo.g = 0.5 + sin(_Time * 31) / 2;
                o.Albedo.b = 0.5 + sin(_Time * 57) / 2;
            }
		}
		ENDCG
	}
	FallBack "Diffuse"
}
