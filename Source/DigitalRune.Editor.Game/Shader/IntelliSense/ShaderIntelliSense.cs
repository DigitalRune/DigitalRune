// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Windows.Themes;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Snippets;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Provides IntelliSense for shaders. (Base for HLSL and Cg.)
    /// </summary>
    internal abstract partial class ShaderIntelliSense
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly ShaderParser _parser;

        // Code completion data:
        // Entries that are only listed in a completion window are stored in Lists.
        // Entries that have to be looked up (to get method insight info or tooltip info) are stored 
        // in NamedObjectCollections.
        private HashSet<string> _fullLookupTable;

        // Members that cache completion data.
        private ICompletionData[] _globalCompletionData;
        private ICompletionData[] _preprocessorCompletionData;
        private ICompletionData[] _annotationCompletionData;
        private ICompletionData[] _codeCompletionData;
        private ICompletionData[] _memberCompletionData;
        private ICompletionData[] _techniqueCompletionData;
        private ICompletionData[] _blendStateCompletionData;
        private ICompletionData[] _depthStencilStateCompletionData;
        private ICompletionData[] _rasterizerStateCompletionData;
        private ICompletionData[] _samplerStateCompletionData;
        private ICompletionData[] _samplerState10CompletionData;
        private ICompletionData[] _stateBlockCompletionData;
        private ICompletionData[] _fullCompletionData;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        protected internal Dictionary<string, SnippetCompletionData> Snippets { get; }


        protected internal List<NamedCompletionData> PreprocessorDirectives { get; }


        protected internal NamedObjectCollection<NamedCompletionData> Keywords { get; }


        protected internal NamedObjectCollection<NamedCompletionData> Types { get; }


        protected internal NamedObjectCollection<NamedCompletionData> ScalarTypes { get; }


        protected internal NamedObjectCollection<NamedCompletionData> SpecialTypes { get; }


        protected internal NamedObjectCollection<NamedCompletionData> EffectTypes { get; }


        protected internal List<NamedCompletionData> Constants { get; }


        protected internal NamedObjectCollection<NamedCompletionData> Macros { get; private set; }


        protected internal NamedObjectCollection<NamedCompletionData> Functions { get; private set; }


        protected internal NamedObjectCollection<NamedCompletionData> Methods { get; }


        protected internal NamedObjectCollection<NamedCompletionData> EffectFunctions { get; }


        protected internal NamedObjectCollection<NamedCompletionData> EffectStates { get; }


        protected internal NamedObjectCollection<NamedCompletionData> BlendStates { get; }


        protected internal NamedObjectCollection<NamedCompletionData> DepthStencilStates { get; }


        protected internal NamedObjectCollection<NamedCompletionData> RasterizerStates { get; }


        protected internal NamedObjectCollection<NamedCompletionData> SamplerStates { get; }


        protected internal NamedObjectCollection<NamedCompletionData> SamplerStates10 { get; }


        protected internal NamedObjectCollection<NamedCompletionData> EffectStateValues { get; }


        internal protected HashSet<string> FullLookupTable
        {
            get
            {
                if (_fullLookupTable == null)
                {
                    _fullLookupTable = new HashSet<string>();
                    foreach (ICompletionData completionData in FullCompletionData)
                    {
                        if (!_fullLookupTable.Contains(completionData.Text))
                            _fullLookupTable.Add(completionData.Text);
                    }
                }
                return _fullLookupTable;
            }
        }


        private ICompletionData[] GlobalCompletionData
        {
            get
            {
                if (_globalCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        Keywords,
                        ScalarTypes,
                        Types,
                        SpecialTypes,
                        EffectTypes,
                        Macros,
                        Functions,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = { };
                    _globalCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _globalCompletionData;
            }
        }


        private ICompletionData[] PreprocessorCompletionData
        {
            get
            {
                if (_preprocessorCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = { PreprocessorDirectives, };
                    IEnumerable<NamedCompletionData>[] stateCategories = { };
                    _preprocessorCompletionData = BuildCompletionData(keywordCategories, stateCategories, false, false);
                }
                return _preprocessorCompletionData;
            }
        }


        private ICompletionData[] AnnotationCompletionData
        {
            get
            {
                if (_annotationCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = { ScalarTypes, };
                    IEnumerable<NamedCompletionData>[] stateCategories = { };
                    _annotationCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, false);
                }
                return _annotationCompletionData;
            }
        }


        private ICompletionData[] CodeCompletionData
        {
            get
            {
                if (_codeCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        Keywords,
                        ScalarTypes,
                        Types,
                        SpecialTypes,
                        Macros,
                        Functions,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = { };
                    _codeCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _codeCompletionData;
            }
        }


        private ICompletionData[] MemberCompletionData
        {
            get
            {
                if (_memberCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = { Methods, };
                    IEnumerable<NamedCompletionData>[] stateCategories = { };
                    _memberCompletionData = BuildCompletionData(keywordCategories, stateCategories, false, false);
                }
                return _memberCompletionData;
            }
        }


        private ICompletionData[] TechniqueCompletionData
        {
            get
            {
                if (_techniqueCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        Keywords,
                        ScalarTypes,
                        Types,
                        SpecialTypes,
                        EffectTypes,
                        Macros,
                        EffectFunctions,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        EffectStates,
                    };
                    _techniqueCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _techniqueCompletionData;
            }
        }


        private ICompletionData[] BlendStateCompletionData
        {
            get
            {
                if (_blendStateCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        BlendStates,
                    };
                    _blendStateCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _blendStateCompletionData;
            }
        }


        private ICompletionData[] DepthStencilStateCompletionData
        {
            get
            {
                if (_depthStencilStateCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        DepthStencilStates,
                    };
                    _depthStencilStateCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _depthStencilStateCompletionData;
            }
        }


        private ICompletionData[] RasterizerStateCompletionData
        {
            get
            {
                if (_rasterizerStateCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        RasterizerStates,
                    };
                    _rasterizerStateCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _rasterizerStateCompletionData;
            }
        }


        private ICompletionData[] SamplerStateCompletionData
        {
            get
            {
                if (_samplerStateCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        SamplerStates,
                    };
                    _samplerStateCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _samplerStateCompletionData;
            }
        }


        private ICompletionData[] SamplerState10CompletionData
        {
            get
            {
                if (_samplerState10CompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        SamplerStates10,
                    };
                    _samplerState10CompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _samplerState10CompletionData;
            }
        }


        private ICompletionData[] StateBlockCompletionData
        {
            get
            {
                if (_stateBlockCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        ScalarTypes,
                        Types,
                        Macros,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        EffectStates,
                    };
                    _stateBlockCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _stateBlockCompletionData;
            }
        }


        private ICompletionData[] FullCompletionData
        {
            get
            {
                if (_fullCompletionData == null)
                {
                    IEnumerable<NamedCompletionData>[] keywordCategories = 
                    {
                        PreprocessorDirectives,
                        Keywords,
                        ScalarTypes,
                        Types,
                        SpecialTypes,
                        EffectTypes,
                        Macros,
                        Functions,
                        Methods,
                        EffectFunctions,
                    };
                    IEnumerable<NamedCompletionData>[] stateCategories = 
                    {
                        EffectStates,
                        BlendStates,
                        DepthStencilStates,
                        RasterizerStates,
                        SamplerStates,
                        SamplerStates10,
                    };
                    _fullCompletionData = BuildCompletionData(keywordCategories, stateCategories, true, true);
                }

                return _fullCompletionData;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        protected ShaderIntelliSense()
        {
            EffectStateValues = new NamedObjectCollection<NamedCompletionData>();
            SamplerStates10 = new NamedObjectCollection<NamedCompletionData>();
            SamplerStates = new NamedObjectCollection<NamedCompletionData>();
            RasterizerStates = new NamedObjectCollection<NamedCompletionData>();
            DepthStencilStates = new NamedObjectCollection<NamedCompletionData>();
            BlendStates = new NamedObjectCollection<NamedCompletionData>();
            EffectStates = new NamedObjectCollection<NamedCompletionData>();
            EffectFunctions = new NamedObjectCollection<NamedCompletionData>();
            Methods = new NamedObjectCollection<NamedCompletionData>();
            Functions = new NamedObjectCollection<NamedCompletionData>();
            Macros = new NamedObjectCollection<NamedCompletionData>();
            Constants = new List<NamedCompletionData>();
            EffectTypes = new NamedObjectCollection<NamedCompletionData>();
            SpecialTypes = new NamedObjectCollection<NamedCompletionData>();
            ScalarTypes = new NamedObjectCollection<NamedCompletionData>();
            Types = new NamedObjectCollection<NamedCompletionData>();
            Keywords = new NamedObjectCollection<NamedCompletionData>();
            PreprocessorDirectives = new List<NamedCompletionData>();
            Snippets = new Dictionary<string, SnippetCompletionData>();

            // Initialize IntelliSense info that is identical in HLSL and Cg here.
            InitializeSnippets();
            InitializePreprocessorDirectives();
            InitializeKeywords();
            InitializeTypes();
            InitializeConstants();

            // Create shader parser
            _parser = new ShaderParser(this);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #region ----- IntelliSense Info -----

        /// <summary>
        /// Initializes the code snippets.
        /// </summary>
        private void InitializeSnippets()
        {
            // "for (|;;)\n{\n}"
            Snippet forSnippet = new Snippet();
            forSnippet.Elements.Add(new SnippetTextElement { Text = "for (" });
            forSnippet.Elements.Add(new SnippetCaretElement());
            forSnippet.Elements.Add(new SnippetTextElement { Text = ";;)\n{\n}" });

            // "if (|)\n{\n}"
            Snippet ifSnippet = new Snippet();
            ifSnippet.Elements.Add(new SnippetTextElement { Text = "if (" });
            ifSnippet.Elements.Add(new SnippetCaretElement());
            ifSnippet.Elements.Add(new SnippetTextElement { Text = ")\n{\n}" });

            // "while (|)\n{\n}"
            Snippet whileSnippet = new Snippet();
            whileSnippet.Elements.Add(new SnippetTextElement { Text = "while (" });
            whileSnippet.Elements.Add(new SnippetCaretElement());
            whileSnippet.Elements.Add(new SnippetTextElement { Text = ")\n{\n}" });

            // "technique |\n{\npass\n{\n}\n}"
            Snippet techniqueSnippet = new Snippet();
            techniqueSnippet.Elements.Add(new SnippetTextElement { Text = "technique " });
            techniqueSnippet.Elements.Add(new SnippetCaretElement());
            techniqueSnippet.Elements.Add(new SnippetTextElement { Text = "\n{\npass\n{\n}\n}" });

            // "pass |\n{\n}"
            Snippet passSnippet = new Snippet();
            passSnippet.Elements.Add(new SnippetTextElement { Text = "pass " });
            passSnippet.Elements.Add(new SnippetCaretElement());
            passSnippet.Elements.Add(new SnippetTextElement { Text = "\n{\n}" });


            SnippetCompletionData[] snippets = 
            {
                new SnippetCompletionData("for", "for loop", MultiColorGlyphs.Snippet, forSnippet),
                new SnippetCompletionData("if", "if statement", MultiColorGlyphs.Snippet, ifSnippet),
                new SnippetCompletionData("while", "while loop", MultiColorGlyphs.Snippet, whileSnippet),
                new SnippetCompletionData("technique", null, MultiColorGlyphs.Snippet, techniqueSnippet),
                new SnippetCompletionData("pass", null, MultiColorGlyphs.Snippet, passSnippet),
            };

            foreach (SnippetCompletionData snippet in snippets)
                Snippets.Add(snippet.Text, snippet);
        }


        /// <summary>
        /// Initializes the preprocessor directives.
        /// </summary>
        private void InitializePreprocessorDirectives()
        {
            string[] preprocessorDirectives =
            {
                "#define", "#elif", "#else", "#endif", "#error", "#if", "#ifdef", "#ifndef", "#include",
                "#line", "#undef",
            };
            foreach (string preprocessorDirective in preprocessorDirectives)
                PreprocessorDirectives.Add(new KeywordCompletionData(preprocessorDirective));
        }


        /// <summary>
        /// Initializes the keywords.
        /// </summary>
        private void InitializeKeywords()
        {
            string[] keywords =
            {
                "typedef",
                "const", "extern", "static", "uniform",
                "else", "if", "switch", "case", "default",
                "do", "for", "while", "break", "continue",
                "discard", "return",
                "inline",
                "in", "inout", "out",
                "vs_1_1", "vs_2_0", "vs_2_x", "vs_3_0",
                "ps_1_1", "ps_1_2", "ps_1_3", "ps_2_0", "ps_2_x", "ps_3_0",
            };
            foreach (string keyword in keywords)
                Keywords.Add(new KeywordCompletionData(keyword));
        }


        /// <summary>
        /// Initializes the types.
        /// </summary>
        private void InitializeTypes()
        {
            string[] scalarTypes =
            {
                "bool",
                "int",
                "half",
                "float",
                "double",
                "string", // Actually not a scalar type, but treated similarly.
            };
            foreach (string type in scalarTypes)
                ScalarTypes.Add(new TypeCompletionData(type));

            string[] types =
            {
                "bool1", "bool2", "bool3", "bool4",
                "bool1x1", "bool1x2", "bool1x3", "bool1x4",
                "bool2x1", "bool2x2", "bool2x3", "bool2x4",
                "bool3x1", "bool3x2", "bool3x3", "bool3x4",
                "bool4x1", "bool4x2", "bool4x3", "bool4x4",
                "int1", "int2", "int3", "int4",
                "int1x1", "int1x2", "int1x3", "int1x4",
                "int2x1", "int2x2", "int2x3", "int2x4",
                "int3x1", "int3x2", "int3x3", "int3x4",
                "int4x1", "int4x2", "int4x3", "int4x4",
                "half1", "half2", "half3", "half4",
                "half1x1", "half1x2", "half1x3", "half1x4",
                "half2x1", "half2x2", "half2x3", "half2x4",
                "half3x1", "half3x2", "half3x3", "half3x4",
                "half4x1", "half4x2", "half4x3", "half4x4",
                "float1", "float2", "float3", "float4",
                "float1x1", "float1x2", "float1x3", "float1x4",
                "float2x1", "float2x2", "float2x3", "float2x4",
                "float3x1", "float3x2", "float3x3", "float3x4",
                "float4x1", "float4x2", "float4x3", "float4x4",
                "double1", "double2", "double3", "double4",
                "double1x1", "double1x2", "double1x3", "double1x4",
                "double2x1", "double2x2", "double2x3", "double2x4",
                "double3x1", "double3x2", "double3x3", "double3x4",
                "double4x1", "double4x2", "double4x3", "double4x4",
            };
            foreach (string type in types)
                Types.Add(new TypeCompletionData(type));

            string[] specialTypes =
            {
                "void",
                "sampler", "sampler1D", "sampler2D", "sampler3D", "samplerCUBE", "sampler_state",
                "struct",
            };
            foreach (string type in specialTypes)
                SpecialTypes.Add(new TypeCompletionData(type));

            string[] effectTypes =
            {
                "technique", "pass",
                "asm",
            };
            foreach (string type in effectTypes)
                EffectTypes.Add(new TypeCompletionData(type));
        }


        /// <summary>
        /// Initializes the constants.
        /// </summary>
        private void InitializeConstants()
        {
            Constants.Add(new ConstantCompletionData("true"));
            Constants.Add(new ConstantCompletionData("false"));
        }
        #endregion


        /// <summary>
        /// Builds the completion data from the given list.
        /// </summary>
        /// <param name="keywordCategories">The keyword categories.</param>
        /// <param name="stateCategories">The state categories.</param>
        /// <param name="includeConstants"><c>true</c> to include constant in the completion data.</param>
        /// <param name="includeSnippets"><c>true</c> to include snippets in the completion data.</param>
        /// <returns></returns>
        private ICompletionData[] BuildCompletionData(IEnumerable<IEnumerable<NamedCompletionData>> keywordCategories, IEnumerable<IEnumerable<NamedCompletionData>> stateCategories, bool includeConstants, bool includeSnippets)
        {
            List<ICompletionData> completionData = new List<ICompletionData>();

            if (includeSnippets)
            {
                // Add Snippets
                foreach (SnippetCompletionData snippet in Snippets.Values)
                    completionData.Add(new SnippetCompletionData(snippet.Text, snippet.Description, MultiColorGlyphs.Snippet, snippet.Snippet));
            }

            // Add Keywords
            foreach (IEnumerable<NamedCompletionData> keywords in keywordCategories)
                foreach (NamedCompletionData keyword in keywords)
                    completionData.Add(keyword);

            // Add Effect States:
            // Different state groups can have the same states.
            // Merge them into a single NamedObjectCollection first, to avoid duplications.
            NamedObjectCollection<NamedCompletionData> stateCompletionData = new NamedObjectCollection<NamedCompletionData>();
            foreach (IEnumerable<NamedCompletionData> states in stateCategories)
                foreach (NamedCompletionData state in states)
                    if (!stateCompletionData.Contains(state.Name))
                        stateCompletionData.Add(state);

            foreach (NamedCompletionData state in stateCompletionData)
                completionData.Add(state);

            // Merge constants and state values
            NamedObjectCollection<NamedCompletionData> constants = new NamedObjectCollection<NamedCompletionData>();
            if (includeConstants)
                foreach (ConstantCompletionData constant in Constants)
                    constants.Add(constant);

            foreach (StateCompletionData state in stateCompletionData)
                foreach (string stateValue in state.AllowedValues)
                    if (!constants.Contains(stateValue))
                        constants.Add(EffectStateValues[stateValue]);

            foreach (NamedCompletionData constant in constants)
                completionData.Add(constant);

            return completionData.ToArray();
        }


        private static ICompletionData[] MergeCompletionData(params IEnumerable[] completionData)
        {
            List<ICompletionData> mergedCompletionData = new List<ICompletionData>();
            foreach (IEnumerable list in completionData)
                foreach (ICompletionData entry in list)
                    mergedCompletionData.Add(entry);

            return mergedCompletionData.ToArray();
        }


        /// <summary>
        /// Gets the lookup tables that provide the insight info.
        /// </summary>
        /// <param name="region">The shader region.</param>
        /// <returns>The lookup tables for the specified region.</returns>
        private NamedObjectCollection<NamedCompletionData>[] GetInsightLookupTables(ShaderRegion region)
        {
            NamedObjectCollection<NamedCompletionData>[] lookupTables;

            switch (region)
            {
                case ShaderRegion.Default:
                case ShaderRegion.Global:
                    lookupTables = new[] { Functions, Methods, EffectFunctions };
                    break;
                case ShaderRegion.StructureOrInterface:
                case ShaderRegion.Code:
                    lookupTables = new[] { Functions, Methods };
                    break;
                case ShaderRegion.TechniqueOrPass:
                case ShaderRegion.TechniqueOrPass10:
                    lookupTables = new[] { EffectFunctions };
                    break;
                default:
                    lookupTables = null;
                    break;
            }
            return lookupTables;
        }

        /// <summary>
        /// Gets the lookup-tables that provide the tooltip info.
        /// </summary>
        /// <param name="region">The region of the shader file.</param>
        /// <returns>The lookup-table containing the symbols for the given region.</returns>
        private NamedObjectCollection<NamedCompletionData>[] GetToolTipLookupTables(ShaderRegion region)
        {
            NamedObjectCollection<NamedCompletionData>[] lookupTables;

            switch (region)
            {
                case ShaderRegion.Default:
                case ShaderRegion.Global:
                    lookupTables = new[] { Functions, Methods, EffectFunctions, Macros };
                    break;

                case ShaderRegion.StructureOrInterface:
                case ShaderRegion.Code:
                    lookupTables = new[] { Functions, Methods, Macros };
                    break;

                case ShaderRegion.TechniqueOrPass:
                case ShaderRegion.TechniqueOrPass10:
                    lookupTables = new[] { EffectFunctions, EffectStates, Macros };
                    break;

                case ShaderRegion.BlendState10:
                    lookupTables = new[] { BlendStates, EffectStates, Macros };
                    break;

                case ShaderRegion.DepthStencilState10:
                    lookupTables = new[] { DepthStencilStates, EffectStates, Macros };
                    break;

                case ShaderRegion.RasterizerState10:
                    lookupTables = new[] { RasterizerStates, EffectStates, Macros };
                    break;

                case ShaderRegion.SamplerState:
                    lookupTables = new[] { SamplerStates, EffectStates, Macros };
                    break;

                case ShaderRegion.SamplerState10:
                    lookupTables = new[] { SamplerStates10, EffectStates, Macros };
                    break;

                case ShaderRegion.StateBlock:
                    lookupTables = new[] { EffectStates, Macros };
                    break;

                default:
                    lookupTables = null;
                    break;
            }
            return lookupTables;
        }
        #endregion
    }
}
