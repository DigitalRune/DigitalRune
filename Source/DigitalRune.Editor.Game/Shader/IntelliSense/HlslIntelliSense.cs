// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Snippets;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Provides IntelliSense for DirectX HLSL and DirectX Effect Files.
    /// </summary>
    internal class HlslIntelliSense : ShaderIntelliSense
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
        /// Initializes a new instance of the <see cref="HlslIntelliSense"/> class.
        /// </summary>
        public HlslIntelliSense()
        {
            // Language specific aspects that are equal in HLSL and Cg are
            // defined in the base class ShaderIntelliSense.

            // Initialize HLSL-specific IntelliSense info here.

            // DirectX 9 and DirectX 10 HLSL
            InitializeSnippets();
            InitializeKeywords();
            InitializeTypes();
            InitializeMacros();
            InitializeFunctions();
            InitializeEffectFunctions();
            InitializeMethods();
            InitializeEffectStates();
            InitializeSamplerStates();
            InitializeStateValues();

            // DirectX 10 HLSL
            InitializeEffectStates10();
            InitializeStateValues10();

            // Validate DirectX Effect States (DEBUG only).
            ValidateStates();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the HLSL snippets.
        /// </summary>
        private void InitializeSnippets()
        {
            Snippet technique10Snippet = new Snippet();
            technique10Snippet.Elements.Add(new SnippetTextElement { Text = "technique10 " });
            technique10Snippet.Elements.Add(new SnippetCaretElement());
            technique10Snippet.Elements.Add(new SnippetTextElement { Text = "\n{\npass\n{\n}\n}" });

            SnippetCompletionData[] snippets =
            {
                new SnippetCompletionData("technique10", null, null, technique10Snippet),
            };

            foreach (SnippetCompletionData snippet in snippets)
                Snippets.Add(snippet.Text, snippet);
        }


        /// <summary>
        /// Initializes the HLSL keywords.
        /// </summary>
        private void InitializeKeywords()
        {
            string[] keywords =
            {
                "nointerpolation", "register", "shared", "snorm", "unorm", "volatile",
                "_linear", "_centroid", "_nointerpolation", "_noperspective",
                "row_major", "column_major",
                "stop",
                "unroll", "loop", "branch",
                "flatten", "target",
                "maxvertexcount",
                "vs_4_0",
                "ps_1_4", "ps_4_0", "ps_4_1",
                "gs_4_0", "gs_4_1"
            };
            foreach (string keyword in keywords)
                Keywords.Add(new KeywordCompletionData(keyword));
        }


        /// <summary>
        /// Initializes the HLSL types.
        /// </summary>
        private void InitializeTypes()
        {
            string[] scalarTypes =
            {
                "uint",
            };
            foreach (string type in scalarTypes)
                ScalarTypes.Add(new TypeCompletionData(type));

            string[] types =
            {
                "uint1", "uint2", "uint3", "uint4",
                "uint1x1", "uint1x2", "uint1x3", "uint1x4",
                "uint2x1", "uint2x2", "uint2x3", "uint2x4",
                "uint3x1", "uint3x2", "uint3x3", "uint3x4",
                "uint4x1", "uint4x2", "uint4x3", "uint4x4",
                "vector", "matrix",
            };
            foreach (string type in types)
                Types.Add(new TypeCompletionData(type));

            string[] specialTypes =
            {
                "Buffer",
                "cbuffer", "tbuffer",
                "point", "line", "triangle", "lineadj", "triangleadj",
                "PointStream", "LineStream", "TriangleStream",
            };
            foreach (string type in specialTypes)
                SpecialTypes.Add(new TypeCompletionData(type));

            string[] effectTypes =
            {
                "SamplerState", "SamplerComparisonState",
                "StateBlock", "stateblock_state",
                "BlendState", "DepthStencilState", "RasterizerState",
                "texture", "Texture1D", "Texture1DArray", "Texture2D", "Texture2DArray", "Texture3D", "TextureCube",
                "Texture2DMS", "Texture2DMSArray",
                "vertexfragment", "pixelfragment",
                "VertexShader", "GeometryShader", "PixelShader",
                "technique10",
            };
            foreach (string type in effectTypes)
                EffectTypes.Add(new TypeCompletionData(type));
        }


        /// <summary>
        /// Initializes the HLSL macros.
        /// </summary>
        private void InitializeMacros()
        {
            Macros.Add(
                new MacroCompletionData(
                    "__LINE__", "Substitutes a decimal integer that is one more than the number of preceding newlines."));
            Macros.Add(
                new MacroCompletionData(
                    "__FILE__", "Substitutes a decimal integer that says which source string number is being processed."));
        }


        /// <summary>
        /// Initializes the HLSL intrinsic functions.
        /// </summary>
        private void InitializeFunctions()
        {
            Functions.Add(
                new FunctionCompletionData(
                    "abs",
                    "Returns the absolute value of the specified value.",
                    new[]
                    {
                        "float abs(float x)", "int abs(int x)",
                        "vector<float,n> abs(vector<float,n> x)", "vector<int,n> abs(vector<int,n> x)",
                        "matrix<float,m,n> abs(matrix<float,m,n> x)", "matrix<int,m,n> abs(matrix<int,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "acos",
                    "Returns the arccosine of the specified value.",
                    new[]
                    {
                        "float acos(float x)", "vector<float,n> acos(vector<float,n> x)",
                        "matrix<float,m,n> acos(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "all",
                    "Determines if all components of the specified value are non-zero.",
                    new[]
                    {
                        "bool all(bool x)", "bool all(float x)", "bool all(int x)",
                        "bool all(vector<bool,n> x)", "bool all(vector<float,n> x)", "bool all(vector<int,n> x)",
                        "bool all(matrix<bool,m,n> x)", "bool all(matrix<float,m,n> x)", "bool all(matrix<int,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "any",
                    "Determines if any components of the specified value are non-zero.",
                    new[]
                    {
                        "bool any(bool x)", "bool any(float x)", "bool any(int x)",
                        "bool any(vector<bool,n> x)", "bool any(vector<float,n> x)", "bool any(vector<int,n> x)",
                        "bool any(matrix<bool,m,n> x)", "bool any(matrix<float,m,n> x)", "bool any(matrix<int,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "asfloat",
                    "Converts the input type to a floating-point number.",
                    new[]
                    {
                        "float asfloat(int x)", "float asfloat(uint x)",
                        "vector<float,n> asfloat(vector<int,n> x)", "vector<float,n> asfloat(vector<uint,n> x)",
                        "matrix<float,m,n> asfloat(matrix<int,m,n> x)", "matrix<float,m,n> asfloat(matrix<uint,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "asin",
                    "Returns the arcsine of the specified value.",
                    new[]
                    {
                        "float asin(float x)", "vector<float,n> asin(vector<float,n> x)",
                        "matrix<float,m,n> asin(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "asint",
                    "Converts the input type to an integer.",
                    new[]
                    {
                        "int asint(float x)", "int asint(uint x)",
                        "vector<int,n> asint(vector<float,n> x)", "vector<int,n> asint(vector<uint,n> x)",
                        "matrix<int,m,n> asint(matrix<float,m,n> x)", "matrix<int,m,n> asint(matrix<uint,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "asuint",
                    "Converts the input type to an unsigned integer.",
                    new[]
                    {
                        "uint asuint(float x)", "uint asuint(int x)",
                        "vector<uint,n> asuint(vector<float,n> x)", "vector<uint,n> asuint(vector<int,n> x)",
                        "matrix<uint,m,n> asuint(matrix<float,m,n> x)", "matrix<uint,m,n> asuint(matrix<int,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "atan",
                    "Returns the arctangent of the specified value.",
                    new[]
                    {
                        "float atan(float x)", "vector<float,n> atan(vector<float,n> x)",
                        "matrix<float,m,n> atan(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "atan2",
                    "Returns the arctangent of two values (x,y).",
                    new[]
                    {
                        "float atan2(float y, float x)",
                        "vector<float,n> atan2(vector<float,n> y, vector<float,n> x)",
                        "matrix<float,m,n> atan2(matrix<float,m,n> y, matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ceil",
                    "Returns the smallest integer value that is greater than or equal to the specified value.",
                    new[]
                    {
                        "float ceil(float x)", "vector<float,n> ceil(vector<float,n> x)",
                        "matrix<float,m,n> ceil(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "clamp",
                    "Clamps the specified value to the specified minimum and maximum range.",
                    new[]
                    {
                        "float clamp(float x, float min, float max)", "int clamp(int x, int min, int max)",
                        "vector<float,n> clamp(vector<float,n> x, vector<float,n> min, vector<float,n> max)",
                        "vector<int,n> clamp(vector<int,n> x, vector<int,n> min, vector<int,n> max)",
                        "matrix<float,m,n> clamp(matrix<float,m,n> x, matrix<float,m,n> min, matrix<float,m,n> max)",
                        "matrix<int,m,n> clamp(matrix<int,m,n> x, matrix<int,m,n> min, matrix<int,m,n> max)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "clip",
                    "Discards the current pixel if the specified value is less than zero. (Pixel Shader.)",
                    new[] { "void clip(float x)", "void clip(vector<float,n> x)", "void clip(matrix<float,m,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "cos",
                    "Returns the cosine of the specified value.",
                    new[]
                    {
                        "float cos(float x)", "vector<float,n> cos(vector<float,n> x)", "matrix<float,m,n> cos(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "cosh",
                    "Returns the hyperbolic cosine of the specified value.",
                    new[]
                    {
                        "float cosh(float x)", "vector<float,n> cosh(vector<float,n> x)",
                        "matrix<float,m,n> cosh(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "cross",
                    "Returns the cross product of two floating-point, 3D vectors.",
                    new[] { "float3 cross(float3 x, float3 y)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "D3DCOLORtoUBYTE4",
                    "Converts a floating-point, 4D vector set by a D3DCOLOR to a UBYTE4.",
                    new[] { "int4 D3DCOLORtoUBYTE4(float4 x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "ddx",
                    "Returns the partial derivative of the specified value with respect to the screen-space x-coordinate. (Pixel Shader)",
                    new[]
                    {
                        "float ddx(float x)", "vector<float,n> ddx(vector<float,n> x)", "matrix<float,m,n> ddx(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "ddy",
                    "Returns the partial derivative of the specified value with respect to the screen-space y-coordinate. (Pixel Shader)",
                    new[]
                    {
                        "float ddy(float x)", "vector<float,n> ddy(vector<float,n> x)", "matrix<float,m,n> ddy(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "degrees",
                    "Converts the specified value from radians to degrees.",
                    new[]
                    {
                        "float degrees(float radians)", "vector<float,n> degrees(vector<float,n> radians)",
                        "matrix<float,m,n> degrees(matrix<float,m,n> radians)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "determinant",
                    "Returns the determinant of the specified floating-point, square matrix.",
                    new[] { "float determinant(matrix<float,n,n> m)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "distance",
                    "Returns the Euclidean distance between two points.",
                    new[] { "float distance(vector<float,n> point1, vector<float,n> point2)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "dot",
                    "Returns the dot product of two vectors.",
                    new[] { "float dot(vector<float,n> x, vector<float,n> y)", "int dot(vector<int,n> x, vector<int,n> y)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "exp",
                    "Returns the base-e exponential, or e^x, of the specified value.",
                    new[]
                    {
                        "float exp(float x)", "vector<float,n> exp(vector<float,n> x)", "matrix<float,m,n> exp(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "exp2",
                    "Returns the base 2 exponential, or 2^x, of the specified value.",
                    new[]
                    {
                        "float exp2(float x)", "vector<float,n> exp2(vector<float,n> x)",
                        "matrix<float,m,n> exp2(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "faceforward",
                    "Flips the surface-normal (if needed) to face in a direction opposite to an incident vector.\nThis function uses the formula: -n * sign(dot(i, ng)).",
                    new[] { "vector<float,n> faceforward(vector<float,n> n, vector<float,n> i, vector<float,n> ng)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "floor",
                    "Returns the largest integer that is less than or equal to the specified value.",
                    new[]
                    {
                        "float floor(float x)", "vector<float,n> floor(vector<float,n> x)",
                        "matrix<float,m,n> floor(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "fmod",
                    "Returns the floating-point remainder of x/y with the same sign as x.",
                    new[]
                    {
                        "float fmod(float x, float y)", "vector<float,n> fmod(vector<float,n> x, vector<float,n> y)",
                        "matrix<float,m,n> fmod(matrix<float,m,n> x, matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "frac",
                    "Returns the fractional (or decimal) part of x; which is greater than or equal to 0 and less than 1.",
                    new[]
                    {
                        "float frac(float x)", "vector<float,n> frac(vector<float,n> x)",
                        "matrix<float,m,n> frac(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "frexp",
                    "Returns the mantissa and exponent of the specified floating-point value.",
                    new[]
                    {
                        "float frexp(float x, out float exp)", "vector<float,n> frexp(vector<float,n> x, out vector<float,n> x)",
                        "matrix<float,m,n> frexp(matrix<float,m,n> x, out matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "fwidth",
                    "Returns the absolute value of the partial derivatives of the specified value. (Pixel Shader)",
                    new[]
                    {
                        "float fwidth(float x)", "vector<float,n> fwidth(vector<float,n> x)",
                        "matrix<float,m,n> fwidth(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "GetRenderTargetSampleCount",
                    "Gets the sampling position (x,y) for a given sample index.",
                    new[] { "uint GetRenderTargetSampleCount(int index)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "GetRenderTargetSamplePosition",
                    "Gets the number of samples for a render target.",
                    new[] { "uint GetRenderTargetSamplePosition()" }));

            Functions.Add(
                new FunctionCompletionData(
                    "isfinite",
                    "Determines if the specified floating-point value is finite.",
                    new[] { "bool isfinite(float x)", "bool isfinite(vector<float,n> x)", "bool isfinite(matrix<float,m,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "isinf",
                    "Determines if the specified value is infinite.",
                    new[] { "bool isinf(float x)", "bool isinf(vector<float,n> x)", "bool isinf(matrix<float,m,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "isnan",
                    "Determines if the specified value is NAN or QNAN.",
                    new[] { "bool isnan(float x)", "bool isnan(vector<float,n> x)", "bool isnan(matrix<float,m,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "ldexp",
                    "Returns the result of multiplying the specified value by two, raised to the power of the specified exponent.\nThis function uses the following formula: x * 2^exp",
                    new[]
                    {
                        "float ldexp(float x, float exp)", "vector<float,n> ldexp(vector<float,n> x, vector<float,n> exp)",
                        "matrix<float,m,n> ldexp(matrix<float,m,n> x, matrix<float,m,n> exp)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "length",
                    "Returns the length of the specified floating-point vector.",
                    new[] { "float length(vector<float,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "lerp",
                    "Performs a linear interpolation based on the following formula: x + s(y - x)",
                    new[]
                    {
                        "float lerp(float x, float y, float s)",
                        "vector<float,n> lerp(vector<float,n> x, vector<float,n> y, vector<float,n> s)",
                        "matrix<float,m,n> lerp(matrix<float,m,n> x, matrix<float,m,n> y, matrix<float,m,n> s)"
                    }));

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
                        "float log(float x)", "vector<float,n> log(vector<float,n> x)", "matrix<float,m,n> log(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "log10",
                    "Returns the base-10 logarithm of the specified value.",
                    new[]
                    {
                        "float log10(float x)", "vector<float,n> log10(vector<float,n> x)",
                        "matrix<float,m,n> log10(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "log2",
                    "Returns the base-2 logarithm of the specified value.",
                    new[]
                    {
                        "float log2(float x)", "vector<float,n> log2(vector<float,n> x)",
                        "matrix<float,m,n> log2(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "max",
                    "Selects the greater of x and y.",
                    new[]
                    {
                        "float max(float x, float y)", "vector<float,n> max(vector<float,n> x, vector<float,n> y)",
                        "matrix<float,m,n> max(matrix<float,m,n> x, matrix<float,m,n> y)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "min",
                    "Selects the lesser of x and y.",
                    new[]
                    {
                        "float min(float x, float y)", "vector<float,n> min(vector<float,n> x, vector<float,n> y)",
                        "matrix<float,m,n> min(matrix<float,m,n> x, matrix<float,m,n> y)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "modf",
                    "Splits the value x into fractional and integer parts, each of which has the same sign as x.",
                    new[]
                    {
                        "float modf(float x, out float ip)", "int modf(int x, out int ip)",
                        "vector<float,n> modf(vector<float,n> x, out vector<float,n> ip)",
                        "vector<int,n> modf(vector<int,n> x, out vector<int,n> ip)",
                        "matrix<float,m,n> modf(matrix<float,m,n> x, out matrix<float,m,n> ip)",
                        "matrix<int,m,n> modf(matrix<int,m,n> x, out matrix<int,m,n> ip)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "mul",
                    "Multiplies x and y using matrix math.",
                    new[]
                    {
                        "float mul(float s1, float s2)", "int mul(int s1, int s2)",
                        "vector<float,n> mul(float s, vector<float,n> v)", "vector<int,n> mul(int s, vector<int,n> v)",
                        "vector<float,n> mul(vector<float,n> v, float s)", "vector<int,n> mul(vector<int,n> v, int s)",
                        "matrix<float,m,n> mul(float s, matrix<float,m,n> M)", "matrix<int,m,n> mul(int s, matrix<int,m,n> M)",
                        "matrix<float,m,n> mul(matrix<float,m,n> M, float s)", "matrix<int,m,n> mul(matrix<int,m,n> M, int s)",
                        "vector<float,n> mul(vector<float,n> v1, vector<float,n> v2)",
                        "vector<int,n> mul(vector<int,n> v1, vector<int,n> v2)",
                        "vector<float,n> mul(vector<float,m> v, matrix<float,m,n> M)",
                        "vector<int,n> mul(vector<int,m> v, matrix<int,m,n> M)",
                        "vector<float,n> mul(matrix<float,n,m> M, vector<float,m> v)",
                        "vector<int,n> mul(vector<int,n,m> M, vector<int,m> v)",
                        "matrix<float,m,n> mul(matrix<float,m,l> M1, matrix<float,l,n> M2)",
                        "matrix<int,m,n> mul(matrix<int,m,l> M1, matrix<int,l,n> M2)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "noise",
                    "Generates a random value using the Perlin-noise algorithm.",
                    new[] { "float noise(vector<float,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "normalize",
                    "Normalizes the specified floating-point vector according to x / length(x).",
                    new[] { "vector<float,n> normalize(vector<float,n> x)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "pow",
                    "Returns the specified value raised to the specified power.",
                    new[]
                    {
                        "float pow(float x, float y)", "vector<float,n> pow(vector<float,n> x, vector<float,n> y)",
                        "matrix<float,m,n> pow(matrix<float,m,n> x, matrix<float,m,n> y)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "radians",
                    "Converts the specified value from degrees to radians.",
                    new[]
                    {
                        "float radians(float degrees)", "vector<float,n> radians(vector<float,n> degrees)",
                        "matrix<float,m,n> radians(matrix<float,m,n> degrees)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "reflect",
                    "Returns a reflection vector using an entering ray direction and a surface normal.",
                    new[] { "vector<float,n> reflect(vector<float,n> incident, vector<float,n> normal)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "refract",
                    "Returns a refraction vector using an entering ray, a surface normal, and a refraction index.",
                    new[] { "vector<float,n> refract(vector<float,n> incident, vector<float,n> normal, float η)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "RestartStrip",
                    "Ends the current-primitive strip and starts a new strip. If the current strip does not have enough vertices emitted to fill the primitive topology, the incomplete primitive at the end will be discarded.",
                    new[] { "void RestartStrip()" }));

            Functions.Add(
                new FunctionCompletionData(
                    "round",
                    "Rounds the specified value to the nearest integer.",
                    new[]
                    {
                        "float round(float x)", "vector<float,n> round(vector<float,n> x)",
                        "matrix<float,m,n> round(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "rsqrt",
                    "Returns the reciprocal of the square root of the specified value.",
                    new[]
                    {
                        "float rsqrt(float x)", "vector<float,n> rsqrt(vector<float,n> x)",
                        "matrix<float,m,n> rsqrt(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "saturate",
                    "Clamps the specified value within the range of 0 to 1.",
                    new[]
                    {
                        "float saturate(float x)", "vector<float,n> saturate(vector<float,n> x)",
                        "matrix<float,m,n> saturate(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sign",
                    "Returns the sign of x.",
                    new[]
                    {
                        "float sign(float x)", "int sign(int x)",
                        "vector<float,n> sign(vector<float,n> x)", "vector<int,n> sign(vector<int,n> x)",
                        "matrix<float,m,n> sign(matrix<float,m,n> x)", "matrix<int,m,n> sign(matrix<int,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sin",
                    "Returns the sine of the specified value.",
                    new[]
                    {
                        "float sin(float x)", "vector<float,n> sin(vector<float,n> x)", "matrix<float,m,n> sin(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sincos",
                    "Returns the sine and cosine of x.",
                    new[]
                    {
                        "void sincos(float x, out float s, out float c)",
                        "void sincos(vector<float,n> x, out vector<float,n> s, out vector<float,n> c)",
                        "void sincos(matrix<float,m,n> x, out matrix<float,m,n> s, out matrix<float,m,n> c)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sinh",
                    "Returns the hyperbolic sine of the specified value.",
                    new[]
                    {
                        "float sinh(float x)", "vector<float,n> sinh(vector<float,n> x)",
                        "matrix<float,m,n> sinh(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "smoothstep",
                    "Returns a smooth Hermite interpolation between 0 and 1, if x is in the range [min, max].",
                    new[]
                    {
                        "float smoothstep(float min, float max, float x)",
                        "vector<float,n> smoothstep(vector<float,n> min, vector<float,n> max, vector<float,n> x)",
                        "matrix<float,m,n> smoothstep(matrix<float,m,n> min, matrix<float,m,n> max, matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "sqrt",
                    "Returns the square root of the specified floating-point value, per component.",
                    new[]
                    {
                        "float sqrt(float x)", "vector<float,n> sqrt(vector<float,n> x)",
                        "matrix<float,m,n> sqrt(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "step",
                    "Compares two values, returning 0 or 1 based on which value is greater.",
                    new[]
                    {
                        "float step(float theshold, float x)",
                        "vector<float,n> step(vector<float,n> theshold, vector<float,n> x)",
                        "matrix<float,m,n> step(matrix<float,m,n> theshold, matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tan",
                    "Returns the tangent of the specified value.",
                    new[]
                    {
                        "float tan(float x)", "vector<float,n> tan(vector<float,n> x)", "matrix<float,m,n> tan(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tanh",
                    "Returns the hyperbolic tangent of the specified value.",
                    new[]
                    {
                        "float tanh(float x)", "vector<float,n> tanh(vector<float,n> x)",
                        "matrix<float,m,n> tanh(matrix<float,m,n> x)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1D",
                    "Samples a 1D texture.",
                    new[] { "float4 tex1D(sampler1D sampler, float t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dgrad",
                    "Samples a 1D texture using a gradient to select the mip level.",
                    new[] { "float4 tex1Dgrad(sampler1D sampler, float t, float ddx, float ddy)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dlod",
                    "Samples a 1D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[] { "float4 tex1Dlod(sampler1D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex1Dproj",
                    "Samples a 1D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[] { "float4 tex1Dproj(sampler1D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2D",
                    "Samples a 2D texture.",
                    new[] { "float4 tex2D(sampler2D sampler, float2 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dgrad",
                    "Samples a 2D texture using a gradient to select the mip level.",
                    new[] { "float4 tex2Dgrad(sampler2D sampler, float2 t, float2 ddx, float2 ddy)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dlod",
                    "Samples a 2D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[] { "float4 tex2Dlod(sampler2D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex2Dproj",
                    "Samples a 2D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[] { "float4 tex2Dproj(sampler2D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3D",
                    "Samples a 3D texture.",
                    new[] { "float4 tex3D(sampler3D sampler, float3 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dbias",
                    "Samples a 3D texture after biasing the mip level by t.w.",
                    new[] { "float4 tex3Dbias(sampler3D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dgrad",
                    "Samples a 3D texture using a gradient to select the mip level.",
                    new[] { "float4 tex3Dgrad(sampler3D sampler, float3 t, float ddx, float ddy)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dlod",
                    "Samples a 3D texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[] { "float4 tex3Dlod(sampler3D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "tex3Dproj",
                    "Samples a 3D texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[] { "float4 tex3Dproj(sampler3D sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBE",
                    "Samples a cube texture.",
                    new[] { "float4 texCUBE(samplerCUBE sampler, float3 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEbias",
                    "Samples a cube texture after biasing the mip level by t.w.",
                    new[] { "float4 texCUBEbias(samplerCUBE sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEgrad",
                    "Samples a cube texture using a gradient to select the mip level.",
                    new[] { "float4 texCUBEgrad(samplerCUBE sampler, float3 t, float ddx, float ddy)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBElod",
                    "Samples a cube texture with mipmaps. The mipmap LOD is specified in t.w.",
                    new[] { "float4 texCUBElod(samplerCUBE sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "texCUBEproj",
                    "Samples a cube texture using a projective divide; the texture coordinate is divided by t.w before the lookup takes place.",
                    new[] { "float4 texCUBEproj(samplerCUBE sampler, float4 t)" }));

            Functions.Add(
                new FunctionCompletionData(
                    "transpose",
                    "Transposes the specified input matrix.",
                    new[]
                    {
                        "matrix<bool,m,n> transpose(matrix<bool,n,m> a)", "matrix<float,m,n> transpose(matrix<float,n,m> a)",
                        "matrix<int,m,n> transpose(matrix<int,n,m> a)"
                    }));

            Functions.Add(
                new FunctionCompletionData(
                    "trunc",
                    "Truncates a floating-point value to the integer component.",
                    new[]
                    {
                        "float trunc(float x)", "vector<float,n> trunc(vector<float,n> x)",
                        "matrix<float,m,n> trunc(matrix<float,m,n> x)"
                    }));
        }


        /// <summary>
        /// Initializes the HLSL object methods.
        /// </summary>
        private void InitializeMethods()
        {
            Methods.Add(
                new FunctionCompletionData(
                    "Append",
                    "Appends geometry-shader-output data to an existing stream.",
                    new[]
                    {
                        "void PointStream<DataType>.Append(DataType data)",
                        "void LineStream<DataType>.Append(DataType data)",
                        "void TriangleStream<DataType>.Append(DataType data)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "CalculateLevelOfDetail",
                    "Calculates the level-of-detail.",
                    new[]
                    {
                        "float Texture1D.CalculateLevelOfDetail(sampler_state s, float x)",
                        "float Texture1DArray.CalculateLevelOfDetail(sampler_state s, float x)",
                        "float Texture2D.CalculateLevelOfDetail(sampler_state s, float2 x)",
                        "float Texture2DArray.CalculateLevelOfDetail(sampler_state s, float2 x)",
                        "float Texture3D.CalculateLevelOfDetail(sampler_state s, float3 x)",
                        "float TextureCube.CalculateLevelOfDetail(sampler_state s, float3 x)",
                        "float TextureCubeArray.CalculateLevelOfDetail(sampler_state s, float3 x)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "CalculateLevelOfDetailUnclamped",
                    "Calculates the LOD without clamping the result.",
                    new[]
                    {
                        "float Texture1D.CalculateLevelOfDetailUnclamped(sampler_state s, float x)",
                        "float Texture1DArray.CalculateLevelOfDetailUnclamped(sampler_state s, float x)",
                        "float Texture2D.CalculateLevelOfDetailUnclamped(sampler_state s, float2 x)",
                        "float Texture2DArray.CalculateLevelOfDetailUnclamped(sampler_state s, float2 x)",
                        "float Texture3D.CalculateLevelOfDetailUnclamped(sampler_state s, float3 x)",
                        "float TextureCube.CalculateLevelOfDetailUnclamped(sampler_state s, float3 x)",
                        "float TextureCubeArray.CalculateLevelOfDetailUnclamped(sampler_state s, float3 x)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "Gather",
                    "Gets the four samples (red component only) that would be used for bilinear interpolation when sampling a texture.",
                    new[]
                    {
                        "float4 Texture2D.Gather(sampler_state s, float2 location)",
                        "float4 Texture2D.Gather(sampler_state s, float2 location, int2 offset)",
                        "float Texture2DArray.Gather(sampler_state s, float3 location)",
                        "float Texture2DArray.Gather(sampler_state s, float3 location, int2 offset)",
                        "float TextureCube.Gather(sampler_state s, float3 location)",
                        "float TextureCubeArray.Gather(sampler_state s, float4 location)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "GetDimensions",
                    "Gets texture-size information.",
                    new[]
                    {
                        "void Texture1D.GetDimensions(uint mipLevel, out uint width, out uint numberOfLevels)",
                        "void Texture1D.GetDimensions(out uint width)",
                        "void Texture1D.GetDimensions(uint mipLevel, out float width, out float numberOfLevels)",
                        "void Texture1D.GetDimensions(out float width)",
                        "void Texture2D.GetDimensions(uint mipLevel, out uint width, out uint height, out uint numberOfLevels)",
                        "void Texture2D.GetDimensions(out uint width, out uint height)",
                        "void Texture2D.GetDimensions(uint mipLevel, out float width, out float height, out float numberOfLevels)",
                        "void Texture2D.GetDimensions(out float width, out float height)",
                        "void Texture2DArray.GetDimensions(uint mipLevel, out uint width, out uint height, out uint elements, out uint numberOfLevels)",
                        "void Texture2DArray.GetDimensions(out uint width, out uint height, out uint elements)",
                        "void Texture2DArray.GetDimensions(uint mipLevel, out float width, out float height, out float elements, out float numberOfLevels)",
                        "void Texture2DArray.GetDimensions(out float width, out float height, out float elements)",
                        "void Texture3D.GetDimensions(uint mipLevel, out uint width, out uint height, out uint depth, out uint numberOfLevels)",
                        "void Texture3D.GetDimensions(out uint width, out uint height, out uint depth)",
                        "void Texture3D.GetDimensions(uint mipLevel, out float width, out float height, out float depth, out float numberOfLevels)",
                        "void Texture3D.GetDimensions(out float width, out float height, out float depth)",
                        "void TextureCube.GetDimensions(uint mipLevel, out uint width, out uint height, out uint numberOfLevels)",
                        "void TextureCube.GetDimensions(out uint width, out uint height)",
                        "void TextureCube.GetDimensions(uint mipLevel, out float width, out float height, out float numberOfLevels)",
                        "void TextureCube.GetDimensions(out float width, out float height)",
                        "void TextureCubeArray.GetDimensions(uint mipLevel, out uint width, out uint height, out uint elements, out uint numberOfLevels)",
                        "void TextureCubeArray.GetDimensions(out uint width, out uint height, out uint elements)",
                        "void TextureCubeArray.GetDimensions(uint mipLevel, out float width, out float height, out float elements, out float numberOfLevels)",
                        "void TextureCubeArray.GetDimensions(out float width, out float height, out float elements)",
                        "void Texture2DMS.GetDimensions(out uint width, out uint height, out uint numberOfSamples)",
                        "void Texture2DMS.GetDimensions(out float width, out float height, out float numberOfSamples)",
                        "void Texture2DMSArray.GetDimensions(out uint width, out uint height, out uint elements, out uint numberOfSamples)",
                        "void Texture2DMSArray.GetDimensions(out float width, out float height, out float elements, out float numberOfSamples)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "GetSamplePosition",
                    "Gets the position of the specified sample.",
                    new[]
                    {
                        "float2 Texture2DMS.GetDimensions(int s)",
                        "float2 Texture2DMSArray.GetDimensions(int s)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "Load",
                    "Reads texel data without any filtering or sampling.",
                    new[]
                    {
                        "DataType Buffer<DataType>.Load(int2 location)",
                        "DataType Buffer<DataType>.Load(int location, int offset)",
                        "float4 Texture1D.Load(int2 location)",
                        "float4 Texture1D.Load(int2 location, int offset)",
                        "float4 Texture1DArray.Load(int3 location)",
                        "float4 Texture1DArray.Load(int3 location, int offset)",
                        "float4 Texture2D.Load(int3 location)",
                        "float4 Texture2D.Load(int3 location, int2 offset)",
                        "float4 Texture2DArray.Load(int4 location)",
                        "float4 Texture2DArray.Load(int4 location, int3 offset)",
                        "float4 Texture2DMS.Load(int2 location)",
                        "float4 Texture2DMS.Load(int2 location, int2 offset)",
                        "float4 Texture2DMS.Load(int2 location, int2 offset, int numberOfSamples)",
                        "float4 Texture2DMSArray.Load(int2 location)",
                        "float4 Texture2DMSArray.Load(int2 location, int2 offset)",
                        "float4 Texture2DMSArray.Load(int2 location, int2 offset, int numberOfSamples)",
                        "float4 Texture3D.Load(int4 location)",
                        "float4 Texture3D.Load(int4 location, int3 offset)",
                        "float4 TextureCube.Load(int4 location)",
                        "float4 TextureCube.Load(int4 location, int4 offset)",
                        "float4 TextureCubeArray.Load(int4 location)",
                        "float4 TextureCubeArray.Load(int4 location, int4 offset)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "Sample",
                    "Samples a texture.",
                    new[]
                    {
                        "DXGI_FORMAT Texture1D.Sample(sampler_state s, float location)",
                        "DXGI_FORMAT Texture1D.Sample(sampler_state s, float location, int offset)",
                        "DXGI_FORMAT Texture1DArray.Sample(sampler_state s, float2 location)",
                        "DXGI_FORMAT Texture1DArray.Sample(sampler_state s, float2 location, int offset)",
                        "DXGI_FORMAT Texture2D.Sample(sampler_state s, float2 location)",
                        "DXGI_FORMAT Texture2D.Sample(sampler_state s, float2 location, int2 offset)",
                        "DXGI_FORMAT Texture2DArray.Sample(sampler_state s, float3 location)",
                        "DXGI_FORMAT Texture2DArray.Sample(sampler_state s, float3 location, int2 offset)",
                        "DXGI_FORMAT Texture3D.Sample(sampler_state s, float3 location)",
                        "DXGI_FORMAT Texture3D.Sample(sampler_state s, float3 location, int3 offset)",
                        "DXGI_FORMAT TextureCube.Sample(sampler_state s, float3 location)",
                        "DXGI_FORMAT TextureCubeArray.Sample(sampler_state s, float4 location)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "SampleBias",
                    "Samples a texture, after applying the input bias to the mipmap level.",
                    new[]
                    {
                        "DXGI_FORMAT Texture1D.SampleBias(sampler_state s, float location, float bias)",
                        "DXGI_FORMAT Texture1D.SampleBias(sampler_state s, float location, float bias, int offset)",
                        "DXGI_FORMAT Texture1DArray.SampleBias(sampler_state s, float2 location, float bias)",
                        "DXGI_FORMAT Texture1DArray.SampleBias(sampler_state s, float2 location, float bias, int offset)",
                        "DXGI_FORMAT Texture2D.SampleBias(sampler_state s, float2 location, float bias)",
                        "DXGI_FORMAT Texture2D.SampleBias(sampler_state s, float2 location, float bias, int2 offset)",
                        "DXGI_FORMAT Texture2DArray.SampleBias(sampler_state s, float3 location, float bias)",
                        "DXGI_FORMAT Texture2DArray.SampleBias(sampler_state s, float3 location, float bias, int2 offset)",
                        "DXGI_FORMAT Texture3D.SampleBias(sampler_state s, float3 location, float bias)",
                        "DXGI_FORMAT Texture3D.SampleBias(sampler_state s, float3 location, float bias, int3 offset)",
                        "DXGI_FORMAT TextureCube.SampleBias(sampler_state s, float3 location, float bias)",
                        "DXGI_FORMAT TextureCubeArray.SampleBias(sampler_state s, float4 location, float bias)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "SampleCmp",
                    "Samples a texture and compares a single component against the specified comparison value.",
                    new[]
                    {
                        "uint Texture1D.SampleCmp(SamplerComparisonState s, float location, float compareValue)",
                        "uint Texture1D.SampleCmp(SamplerComparisonState s, float location, float compareValue, int offset)",
                        "uint Texture1DArray.SampleCmp(SamplerComparisonState s, float2 location, float compareValue)",
                        "uint Texture1DArray.SampleCmp(SamplerComparisonState s, float2 location, float compareValue, int offset)",
                        "uint Texture2D.SampleCmp(SamplerComparisonState s, float2 location, float compareValue)",
                        "uint Texture2D.SampleCmp(SamplerComparisonState s, float2 location, float compareValue, int2 offset)",
                        "uint Texture2DArray.SampleCmp(SamplerComparisonState s, float3 location, float compareValue)",
                        "uint Texture2DArray.SampleCmp(SamplerComparisonState s, float3 location, float compareValue, int2 offset)",
                        "uint TextureCube.SampleCmp(SamplerComparisonState s, float3 location, float compareValue)",
                        "uint TextureCube.SampleCmp(SamplerComparisonState s, float3 location, float compareValue), int3 offset",
                        "uint TextureCubeArray.SampleCmp(SamplerComparisonState s, float4 location, float compareValue)",
                        "uint TextureCubeArray.SampleCmp(SamplerComparisonState s, float4 location, float compareValue), int3 offset"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "SampleCmpLevelZero",
                    "Sample a texture and compare the result to a comparison value. This function is identical to calling SampleCmpLevelZero on mipmap level 0 only.",
                    new[]
                    {
                        "uint Texture1D.SampleCmpLevelZero(SamplerComparisonState s, float location, float compareValue)",
                        "uint Texture1D.SampleCmpLevelZero(SamplerComparisonState s, float location, float compareValue, int offset)",
                        "uint Texture1DArray.SampleCmpLevelZero(SamplerComparisonState s, float2 location, float compareValue)",
                        "uint Texture1DArray.SampleCmpLevelZero(SamplerComparisonState s, float2 location, float compareValue, int offset)",
                        "uint Texture2D.SampleCmpLevelZero(SamplerComparisonState s, float2 location, float compareValue)",
                        "uint Texture2D.SampleCmpLevelZero(SamplerComparisonState s, float2 location, float compareValue, int2 offset)",
                        "uint Texture2DArray.SampleCmpLevelZero(SamplerComparisonState s, float3 location, float compareValue)",
                        "uint Texture2DArray.SampleCmpLevelZero(SamplerComparisonState s, float3 location, float compareValue, int2 offset)",
                        "uint TextureCube.SampleCmpLevelZero(SamplerComparisonState s, float3 location, float compareValue)",
                        "uint TextureCube.SampleCmpLevelZero(SamplerComparisonState s, float3 location, float compareValue), int3 offset",
                        "uint TextureCubeArray.SampleCmpLevelZero(SamplerComparisonState s, float4 location, float compareValue)",
                        "uint TextureCubeArray.SampleCmpLevelZero(SamplerComparisonState s, float4 location, float compareValue), int3 offset"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "SampleGrad",
                    "Samples a texture using a gradient to influence the way the sample location is calculated.",
                    new[]
                    {
                        "DXGI_FORMAT Texture1D.SampleGrad(sampler_state s, float location, float ddx, float ddy)",
                        "DXGI_FORMAT Texture1D.SampleGrad(sampler_state s, float location, float ddx, float ddy, int offset)",
                        "DXGI_FORMAT Texture1DArray.SampleGrad(sampler_state s, float2 location, float2 ddx, float2 ddy)",
                        "DXGI_FORMAT Texture1DArray.SampleGrad(sampler_state s, float2 location, float2 ddx, float2 ddy, int offset)",
                        "DXGI_FORMAT Texture2D.SampleGrad(sampler_state s, float2 location, float2 ddx, float2 ddy)",
                        "DXGI_FORMAT Texture2D.SampleGrad(sampler_state s, float2 location, float2 ddx, float2 ddy, int2 offset)",
                        "DXGI_FORMAT Texture2DArray.SampleGrad(sampler_state s, float3 location, float2 ddx, float2 ddy)",
                        "DXGI_FORMAT Texture2DArray.SampleGrad(sampler_state s, float3 location, float2 ddx, float2 ddy, int2 offset)",
                        "DXGI_FORMAT Texture3D.SampleGrad(sampler_state s, float3 location, float3 ddx, float3 ddy)",
                        "DXGI_FORMAT Texture3D.SampleGrad(sampler_state s, float3 location, float3 ddx, float3 ddy, int3 offset)",
                        "DXGI_FORMAT TextureCube.SampleGrad(sampler_state s, float3 location, float3 ddx, float3 ddy)",
                        "DXGI_FORMAT TextureCubeArray.SampleGrad(sampler_state s, float4 location, float3 ddx, float3 ddy)"
                    }));

            Methods.Add(
                new FunctionCompletionData(
                    "SampleLevel",
                    "Samples a texture using a mipmap-level offset.",
                    new[]
                    {
                        "DXGI_FORMAT Texture1D.SampleLevel(sampler_state s, float location, float lod)",
                        "DXGI_FORMAT Texture1D.SampleLevel(sampler_state s, float location, float lod, int offset)",
                        "DXGI_FORMAT Texture1DArray.SampleLevel(sampler_state s, float2 location, float lod)",
                        "DXGI_FORMAT Texture1DArray.SampleLevel(sampler_state s, float2 location, float lod, int offset)",
                        "DXGI_FORMAT Texture2D.SampleLevel(sampler_state s, float2 location, float lod)",
                        "DXGI_FORMAT Texture2D.SampleLevel(sampler_state s, float2 location, float lod, int2 offset)",
                        "DXGI_FORMAT Texture2DArray.SampleLevel(sampler_state s, float3 location, float lod)",
                        "DXGI_FORMAT Texture2DArray.SampleLevel(sampler_state s, float3 location, float lod, int2 offset)",
                        "DXGI_FORMAT Texture3D.SampleLevel(sampler_state s, float3 location, float lod)",
                        "DXGI_FORMAT Texture3D.SampleLevel(sampler_state s, float3 location, float lod, int3 offset)",
                        "DXGI_FORMAT TextureCube.SampleLevel(sampler_state s, float3 location, float lod)",
                        "DXGI_FORMAT TextureCubeArray.SampleLevel(sampler_state s, float4 location, float lod)"
                    }));
        }


        /// <summary>
        /// Initializes the DirectX Effect functions.
        /// </summary>
        private void InitializeEffectFunctions()
        {
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetVertexShader",
                    "Sets a vertex shader. (Direct3D 10)",
                    new[] { "void SetVertexShader(CompiledShader shader)" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetGeometryShader",
                    "Sets a geometry shader. (Direct3D 10)",
                    new[] { "void SetGeometryShader(CompiledShader shader)" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetPixelShader", "Sets a pixel shader. (Direct3D 10)", new[] { "void SetPixelShader(CompiledShader shader)" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "CompileShader",
                    "Compiles a shader. (Direct3D 10)",
                    new[] { "CompiledShader CompileShader(ShaderTarget profile, ShaderFunction function)" }));
            EffectFunctions.Add(
                new FunctionCompletionData("compile", "Compiles a shader.", new[] { "compile <ShaderTarget> <ShaderFunction>" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetBlendState",
                    "Sets the blend state. (Direct3D 10)",
                    new[] { "void SetBlendState(BlendState state, float4 blendFactor, uint sampleMask)" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetDepthStencilState",
                    "Sets the depth-stencil state. (Direct3D 10)",
                    new[] { "void SetDepthStencilState(DepthStencilState state, uint stencilReferenceValue)" }));
            EffectFunctions.Add(
                new FunctionCompletionData(
                    "SetRasterizerState",
                    "Sets the rasterizer state. (Direct3D 10)",
                    new[] { "void SetRasterizerState(RasterizerState state)" }));
        }


        /// <summary>
        /// Initializes the DirectX 9 Effect states.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InitializeEffectStates()
        {
            // General state block
            // Note: State block is not really documented and I did not find any samples.
            EffectStates.Add(new StateCompletionData("StateBlock", "A sequence of general state."));

            // Light States
            EffectStates.Add(new StateCompletionData("LightAmbient", "Ambient color emitted by the light.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "LightAttenuation0", "Value specifying how the light intensity changes over distance.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "LightAttenuation1", "Value specifying how the light intensity changes over distance.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "LightAttenuation2", "Value specifying how the light intensity changes over distance.", true));
            EffectStates.Add(new StateCompletionData("LightDiffuse", "Diffuse color emitted by the light.", true));
            EffectStates.Add(
                new StateCompletionData("LightDirection", "Direction that the light is pointing in world space.", true));
            string[] boolValues = new[] { "TRUE", "FALSE" };
            EffectStates.Add(
                new StateCompletionData("LightEnable", "Enables or disables a set of lighting parameters.", boolValues, true));
            EffectStates.Add(
                new StateCompletionData(
                    "LightFalloff",
                    "Decrease in illumination between a spotlight's inner cone (the angle specified by Theta) and the outer edge of the outer cone (the angle specified by Phi).",
                    new string[] { }));
            EffectStates.Add(
                new StateCompletionData(
                    "LightPhi", "Angle, in radians, defining the outer edge of the spotlight's outer cone.", true));
            EffectStates.Add(new StateCompletionData("LightPosition", "Position of the light in world space.", true));
            EffectStates.Add(new StateCompletionData("LightRange", "Distance beyond which the light has no effect.", true));
            EffectStates.Add(new StateCompletionData("LightSpecular", "Specular color emitted by the light.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "LightTheta",
                    "Angle, in radians, of a spotlight's inner cone - that is, the fully illuminated spotlight cone.",
                    true));
            EffectStates.Add(
                new StateCompletionData("LightType", "Defines the light type.", new[] { "POINT", "SPOT", "DIRECTIONAL" }, true));

            // Material States
            EffectStates.Add(new StateCompletionData("MaterialAmbient", "Ambient color of the material."));
            EffectStates.Add(new StateCompletionData("MaterialDiffuse", "Diffuse color of the material."));
            EffectStates.Add(new StateCompletionData("MaterialEmissive", "Emissive color of the material."));
            EffectStates.Add(
                new StateCompletionData(
                    "MaterialPower", "Floating-point value specifying the sharpness of specular highlights."));
            EffectStates.Add(new StateCompletionData("MaterialSpecular", "Specular color of the material."));

            // Vertex Pipe Render States
            EffectStates.Add(new StateCompletionData("Ambient", "Color of the ambient light."));
            string[] materialSource = new[] { "MATERIAL", "COLOR1", "COLOR2" };
            EffectStates.Add(
                new StateCompletionData(
                    "AmbientMaterialSource", "Defines the source of the ambient material color.", materialSource));
            EffectStates.Add(new StateCompletionData("Clipping", "Enables or disable clipping.", boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "ClipPlaneEnable",
                    "Sets the enabled clip planes. (Bitwise combination of CLIPPLANE0 - CLIPPLANE5)",
                    new[] { "CLIPPLANE0", "CLIPPLANE1", "CLIPPLANE2", "CLIPPLANE3", "CLIPPLANE4", "CLIPPLANE5" }));
            EffectStates.Add(new StateCompletionData("ColorVertex", "Enable or disable per-vertex color.", boolValues));
            EffectStates.Add(new StateCompletionData("CullMode", "Culling mode.", new[] { "NONE", "CW", "CCW" }));
            EffectStates.Add(
                new StateCompletionData(
                    "DiffuseMaterialSource", "Defines the source of the diffuse material color.", materialSource));
            EffectStates.Add(
                new StateCompletionData(
                    "EmissiveMaterialSource", "Defines the source of the emissive material color.", materialSource));
            EffectStates.Add(new StateCompletionData("FogColor", "Fog color."));
            EffectStates.Add(
                new StateCompletionData(
                    "FogDensity", "Fog density for pixel or vertex fog used in the exponential fog modes. Range [0, 1]."));
            EffectStates.Add(new StateCompletionData("FogEnable", "Enables or disables fog blending.", boolValues));
            EffectStates.Add(
                new StateCompletionData("FogEnd", "Depth at which pixel or vertex fog effects end for linear fog mode. "));
            EffectStates.Add(
                new StateCompletionData("FogStart", "Depth at which pixel or vertex fog effects begin for linear fog mode."));
            string[] fogModes = new[] { "NONE", "EXP", "EXP2", "LINEAR" };
            EffectStates.Add(new StateCompletionData("FogTableMode", "The fog formula to be used for pixel fog.", fogModes));
            EffectStates.Add(new StateCompletionData("FogVertexMode", "The fog formula to be used for vertex fog.", fogModes));
            EffectStates.Add(
                new StateCompletionData("IndexedVertexBlendEnable", "Enables or disables indexed vertex blending.", boolValues));
            EffectStates.Add(new StateCompletionData("Lighting", "Enables or disables Direct3D lighting.", boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "LocalViewer",
                    "TRUE to enable camera-relative specular highlights, or FALSE to use orthogonal specular highlights.",
                    boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "MultiSampleAntialias",
                    "Determines how individual samples are computed when using a multisample render-target buffer.",
                    boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "MultiSampleMask",
                    "Each bit in this mask, starting at the least significant bit (LSB), controls modification of one of the samples in a multisample render target."));
            EffectStates.Add(
                new StateCompletionData(
                    "NormalizeNormals", "Enables or disables automatic normalization of vertex normals.", boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "PatchSegments",
                    "Specifies the number of subdivision segments for N-patches. (Value < 1.0 disables N-patches.)"));
            EffectStates.Add(
                new StateCompletionData(
                    "PointScale_A", "A float value that controls for distance-based size attenuation for point primitives."));
            EffectStates.Add(
                new StateCompletionData(
                    "PointScale_B", "A float value that controls for distance-based size attenuation for point primitives."));
            EffectStates.Add(
                new StateCompletionData(
                    "PointScale_C", "A float value that controls for distance-based size attenuation for point primitives."));
            EffectStates.Add(
                new StateCompletionData(
                    "PointScaleEnable",
                    "Value that controls computation of size for point primitives. (TRUE = camera space, FALSE = screen space)",
                    boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "PointSize",
                    "The size to use for point size computation in cases where point size is not specified for each vertex."));
            EffectStates.Add(new StateCompletionData("PointSize_Min", "The minimum size of point primitives."));
            EffectStates.Add(new StateCompletionData("PointSize_Max", "the maximum size of point primitives."));
            EffectStates.Add(
                new StateCompletionData(
                    "PointSpriteEnable",
                    "When TRUE, texture coordinates of point primitives are set so that full textures are mapped on each point. When FALSE, the vertex texture coordinates are used for the entire point.",
                    boolValues));
            EffectStates.Add(
                new StateCompletionData("RangeFogEnable", "Enables or disables range-based vertex fog.", boolValues));
            EffectStates.Add(
                new StateCompletionData("SpecularEnable", "Enables or disables specular highlights.", boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "SpecularMaterialSource", "Defines the source of the specular material color.", materialSource));
            EffectStates.Add(new StateCompletionData("TweenFactor", "The tween factor."));
            EffectStates.Add(
                new StateCompletionData(
                    "VertexBlend",
                    "Control the number or matrices that the system applies when performing multimatrix vertex blending.",
                    new[] { "DISABLE", "0WEIGHTS", "1WEIGHTS", "2WEIGHTS", "3WEIGHTS", "TWEENING" }));

            // Pixel Pipe Render States
            EffectStates.Add(
                new StateCompletionData("AlphaBlendEnable", "Enables or disables alpha-blended transparency.", boolValues));
            string[] comparisonFunction =
            {
                "NEVER", "LESS", "EQUAL", "LESSEQUAL", "GREATER", "NOTEQUAL", "GREATEREQUAL", "ALWAYS"
            };
            EffectStates.Add(new StateCompletionData("AlphaFunc", "Sets the alpha compare functions.", comparisonFunction));
            EffectStates.Add(
                new StateCompletionData(
                    "AlphaRef", "The reference alpha value against which pixels are tested when alpha testing is enabled."));
            EffectStates.Add(
                new StateCompletionData("AlphaTestEnable", "Enables or disables per-pixel alpha testing.", boolValues));
            EffectStates.Add(
                new StateCompletionData(
                    "BlendOp",
                    "The arithmetic operation applied when alpha blending (AlphaBlendEnable) is enabled.",
                    new[] { "ADD", "SUBTRACT", "REVSUBTRACT", "MIN", "MAX" }));
            EffectStates.Add(
                new StateCompletionData(
                    "ColorWriteEnable",
                    "Enables a per-channel write for the render-target color buffer. (Bitwise combination of RED|GREEN|BLUE|ALPHA.)",
                    new[] { "RED", "GREEN", "BLUE", "ALPHA" }));
            EffectStates.Add(
                new StateCompletionData("DepthBias", "A floating-point value that is used for comparison of depth values."));
            string[] blendFactor =
            {
                "ZERO", "ONE",
                "SRCCOLOR", "INVSRCCOLOR", "SRCALPHA", "INVSRCALPHA",
                "DESTALPHA", "INVDESTALPHA", "DESTCOLOR", "INVDESTCOLOR",
                "SRCALPHASAT", "BOTHSRCALPHA", "BOTHINVSRCALPHA",
                "BLENDFACTOR", "INVBLENDFACTOR", "SRCCOLOR2", "INVSRCCOLOR2"
            };
            EffectStates.Add(new StateCompletionData("DestBlend", "Destination blend factor.", blendFactor));
            EffectStates.Add(new StateCompletionData("DitherEnable", "Enables or disables dithering.", boolValues));
            EffectStates.Add(new StateCompletionData("FillMode", "The fill mode.", new[] { "POINT", "WIREFRAME", "SOLID" }));
            EffectStates.Add(
                new StateCompletionData("LastPixel", "Enables or disables drawing of the last pixel in a line.", boolValues));
            EffectStates.Add(new StateCompletionData("ShadeMode", "The shading mode.", new[] { "FLAT", "GOURAUD", "PHONG" }));
            EffectStates.Add(
                new StateCompletionData(
                    "SlopeScaleDepthBias",
                    "Used to determine how much bias can be applied to co-planar primitives to reduce z-fighting."));
            EffectStates.Add(new StateCompletionData("SrcBlend", "Source blend factor.", blendFactor));
            EffectStates.Add(new StateCompletionData("StencilEnable", "Enables or disables stenciling.", boolValues));
            string[] stencilOperation =
            {
                "KEEP", "ZERO", "REPLACE", "INCRSAT", "DECRSAT", "INVERT", "INCR", "DECR"
            };
            EffectStates.Add(
                new StateCompletionData(
                    "StencilFail", "Stencil operation to perform if the stencil test fails.", stencilOperation));
            EffectStates.Add(
                new StateCompletionData("StencilFunc", "Comparison function for the stencil test.", comparisonFunction));
            EffectStates.Add(
                new StateCompletionData(
                    "StencilMask",
                    "Mask applied to the reference value and each stencil buffer entry to determine the significant bits for the stencil test."));
            EffectStates.Add(
                new StateCompletionData(
                    "StencilPass",
                    "Stencil operation to perform if both the stencil and the depth (z) tests pass.",
                    stencilOperation));
            EffectStates.Add(new StateCompletionData("StencilRef", "An int reference value for the stencil test."));
            EffectStates.Add(
                new StateCompletionData("StencilWriteMask", "Write mask applied to values written into the stencil buffer."));
            EffectStates.Add(
                new StateCompletionData(
                    "StencilZFail",
                    "Stencil operation to perform if the stencil test passes and the depth test (z-test) fails.",
                    stencilOperation));
            EffectStates.Add(new StateCompletionData("TextureFactor", "Color used for multiple-texture blending."));
            const string wrapDescription =
                "Texture-wrapping behavior for multiple sets of texture coordinates. (Bitwise combination.)";
            string[] wrap =
            {
                "COORD0", "COORD1", "COORD2", "COORD3", "U", "V", "W"
            };
            EffectStates.Add(new StateCompletionData("Wrap0", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap1", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap2", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap3", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap4", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap5", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap6", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap7", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap8", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap9", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap10", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap11", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap12", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap13", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap14", wrapDescription, wrap));
            EffectStates.Add(new StateCompletionData("Wrap15", wrapDescription, wrap));
            EffectStates.Add(
                new StateCompletionData(
                    "ZEnable",
                    "Disables depth buffering, enables z-buffering, or enables w-buffering.",
                    new[] { "FALSE", "TRUE", "USEW" }));
            EffectStates.Add(
                new StateCompletionData("ZFunc", "The comparison function for the depth-buffer test.", comparisonFunction));
            EffectStates.Add(
                new StateCompletionData("ZWriteEnable", "Enables or disables writing to the depth buffer.", boolValues));

            // Sampler States
            EffectStates.Add(new StateCompletionData("Sampler", "Sets a sampler state block."));

            // Sampler Stage States
            string[] textureAddressMode =
            {
                "WRAP", "MIRROR", "CLAMP", "BORDER", "MIRRORONCE"
            };
            EffectStates.Add(
                new StateCompletionData("AddressU", "Texture-address mode for the u coordinate.", textureAddressMode, true));
            EffectStates.Add(
                new StateCompletionData("AddressV", "Texture-address mode for the v coordinate.", textureAddressMode, true));
            EffectStates.Add(
                new StateCompletionData("AddressW", "Texture-address mode for the w coordinate.", textureAddressMode, true));
            EffectStates.Add(new StateCompletionData("BorderColor", "The texture border color as DWORD (0xAARRGGBB). The default value is 0x00000000.", true));
            string[] textureFilterType =
            {
                "NONE", "POINT", "LINEAR", "ANISOTROPIC", "PYRAMIDALQUAD", "GAUSSIANQUAD", "CONVOLUTIONMONO"
            };
            EffectStates.Add(new StateCompletionData("MagFilter", "Texture magnification filter.", textureFilterType, true));
            EffectStates.Add(new StateCompletionData("MaxAnisotropy", "The maximum anisotropy.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "MaxMipLevel",
                    "level-of-detail index of largest map to use. Values range from 0 to (n - 1) where 0 is the largest.",
                    true));
            EffectStates.Add(new StateCompletionData("MinFilter", "Texture minification filter ", textureFilterType, true));
            EffectStates.Add(
                new StateCompletionData("MipFilter", "Mipmap filter to use during minification", textureFilterType, true));
            EffectStates.Add(new StateCompletionData("MipMapLodBias", "Mipmap level-of-detail bias.", true));
            EffectStates.Add(new StateCompletionData("SRGBTexture", "Gamma correction value."));

            // Shader States
            EffectStates.Add(new StateCompletionData("VertexShader", "The vertex shader."));
            EffectStates.Add(new StateCompletionData("GeometryShader", "The geometry shader."));
            EffectStates.Add(new StateCompletionData("PixelShader", "The pixel shader."));

            // Shader Constant States
            EffectStates.Add(
                new StateCompletionData("PixelShaderConstant", "m x n array of floats; m and n are optional. float[m[n]]"));
            EffectStates.Add(new StateCompletionData("PixelShaderConstant1", "One 4D float. float4"));
            EffectStates.Add(new StateCompletionData("PixelShaderConstant2", "Two 4D floats. float4x2"));
            EffectStates.Add(new StateCompletionData("PixelShaderConstant3", "Three 4D floats. float4x3"));
            EffectStates.Add(new StateCompletionData("PixelShaderConstant4", "Four 4D floats. float4x4"));
            EffectStates.Add(
                new StateCompletionData("PixelShaderConstantB", "m x n array of bools; m and n are optional. bool[m[n]]"));
            EffectStates.Add(
                new StateCompletionData("PixelShaderConstantI", "m x n array of ints. m and n are optional. int[m[n]]"));
            EffectStates.Add(
                new StateCompletionData("PixelShaderConstantF", "m x n array of floats. m and n are optional. float[m[n]]"));
            EffectStates.Add(
                new StateCompletionData("VertexShaderConstant", "m x n array of floats. m and n are optional. float[m[n]]"));
            EffectStates.Add(new StateCompletionData("VertexShaderConstant1", "One 4D float. float4"));
            EffectStates.Add(new StateCompletionData("VertexShaderConstant2", "Two 4D floats. float4x2"));
            EffectStates.Add(new StateCompletionData("VertexShaderConstant3", "Three 4D floats. float4x3"));
            EffectStates.Add(new StateCompletionData("VertexShaderConstant4", "Four 4D floats. float4x4"));
            EffectStates.Add(
                new StateCompletionData("VertexShaderConstantB", "m x n array of bools. m and n are optional. bool[m[n]]"));
            EffectStates.Add(
                new StateCompletionData("VertexShaderConstantI", "m x n array of ints. m and n are optional. int[m[n]]"));
            EffectStates.Add(
                new StateCompletionData("VertexShaderConstantF", "m x n array of floats. m and n are optional. float[m[n]]"));

            // Texture States
            EffectStates.Add(new StateCompletionData("Texture", "Set texture.", true));

            // Texture Stage States
            string[] textureOperation =
            {
                "DISABLE", "SELECTARG1", "SELECTARG2", "MODULATE", "MODULATE2X", "MODULATE4X",
                "ADD", "ADDSIGNED", "ADDSIGNED2X", "SUBTRACT", "ADDSMOOTH",
                "BLENDDIFFUSEALPHA", "BLENDTEXTUREALPHA", "BLENDFACTORALPHA", "BLENDTEXTUREALPHAPM", "BLENDCURRENTALPHA",
                "PREMODULATE", "MODULATEALPHA_ADDCOLOR", "MODULATECOLOR_ADDALPHA", "MODULATEINVALPHA_ADDCOLOR",
                "MODULATEINVCOLOR_ADDALPHA",
                "BUMPENVMAP", "BUMPENVMAPLUMINANCE", "DOTPRODUCT3", "MULTIPLYADD", "LERP"
            };
            EffectStates.Add(new StateCompletionData("AlphaOp", "The texture-blending operation.", textureOperation, true));
            EffectStates.Add(
                new StateCompletionData(
                    "AlphaArg0",
                    "The alpha channel selector operand for triadic texture stage operations (multiply, add, and linearly interpolate).",
                    true));
            EffectStates.Add(new StateCompletionData("AlphaArg1", "The first alpha argument for the texture stage.", true));
            EffectStates.Add(new StateCompletionData("AlphaArg2", "The second alpha argument for the stage.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "ColorArg0",
                    "The third color operand for triadic texture stage operations (multiply, add, and linearly interpolate).",
                    true));
            EffectStates.Add(new StateCompletionData("ColorArg1", "The first color argument for the texture stage.", true));
            EffectStates.Add(new StateCompletionData("ColorArg2", "The second color argument for the texture stage.", true));
            EffectStates.Add(
                new StateCompletionData("ColorOp", "The texture color blending operation.", textureOperation, true));
            EffectStates.Add(
                new StateCompletionData("BumpEnvLScale", "Floating-point scale value for bump-map luminance.", true));
            EffectStates.Add(
                new StateCompletionData("BumpEnvLOffset", "Floating-point offset value for bump-map luminance.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "BumpEnvMat00",
                    "Texture-stage state is a floating-point value for the [0][0] coefficient in a bump-mapping matrix.",
                    true));
            EffectStates.Add(
                new StateCompletionData(
                    "BumpEnvMat01",
                    "Texture-stage state is a floating-point value for the [0][1] coefficient in a bump-mapping matrix.",
                    true));
            EffectStates.Add(
                new StateCompletionData(
                    "BumpEnvMat10",
                    "Texture-stage state is a floating-point value for the [1][0] coefficient in a bump-mapping matrix.",
                    true));
            EffectStates.Add(
                new StateCompletionData(
                    "BumpEnvMat11",
                    "Texture-stage state is a floating-point value for the [1][1] coefficient in a bump-mapping matrix.",
                    true));
            EffectStates.Add(
                new StateCompletionData("ResultArg", "Selects the destination register for the result of this stage.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TexCoordIndex", "Index of the texture coordinate set to use with this texture stage.", true));
            EffectStates.Add(
                new StateCompletionData(
                    "TextureTransformFlags", "Controls the transformation of texture coordinates for this texture stage.", true));

            // Transform States
            EffectStates.Add(new StateCompletionData("ProjectionTransform", "The projection matrix. (float4x4)"));
            EffectStates.Add(
                new StateCompletionData(
                    "TextureTransform", "Transformation matrix set for the specified texture stage. (float4x4)", true));
            EffectStates.Add(new StateCompletionData("ViewTransform", "The view matrix. (float4x4)"));
            EffectStates.Add(new StateCompletionData("WorldTransform", "The world matrix. (float4x4)"));
        }


        /// <summary>
        /// Initializes the DirectX 9 sampler states.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InitializeSamplerStates()
        {
            // Sampler Stage States
            string[] textureAddressMode =
            {
                "WRAP", "MIRROR", "CLAMP", "BORDER", "MIRRORONCE"
            };
            SamplerStates.Add(
                new StateCompletionData("AddressU", "Texture-address mode for the u coordinate.", textureAddressMode));
            SamplerStates.Add(
                new StateCompletionData("AddressV", "Texture-address mode for the v coordinate.", textureAddressMode));
            SamplerStates.Add(
                new StateCompletionData("AddressW", "Texture-address mode for the w coordinate.", textureAddressMode));
            SamplerStates.Add(new StateCompletionData("BorderColor", "The texture border color as DWORD (0xAARRGGBB). The default value is 0x00000000."));
            string[] textureFilterType =
            {
                "POINT", "LINEAR", "ANISOTROPIC", "PYRAMIDALQUAD", "GAUSSIANQUAD", "CONVOLUTIONMONO"
            };
            SamplerStates.Add(new StateCompletionData("MagFilter", "Texture magnification filter.", textureFilterType));
            SamplerStates.Add(new StateCompletionData("MaxAnisotropy", "The maximum anisotropy."));
            SamplerStates.Add(
                new StateCompletionData(
                    "MaxMipLevel",
                    "level-of-detail index of largest map to use. Values range from 0 to (n - 1) where 0 is the largest."));
            SamplerStates.Add(new StateCompletionData("MinFilter", "Texture minification filter ", textureFilterType));
            string[] mipTextureFilterType =
            {
                "NONE", "POINT", "LINEAR"
            };
            SamplerStates.Add(
                new StateCompletionData("MipFilter", "Mipmap filter to use during minification", mipTextureFilterType));
            SamplerStates.Add(new StateCompletionData("MipMapLodBias", "Mipmap level-of-detail bias."));
            SamplerStates.Add(new StateCompletionData("SRGBTexture", "Gamma correction value."));
            SamplerStates.Add(new StateCompletionData("Texture", "Texture used by this sampler state."));
        }


        /// <summary>
        /// Initializes the values for DirectX Effect states.
        /// </summary>
        private void InitializeStateValues()
        {
            string[] stateValues =
            {
                "NULL", "TRUE", "FALSE",
                // Light State Values
                "POINT", "SPOT", "DIRECTIONAL",
                // Vertex Pipe Render State Values
                "MATERIAL", "COLOR1", "COLOR2", "CLIPPLANE0",
                "CLIPPLANE1", "CLIPPLANE2", "CLIPPLANE3", "CLIPPLANE4", "CLIPPLANE5",
                "NONE", "CW", "CCW", "EXP", "EXP2", "LINEAR", "DISABLE",
                "0WEIGHTS", "1WEIGHTS", "2WEIGHTS", "3WEIGHTS", "TWEENING",
                // Pixel Pipe Render State Values
                "NEVER", "LESS", "EQUAL", "LESSEQUAL", "GREATER", "NOTEQUAL", "GREATEREQUAL", "ALWAYS",
                "ADD", "SUBTRACT", "REVSUBTRACT", "MIN", "MAX",
                "RED", "GREEN", "BLUE", "ALPHA",
                "ZERO", "ONE", "SRCCOLOR", "INVSRCCOLOR", "SRCALPHA", "INVSRCALPHA",
                "DESTALPHA", "INVDESTALPHA", "DESTCOLOR", "INVDESTCOLOR", "SRCALPHASAT",
                "BOTHSRCALPHA", "BOTHINVSRCALPHA",
                "BLENDFACTOR", "INVBLENDFACTOR", "SRCCOLOR2", "INVSRCCOLOR2",
                "WIREFRAME", "SOLID",
                "FLAT", "GOURAUD", "PHONG",
                "KEEP", "REPLACE", "INCRSAT", "DECRSAT", "INVERT", "INCR", "DECR",
                "COORD0", "COORD1", "COORD2", "COORD3", "U", "V", "W",
                "USEW",
                // Sampler Stage State Values
                "WRAP", "MIRROR", "CLAMP", "BORDER", "MIRRORONCE",
                "ANISOTROPIC", "PYRAMIDALQUAD", "GAUSSIANQUAD", "CONVOLUTIONMONO",
                // Texture Stage State Values
                "SELECTARG1", "SELECTARG2",
                "MODULATE", "MODULATE2X", "MODULATE4X",
                "ADDSIGNED", "ADDSIGNED2X", "ADDSMOOTH",
                "BLENDDIFFUSEALPHA", "BLENDTEXTUREALPHA", "BLENDFACTORALPHA",
                "BLENDTEXTUREALPHAPM", "BLENDCURRENTALPHA",
                "PREMODULATE", "MODULATEALPHA_ADDCOLOR", "MODULATECOLOR_ADDALPHA",
                "MODULATEINVALPHA_ADDCOLOR", "MODULATEINVCOLOR_ADDALPHA",
                "BUMPENVMAP", "BUMPENVMAPLUMINANCE", "DOTPRODUCT3", "MULTIPLYADD",
                "LERP",
            };

            foreach (string value in stateValues)
                EffectStateValues.Add(new ConstantCompletionData(value));
        }


        /// <summary>
        /// Initializes the DirectX 10 Effect states.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void InitializeEffectStates10()
        {
            // Blend States
            string[] boolValues = { "TRUE", "FALSE" };
            BlendStates.Add(
                new StateCompletionData(
                    "AlphaToCoverageEnable", "Enables or disables alpha-to-coverage as a multisampling technique.", boolValues));
            BlendStates.Add(new StateCompletionData("BlendEnable", "Enables or disables blending.", boolValues, true));
            string[] blendOptions =
            {
                "ZERO", "ONE",
                "SRC_COLOR", "INV_SRC_COLOR", "SRC_ALPHA",
                "INV_SRC_ALPHA", "DEST_ALPHA", "INV_DEST_ALPHA",
                "DEST_COLOR", "INV_DEST_COLOR", "SRC_ALPHA_SAT",
                "BLEND_FACTOR", "INV_BLEND_FACTOR", "SRC1_COLOR",
                "INV_SRC1_COLOR", "SRC1_ALPHA", "INV_SRC1_ALPHA"
            };
            BlendStates.Add(
                new StateCompletionData(
                    "SrcBlend", "Specifies the first RGB data source and includes an optional pre-blend operation.", blendOptions));
            BlendStates.Add(
                new StateCompletionData(
                    "DestBlend",
                    "Specifies the second RGB data source and includes an optional pre-blend operation.",
                    blendOptions));
            string[] blendOperation =
            {
                "ADD", "SUBTRACT", "REV_SUBTRACT", "MIN", "MAX"
            };
            BlendStates.Add(new StateCompletionData("BlendOp", "Defines how to combine RGB data sources.", blendOperation));
            BlendStates.Add(
                new StateCompletionData(
                    "SrcBlendAlpha",
                    "Specifies the first alpha data source and includes an optional pre-blend operation.",
                    blendOptions));
            BlendStates.Add(
                new StateCompletionData(
                    "DestBlendAlpha",
                    "Specifies the second alpha data source and includes an optional pre-blend operation.",
                    blendOptions));
            BlendStates.Add(
                new StateCompletionData("BlendOpAlpha", "Defines how to combine alpha data sources.", blendOperation));
            BlendStates.Add(
                new StateCompletionData(
                    "RenderTargetWriteMask",
                    "A per-pixel write mask that allows control over which components can be written. (Bitwise combination.)",
                    new[] { "RED", "GREEN", "BLUE", "ALPHA", "ALL" },
                    true));

            // Depth and Stencil States
            DepthStencilStates.Add(new StateCompletionData("DepthEnable", "Enables or disables depth testing.", boolValues));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "DepthWriteMask",
                    "Identifies a portion of the depth-stencil buffer that can be modified by depth data.",
                    new[] { "ZERO", "ALL" }));
            string[] comparisonFunction =
            {
                "NEVER", "LESS", "EQUAL", "LESS_EQUAL", "GREATER", "NOT_EQUAL", "GREATER_EQUAL", "ALWAYS"
            };
            DepthStencilStates.Add(
                new StateCompletionData(
                    "DepthFunc", "A function that compares depth data against existing depth data.", comparisonFunction));
            DepthStencilStates.Add(new StateCompletionData("StencilEnable", "Enable stencil testing.", boolValues));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "StencilReadMask", "Identify a portion of the depth-stencil buffer for reading stencil data."));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "StencilWriteMask", "Identify a portion of the depth-stencil buffer for writing stencil data."));
            string[] stencilOperation =
            {
                "KEEP", "ZERO", "REPLACE", "INCR_SAT", "DECR_SAT", "INVERT", "INCR", "DECR"
            };
            DepthStencilStates.Add(
                new StateCompletionData(
                    "FrontFaceStencilFail",
                    "The stencil operation to perform when stencil testing fails for pixels whose surface normal is facing towards the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "FrontFaceStencilZFail",
                    "The stencil operation to perform when stencil testing passes and depth testing fails for pixels whose surface normal is facing towards the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "FrontFaceStencilPass",
                    "The stencil operation to perform when stencil testing and depth testing both pass for pixels whose surface normal is facing towards the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "FrontFaceStencilFunc",
                    "A function that compares stencil data against existing stencil data for pixels whose surface normal is facing towards the camera.",
                    comparisonFunction));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "BackFaceStencilFail",
                    "The stencil operation to perform when stencil testing fails for pixels whose surface normal is facing away from the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "BackFaceStencilZFail",
                    "The stencil operation to perform when stencil testing passes and depth testing fails for pixels whose surface normal is facing away from the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "BackFaceStencilPass",
                    "The stencil operation to perform when stencil testing and depth testing both pass for pixels whose surface normal is facing away from the camera.",
                    stencilOperation));
            DepthStencilStates.Add(
                new StateCompletionData(
                    "BackFaceStencilFunc",
                    "A function that compares stencil data against existing stencil data for pixels whose surface normal is facing away from the camera.",
                    comparisonFunction));

            // Rasterizer State
            RasterizerStates.Add(
                new StateCompletionData(
                    "FillMode", "The fill mode to use when rendering triangles.", new[] { "WIREFRAME", "SOLID" }));
            RasterizerStates.Add(
                new StateCompletionData(
                    "CullMode", "Removes triangles facing a particular direction.", new[] { "NONE", "FRONT", "BACK" }));
            RasterizerStates.Add(
                new StateCompletionData(
                    "FrontCounterClockwise", "Determines if a triangle is front- or back-facing.", boolValues));
            RasterizerStates.Add(new StateCompletionData("DepthBias", "Depth value added to a given pixel."));
            RasterizerStates.Add(new StateCompletionData("DepthBiasClamp", "Maximum depth bias of a pixel."));
            RasterizerStates.Add(new StateCompletionData("SlopeScaledDepthBias", "Scalar on a given pixel's slope."));
            RasterizerStates.Add(new StateCompletionData("ZClipEnable", "Enables clipping based on distance.", boolValues));
            RasterizerStates.Add(new StateCompletionData("ScissorEnable", "Enables scissor-rectangle culling.", boolValues));
            RasterizerStates.Add(
                new StateCompletionData("MultisampleEnable", "Enables multisample antialiasing.", boolValues));
            RasterizerStates.Add(
                new StateCompletionData(
                    "AntialiasedLineEnable",
                    "Enables line antialiasing; only applies if doing line drawing and MultisampleEnable is false.",
                    boolValues));

            // Sampler State
            string[] textureAddressMode =
            {
                "WRAP", "MIRROR", "CLAMP", "BORDER", "MIRROR_ONCE"
            };
            SamplerStates10.Add(
                new StateCompletionData(
                    "AddressU",
                    "Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.",
                    textureAddressMode));
            SamplerStates10.Add(
                new StateCompletionData(
                    "AddressV",
                    "Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.",
                    textureAddressMode));
            SamplerStates10.Add(
                new StateCompletionData(
                    "AddressW",
                    "Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.",
                    textureAddressMode));
            SamplerStates10.Add(
                new StateCompletionData(
                    "BorderColor", "Border color to use if is specified for AddressU, AddressV, or AddressW. The texture border color as float4. The default value is float4(0.0f, 0.0f, 0.0f, 0.0f)."));
                    string[] filter =
                    {
                        "MIN_MAG_MIP_POINT", "MIN_MAG_POINT_MIP_LINEAR", "MIN_POINT_MAG_LINEAR_MIP_POINT",
                        "MIN_POINT_MAG_MIP_LINEAR", "MIN_LINEAR_MAG_MIP_POINT", "MIN_LINEAR_MAG_POINT_MIP_LINEAR",
                        "MIN_MAG_LINEAR_MIP_POINT", "MIN_MAG_MIP_LINEAR", "ANISOTROPIC"
                    };
            SamplerStates10.Add(new StateCompletionData("Filter", "Filtering method to use when sampling a texture.", filter));
            SamplerStates10.Add(
                new StateCompletionData(
                    "MaxAnisotropy",
                    "Clamping value used if Filter is ANISOTROPIC or ComparisonFilter is COMPARISON_ANISOTROPIC. Valid values are between 1 and 16."));
            SamplerStates10.Add(
                new StateCompletionData(
                    "MaxLOD",
                    "Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed."));
            SamplerStates10.Add(
                new StateCompletionData(
                    "MinLOD",
                    "Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed."));
            SamplerStates10.Add(new StateCompletionData("MipLODBias", "Offset from the calculated mipmap level."));

            // Sampler-comparison state
            string[] comparisonFilter =
            {
                "COMPARISON_MIN_MAG_MIP_POINT", "COMPARISON_MIN_MAG_POINT_MIP_LINEAR",
                "COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT",
                "COMPARISON_MIN_POINT_MAG_MIP_LINEAR", "COMPARISON_MIN_LINEAR_MAG_MIP_POINT",
                "COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR",
                "COMPARISON_MIN_MAG_LINEAR_MIP_POINT", "COMPARISON_MIN_MAG_MIP_LINEAR", "COMPARISON_ANISOTROPIC"
            };
            SamplerStates10.Add(
                new StateCompletionData(
                    "ComparisonFunc", "A function that compares sampled data against existing sampled data.", comparisonFunction));
            SamplerStates10.Add(
                new StateCompletionData(
                    "ComparisonFilter",
                    "Filtering method to use when sampling a texture using a comparison-sampler.",
                    comparisonFilter));
        }


        /// <summary>
        /// Initializes the values for DirectX 10 Effect states.
        /// </summary>
        private void InitializeStateValues10()
        {
            string[] stateValues =
            {
                "SRC_COLOR", "INV_SRC_COLOR", "SRC_ALPHA",
                "INV_SRC_ALPHA", "DEST_ALPHA", "INV_DEST_ALPHA",
                "DEST_COLOR", "INV_DEST_COLOR", "SRC_ALPHA_SAT",
                "BLEND_FACTOR", "INV_BLEND_FACTOR", "SRC1_COLOR",
                "INV_SRC1_COLOR", "SRC1_ALPHA", "INV_SRC1_ALPHA",
                "REV_SUBTRACT",
                "ALL",
                "LESS_EQUAL", "NOT_EQUAL", "GREATER_EQUAL",
                "INCR_SAT", "DECR_SAT",
                "FRONT", "BACK",
                "MIRROR_ONCE",
                "MAX_ANISOTROPY",
                "MIN_MAG_MIP_POINT", "MIN_MAG_POINT_MIP_LINEAR", "MIN_POINT_MAG_LINEAR_MIP_POINT",
                "MIN_POINT_MAG_MIP_LINEAR", "MIN_LINEAR_MAG_MIP_POINT", "MIN_LINEAR_MAG_POINT_MIP_LINEAR",
                "MIN_MAG_LINEAR_MIP_POINT", "MIN_MAG_MIP_LINEAR",
                "COMPARISON_MIN_MAG_MIP_POINT", "COMPARISON_MIN_MAG_POINT_MIP_LINEAR",
                "COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT",
                "COMPARISON_MIN_POINT_MAG_MIP_LINEAR", "COMPARISON_MIN_LINEAR_MAG_MIP_POINT",
                "COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR",
                "COMPARISON_MIN_MAG_LINEAR_MIP_POINT", "COMPARISON_MIN_MAG_MIP_LINEAR", "COMPARISON_ANISOTROPIC",
            };

            foreach (string value in stateValues)
                EffectStateValues.Add(new ConstantCompletionData(value));
        }


        /// <summary>
        /// Validates the DirectX Effect states.
        /// </summary>
        [Conditional("DEBUG")]
        private void ValidateStates()
        {
            // Check whether all state values are specified.
            LookupConstants(EffectStates);
            LookupConstants(BlendStates);
            LookupConstants(DepthStencilStates);
            LookupConstants(RasterizerStates);
            LookupConstants(SamplerStates);
        }


        /// <summary>
        /// Looks up the values for the given DirectX Effect states.
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
