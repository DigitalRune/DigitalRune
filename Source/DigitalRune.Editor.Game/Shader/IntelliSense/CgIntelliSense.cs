// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Provides IntelliSense for NVIDIA Cg and CgFX files.
    /// </summary>
    internal class CgIntelliSense : ShaderIntelliSense
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CgIntelliSense"/> class.
        /// </summary>
        public CgIntelliSense()
        {
            // Language specific aspects that are equal in HLSL and Cg are
            // defined in the base class ShaderIntelliSense.

            // Cg-specific IntelliSense is initialized here:
            InitializeSnippets();
            InitializePreprocessorDirectives();
            InitializeKeywords();
            InitializeTypes();
            InitializeFunctions();
            InitializeEffectFunctions();
            InitializeEffectStates();
            InitializeSamplerStates();
            InitializeStateValues();

            // Validate all states (only in DEBUG).
            ValidateStates();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the Cg-specific snippets.
        /// </summary>
        private void InitializeSnippets()
        {
        }


        /// <summary>
        /// Initializes the Cg preprocessor directives.
        /// </summary>
        private void InitializePreprocessorDirectives()
        {
            PreprocessorDirectives.Add(new KeywordCompletionData("#pragma"));
        }


        /// <summary>
        /// Initializes the Cg and CgFX keywords.
        /// </summary>
        private void InitializeKeywords()
        {
            string[] keywords =
            {
                "packed", "varying",
                "dx8ps", "dx8vs", "dx9ps2", "dxvs2", "hlslf", "hlslv",
                "arbfp1", "arbvp1", "fp20", "fp30", "fp30unlimited", "fp40", "fp40unlimited", "glslf", "glslv", "gp4fp",
                "gp4gp", "gp4vp", "gpu_fp", "gpu_gp", "gpu_vp", "vp20", "vp30", "vp40",
            };
            foreach (string keyword in keywords)
                Keywords.Add(new KeywordCompletionData(keyword));
        }


        /// <summary>
        /// Initializes the Cg types.
        /// </summary>
        private void InitializeTypes()
        {
            string[] scalarTypes =
            {
                "unsigned",
                "fixed",
                "char",
                "short",
            };
            foreach (string type in scalarTypes)
                ScalarTypes.Add(new TypeCompletionData(type));

            string[] types =
            {
                "fixed1", "fixed2", "fixed3", "fixed4",
                "fixed1x1", "fixed1x2", "fixed1x3", "fixed1x4",
                "fixed2x1", "fixed2x2", "fixed2x3", "fixed2x4",
                "fixed3x1", "fixed3x2", "fixed3x3", "fixed3x4",
                "fixed4x1", "fixed4x2", "fixed4x3", "fixed4x4",
            };
            foreach (string type in types)
                Types.Add(new TypeCompletionData(type));

            string[] specialTypes =
            {
                "POINT", "POINT_OUT", "LINE", "LINE_ADJ", "LINE_OUT", "TRIANGLE", "TRIANGLE_ADJ", "TRIANGLE_OUT",
                "AttribArray"
            };
            foreach (string type in specialTypes)
                SpecialTypes.Add(new TypeCompletionData(type));

            string[] effectTypes =
            {
                "interface",
                "isampler1D", "usampler1D",
                "sampler1DARRAY", "isampler1DARRAY", "usampler1DARRAY",
                "isampler2D", "usampler2D",
                "sampler2DARRAY", "isampler2DARRAY", "usampler2DARRAY",
                "isampler3D", "usampler3D",
                "samplerBUF", "isamplerBUF", "usamplerBUF",
                "isamplerCUBE", "usamplerCUBE",
                "samplerCUBEARRAY", "isamplerCUBEARRAY", "usamplerCUBEARRAY",
                "samplerRECT", "isamplerRECT", "usamplerRECT",
                "texture",
            };
            foreach (string type in effectTypes)
                EffectTypes.Add(new TypeCompletionData(type));
        }


        /// <summary>
        /// Initializes the Cg intrinsic functions.
        /// </summary>
        private void InitializeFunctions()
        {
            // TODO: Document function as soon as NVIDIA provides enough documentation. See comments below.

            Functions.Add(
                new FunctionCompletionData(
                    "abs",
                    "Returns the absolute value of the specified value.",
                    new[]
                    {
                        "float abs(float x)", "float2 abs(float2 x)", "float3 abs(float3 x)", "float4 abs(float4 x)",
                        "half abs(half x)", "half2 abs(half2 x)", "half3 abs(half3 x)", "half4 abs(half4 x)",
                        "fixed abs(fixed x)", "fixed2 abs(fixed2 x)", "fixed3 abs(fixed3 x)", "fixed4 abs(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "acos",
                    "Returns the arccosine of the specified value.",
                    new[]
                    {
                        "float acos(float x)", "float2 acos(float2 x)", "float3 acos(float3 x)", "float4 acos(float4 x)",
                        "half acos(half x)", "half2 acos(half2 x)", "half3 acos(half3 x)", "half4 acos(half4 x)",
                        "fixed acos(fixed x)", "fixed2 acos(fixed2 x)", "fixed3 acos(fixed3 x)", "fixed4 acos(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "all",
                    "Determines if all components of the specified value are non-zero.",
                    new[]
                    {
                        "bool all(bool x)", "bool all(bool2 x)", "bool all(bool3 x)", "bool all(bool4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "any",
                    "Determines if any components of the specified value are non-zero.",
                    new[]
                    {
                        "bool any(bool x)", "bool any(bool2 x)", "bool any(bool3 x)", "bool any(bool4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "asin",
                    "Returns the arcsine of the specified value.",
                    new[]
                    {
                        "float asin(float x)", "float2 asin(float2 x)", "float3 asin(float3 x)", "float4 asin(float4 x)",
                        "half asin(half x)", "half2 asin(half2 x)", "half3 asin(half3 x)", "half4 asin(half4 x)",
                        "fixed asin(fixed x)", "fixed2 asin(fixed2 x)", "fixed3 asin(fixed3 x)", "fixed4 asin(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "atan",
                    "Returns the arctangent of the specified value.",
                    new[]
                    {
                        "float atan(float x)", "float2 atan(float2 x)", "float3 atan(float3 x)", "float4 atan(float4 x)",
                        "half atan(half x)", "half2 atan(half2 x)", "half3 atan(half3 x)", "half4 atan(half4 x)",
                        "fixed atan(fixed x)", "fixed2 atan(fixed2 x)", "fixed3 atan(fixed3 x)", "fixed4 atan(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "atan2",
                    "Returns the arctangent of two values (x,y).",
                    new[]
                    {
                        "float atan2(float y, float x)", "float atan2(float2 y, float2 x)", "float atan2(float3 y, float3 x)",
                        "float4 atan2(float4 y, float4 x)",
                        "half atan2(half y, half x)", "half atan2(half2 y, half2 x)", "half atan2(half3 y, half3 x)", "half4 atan2(half4 y, half4 x)",
                        "fixed atan2(fixed y, fixed x)", "fixed atan2(fixed2 y, fixed2 x)", "fixed atan2(fixed3 y, fixed3 x)",
                        "fixed4 atan2(fixed4 y, fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ceil",
                    "Returns the smallest integer value that is greater than or equal to the specified value.",
                    new[]
                    {
                        "float ceil(float x)", "float2 ceil(float2 x)", "float3 ceil(float3 x)", "float4 ceil(float4 x)",
                        "half ceil(half x)", "half2 ceil(half2 x)", "half3 ceil(half3 x)", "half4 ceil(half4 x)",
                        "fixed ceil(fixed x)", "fixed2 ceil(fixed2 x)", "fixed3 ceil(fixed3 x)", "fixed4 ceil(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "clamp",
                    "Clamps the specified value to the specified minimum and maximum range.",
                    new[]
                    {
                        "float clamp(float x, float min, float max)", "float2 clamp(float2 x, float2 min, float2 max)",
                        "float3 clamp(float3 x, float3 min, float3 max)", "float4 clamp(float4 x, float4 min, float4 max)",
                        "half clamp(half x, half min, half max)", "half2 clamp(half2 x, half2 min, half2 max)",
                        "half3 clamp(half3 x, half3 min, half3 max)", "half4 clamp(half4 x, half4 min, half4 max)",
                        "fixed clamp(fixed x, fixed min, fixed max)", "fixed2 clamp(fixed2 x, fixed2 min, fixed2 max)",
                        "fixed3 clamp(fixed3 x, fixed3 min, fixed3 max)", "fixed4 clamp(fixed4 x, fixed4 min, fixed4 max)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "cos",
                    "Returns the cosine of the specified value.",
                    new[]
                    {
                        "float cos(float x)", "float2 cos(float2 x)", "float3 cos(float3 x)", "float4 cos(float4 x)",
                        "half cos(half x)", "half2 cos(half2 x)", "half3 cos(half3 x)", "half4 cos(half4 x)",
                        "fixed cos(fixed x)", "fixed2 cos(fixed2 x)", "fixed3 cos(fixed3 x)", "fixed4 cos(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "cosh",
                    "Returns the hyperbolic cosine of the specified value.",
                    new[]
                    {
                        "float cosh(float x)", "float2 cosh(float2 x)", "float3 cosh(float3 x)", "float4 cosh(float4 x)",
                        "half cosh(half x)", "half2 cosh(half2 x)", "half3 cosh(half3 x)", "half4 cosh(half4 x)",
                        "fixed cosh(fixed x)", "fixed2 cosh(fixed2 x)", "fixed3 cosh(fixed3 x)", "fixed4 cosh(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "cross",
                    "Returns the cross product of two floating-point, 3D vectors.",
                    new[]
                    {
                        "float3 cross(float3 x, float3 y)",
                        "half3 cross(half3 x, half3 y)",
                        "fixed3 cross(fixed3 x, fixed3 y)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ddx",
                    "Returns the partial derivative of the specified value with respect to the screen-space x-coordinate. (Pixel Shader)",
                    new[]
                    {
                        "float ddx(float x)", "float2 ddx(float2 x)", "float3 ddx(float3 x)", "float4 ddx(float4 x)",
                        "half ddx(half x)", "half2 ddx(half2 x)", "half3 ddx(half3 x)", "half4 ddx(half4 x)",
                        "fixed ddx(fixed x)", "fixed2 ddx(fixed2 x)", "fixed3 ddx(fixed3 x)", "fixed4 ddx(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ddy",
                    "Returns the partial derivative of the specified value with respect to the screen-space y-coordinate. (Pixel Shader)",
                    new[]
                    {
                        "float ddy(float x)", "float2 ddy(float2 x)", "float3 ddy(float3 x)", "float4 ddy(float4 x)",
                        "half ddy(half x)", "half2 ddy(half2 x)", "half3 ddy(half3 x)", "half4 ddy(half4 x)",
                        "fixed ddy(fixed x)", "fixed2 ddy(fixed2 x)", "fixed3 ddy(fixed3 x)", "fixed4 ddy(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "degrees",
                    "Converts the specified value from radians to degrees.",
                    new[]
                    {
                        "float degrees(float x)", "float2 degrees(float2 x)", "float3 degrees(float3 x)", "float4 degrees(float4 x)",
                        "half degrees(half x)", "half2 degrees(half2 x)", "half3 degrees(half3 x)", "half4 degrees(half4 x)",
                        "fixed degrees(fixed x)", "fixed2 degrees(fixed2 x)", "fixed3 degrees(fixed3 x)", "fixed4 degrees(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "debug",
                    "If the compiler's DEBUG option is enabled, calling this function causes the value x to be copied to the COLOR output of the program, and execution of the program is terminated.\nIf the compiler's DEBUG option is not enabled, this function does nothing.",
                    new[]
                    {
                        "void debug(float4 x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "determinant",
                    "Returns the determinant of the specified floating-point, square matrix.",
                    new[]
                    {
                        "float determinant(float1x1 m)", "float determinant(float2x2 m)", "float determinant(float3x3 m)",
                        "float determinant(float4x4 m)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
                new FunctionCompletionData(
                    "distance",
                    "Returns the Euclidean distance between two points.",
                    new[]
                    {
                        "float distance(float point1, float point2)", "float2 distance(float2 point1, float2 point2)",
                        "float3 distance(float3 point1, float3 point2)", "float4 distance(float4 point1, float4 point2)",
                        "half distance(half point1, half point2)", "half2 distance(half2 point1, half2 point2)",
                        "half3 distance(half3 point1, half3 point2)", "half4 distance(half4 point1, half4 point2)",
                        "fixed distance(fixed point1, fixed point2)", "fixed2 distance(fixed2 point1, fixed2 point2)",
                        "fixed3 distance(fixed3 point1, fixed3 point2)", "fixed4 distance(fixed4 point1, fixed4 point2)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "dot",
                    "Returns the dot product of two vectors.",
                    new[]
                    {
                        "float dot(float x, float y)", "float dot(float2 x, float2 y)", "float dot(float3 x, float3 y)", "float dot(float4 x, float4 y)",
                        "half dot(half x, half y)", "half dot(half2 x, half2 y)", "half dot(half3 x, half3 y)", "half dot(half4 x, half4 y)",
                        "fixed dot(fixed x, fixed y)", "fixed dot(fixed2 x, fixed2 y)", "fixed dot(fixed3 x, fixed3 y)", "fixed dot(fixed4 x, fixed4 y)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
                new FunctionCompletionData(
                    "emitVertex",
                    "Appends a vertex to the geometry-shader output stream.",
                    new[] { "void emitVertex(... vertexData ...)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "exp",
                    "Returns the base-e exponential, or e^x, of the specified value.",
                    new[]
                    {
                        "float exp(float x)", "float2 exp(float2 x)", "float3 exp(float3 x)", "float4 exp(float4 x)",
                        "half exp(half x)", "half2 exp(half2 x)", "half3 exp(half3 x)", "half4 exp(half4 x)",
                        "fixed exp(fixed x)", "fixed2 exp(fixed2 x)", "fixed3 exp(fixed3 x)", "fixed4 exp(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "exp2",
                    "Returns the base 2 exponential, or 2^x, of the specified value.",
                    new[]
                    {
                        "float exp2(float x)", "float2 exp2(float2 x)", "float3 exp2(float3 x)", "float4 exp2(float4 x)",
                        "half exp2(half x)", "half2 exp2(half2 x)", "half3 exp2(half3 x)", "half4 exp2(half4 x)",
                        "fixed exp2(fixed x)", "fixed2 exp2(fixed2 x)", "fixed3 exp2(fixed3 x)", "fixed4 exp2(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "faceforward",
                    "Flips the surface-normal (if needed) to face in a direction opposite to an incident vector.\nThis function uses the formula: -n * sign(dot(i, ng)).",
                    new[]
                    {
                        "float faceforward(float n, float i, float ng)", "float2 faceforward(float2 n, float2 i, float2 ng)",
                        "float3 faceforward(float3 n, float3 i, float3 ng)", "float4 faceforward(float4 n, float4 i, float4 ng)",
                        "half faceforward(half n, half i, half ng)", "half2 faceforward(half2 n, half2 i, half2 ng)",
                        "half3 faceforward(half3 n, half3 i, half3 ng)", "half4 faceforward(half4 n, half4 i, half4 ng)",
                        "fixed faceforward(fixed n, fixed i, fixed ng)", "fixed2 faceforward(fixed2 n, fixed2 i, fixed2 ng)",
                        "fixed3 faceforward(fixed3 n, fixed3 i, fixed3 ng)", "fixed4 faceforward(fixed4 n, fixed4 i, fixed4 ng)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "floatToIntBits",
                    "Converts the input type to an integer.",
                    new[]
                    {
                        "int floatToIntBits(float x)", "int2 floatToIntBits(float2 x)", "int3 floatToIntBits(float3 x)", "int4 floatToIntBits(float4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "floatToRawIntBits",
                    "Converts the input type to an integer.",
                    new[]
                    {
                        "int floatToRawIntBits(float x)", "int2 floatToRawIntBits(float2 x)", "int3 floatToRawIntBits(float3 x)",
                        "int4 floatToRawIntBits(float4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "floor",
                    "Returns the largest integer that is less than or equal to the specified value.",
                    new[]
                    {
                        "float floor(float x)", "float2 floor(float2 x)", "float3 floor(float3 x)", "float4 floor(float4 x)",
                        "half floor(half x)", "half2 floor(half2 x)", "half3 floor(half3 x)", "half4 floor(half4 x)",
                        "fixed floor(fixed x)", "fixed2 floor(fixed2 x)", "fixed3 floor(fixed3 x)", "fixed4 floor(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "fmod",
                    "Returns the floating-point remainder of x/y with the same sign as x.",
                    new[]
                    {
                        "float fmod(float x, float y)", "float2 fmod(float2 x, float2 y)", "float3 fmod(float3 x, float3 y)",
                        "float4 fmod(float4 x, float4 y)",
                        "half fmod(half x, half y)", "half2 fmod(half2 x, half2 y)", "half3 fmod(half3 x, half3 y)", "half4 fmod(half4 x, half4 y)",
                        "fixed fmod(fixed x, fixed y)", "fixed2 fmod(fixed2 x, fixed2 y)", "fixed3 fmod(fixed3 x, fixed3 y)",
                        "fixed4 fmod(fixed4 x, fixed4 y)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "frac",
                    "Returns the fractional (or decimal) part of x; which is greater than or equal to 0 and less than 1.",
                    new[]
                    {
                        "float frac(float x)", "float2 frac(float2 x)", "float3 frac(float3 x)", "float4 frac(float4 x)",
                        "half frac(half x)", "half2 frac(half2 x)", "half3 frac(half3 x)", "half4 frac(half4 x)",
                        "fixed frac(fixed x)", "fixed2 frac(fixed2 x)", "fixed3 frac(fixed3 x)", "fixed4 frac(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "frexp",
                    "Returns the mantissa and exponent of the specified floating-point value.",
                    new[]
                    {
                        "float frexp(float x, out float exp)", "float2 frexp(float2 x, out float2 exp)", "float3 frexp(float3 x, out float3 exp)",
                        "float4 frexp(float4 x, out float4 exp)",
                        "half frexp(half x, out half exp)", "half2 frexp(half2 x, out half2 exp)", "half3 frexp(half3 x, out half3 exp)",
                        "half4 frexp(half4 x, out half4 exp)",
                        "fixed frexp(fixed x, out fixed exp)", "fixed2 frexp(fixed2 x, out fixed2 exp)", "fixed3 frexp(fixed3 x, out fixed3 exp)",
                        "fixed4 frexp(fixed4 x, out fixed4 exp)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "fwidth",
                    "Returns the absolute value of the partial derivatives of the specified value. (Pixel Shader)",
                    new[]
                    {
                        "float fwidth(float x)", "float2 fwidth(float2 x)", "float3 fwidth(float3 x)", "float4 fwidth(float4 x)",
                        "half fwidth(half x)", "half2 fwidth(half2 x)", "half3 fwidth(half3 x)", "half4 fwidth(half4 x)",
                        "fixed fwidth(fixed x)", "fixed2 fwidth(fixed2 x)", "fixed3 fwidth(fixed3 x)", "fixed4 fwidth(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "intBitsToFloat",
                    "Converts the input type to a floating-point number.",
                    new[]
                    {
                        "float intBitsToFloat(int x)", "float2 intBitsToFloat(int2 x)", "float3 intBitsToFloat(int3 x)", "float4 intBitsToFloat(int4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "isfinite",
                    "Determines if the specified floating-point value is finite.",
                    new[]
                    {
                        "bool isfinite(float x)", "bool2 isfinite(float2 x)", "bool3 isfinite(float3 x)", "bool4 isfinite(float4 x)",
                        "bool isfinite(half x)", "bool2 isfinite(half2 x)", "bool3 isfinite(half3 x)", "bool4 isfinite(half4 x)",
                        "bool isfinite(fixed x)", "bool2 isfinite(fixed2 x)", "bool3 isfinite(fixed3 x)", "bool4 isfinite(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "isinf",
                    "Determines if the specified value is infinite.",
                    new[]
                    {
                        "bool isinf(float x)", "bool2 isinf(float2 x)", "bool3 isinf(float3 x)", "bool4 isinf(float4 x)",
                        "bool isinf(half x)", "bool2 isinf(half2 x)", "bool3 isinf(half3 x)", "bool4 isinf(half4 x)",
                        "bool isinf(fixed x)", "bool2 isinf(fixed2 x)", "bool3 isinf(fixed3 x)", "bool4 isinf(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "isnan",
                    "Determines if the specified value is NAN or QNAN.",
                    new[]
                    {
                        "bool isnan(float x)", "bool2 isnan(float2 x)", "bool3 isnan(float3 x)", "bool4 isnan(float4 x)",
                        "bool isnan(half x)", "bool2 isnan(half2 x)", "bool3 isnan(half3 x)", "bool4 isnan(half4 x)",
                        "bool isnan(fixed x)", "bool2 isnan(fixed2 x)", "bool3 isnan(fixed3 x)", "bool4 isnan(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ldexp",
                    "Returns the result of multiplying the specified value by two, raised to the power of the specified exponent.\nThis function uses the following formula: x * 2^exp",
                    new[]
                    {
                        "float ldexp(float x, float n)", "float2 ldexp(float2 x, float2 n)", "float3 ldexp(float3 x, float3 n)",
                        "float4 ldexp(float4 x, float4 n)",
                        "half ldexp(half x, half n)", "half2 ldexp(half2 x, half2 n)", "half3 ldexp(half3 x, half3 n)", "half4 ldexp(half4 x, half4 n)",
                        "fixed ldexp(fixed x, fixed n)", "fixed2 ldexp(fixed2 x, fixed2 n)", "fixed3 ldexp(fixed3 x, fixed3 n)",
                        "fixed4 ldexp(fixed4 x, fixed4 n)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "length",
                    "Returns the length of the specified floating-point vector.",
                    new[]
                    {
                        "float length(float x)", "float length(float2 x)", "float length(float3 x)", "float length(float4 x)",
                        "half length(half x)", "half length(half2 x)", "half length(half3 x)", "half length(half4 x)",
                        "fixed length(fixed x)", "fixed length(fixed2 x)", "fixed length(fixed3 x)", "fixed length(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "lerp",
                    "Performs a linear interpolation based on the following formula: x + s(y - x)",
                    new[]
                    {
                        "float lerp(float x, float y, float s)",
                        "float2 lerp(float2 x, float2 y, float s)", "float3 lerp(float3 x, float3 y, float s)",
                        "float4 lerp(float4 x, float4 y, float s)",
                        "float2 lerp(float2 x, float2 y, float2 s)", "float3 lerp(float3 x, float3 y, float3 s)",
                        "float4 lerp(float4 x, float4 y, float4 s)",
                        "half lerp(half x, half y, half s)",
                        "half2 lerp(half2 x, half2 y, half s)", "half3 lerp(half3 x, half3 y, half s)", "half4 lerp(half4 x, half4 y, half s)",
                        "half2 lerp(half2 x, half2 y, half2 s)", "half3 lerp(half3 x, half3 y, half3 s)", "half4 lerp(half4 x, half4 y, half4 s)",
                        "fixed lerp(fixed x, fixed y, fixed s)",
                        "fixed2 lerp(fixed2 x, fixed2 y, fixed s)", "fixed3 lerp(fixed3 x, fixed3 y, fixed s)",
                        "fixed4 lerp(fixed4 x, fixed4 y, fixed s)",
                        "fixed2 lerp(fixed2 x, fixed2 y, fixed2 s)", "fixed3 lerp(fixed3 x, fixed3 y, fixed3 s)",
                        "fixed4 lerp(fixed4 x, fixed4 y, fixed4 s)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
              new FunctionCompletionData(
                "lit",
                "Returns a lighting coefficient vector.",
                new[] { "float4 lit(float NdotL, float NdotH, float specularExponent)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "log",
                    "Returns the base-e logarithm of the specified value.",
                    new[]
                    {
                        "float log(float x)", "float2 log(float2 x)", "float3 log(float3 x)", "float4 log(float4 x)",
                        "half log(half x)", "half2 log(half2 x)", "half3 log(half3 x)", "half4 log(half4 x)",
                        "fixed log(fixed x)", "fixed2 log(fixed2 x)", "fixed3 log(fixed3 x)", "fixed4 log(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "log10",
                    "Returns the base-10 logarithm of the specified value.",
                    new[]
                    {
                        "float log10(float x)", "float2 log10(float2 x)", "float3 log10(float3 x)", "float4 log10(float4 x)",
                        "half log10(half x)", "half2 log10(half2 x)", "half3 log10(half3 x)", "half4 log10(half4 x)",
                        "fixed log10(fixed x)", "fixed2 log10(fixed2 x)", "fixed3 log10(fixed3 x)", "fixed4 log10(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "log2",
                    "Returns the base-2 logarithm of the specified value.",
                    new[]
                    {
                        "float log2(float x)", "float2 log2(float2 x)", "float3 log2(float3 x)", "float4 log2(float4 x)",
                        "half log2(half x)", "half2 log2(half2 x)", "half3 log2(half3 x)", "half4 log2(half4 x)",
                        "fixed log2(fixed x)", "fixed2 log2(fixed2 x)", "fixed3 log2(fixed3 x)", "fixed4 log2(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "max",
                    "Selects the greater of x and y.",
                    new[]
                    {
                        "float max(float x, float y)", "float2 max(float2 x, float2 y)", "float3 max(float3 x, float3 y)",
                        "float4 max(float4 x, float4 y)",
                        "half max(half x, half y)", "half2 max(half2 x, half2 y)", "half3 max(half3 x, half3 y)", "half4 max(half4 x, half4 y)",
                        "fixed max(fixed x, fixed y)", "fixed2 max(fixed2 x, fixed2 y)", "fixed3 max(fixed3 x, fixed3 y)",
                        "fixed4 max(fixed4 x, fixed4 y)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "min",
                    "Selects the lesser of x and y.",
                    new[]
                    {
                        "float min(float x, float y)", "float2 min(float2 x, float2 y)", "float3 min(float3 x, float3 y)",
                        "float4 min(float4 x, float4 y)",
                        "half min(half x, half y)", "half2 min(half2 x, half2 y)", "half3 min(half3 x, half3 y)", "half4 min(half4 x, half4 y)",
                        "fixed min(fixed x, fixed y)", "fixed2 min(fixed2 x, fixed2 y)", "fixed3 min(fixed3 x, fixed3 y)",
                        "fixed4 min(fixed4 x, fixed4 y)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "mul",
                    "Multiplies x and y using matrix math.",
                    new[]
                    {
                        "floatN mul(floatN v1, floatN v2)",
                        "floatN mul(floatM v, floatMxN m)",
                        "floatN mul(floatNxM m, floatM v)",
                        "floatMxN mul(floatMxL m1, floatLxN m2)",
                        "halfN mul(halfN v1, halfN v2)",
                        "halfN mul(halfM v, halfMxN m)",
                        "halfN mul(halfNxM m, halfM v)",
                        "halfMxN mul(halfMxL m1, halfLxN m2)",
                        "fixedN mul(fixedN v1, fixedN v2)",
                        "fixedN mul(fixedM v, fixedMxN m)",
                        "fixedN mul(fixedNxM m, fixedM v)",
                        "fixedMxN mul(fixedMxL m1, fixedLxN m2)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
                new FunctionCompletionData(
                    "noise",
                    "Generates a random value using the Perlin-noise algorithm.",
                    new[] { "float noise(vector<float,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "normalize",
                    "Normalizes the specified floating-point vector according to x / length(x).",
                    new[]
                    {
                        "float normalize(float x)", "float normalize(float2 x)", "float normalize(float3 x)", "float normalize(float4 x)",
                        "half normalize(half x)", "half normalize(half2 x)", "half normalize(half3 x)", "half normalize(half4 x)",
                        "fixed normalize(fixed x)", "fixed normalize(fixed2 x)", "fixed normalize(fixed3 x)", "fixed normalize(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "offsettex2D",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "offsettex2D(uniform sampler2D sampler, float2 st, float4 prevlookup, uniform float4 m)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "offsettexRECT",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "offsettexRECT(uniform samplerRECT sampler, float2 st, float4 prevlookup, uniform float4 m)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "offsettex2DScaleBias",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "offsettex2DScaleBias(uniform sampler2D sampler, float2 st, float4 prevlookup, uniform float4 m, uniform float scale, uniform float bias)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "offsettexRECTScaleBias",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "offsettexRECTScaleBias(uniform samplerRECT sampler, float2 st, float4 prevlookup, uniform float4 m, uniform float scale, uniform float bias)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "pack_2half",
                    "Converts the components into a pair of 16-bit floating point values and packs the components into a single 32-bit result.",
                    new[]
                    {
                        "float pack_2half(float2 a)", "float pack_2half(half2 a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "pack_2ushort",
                    "Converts the components into a pair of 16-bit unsigned integer values and packs the components into a single 32-bit result.",
                    new[]
                    {
                        "float pack_2ushort(float2 a)", "float pack_2ushort(half2 a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "pack_4byte",
                    "Converts the four components into a 8-bit signed integer values and packs the components into a single 32-bit result.",
                    new[]
                    {
                        "float pack_4byte(float4 a)", "float pack_4byte(half4 a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "pack_4ubyte",
                    "Converts the four components into a 8-bit unsigned integer values and packs the components into a single 32-bit result.",
                    new[]
                    {
                        "float pack_4ubyte(float4 a)", "float pack_4ubyte(half4 a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "pow",
                    "Returns the specified value raised to the specified power.",
                    new[]
                    {
                        "float pow(float x, float y)", "float2 pow(float2 x, float2 y)", "float3 pow(float3 x, float3 y)",
                        "float4 pow(float4 x, float4 y)",
                        "half pow(half x, half y)", "half2 pow(half2 x, half2 y)", "half3 pow(half3 x, half3 y)", "half4 pow(half4 x, half4 y)",
                        "fixed pow(fixed x, fixed y)", "fixed2 pow(fixed2 x, fixed2 y)", "fixed3 pow(fixed3 x, fixed3 y)",
                        "fixed4 pow(fixed4 x, fixed4 y)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "radians",
                    "Converts the specified value from degrees to radians.",
                    new[]
                    {
                        "float radians(float x)", "float2 radians(float2 x)", "float3 radians(float3 x)", "float4 radians(float4 x)",
                        "half radians(half x)", "half2 radians(half2 x)", "half3 radians(half3 x)", "half4 radians(half4 x)",
                        "fixed radians(fixed x)", "fixed2 radians(fixed2 x)", "fixed3 radians(fixed3 x)", "fixed4 radians(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "reflect",
                    "Returns a reflection vector using an entering ray direction and a surface normal.",
                    new[]
                    {
                        "float reflect(float i, float n)", "float2 reflect(float2 i, float2 n)", "float3 reflect(float3 i, float3 n)",
                        "float4 reflect(float4 i, float4 n)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "refract",
                    "Returns a refraction vector using an entering ray, a surface normal, and a refraction index.",
                    new[]
                    {
                        "float refract(float i, float n)", "float2 refract(float2 i, float2 n)", "float3 refract(float3 i, float3 n)",
                        "float4 refract(float4 i, float4 n)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "restartStrip",
                    "Ends the current-primitive strip and starts a new strip. If the current strip does not have enough vertices emitted to fill the primitive topology, the incomplete primitive at the end will be discarded.",
                    new[] { "void restartStrip()" }));

            Functions.Add(
                new FunctionCompletionData(
                    "round",
                    "Rounds the specified value to the nearest integer.",
                    new[]
                    {
                        "float round(float x)", "float2 round(float2 x)", "float3 round(float3 x)", "float4 round(float4 x)",
                        "half round(half x)", "half2 round(half2 x)", "half3 round(half3 x)", "half4 round(half4 x)",
                        "fixed round(fixed x)", "fixed2 round(fixed2 x)", "fixed3 round(fixed3 x)", "fixed4 round(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "rsqrt",
                    "Returns the reciprocal of the square root of the specified value.",
                    new[]
                    {
                        "float rsqrt(float x)", "float2 rsqrt(float2 x)", "float3 rsqrt(float3 x)", "float4 rsqrt(float4 x)",
                        "half rsqrt(half x)", "half2 rsqrt(half2 x)", "half3 rsqrt(half3 x)", "half4 rsqrt(half4 x)",
                        "fixed rsqrt(fixed x)", "fixed2 rsqrt(fixed2 x)", "fixed3 rsqrt(fixed3 x)", "fixed4 rsqrt(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "saturate",
                    "Clamps the specified value within the range of 0 to 1.",
                    new[]
                    {
                        "float saturate(float x)", "float2 saturate(float2 x)", "float3 saturate(float3 x)", "float4 saturate(float4 x)",
                        "half saturate(half x)", "half2 saturate(half2 x)", "half3 saturate(half3 x)", "half4 saturate(half4 x)",
                        "fixed saturate(fixed x)", "fixed2 saturate(fixed2 x)", "fixed3 saturate(fixed3 x)", "fixed4 saturate(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sign",
                    "Returns the sign of x.",
                    new[]
                    {
                        "float sign(float x)", "float2 sign(float2 x)", "float3 sign(float3 x)", "float4 sign(float4 x)",
                        "half sign(half x)", "half2 sign(half2 x)", "half3 sign(half3 x)", "half4 sign(half4 x)",
                        "fixed sign(fixed x)", "fixed2 sign(fixed2 x)", "fixed3 sign(fixed3 x)", "fixed4 sign(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sin",
                    "Returns the sine of the specified value.",
                    new[]
                    {
                        "float sin(float x)", "float2 sin(float2 x)", "float3 sin(float3 x)", "float4 sin(float4 x)",
                        "half sin(half x)", "half2 sin(half2 x)", "half3 sin(half3 x)", "half4 sin(half4 x)",
                        "fixed sin(fixed x)", "fixed2 sin(fixed2 x)", "fixed3 sin(fixed3 x)", "fixed4 sin(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sincos",
                    "Returns the sine and cosine of x.",
                    new[]
                    {
                        "void sincos(float x, out float s, out float c)", "void sincos(float2 x, out float2 s, out float2 c)",
                        "void sincos(float3 x, out float3 s, out float3 c)", "void sincos(float4 x, out float4 s, out float4 c)",
                        "void sincos(half x, out half s, out half c)", "void sincos(half2 x, out half2 s, out half2 c)",
                        "void sincos(half3 x, out half3 s, out half3 c)", "void sincos(half4 x, out half4 s, out half4 c)",
                        "void sincos(fixed x, out fixed s, out fixed c)", "void sincos(fixed2 x, out fixed2 s, out fixed2 c)",
                        "void sincos(fixed3 x, out fixed3 s, out fixed3 c)", "void sincos(fixed4 x, out fixed4 s, out fixed4 c)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sinh",
                    "Returns the hyperbolic sine of the specified value.",
                    new[]
                    {
                        "float sinh(float x)", "float2 sinh(float2 x)", "float3 sinh(float3 x)", "float4 sinh(float4 x)",
                        "half sinh(half x)", "half2 sinh(half2 x)", "half3 sinh(half3 x)", "half4 sinh(half4 x)",
                        "fixed sinh(fixed x)", "fixed2 sinh(fixed2 x)", "fixed3 sinh(fixed3 x)", "fixed4 sinh(fixed4 x)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
                new FunctionCompletionData(
                    "smoothstep",
                    "Returns a smooth Hermite interpolation between 0 and 1, if x is in the range [min, max].",
                    new[]
                    {
                        "float smoothstep(float min, float max, float x)", "float2 smoothstep(float2 min, float2 max, float2 x)",
                        "float smoothstep(float3 min, float3 max, float3 x)", "float4 smoothstep(float4 min, float4 max, float4 x)",
                        "half smoothstep(half min, half max, half x)", "half2 smoothstep(half2 min, half2 max, half2 x)",
                        "half smoothstep(half3 min, half3 max, half3 x)", "half4 smoothstep(half4 min, half4 max, half4 x)",
                        "fixed smoothstep(fixed min, fixed max, fixed x)", "fixed2 smoothstep(fixed2 min, fixed2 max, fixed2 x)",
                        "fixed smoothstep(fixed3 min, fixed3 max, fixed3 x)", "fixed4 smoothstep(fixed4 min, fixed4 max, fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sqrt",
                    "Returns the square root of the specified floating-point value, per component.",
                    new[]
                    {
                        "float sqrt(float x)", "float2 sqrt(float2 x)", "float3 sqrt(float3 x)", "float4 sqrt(float4 x)",
                        "half sqrt(half x)", "half2 sqrt(half2 x)", "half3 sqrt(half3 x)", "half4 sqrt(half4 x)",
                        "fixed sqrt(fixed x)", "fixed2 sqrt(fixed2 x)", "fixed3 sqrt(fixed3 x)", "fixed4 sqrt(fixed4 x)",
                    }));

            // Function not properly documented by NVIDIA
            Functions.Add(
                new FunctionCompletionData(
                    "step",
                    "Compares two values, returning 0 or 1 based on which value is greater.",
                    new[]
                    {
                        "float step(float theshold, float x)", "float2 step(float2 theshold, float2 x)", "float3 step(float3 theshold, float3 x)",
                        "float4 step(float4 theshold, float4 x)",
                        "half step(half theshold, half x)", "half2 step(half2 theshold, half2 x)", "half3 step(half3 theshold, half3 x)",
                        "half4 step(half4 theshold, half4 x)",
                        "fixed step(fixed theshold, fixed x)", "fixed2 step(fixed2 theshold, fixed2 x)", "fixed3 step(fixed3 theshold, fixed3 x)",
                        "fixed4 step(fixed4 theshold, fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tan",
                    "Returns the tangent of the specified value.",
                    new[]
                    {
                        "float tan(float x)", "float2 tan(float2 x)", "float3 tan(float3 x)", "float4 tan(float4 x)",
                        "half tan(half x)", "half2 tan(half2 x)", "half3 tan(half3 x)", "half4 tan(half4 x)",
                        "fixed tan(fixed x)", "fixed2 tan(fixed2 x)", "fixed3 tan(fixed3 x)", "fixed4 tan(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tanh",
                    "Returns the hyperbolic tangent of the specified value.",
                    new[]
                    {
                        "float tanh(float x)", "float2 tanh(float2 x)", "float3 tanh(float3 x)", "float4 tanh(float4 x)",
                        "half tanh(half x)", "half2 tanh(half2 x)", "half3 tanh(half3 x)", "half4 tanh(half4 x)",
                        "fixed tanh(fixed x)", "fixed2 tanh(fixed2 x)", "fixed3 tanh(fixed3 x)", "fixed4 tanh(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex_dp3x2_depth",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "tex_dp3x2_depth(float3 str, float4 intermediate_coord, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1D",
                    "Samples a 1D texture.",
                    new[]
                    {
                        "float4 tex1D(sampler1D sampler, float t)",
                        "float4 tex1D(sampler1D sampler, float t, int texelOffset)",
                        "float4 tex1D(sampler1D sampler, float2 t)",
                        "float4 tex1D(sampler1D sampler, float2 t, int texelOffset)",
                        "float4 tex1D(sampler1D sampler, float t, float dx, float dy)",
                        "float4 tex1D(sampler1D sampler, float t, float dx, float dy, int texelOffset)",
                        "float4 tex1D(sampler1D sampler, float2 t, float dx, float dy)",
                        "float4 tex1D(sampler1D sampler, float2 t, float dx, float dy, int texelOffset)",
                        "int4 tex1D(isampler1D sampler, float t)",
                        "int4 tex1D(isampler1D sampler, float t, int texelOffset)",
                        "int4 tex1D(isampler1D sampler, float t, float dx, float dy)",
                        "int4 tex1D(isampler1D sampler, float t, float dx, float dy, int texelOffset)",
                        "unsigned int4 tex1D(usampler1D sampler, float t)",
                        "unsigned int4 tex1D(usampler1D sampler, float t, int texelOffset)",
                        "unsigned int4 tex1D(usampler1D sampler, float t, float dx, float dy)",
                        "unsigned int4 tex1D(usampler1D sampler, float t, float dx, float dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1D_dp3",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "tex1D_dp3(sampler1D sampler, float3 str, float4 prevlookup"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAY",
                    "Performs a texture lookup in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float2 t)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float2 t, int texelOffset)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float3 t)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float3 t, int texelOffset)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float2 t, float dx, float dy)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float2 t, float dx, float dy, int texelOffset)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float3 t, float dx, float dy)",
                        "float4 tex1DARRAY(sampler1DARRAY sampler, float3 t, float dx, float dy, int texelOffset)",
                        "int4 tex1DARRAY(isampler1DARRAY sampler, float2 t)",
                        "int4 tex1DARRAY(isampler1DARRAY sampler, float2 t, int texelOffset)",
                        "int4 tex1DARRAY(isampler1DARRAY sampler, float2 t, float dx, float dy)",
                        "int4 tex1DARRAY(isampler1DARRAY sampler, float2 t, float dx, float dy, int texelOffset)",
                        "unsigned int4 tex1DARRAY(usampler1DARRAY sampler, float2 t)",
                        "unsigned int4 tex1DARRAY(usampler1DARRAY sampler, float2 t, int texelOffset)",
                        "unsigned int4 tex1DARRAY(usampler1DARRAY sampler, float2 t, float dx, float dy)",
                        "unsigned int4 tex1DARRAY(usampler1DARRAY sampler, float2 t, float dx, float dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYbias",
                    "Performs a texture lookup with bias in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYbias(sampler1DARRAY sampler, float4 t)",
                        "float4 tex1DARRAYbias(sampler1DARRAY sampler, float4 t, int texelOffset)",
                        "int4 tex1DARRAYbias(isampler1DARRAY sampler, float4 t)",
                        "int4 tex1DARRAYbias(isampler1DARRAY sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex1DARRAYbias(usampler1DARRAY sampler, float4 t)",
                        "unsigned int4 tex1DARRAYbias(usampler1DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYcmpbias",
                    "Performs a texture lookup with shadow compare and bias in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYcmpbias(sampler1DARRAY sampler, float4 t)",
                        "float4 tex1DARRAYcmpbias(sampler1DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYcmplod",
                    "Performs a texture lookup with shadow compare and level of detail in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYcmplod(sampler1DARRAY sampler, float4 t)",
                        "float4 tex1DARRAYcmplod(sampler1DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYfetch",
                    "Performs an unfiltered texture lookup in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYfetch(sampler1DARRAY sampler, int4 t)",
                        "float4 tex1DARRAYfetch(sampler1DARRAY sampler, int4 t, int texelOffset)",
                        "int4 tex1DARRAYfetch(isampler1DARRAY sampler, int4 t)",
                        "int4 tex1DARRAYfetch(isampler1DARRAY sampler, int4 t, int texelOffset)",
                        "unsigned int4 tex1DARRAYfetch(usampler1DARRAY sampler, int4 t)",
                        "unsigned int4 tex1DARRAYfetch(usampler1DARRAY sampler, int4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYlod",
                    "Performs a texture lookup with a specified level of detail in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYlod(sampler1DARRAY sampler, float4 t)",
                        "float4 tex1DARRAYlod(sampler1DARRAY sampler, float4 t, int texelOffset)",
                        "int4 tex1DARRAYlod(isampler1DARRAY sampler, float4 t)",
                        "int4 tex1DARRAYlod(isampler1DARRAY sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex1DARRAYlod(usampler1DARRAY sampler, float4 t)",
                        "unsigned int4 tex1DARRAYlod(usampler1DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYproj",
                    "Performs a texture lookup with projection in a given sampler array.",
                    new[]
                    {
                        "float4 tex1DARRAYproj(sampler1DARRAY sampler, float2 t)",
                        "float4 tex1DARRAYproj(sampler1DARRAY sampler, float2 t, int texelOffset)",
                        "float4 tex1DARRAYproj(sampler1DARRAY sampler, float3 t)",
                        "float4 tex1DARRAYproj(sampler1DARRAY sampler, float3 t, int texelOffset)",
                        "int4 tex1DARRAYproj(isampler1DARRAY sampler, float2 t)",
                        "int4 tex1DARRAYproj(isampler1DARRAY sampler, float2 t, int texelOffset)",
                        "unsigned int4 tex1DARRAYproj(usampler1DARRAY sampler, float2 t)",
                        "unsigned int4 tex1DARRAYproj(usampler1DARRAY sampler, float2 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1DARRAYsize",
                    "Returns the size of a given texture array image for a given level of detail.",
                    new[]
                    {
                        "int3 tex1DARRAYsize(sampler1DARRAY sampler, int lod)",
                        "int3 tex1DARRAYsize(isampler1DARRAY sampler, int lod)",
                        "int3 tex1DARRAYsize(usampler1DARRAY sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dbias",
                    "Samples a 1D texture after biasing the mip level by t.w.",
                    new[]
                    {
                        "float4 tex1Dbias(sampler1D sampler, float4 t)",
                        "float4 tex1Dbias(sampler1D sampler, float4 t, int texelOffset)",
                        "int4 tex1Dbias(isampler1D sampler, float4 t)",
                        "int4 tex1Dbias(isampler1D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex1Dbias(usampler1D sampler, float4 t)",
                        "unsigned int4 tex1Dbias(usampler1D sampler, float4 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dcmpbias",
                    "Performs a texture lookup with shadow compare and bias in a given sampler.",
                    new[]
                    {
                        "float4 tex1Dcmpbias(sampler1D sampler, float4 t)",
                        "float4 tex1Dcmpbias(sampler1D sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dcmplod",
                    "Performs a texture lookup with shadow compare and level of detail in a given sampler.",
                    new[]
                    {
                        "float4 tex1Dcmplod(sampler1D sampler, float4 t)",
                        "float4 tex1Dcmplod(sampler1D sampler, float4 t, int texelOffset)",
                        "int4 tex1Dcmplod(isampler1D sampler, float4 t)",
                        "int4 tex1Dcmplod(isampler1D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex1Dcmplod(usampler1D sampler, float4 t)",
                        "unsigned int4 tex1Dcmplod(usampler1D sampler, float4 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dfetch",
                    "Performs an unfiltered texture lookup in a given sampler.",
                    new[]
                    {
                        "float4 tex1Dfetch(sampler1D sampler, int4 t)",
                        "float4 tex1Dfetch(sampler1D sampler, int4 t, int texelOffset)",
                        "int4 tex1Dfetch(isampler1D sampler, int4 t)",
                        "int4 tex1Dfetch(isampler1D sampler, int4 t, int texelOffset)",
                        "unsigned int4 tex1Dfetch(usampler1D sampler, int4 t)",
                        "unsigned int4 tex1Dfetch(usampler1D sampler, int4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dlod",
                    "Samples a 1D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[]
                    {
                        "float4 tex1Dlod(sampler1D sampler, float4 t)",
                        "float4 tex1Dlod(sampler1D sampler, float4 t, int texelOffset)",
                        "int4 tex1Dlod(isampler1D sampler, float4 t)",
                        "int4 tex1Dlod(isampler1D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex1Dlod(usampler1D sampler, float4 t)",
                        "unsigned int4 tex1Dlod(usampler1D sampler, float4 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dproj",
                    "Samples a 1D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[]
                    {
                        "float4 tex1Dproj(sampler1D sampler, float2 t)",
                        "float4 tex1Dproj(sampler1D sampler, float2 t, int texelOffset)",
                        "float4 tex1Dproj(sampler1D sampler, float3 t)",
                        "float4 tex1Dproj(sampler1D sampler, float3 t, int texelOffset)",
                        "int4 tex1Dproj(isampler1D sampler, float2 t)",
                        "int4 tex1Dproj(isampler1D sampler, float2 t, int texelOffset)",
                        "unsigned int4 tex1Dproj(usampler1D sampler, float2 t)",
                        "unsigned int4 tex1Dproj(usampler1D sampler, float2 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dsize",
                    "Returns the size of a given texture image for a given level of detail.",
                    new[]
                    {
                        "int3 tex1Dsize(sampler1D sampler, int lod)",
                        "int3 tex1Dsize(isampler1D sampler, int lod)",
                        "int3 tex1Dsize(usampler1D sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2D",
                    "Samples a 2D texture.",
                    new[]
                    {
                        "float4 tex2D(sampler2D sampler, float2 t)",
                        "float4 tex2D(sampler2D sampler, float2 t, int texelOffset)",
                        "float4 tex2D(sampler2D sampler, float3 t)",
                        "float4 tex2D(sampler2D sampler, float3 t, int texelOffset)",
                        "float4 tex2D(sampler2D sampler, float2 t, float2 dx, float2 dy)",
                        "float4 tex2D(sampler2D sampler, float2 t, float2 dx, float2 dy, int texelOffset)",
                        "float4 tex2D(sampler2D sampler, float3 t, float2 dx, float2 dy)",
                        "float4 tex2D(sampler2D sampler, float3 t, float2 dx, float2 dy, int texelOffset)",
                        "int4 tex2D(isampler2D sampler, float2 t)",
                        "int4 tex2D(isampler2D sampler, float2 t, int texelOffset)",
                        "int4 tex2D(isampler2D sampler, float2 t, float2 dx, float2 dy)",
                        "int4 tex2D(isampler2D sampler, float2 t, float2 dx, float2 dy, int texelOffset)",
                        "unsigned int4 tex2D(usampler2D sampler, float2 t)",
                        "unsigned int4 tex2D(usampler2D sampler, float2 t, int texelOffset)",
                        "unsigned int4 tex2D(usampler2D sampler, float2 t, float2 dx, float2 dy)",
                        "unsigned int4 tex2D(usampler2D sampler, float2 t, float2 dx, float2 dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2D_dp3x2",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "tex2D_dp3x2(uniform sampler2D sampler, float3 str, float4 intermediate_coord, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAY",
                    "Performs a texture lookup in a given sampler array.",
                    new[]
                    {
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float3 t)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float3 t, int texelOffset)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float4 t)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float4 t, int texelOffset)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float3 t, float dx, float dy)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float3 t, float dx, float dy, int texelOffset)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float4 t, float dx, float dy)",
                        "float4 tex2DARRAY(sampler2DARRAY sampler, float4 t, float dx, float dy, int texelOffset)",
                        "int4 tex2DARRAY(isampler2DARRAY sampler, float3 t)",
                        "int4 tex2DARRAY(isampler2DARRAY sampler, float3 t, int texelOffset)",
                        "int4 tex2DARRAY(isampler2DARRAY sampler, float3 t, float dx, float dy)",
                        "int4 tex2DARRAY(isampler2DARRAY sampler, float3 t, float dx, float dy, int texelOffset)",
                        "unsigned int4 tex2DARRAY(usampler2DARRAY sampler, float3 t)",
                        "unsigned int4 tex2DARRAY(usampler2DARRAY sampler, float3 t, int texelOffset)",
                        "unsigned int4 tex2DARRAY(usampler2DARRAY sampler, float3 t, float dx, float dy)",
                        "unsigned int4 tex2DARRAY(usampler2DARRAY sampler, float3 t, float dx, float dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAYbias",
                    "Performs a texture lookup with bias in a given sampler array.",
                    new[]
                    {
                        "float4 tex2DARRAYbias(sampler2DARRAY sampler, float4 t)",
                        "float4 tex2DARRAYbias(sampler2DARRAY sampler, float4 t, int texelOffset)",
                        "int4 tex2DARRAYbias(isampler2DARRAY sampler, float4 t)",
                        "int4 tex2DARRAYbias(isampler2DARRAY sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex2DARRAYbias(usampler2DARRAY sampler, float4 t)",
                        "unsigned int4 tex2DARRAYbias(usampler2DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAYfetch",
                    "Performs an unfiltered texture lookup in a given sampler array.",
                    new[]
                    {
                        "float4 tex2DARRAYfetch(sampler2DARRAY sampler, int4 t)",
                        "float4 tex2DARRAYfetch(sampler2DARRAY sampler, int4 t, int texelOffset)",
                        "int4 tex2DARRAYfetch(isampler2DARRAY sampler, int4 t)",
                        "int4 tex2DARRAYfetch(isampler2DARRAY sampler, int4 t, int texelOffset)",
                        "unsigned int4 tex2DARRAYfetch(usampler2DARRAY sampler, int4 t)",
                        "unsigned int4 tex2DARRAYfetch(usampler2DARRAY sampler, int4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAYlod",
                    "Performs a texture lookup with a specified level of detail in a given sampler array.",
                    new[]
                    {
                        "float4 tex2DARRAYlod(sampler2DARRAY sampler, float4 t)",
                        "float4 tex2DARRAYlod(sampler2DARRAY sampler, float4 t, int texelOffset)",
                        "int4 tex2DARRAYlod(isampler2DARRAY sampler, float4 t)",
                        "int4 tex2DARRAYlod(isampler2DARRAY sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex2DARRAYlod(usampler2DARRAY sampler, float4 t)",
                        "unsigned int4 tex2DARRAYlod(usampler2DARRAY sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAYproj",
                    "Performs a texture lookup with projection in a given sampler array.",
                    new[]
                    {
                        "float4 tex2DARRAYproj(sampler2DARRAY sampler, float3 t)",
                        "float4 tex2DARRAYproj(sampler2DARRAY sampler, float3 t, int texelOffset)",
                        "int4 tex2DARRAYproj(isampler2DARRAY sampler, float3 t)",
                        "int4 tex2DARRAYproj(isampler2DARRAY sampler, float3 t, int texelOffset)",
                        "unsigned int4 tex2DARRAYproj(usampler2DARRAY sampler, float3 t)",
                        "unsigned int4 tex2DARRAYproj(usampler2DARRAY sampler, float3 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2DARRAYsize",
                    "Returns the size of a given texture array image for a given level of detail.",
                    new[]
                    {
                        "int3 tex2DARRAYsize(sampler2DARRAY sampler, int lod)",
                        "int3 tex2DARRAYsize(isampler2DARRAY sampler, int lod)",
                        "int3 tex2DARRAYsize(usampler2DARRAY sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dbias",
                    "Samples a 2D texture after biasing the mip level by t.w.",
                    new[]
                    {
                        "float4 tex2Dbias(sampler2D sampler, float4 t)",
                        "float4 tex2Dbias(sampler2D sampler, float4 t, int texelOffset)",
                        "int4 tex2Dbias(isampler2D sampler, float4 t)",
                        "int4 tex2Dbias(isampler2D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex2Dbias(usampler2D sampler, float4 t)",
                        "unsigned int4 tex2Dbias(usampler2D sampler, float4 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dcmpbias",
                    "Performs a texture lookup with shadow compare and bias in a given sampler.",
                    new[]
                    {
                        "float4 tex2Dcmpbias(sampler2D sampler, float4 t)",
                        "float4 tex2Dcmpbias(sampler2D sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dcmplod",
                    "Performs a texture lookup with shadow compare and level of detail in a given sampler.",
                    new[]
                    {
                        "float4 tex2Dcmplod(sampler2D sampler, float4 t)",
                        "float4 tex2Dcmplod(sampler2D sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dfetch",
                    "Performs an unfiltered texture lookup in a given sampler.",
                    new[]
                    {
                        "float4 tex2Dfetch(sampler2D sampler, int4 t)",
                        "float4 tex2Dfetch(sampler2D sampler, int4 t, int texelOffset)",
                        "int4 tex2Dfetch(isampler2D sampler, int4 t)",
                        "int4 tex2Dfetch(isampler2D sampler, int4 t, int texelOffset)",
                        "unsigned int4 tex2Dfetch(usampler2D sampler, int4 t)",
                        "unsigned int4 tex2Dfetch(usampler2D sampler, int4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dlod",
                    "Samples a 2D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[]
                    {
                        "float4 tex2Dlod(sampler2D sampler, float4 t)",
                        "float4 tex2Dlod(sampler2D sampler, float4 t, int texelOffset)",
                        "int4 tex2Dlod(isampler2D sampler, float4 t)",
                        "int4 tex2Dlod(isampler2D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex2Dlod(usampler2D sampler, float4 t)",
                        "unsigned int4 tex2Dlod(usampler2D sampler, float4 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dproj",
                    "Samples a 2D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[]
                    {
                        "float4 tex2Dproj(sampler2D sampler, float3 t)",
                        "float4 tex2Dproj(sampler2D sampler, float3 t, int texelOffset)",
                        "float4 tex2Dproj(sampler2D sampler, float4 t)",
                        "float4 tex2Dproj(sampler2D sampler, float4 t, int texelOffset)",
                        "int4 tex2Dproj(isampler2D sampler, float3 t)",
                        "int4 tex2Dproj(isampler2D sampler, float3 t, int texelOffset)",
                        "unsigned int4 tex2Dproj(usampler2D sampler, float3 t)",
                        "unsigned int4 tex2Dproj(usampler2D sampler, float3 t, int texelOffset)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dsize",
                    "Returns the size of a given texture image for a given level of detail.",
                    new[]
                    {
                        "int3 tex2Dsize(sampler2D sampler, int lod)",
                        "int3 tex2Dsize(isampler2D sampler, int lod)",
                        "int3 tex2Dsize(usampler2D sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3D",
                    "Samples a 3D texture.",
                    new[]
                    {
                        "float4 tex3D(sampler3D sampler, float3 t)",
                        "float4 tex3D(sampler3D sampler, float3 t, int texelOffset)",
                        "float4 tex3D(sampler3D sampler, float3 t, float3 dx, float3 dy)",
                        "float4 tex3D(sampler3D sampler, float3 t, float3 dx, float3 dy, int texelOffset)",
                        "int4 tex3D(isampler3D sampler, float3 t)",
                        "int4 tex3D(isampler3D sampler, float3 t, int texelOffset)",
                        "int4 tex3D(isampler3D sampler, float3 t, float3 dx, float3 dy)",
                        "int4 tex3D(isampler3D sampler, float3 t, float3 dx, float3 dy, int texelOffset)",
                        "unsigned int4 tex3D(usampler3D sampler, float3 t)",
                        "unsigned int4 tex3D(usampler3D sampler, float3 t, int texelOffset)",
                        "unsigned int4 tex3D(usampler3D sampler, float3 t, float3 dx, float3 dy)",
                        "unsigned int4 tex3D(usampler3D sampler, float3 t, float3 dx, float3 dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3D_dp3x3",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "tex3D_dp3x3(sampler3D sampler, float3 str, float4 intermediate_coord1, float4 intermediate_coord2, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dbias",
                    "Samples a 3D texture after biasing the mip level by t.w.",
                    new[]
                    {
                        "float4 tex3Dbias(sampler3D sampler, float4 t)",
                        "float4 tex3Dbias(sampler3D sampler, float4 t, int texelOffset)",
                        "int4 tex3Dbias(isampler3D sampler, float4 t)",
                        "int4 tex3Dbias(isampler3D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex3Dbias(usampler3D sampler, float4 t)",
                        "unsigned int4 tex3Dbias(usampler3D sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dfetch",
                    "Performs an unfiltered texture lookup in a given sampler.",
                    new[]
                    {
                        "float4 tex3Dfetch(sampler3D sampler, int4 t)",
                        "float4 tex3Dfetch(sampler3D sampler, int4 t, int texelOffset)",
                        "int4 tex3Dfetch(isampler3D sampler, int4 t)",
                        "int4 tex3Dfetch(isampler3D sampler, int4 t, int texelOffset)",
                        "unsigned int4 tex3Dfetch(suampler3D sampler, int4 t)",
                        "unsigned int4 tex3Dfetch(usampler3D sampler, int4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dlod",
                    "Samples a 3D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[]
                    {
                        "float4 tex3Dlod(sampler3D sampler, float4 t)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dproj",
                    "Samples a 3D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[]
                    {
                        "float4 tex3Dlod(sampler3D sampler, float4 t)",
                        "float4 tex3Dlod(sampler3D sampler, float4 t, int texelOffset)",
                        "int4 tex3Dlod(isampler3D sampler, float4 t)",
                        "int4 tex3Dlod(isampler3D sampler, float4 t, int texelOffset)",
                        "unsigned int4 tex3Dlod(usampler3D sampler, float4 t)",
                        "unsigned int4 tex3Dlod(usampler3D sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dsize",
                    "Returns the size of a given texture image for a given level of detail.",
                    new[]
                    {
                        "int3 tex3Dsize(sampler3D sampler, int lod)",
                        "int3 tex3Dsize(isampler3D sampler, int lod)",
                        "int3 tex3Dsize(usampler3D sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texBUF",
                    "Performs an unfiltered texture lookup in a given texture buffer sampler.",
                    new[]
                    {
                        "float4 texBUF(samplerBUF sampler, int t)",
                        "int4 texBUF(isamplerBUF sampler, int t)",
                        "unsigned int4 texBUF(usamplerBUF sampler, int t)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texBUFsize",
                    "Returns the size of a given texture image for a given level of detail.",
                    new[]
                    {
                        "int3 texBUFsize(samplerBUF sampler, int lod)",
                        "int3 texBUFsize(isamplerBUF sampler, int lod)",
                        "int3 texBUFsize(usamplerBUF sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBE",
                    "Samples a cube texture.",
                    new[]
                    {
                        "float4 texCUBE(samplerCUBE sampler, float3 t)",
                        "float4 texCUBE(samplerCUBE sampler, float4 t)",
                        "float4 texCUBE(samplerCUBE sampler, float3 t, float3 dx, float3 dy)",
                        "float4 texCUBE(samplerCUBE sampler, float4 t, float3 dx, float3 dy)",
                        "int4 texCUBE(isamplerCUBE sampler, float3 t)",
                        "int4 texCUBE(isamplerCUBE sampler, float3 t, float3 dx, float3 dy)",
                        "unsigned int4 texCUBE(usamplerCUBE sampler, float3 t)",
                        "unsigned int4 texCUBE(usamplerCUBE sampler, float3 t, float3 dx, float3 dy)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBE_dp3x3",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "texCUBE_dp3x3(samplerCUBE sampler, float3 str, float4 intermediate_coord1, float4 intermediate_coord2, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBE_reflect_dp3x3",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "texCUBE_reflect_dp3x3(uniform samplerCUBE sampler, float4 strq, float4 intermediate_coord1, float4 intermediate_coord2, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBE_reflect_eye_dp3x3",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "texCUBE_reflect_eye_dp3x3(uniform samplerCUBE sampler, float3 str, float4 intermediate_coord1, float4 intermediate_coord2, float4 prevlookup, uniform float3 eye)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEARRAY",
                    "Performs a texture lookup in a given sampler array.",
                    new[]
                    {
                        "float4 texCUBEARRAY(usamplerCUBEARRAY sampler, float4 t)",
                        "float4 texCUBEARRAY(usamplerCUBEARRAY sampler, float4 t, float dx, float dy)",
                        "int4 texCUBEARRAY(isamplerCUBEARRAY sampler, float4 t)",
                        "int4 texCUBEARRAY(isamplerCUBEARRAY sampler, float4 t, float dx, float dy)",
                        "unsigned int4 texCUBEARRAY(usamplerCUBEARRAY sampler, float4 t)",
                        "unsigned int4 texCUBEARRAY(usamplerCUBEARRAY sampler, float4 t, float dx, float dy)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEARRAYsize",
                    "Returns the size of a given texture array image for a given level of detail.",
                    new[]
                    {
                        "int3 texCUBEARRAYsize(samplerCUBEARRAY sampler, int lod)",
                        "int3 texCUBEARRAYsize(isamplerCUBEARRAY sampler, int lod)",
                        "int3 texCUBEARRAYsize(usamplerCUBEARRAY sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEbias",
                    "Samples a cube texture after biasing the mip level by t.w.",
                    new[]
                    {
                        "float4 texCUBEbias(samplerCUBE sampler, float4 t)",
                        "int4 texCUBEbias(isamplerCUBE sampler, float4 t)",
                        "unsigned int4 texCUBEbias(usamplerCUBE sampler, float4 t)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBElod",
                    "Samples a cube texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[]
                    {
                        "float4 texCUBElod(samplerCUBE sampler, float4 t)",
                        "int4 texCUBElod(isamplerCUBE sampler, float4 t)",
                        "unsigned int4 texCUBElod(usamplerCUBE sampler, float4 t)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEproj",
                    "Samples a cube texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[]
                    {
                        "float4 texCUBEproj(samplerCUBE sampler, float4 t)",
                        "int4 texCUBEproj(isamplerCUBE sampler, float4 t)",
                        "unsigned int4 texCUBEproj(usamplerCUBE sampler, float4 t)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEsize",
                    "Returns the size of a given texture array image for a given level of detail.",
                    new[]
                    {
                        "int3 texCUBEsize(samplerCUBE sampler, int lod)",
                        "int3 texCUBEsize(isamplerCUBE sampler, int lod)",
                        "int3 texCUBEsize(usamplerCUBE sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECT",
                    "Performs a texture lookup in a given RECT sampler.",
                    new[]
                    {
                        "float4 texRECT(samplerRECT sampler, float2 t)",
                        "float4 texRECT(samplerRECT sampler, float2 t, int texelOffset)",
                        "float4 texRECT(samplerRECT sampler, float3 t)",
                        "float4 texRECT(samplerRECT sampler, float3 t, int texelOffset)",
                        "float4 texRECT(samplerRECT sampler, float2 t, float2 dx, float2 dy)",
                        "float4 texRECT(samplerRECT sampler, float2 t, float2 dx, float2 dy, int texelOffset)",
                        "float4 texRECT(samplerRECT sampler, float3 t, float2 dx, float2 dy)",
                        "float4 texRECT(samplerRECT sampler, float3 t, float2 dx, float2 dy, int texelOffset)",
                        "int4 texRECT(isamplerRECT sampler, float2 t)",
                        "int4 texRECT(isamplerRECT sampler, float2 t, int texelOffset)",
                        "int4 texRECT(isamplerRECT sampler, float2 t, float2 dx, float2 dy)",
                        "int4 texRECT(isamplerRECT sampler, float2 t, float2 dx, float2 dy, int texelOffset)",
                        "unsigned int4 texRECT(usamplerRECT sampler, float2 t)",
                        "unsigned int4 texRECT(usamplerRECT sampler, float2 t, int texelOffset)",
                        "unsigned int4 texRECT(usamplerRECT sampler, float2 t, float2 dx, float2 dy)",
                        "unsigned int4 texRECT(usamplerRECT sampler, float2 t, float2 dx, float2 dy, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECT_dp3x2",
                    "See Cg Profile fp20.",
                    new[]
                    {
                        "texRECT_dp3x2(uniform samplerRECT sampler, float3 str, float4 intermediate_coord, float4 prevlookup)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECTbias",
                    "Performs a texture lookup with bias in a given RECT sampler.",
                    new[]
                    {
                        "float4 texRECTbias(samplerRECT sampler, float4 t)",
                        "float4 texRECTbias(samplerRECT sampler, float4 t, int2 texelOffset)",
                        "int4 texRECTbias(isamplerRECT sampler, float4 t)",
                        "int4 texRECTbias(isamplerRECT sampler, float4 t, int2 texelOffset)",
                        "unsigned int4 texRECTbias(usamplerRECT sampler, float4 t)",
                        "unsigned int4 texRECTbias(usamplerRECT sampler, float4 t, int2 texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECTfetch",
                    "Performs an unfiltered texture lookup in a given RECT sampler.",
                    new[]
                    {
                        "float4 texRECTfetch(samplerRECT sampler, int4 t)",
                        "float4 texRECTfetch(samplerRECT sampler, int4 t, int2 texelOffset)",
                        "int4 texRECTfetch(isamplerRECT sampler, int4 t)",
                        "int4 texRECTfetch(isamplerRECT sampler, int4 t, int2 texelOffset)",
                        "unsigned int4 texRECTfetch(usamplerRECT sampler, int4 t)",
                        "unsigned int4 texRECTfetch(usamplerRECT sampler, int4 t, int2 texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECTlod",
                    "Performs a texture lookup with a specified level of detail in a given RECT sampler.",
                    new[]
                    {
                        "float4 texRECTlod(samplerRECT sampler, float4 t)",
                        "float4 texRECTlod(samplerRECT sampler, float4 t, int texelOffset)",
                        "int4 texRECTlod(isamplerRECT sampler, float4 t)",
                        "int4 texRECTlod(isamplerRECT sampler, float4 t, int texelOffset)",
                        "unsigned int4 texRECTlod(usamplerRECT sampler, float4 t)",
                        "unsigned int4 texRECTlod(usamplerRECT sampler, float4 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECTproj",
                    "Performs a texture lookup with projection in a given RECT sampler.",
                    new[]
                    {
                        "float4 texRECTproj(samplerRECT sampler, float3 t)",
                        "float4 texRECTproj(samplerRECT sampler, float3 t, int texelOffset)",
                        "float4 texRECTproj(samplerRECT sampler, float4 t)",
                        "float4 texRECTproj(samplerRECT sampler, float4 t, int texelOffset)",
                        "int4 texRECTproj(isamplerRECT sampler, float3 t)",
                        "int4 texRECTproj(isamplerRECT sampler, float3 t, int texelOffset)",
                        "unsigned int4 texRECTproj(usamplerRECT sampler, float3 t)",
                        "unsigned int4 texRECTproj(usamplerRECT sampler, float3 t, int texelOffset)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "texRECTsize",
                    "Returns the size of a given texture array image for a given level of detail.",
                    new[]
                    {
                        "int3 texRECTsize(samplerRECT sampler, int lod)",
                        "int3 texRECTsize(isamplerRECT sampler, int lod)",
                        "int3 texRECTsize(usamplerRECT sampler, int lod)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "transpose",
                    "Transposes the specified input matrix.",
                    new[]
                    {
                        "float4x4 transpose(float4x4 m)",
                        "float3x4 transpose(float4x2 m)",
                        "float2x4 transpose(float4x2 m)",
                        "float1x4 transpose(float4x1 m)",
                        "float4x3 transpose(float3x4 m)",
                        "float3x3 transpose(float3x2 m)",
                        "float2x3 transpose(float3x2 m)",
                        "float1x3 transpose(float3x1 m)",
                        "float4x2 transpose(float2x4 m)",
                        "float3x2 transpose(float2x2 m)",
                        "float2x2 transpose(float2x2 m)",
                        "float1x2 transpose(float2x1 m)",
                        "float4x1 transpose(float1x4 m)",
                        "float3x1 transpose(float1x2 m)",
                        "float2x1 transpose(float1x2 m)",
                        "float1x1 transpose(float1x1 m)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "trunc",
                    "Truncates a floating-point value to the integer component.",
                    new[]
                    {
                        "float trunc(float x)", "float2 trunc(float2 x)", "float3 trunc(float3 x)", "float4 trunc(float4 x)",
                        "half trunc(half x)", "half2 trunc(half2 x)", "half3 trunc(half3 x)", "half4 trunc(half4 x)",
                        "fixed trunc(fixed x)", "fixed2 trunc(fixed2 x)", "fixed3 trunc(fixed3 x)", "fixed4 trunc(fixed4 x)",
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "unpack_2half",
                    "Unpacks a 32-bit value into two 16-bit floating points values.",
                    new[]
                    {
                        "half2 unpack_2half(float a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "unpack_2ushort",
                    "Unpacks two 16-bit unsigned integer values and scales the result into individual floating point values between 0.0 and 1.0.",
                    new[]
                    {
                        "float2 unpack_2ushort(float a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "unpack_4byte",
                    "Unpacks four 8-bit integer values and scales the result into individual floating point values between -(128/127) and +(127/127).",
                    new[]
                    {
                        "half4 unpack_4byte(float a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "unpack_4ubyte",
                    "Unpacks four 8-bit unsigned integer values and scales the result into individual floating point values between 0.0 and 1.0.",
                    new[]
                    {
                        "half4 unpack_4ubyte(float a)"
                    }));
        }


        /// <summary>
        /// Initializes the CgFX functions.
        /// </summary>
        private void InitializeEffectFunctions()
        {
            EffectFunctions.Add(new FunctionCompletionData("compile", "Compiles a shader.", new[] { "compile <Profile> <ShaderFunction>" }));
        }


        /// <summary>
        /// Initializes the CgFX effect states.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InitializeEffectStates()
        {
            string[] comparisonFunctions =
            {
                "Never", "Less", "LEqual", "Equal", "Greater", "NotEqual", "GEqual", "Always"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "AlphaFunc",
                    "The alpha compare function and the reference value.\nAlphaFunc = float2(function, referenceValue)",
                    comparisonFunctions));
            string[] boolValues =
            {
                "true", "false"
            };
            EffectStates.Add(new StateCompletionData("AlphaTestEnable", "Enables or disables per-pixel alpha testing.", boolValues));
            EffectStates.Add(new StateCompletionData("AutoNormalEnable", "Enables or disables auto normal for evaluators.", boolValues));
            EffectStates.Add(new StateCompletionData("BlendEnable", "Enables or disables alpha-blended transparency.", boolValues));
            string[] blendFactors =
            {
                "Zero", "One", "DestColor", "OneMinusDestColor", "SrcAlpha", "OneMinusSrcAlpha",
                "DstAlpha", "OneMinusDstAlpha", "SrcAlphaSaturate", "SrcColor", "OneMinusSrcColor",
                "ConstantColor", "OneMinusConstantColor", "ConstantAlpha", "OneMinusConstantAlpha"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "BlendFunc", "The factors used when alpha blending.\nBlendFunc = int2(sourceFactor, destinationFactor)", blendFactors));
            EffectStates.Add(
                new StateCompletionData(
                    "BlendFuncSeparate",
                    "The factors used when alpha blending (using separate factors for RGB and alpha values).\nBlendSeparate = int4(rgbSourceFactor, rgbDestinationFactor, alphaSourceFactor, alphaDestinationFactor)",
                    blendFactors));
            string[] blendEquation =
            {
                "FuncAdd", "FuncSubtract", "Min", "Max", "LogicOp"
            };
            EffectStates.Add(new StateCompletionData("BlendEquation", "The arithmetic operation applied when alpha blending.", blendEquation));
            EffectStates.Add(
                new StateCompletionData(
                    "BlendEquationSeparate",
                    "The arithmetic operation applied when alpha blending (using a separate operation for RGB and alpha values).\nBlendEquationSeparate = int2(rgb, alpha)",
                    blendEquation));
            EffectStates.Add(new StateCompletionData("BlendColor", "The blend color."));
            EffectStates.Add(new StateCompletionData("ClearColor", "The color to clear the color buffer."));
            EffectStates.Add(new StateCompletionData("ClearStencil", "The int value to clear the stencil buffer."));
            EffectStates.Add(new StateCompletionData("ClearDepth", "The float value to clear the depth buffer."));
            EffectStates.Add(new StateCompletionData("ClipPlane", "Sets the clip plane equation.\nClipPlane[n] = float4(A, B, C, D).", true));
            EffectStates.Add(new StateCompletionData("ClipPlaneEnable", "Enables or disables the clip plane.", boolValues, true));
            EffectStates.Add(new StateCompletionData("ColorLogicOpEnable", "Enables or disables the color logical operation.", boolValues));
            EffectStates.Add(new StateCompletionData("ColorMask", "The color write mask.\nColorMask = bool4(red, green, blue, alpha)"));
            EffectStates.Add(new StateCompletionData("ColorMatrix", "The color matrix."));
            string[] colorMaterialValues =
            {
                "Front", "Back", "FrontAndBack", "Emission", "Ambient", "Diffuse", "Specular", "AmbientAndDiffuse"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "ColorMaterial",
                    "Associate a material property with the current color.\nColorMaterial = int2(face, materialProperty)",
                    colorMaterialValues));
            string[] faceValues =
            {
                "Front", "Back", "FrontAndBack"
            };
            EffectStates.Add(new StateCompletionData("CullFace", "Indicates which faces should be discarded (culled).", faceValues));
            EffectStates.Add(new StateCompletionData("CullFaceEnable", "Enables or disables the cull face.", boolValues));
            EffectStates.Add(new StateCompletionData("DepthBounds", "The depth bounds.\nDepthBounds = float2(zmin, zmax)"));
            EffectStates.Add(new StateCompletionData("DepthBoundsEnable", "Enables or disables the depth bounds test.", boolValues));
            EffectStates.Add(new StateCompletionData("DepthFunc", "The comparison function for the depth-buffer test.", comparisonFunctions));
            EffectStates.Add(new StateCompletionData("DepthClampEnable", "Enables or disables depth clamp.", boolValues));
            EffectStates.Add(new StateCompletionData("DepthTestEnable", "Enables or disables depth test.", boolValues));
            EffectStates.Add(new StateCompletionData("DepthMask", "Enables or disables depth writes.", boolValues));
            EffectStates.Add(new StateCompletionData("DepthRange", "The depth range.\nDepthRange = float2(near, far)"));
            EffectStates.Add(new StateCompletionData("DitherEnable", "Enables or disables dithering.", boolValues));
            EffectStates.Add(new StateCompletionData("FogColor", "Fog color."));
            EffectStates.Add(
                new StateCompletionData("FogDensity", "Fog density for pixel or vertex fog used in the exponential fog modes. Range [0, 1]."));
            EffectStates.Add(new StateCompletionData("FogEnable", "Enables or disables fog blending.", boolValues));
            EffectStates.Add(new StateCompletionData("FogEnd", "Depth at which pixel or vertex fog effects end for linear fog mode. "));
            EffectStates.Add(new StateCompletionData("FogStart", "Depth at which pixel or vertex fog effects begin for linear fog mode."));
            string[] fogModes =
            {
                "Exp", "Exp2", "Linear"
            };
            EffectStates.Add(new StateCompletionData("FogMode", "The fog formula to be used for fog.", fogModes));
            string[] fogCoordSources =
            {
                "FragmentDepth", "FogCoord"
            };
            EffectStates.Add(new StateCompletionData("FogCoordSrc", "The fog coordinate source.", fogCoordSources));
            string[] fogDistanceMode =
            {
                "FragmentDepth", "EyeRadial", "EyePlane", "EyePlaneAbsolute"
            };
            EffectStates.Add(new StateCompletionData("FogDistanceMode", "The fog distance mode.", fogDistanceMode));
            EffectStates.Add(new StateCompletionData("FragmentEnvParameter", "See ARB_fragment_program.", true));
            EffectStates.Add(new StateCompletionData("FragmentLocalParameter", "See ARB_fragment_program.", true));
            string[] frontFace =
            {
                "CW", "CCW"
            };
            EffectStates.Add(new StateCompletionData("FrontFace", "Sets the front faces.", frontFace));
            EffectStates.Add(new StateCompletionData("LightingEnable", "Enables or disables per-vertex lighting.", boolValues));
            EffectStates.Add(new StateCompletionData("LightEnable", "Enables or disables a set of lighting parameters.", boolValues, true));
            EffectStates.Add(new StateCompletionData("LightModelAmbient", "Global ambient light intensity."));
            EffectStates.Add(new StateCompletionData("LightAmbient", "Ambient light intensity.", true));
            EffectStates.Add(new StateCompletionData("LightDiffuse", "Diffuse light intensity.", true));
            EffectStates.Add(new StateCompletionData("LightSpecular", "Specular light intensity.", true));
            EffectStates.Add(new StateCompletionData("LightPosition", "Light position.", true));
            EffectStates.Add(new StateCompletionData("LightSpotCutoff", "Spotlight cutoff angle.", true));
            EffectStates.Add(new StateCompletionData("LightSpotDirection", "Spotlight direction.", true));
            EffectStates.Add(new StateCompletionData("LightSpotExponent", "Spotlight exponent.", true));
            EffectStates.Add(new StateCompletionData("LightConstantAttenuation", "Constant attenuation factor.", true));
            EffectStates.Add(new StateCompletionData("LightLinearAttenuation", "Linear attenuation factor.", true));
            EffectStates.Add(new StateCompletionData("LightQuadraticAttenuation", "Linear attenuation factor.", true));
            EffectStates.Add(
                new StateCompletionData("LightModelLocalViewerEnable", "Enables or disables the local viewer light model.", boolValues));
            EffectStates.Add(new StateCompletionData("LightModelTwoSideEnable", "Enables or disables the local viewer light model.", boolValues));
            string[] colorControl =
            {
                "SingleColor", "SeparateSpecular"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "LightModelColorControl",
                    "Sets a value indicating whether specular color is calculated separately from ambient and diffuse.",
                    colorControl));
            EffectStates.Add(new StateCompletionData("LineSmoothEnable", "Enables or disables line antialiasing.", boolValues));
            EffectStates.Add(new StateCompletionData("LineStipple", "The line stipple.\nLineStipple = int2(factor, pattern)"));
            EffectStates.Add(new StateCompletionData("LineStippleEnable", "Enables or disables line stippling.", boolValues));
            EffectStates.Add(new StateCompletionData("LineWidth", "The line width."));
            string[] logicOperations =
            {
                "Clear", "And", "AndReverse", "Copy", "AndInverted", "Noop", "Xor",
                "Or", "Nor", "Equiv", "Invert", "OrReverse", "CopyInverted", "Nand", "Set"
            };
            EffectStates.Add(new StateCompletionData("LogicOp", "The Color index logical operation.", logicOperations));
            EffectStates.Add(new StateCompletionData("LogicOpEnable", "Enables or disables color index logical operations.", boolValues));
            EffectStates.Add(new StateCompletionData("MaterialAmbient", "Ambient color of material."));
            EffectStates.Add(new StateCompletionData("MaterialDiffuse", "Diffuse color of material."));
            EffectStates.Add(new StateCompletionData("MaterialSpecular", "Specular color of material."));
            EffectStates.Add(new StateCompletionData("MaterialShininess", "Specular exponent of material."));
            EffectStates.Add(new StateCompletionData("MaterialEmission", "Emissive color of material."));
            EffectStates.Add(new StateCompletionData("ModelViewMatrix", "The model-view matrix."));
            EffectStates.Add(new StateCompletionData("MultisampleEnable", "Enables or disables multisampling.", boolValues));
            EffectStates.Add(new StateCompletionData("NormalizeEnable", "Enables or disables surface normal vector normalization.", boolValues));
            EffectStates.Add(
                new StateCompletionData("PointDistanceAttenuation", "The point distance attenuation.\nPointDistanceAttenuation = float3(a, b, c)"));
            EffectStates.Add(new StateCompletionData("PointFadeThresholdSize", "The point fade size threshold."));
            EffectStates.Add(new StateCompletionData("PointSize", "The point size."));
            EffectStates.Add(new StateCompletionData("PointSizeMin", "The minimal point size."));
            EffectStates.Add(new StateCompletionData("PointSizeMax", "The maximal point size."));
            EffectStates.Add(new StateCompletionData("PointSmoothEnable", "Enables or disables antialiasing for points.", boolValues));
            string[] pointSpriteCoordOrigin =
            {
                "LowerLeft", "UpperLeft"
            };
            EffectStates.Add(
                new StateCompletionData("PointSpriteCoordOrigin", "The origin of the coordinate system for point sprites.", pointSpriteCoordOrigin));
            EffectStates.Add(
                new StateCompletionData(
                    "PointSpriteCoordReplace", "Enables the iteration of texture coordinates for point sprites for the given texture unit.", true));
            string[] pointSpriteRMode =
            {
                "Zero", "R", "S"
            };
            EffectStates.Add(new StateCompletionData("PointSpriteRMode", "See NV_point_sprite.", pointSpriteRMode, true));
            EffectStates.Add(new StateCompletionData("PointSpriteEnable", "Enables or disables points sprite rendering.", boolValues));
            string[] renderingModes =
            {
                "Front", "Back", "FrontAndBack", "Point", "Line", "Fill"
            };
            EffectStates.Add(
                new StateCompletionData("PolygonMode", "Sets the polygon rendering mode.\nPolygonMode = int2(face, mode)", renderingModes));
            EffectStates.Add(new StateCompletionData("PolygonOffset", "The polygon offset.\nPolygonOffset = float2(factor, units)"));
            EffectStates.Add(new StateCompletionData("PolygonOffsetFillEnable", "Enables or disables polygon offset fill.", boolValues));
            EffectStates.Add(new StateCompletionData("PolygonOffsetLineEnable", "Enables or disables polygon offset line.", boolValues));
            EffectStates.Add(new StateCompletionData("PolygonOffsetPointEnable", "Enables or disables polygon offset point.", boolValues));
            EffectStates.Add(new StateCompletionData("PolygonSmoothEnable", "Enables or disables antialiasing for polygons.", boolValues));
            EffectStates.Add(new StateCompletionData("ProjectionMatrix", "The projection matrix.", boolValues));
            EffectStates.Add(new StateCompletionData("RescaleNormalEnable", "Enables or disables rescaling of normals.", boolValues));
            EffectStates.Add(new StateCompletionData("SampleAlphaToCoverageEnable", "Enables or disables sample alpha to coverage.", boolValues));
            EffectStates.Add(new StateCompletionData("SampleAlphaToOneEnable", "Enables or disables sample alpha to one.", boolValues));
            EffectStates.Add(new StateCompletionData("SampleCoverageEnable", "Enables or disables sample to coverage.", boolValues));
            EffectStates.Add(new StateCompletionData("Scissor", "The scissor rectangle.\nScissor = int4(x, y, width, height)"));
            EffectStates.Add(new StateCompletionData("ScissorTestEnable", "Enables or disables the scissor test.", boolValues));
            string[] shadingMode =
            {
                "Flat", "Smooth"
            };
            EffectStates.Add(new StateCompletionData("ShadeModel", "Sets the shading mode.", shadingMode));
            EffectStates.Add(
              new StateCompletionData("StencilFunc", "The stencil comparison function.\nStencilFunc = int3(func, ref, mask)", comparisonFunctions));
            string[] stencilFuncSeparate =
            {
                "Front", "Back", "FrontAndBack",
                "Never", "Less", "LEqual", "Equal", "Greater", "NotEqual", "GEqual", "Always"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "StencilFuncSeparate", "The stencil operation.\nStencilFuncSeparate = int4(face, func, ref, mask)", stencilFuncSeparate));
            EffectStates.Add(new StateCompletionData("StencilMask", "The stencil write mask."));
            EffectStates.Add(
                new StateCompletionData("StencilMaskSeparate", "The stencil write mask.\nStencilMaskSeparate = int2(face, mask)", faceValues));
            string[] stencilOperation =
            {
                "Keep", "Zero", "Replace", "Incr", "Decr", "Invert", "IncrWrap", "DecrWrap"
            };
            EffectStates.Add(
                new StateCompletionData("StencilOp", "The stencil operation.\nStencilOp = int3(fail, zfail, zpass)", stencilOperation));
            EffectStates.Add(
                new StateCompletionData(
                    "StencilOpSeparate", "The stencil operation.\nStencilOpSeparate = int4(face, fail, zfail, zpass)", stencilOperation));
            EffectStates.Add(new StateCompletionData("StencilTestEnable", "Enables or disables the stenciling.", boolValues));
            string[] textureGenerationModes0 =
            {
                "ObjectLinear", "EyeLinear", "SphereMap", "ReflectionMap", "NormalMap"
            };
            string[] textureGenerationModes1 =
            {
                "ObjectLinear", "EyeLinear", "ReflectionMap", "NormalMap"
            };
            string[] textureGenerationModes2 =
            {
                "ObjectLinear", "EyeLinear"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenQEnable", "Enables or disables texture coordinate generation for Q for the given texture coordinate set.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData("TexGenQEyePlane", "The reference plane in eye coordinates for texture coordinate generation for Q.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenQObjectPlane", "The reference plane in object coordinates for texture coordinate generation for Q.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenQMode", "The texture coordinate generation mode for Q for the given texture coordinate set.", textureGenerationModes2, true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenREnable", "Enables or disables texture coordinate generation for R for the given texture coordinate set.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData("TexGenREyePlane", "The reference plane in eye coordinates for texture coordinate generation for R.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenRObjectPlane", "The reference plane in object coordinates for texture coordinate generation for R.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenRMode", "The texture coordinate generation mode for Q for the given texture coordinate set.", textureGenerationModes1, true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenSEnable", "Enables or disables texture coordinate generation for S for the given texture coordinate set.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData("TexGenSEyePlane", "The reference plane in eye coordinates for texture coordinate generation for S.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenSObjectPlane", "The reference plane in object coordinates for texture coordinate generation for S.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenSMode", "The texture coordinate generation mode for T for the given texture coordinate set.", textureGenerationModes0, true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenTEnable", "Enables or disables texture coordinate generation for T for the given texture coordinate set.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData("TexGenTEyePlane", "The reference plane in eye coordinates for texture coordinate generation for T.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenTObjectPlane", "The reference plane in object coordinates for texture coordinate generation for T.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexGenTMode", "The texture coordinate generation mode for T for the given texture coordinate set.", textureGenerationModes0, true));
            EffectStates.Add(
                new StateCompletionData(
                    "Texture1DEnable", "Enables or disables a one-dimensional texture for the given texture unit.", boolValues, true));
            EffectStates.Add(new StateCompletionData("Texture1D", "The sampler1D used for the given texture unit.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "Texture2DEnable", "Enables or disables a two-dimensional texture for the given texture unit.", boolValues, true));
            EffectStates.Add(new StateCompletionData("Texture2D", "The sampler2D used for the given texture unit.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "Texture3DEnable", "Enables or disables a three-dimensional texture for the given texture unit.", boolValues, true));
            EffectStates.Add(new StateCompletionData("Texture3D", "The sampler3D used for the given texture unit.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TextureCubeMapEnable", "Enables or disables a cube map texture for the given texture unit.", boolValues, true));
            EffectStates.Add(new StateCompletionData("TextureCubeMap", "The samplerCUBE used for the given texture unit.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData(
                    "TextureRectangleEnable", "Enables or disables a texture rectangle for the given texture unit.", boolValues, true));
            EffectStates.Add(new StateCompletionData("TextureRectangle", "The samplerRECT used for the given texture unit.", true));
            EffectStates.Add(new StateCompletionData("TextureEnvColor", "The texture blend color used for the given texture unit.", true));
            string[] textureMode =
            {
                "Modulate", "Decal", "Blend", "Replace", "Add"
            };
            EffectStates.Add(
                new StateCompletionData("TextureEnvMode", "The texture blend mode used for the given texture unit.", textureMode, true));
            EffectStates.Add(new StateCompletionData("VertexLocalParameter", "See ARB_vetex_program.", true));

            // Shader States
            EffectStates.Add(new StateCompletionData("VertexProgram", "The vertex program."));
            EffectStates.Add(new StateCompletionData("GeometryProgram", "The geometry program."));
            EffectStates.Add(new StateCompletionData("FragmentProgram", "The fragment program."));
        }


        /// <summary>
        /// Initializes the sampler states.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InitializeSamplerStates()
        {
            SamplerStates.Add(new StateCompletionData("BorderColor", "The texture border color as DWORD (0xAARRGGBB). The default value is 0x00000000."));
            string[] compareMode =
            {
                "None", "CompareRToTexture"
            };
            SamplerStates.Add(new StateCompletionData("CompareMode", "The texture compare mode (see ARB_shadow).", compareMode));
            SamplerStates.Add(
                new StateCompletionData("CompareFunc", "The texture compare function (see ARB_shadow, EXT_shadow_funcs).", compareMode));
            string[] depthMode =
            {
                "Alpha", "Intensity", "Luminance"
            };
            SamplerStates.Add(new StateCompletionData("DepthMode", "The depth mode (see ARB_depth_texture).", depthMode));
            string[] boolValues =
            {
                "true", "false"
            };
            SamplerStates.Add(
                new StateCompletionData("GenerateMipMap", "Indicates whether to generate a mip-map for the given texture.", boolValues));
            SamplerStates.Add(new StateCompletionData("LODBias", "The LOD bias."));
            string[] minFilter =
            {
                "Nearest", "Linear", "LinearMipMapNearest", "NearestMipMapNearest", "NearestMipMapLinear", "LinearMipMapLinear"
            };
            SamplerStates.Add(new StateCompletionData("MinFilter", "The filtering method used for minification.", minFilter));
            string[] magFilter =
            {
                "Nearest", "Linear"
            };
            SamplerStates.Add(new StateCompletionData("MagFilter", "The filtering method used for magnification.", magFilter));
            SamplerStates.Add(new StateCompletionData("MaxMipLevel", "The maximal mipmap level used."));
            SamplerStates.Add(new StateCompletionData("MaxAnisotropy", "The max level of anisotropy."));
            SamplerStates.Add(new StateCompletionData("MinMipLevel", "The minimal mipmap level used."));
            string[] textureAddressMode =
            {
                "Repeat", "Clamp", "ClampToEdge", "ClampToBorder", "MirroredRepeat", "MirrorClamp", "MirrorClampToEdge", "MirrorClampToBorder"
            };
            SamplerStates.Add(new StateCompletionData("WrapR", "Texture-address mode for the R coordinate.", textureAddressMode));
            SamplerStates.Add(new StateCompletionData("WrapS", "Texture-address mode for the S coordinate.", textureAddressMode));
            SamplerStates.Add(new StateCompletionData("WrapT", "Texture-address mode for the T coordinate.", textureAddressMode));
            SamplerStates.Add(new StateCompletionData("Texture", "Texture used by this sampler state."));
        }


        /// <summary>
        /// Initializes the CgFX state values.
        /// </summary>
        private void InitializeStateValues()
        {
            string[] stateValues =
            {
                "Never", "Less", "LEqual", "Equal", "Greater", "NotEqual", "GEqual", "Always",
                "true", "false",
                "Zero", "One", "DestColor", "OneMinusDestColor", "SrcAlpha", "OneMinusSrcAlpha",
                "DstAlpha", "OneMinusDstAlpha", "SrcAlphaSaturate", "SrcColor", "OneMinusSrcColor",
                "ConstantColor", "OneMinusConstantColor", "ConstantAlpha", "OneMinusConstantAlpha",
                "FuncAdd", "FuncSubtract", "Min", "Max", "LogicOp",
                "Front", "Back", "FrontAndBack", "Emission", "Ambient", "Diffuse", "Specular", "AmbientAndDiffuse",
                "Exp", "Exp2", "Linear",
                "FragmentDepth", "FogCoord",
                "EyeRadial", "EyePlane", "EyePlaneAbsolute",
                "CW", "CCW",
                "SingleColor", "SeparateSpecular",
                "Clear", "And", "AndReverse", "Copy", "AndInverted", "Noop", "Xor",
                "Or", "Nor", "Equiv", "Invert", "OrReverse", "CopyInverted", "Nand", "Set",
                "LowerLeft", "UpperLeft",
                "R", "S",
                "Point", "Line", "Fill",
                "Flat", "Smooth",
                "Keep", "Replace", "Incr", "Decr", "IncrWrap", "DecrWrap",
                "ObjectLinear", "EyeLinear", "SphereMap", "ReflectionMap", "NormalMap",
                "Modulate", "Decal", "Blend", "Add",
                "None", "CompareRToTexture",
                "Alpha", "Intensity", "Luminance",
                "Nearest", "LinearMipMapNearest", "NearestMipMapNearest", "NearestMipMapLinear",
                "LinearMipMapLinear",
                "Repeat", "Clamp", "ClampToEdge", "ClampToBorder", "MirroredRepeat", "MirrorClamp", "MirrorClampToEdge",
                "MirrorClampToBorder"
            };

            foreach (string value in stateValues)
                EffectStateValues.Add(new ConstantCompletionData(value));
        }


        /// <summary>
        /// Validates the effect states.
        /// </summary>
        [Conditional("DEBUG")]
        private void ValidateStates()
        {
            LookupConstants(EffectStates);
            LookupConstants(SamplerStates);
        }


        /// <summary>
        /// Looks up the effect state values.
        /// </summary>
        /// <param name="states">The states.</param>
        [Conditional("DEBUG")]
        private void LookupConstants(IEnumerable<NamedCompletionData> states)
        {
            foreach (StateCompletionData state in states)
            {
                foreach (string value in state.AllowedValues)
                {
                    if (!EffectStateValues.Contains(value))
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, "Effect state value \"{0}\" is not registered.", value);
                        throw new EditorException(message);
                    }
                }
            }
        }
        #endregion
    }
}
