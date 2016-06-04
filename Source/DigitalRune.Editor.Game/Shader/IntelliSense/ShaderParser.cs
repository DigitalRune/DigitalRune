// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using ICSharpCode.AvalonEdit.Document;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Parses a DirectX Effect or CgFX file.
    /// </summary>
    internal class ShaderParser
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly ShaderIntelliSense _intelliSense;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderParser"/> class.
        /// </summary>
        /// <param name="intelliSense">Te IntelliSense provider.</param>
        public ShaderParser(ShaderIntelliSense intelliSense)
        {
            if (intelliSense == null)
                throw new ArgumentNullException(nameof(intelliSense));

            _intelliSense = intelliSense;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        /// <summary>
        /// Gets the techniques and passes of the specified effect.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A list of all techniques and their passes.</returns>
        public List<Tuple<string, List<string>>> GetTechniquesAndPasses(ITextSource document)
        {
            List<Tuple<string, List<string>>> techniquesAndPasses = new List<Tuple<string, List<string>>>();
            string technique = null;       // The name of the current technique.
            bool techniqueKeywordFound = false;   // true if last token was keyword 'technique'.
            int offset = 0;

            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '/':
                        // Skip comments
                        if (offset + 1 < document.TextLength)
                        {
                            char nextChar = document.GetCharAt(offset + 1);
                            if (nextChar == '/')
                            {
                                // Line comment
                                offset = SkipLineComment(document, offset + 2);
                            }
                            else if (nextChar == '*')
                            {
                                // Block comment
                                offset = SkipBlockComment(document, offset + 2);
                            }
                            else
                            {
                                // No comment
                                ++offset;
                            }
                        }
                        else
                        {
                            // End of file -> Skip past end to terminate algorithm.
                            ++offset;
                        }
                        break;
                    case '"':
                        // Skip strings 
                        offset = SkipString(document, offset + 1);
                        techniqueKeywordFound = false;
                        technique = null;
                        break;
                    case '\'':
                        // Skip character literals
                        offset = SkipCharacterLiteral(document, offset + 1);
                        techniqueKeywordFound = false;
                        technique = null;
                        break;
                    case '{':
                        // Find passes or skip block.
                        int startOffset = offset;
                        offset = TextUtilities.FindClosingBracket(document, offset + 1, '{', '}');
                        if (offset == -1)
                        {
                            // Unmatched bracket. Skip past end to terminate algorithm.
                            offset = document.TextLength;
                        }
                        else if (technique != null)
                        {
                            // Descend and get passes.
                            var passes = GetPasses(document, startOffset + 1, offset);
                            techniquesAndPasses.Add(Tuple.Create(technique, passes));
                        }
                        else
                        {
                            ++offset;
                        }
                        techniqueKeywordFound = false;
                        technique = null;
                        break;
                    case '<':
                        // Check whether this is an annotation
                        if (IsStartOfAnnotation(document, offset))
                        {
                            // Skip annotation.
                            offset = TextUtilities.FindClosingBracket(document, offset + 1, '<', '>');
                            if (offset == -1)
                                offset = document.TextLength;
                            else
                                ++offset;
                        }
                        else
                        {
                            ++offset;
                        }
                        techniqueKeywordFound = false;
                        technique = null;
                        break;
                    default:
                        if (char.IsLetter(c) || c == '_')
                        {
                            string identifier = TextUtilities.GetIdentifierAt(document, offset);
                            if (techniqueKeywordFound)
                            {
                                // The previous token was the keyword 'technique'.
                                if (!string.IsNullOrWhiteSpace(identifier))
                                {
                                    // Technique name found.
                                    technique = identifier;
                                }

                                techniqueKeywordFound = false;
                            }
                            else if (identifier == "technique"
                                     || identifier == "Technique"
                                     || identifier == "technique10")
                            {
                                // The next identifier will be the technique name.
                                techniqueKeywordFound = true;
                                technique = null;
                            }

                            offset += identifier.Length;
                        }
                        else if (char.IsDigit(c))
                        {
                            offset = SkipNumber(document, offset);
                            techniqueKeywordFound = false;
                            technique = null;
                        }
                        else
                        {
                            ++offset;
                        }
                        break;
                }
            }

            return techniquesAndPasses;
        }


        private List<string> GetPasses(ITextSource document, int startOffset, int endOffset)
        {
            List<string> passes = new List<string>();
            bool passKeywordFound = false;   // true if last token was the keyword 'pass'.
            int offset = startOffset;

            while (offset < document.TextLength && offset < endOffset)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '/':
                        // Skip comments
                        if (offset + 1 < document.TextLength)
                        {
                            char nextChar = document.GetCharAt(offset + 1);
                            if (nextChar == '/')
                            {
                                // Line comment
                                offset = SkipLineComment(document, offset + 2);
                            }
                            else if (nextChar == '*')
                            {
                                // Block comment
                                offset = SkipBlockComment(document, offset + 2);
                            }
                            else
                            {
                                // No comment
                                ++offset;
                            }
                        }
                        else
                        {
                            // End of file -> Skip past end to terminate algorithm.
                            ++offset;
                        }
                        break;
                    case '"':
                        // Skip strings 
                        offset = SkipString(document, offset + 1);
                        passKeywordFound = false;
                        break;
                    case '\'':
                        // Skip character literals
                        offset = SkipCharacterLiteral(document, offset + 1);
                        passKeywordFound = false;
                        break;
                    case '{':
                        // Skip block.
                        offset = TextUtilities.FindClosingBracket(document, offset + 1, '{', '}');
                        if (offset == -1)
                            offset = document.TextLength;
                        else
                            ++offset;

                        passKeywordFound = false;
                        break;
                    case '<':
                        // Check whether this is an annotation
                        if (IsStartOfAnnotation(document, offset))
                        {
                            // Skip annotation.
                            offset = TextUtilities.FindClosingBracket(document, offset + 1, '<', '>');
                            if (offset == -1)
                                offset = document.TextLength;
                            else
                                ++offset;
                        }
                        else
                        {
                            ++offset;
                        }
                        passKeywordFound = false;
                        break;
                    default:
                        if (char.IsLetter(c) || c == '_')
                        {
                            string identifier = TextUtilities.GetIdentifierAt(document, offset);
                            if (passKeywordFound)
                            {
                                // The previous token was the keyword 'pass'.
                                if (!string.IsNullOrWhiteSpace(identifier))
                                    passes.Add(identifier);

                                passKeywordFound = false;
                            }
                            else if (identifier == "pass" || identifier == "Pass")
                            {
                                passKeywordFound = true;
                            }

                            offset += identifier.Length;
                        }
                        else if (char.IsDigit(c))
                        {
                            offset = SkipNumber(document, offset);
                            passKeywordFound = false;
                        }
                        else
                        {
                            ++offset;
                        }
                        break;
                }
            }

            return passes;
        }


        /// <summary>
        /// Identifies the source code region at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="targetOffset">The target offset where the region shall be evaluated.</param>
        /// <returns>The identified source code region.</returns>
        public ShaderRegion IdentifyRegion(ITextSource document, int targetOffset)
        {
            // Parse file from start (offset == 0).
            ShaderRegion region = IdentifyRegion(document, 0, targetOffset, null, null);
            return (region == ShaderRegion.Default) ? ShaderRegion.Global : region;
        }


        /// <summary>
        /// Identifies the source code region at the given offset and collect all identifiers up to this 
        /// offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="targetOffset">The target offset where the region shall be evaluated.</param>
        /// <param name="collectedIdentifiers">The collected identifiers.</param>
        /// <param name="collectedFields">The collected field identifiers.</param>
        /// <returns>The identified source code region.</returns>
        /// <remarks>
        /// The method returns the identified source code region. While parsing the source code up to 
        /// <paramref name="targetOffset"/> it collects all unknown identifiers in 
        /// <paramref name="collectedIdentifiers"/> and all identifiers that look like fields of structs 
        /// in <paramref name="collectedFields"/>.
        /// </remarks>
        public ShaderRegion IdentifyRegion(ITextSource document, int targetOffset, out IList<NamedCompletionData> collectedIdentifiers, out IList<NamedCompletionData> collectedFields)
        {
            NamedObjectCollection<NamedCompletionData> identifiers = new NamedObjectCollection<NamedCompletionData>();
            NamedObjectCollection<NamedCompletionData> fields = new NamedObjectCollection<NamedCompletionData>();

            // Parse file from start (offset == 0).
            ShaderRegion region = IdentifyRegion(document, 0, targetOffset, identifiers, fields);
            collectedIdentifiers = identifiers;
            collectedFields = fields;
            return (region == ShaderRegion.Default) ? ShaderRegion.Global : region;
        }


        /// <summary>
        /// Identifies the source code region at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="startOffset">The start offset where to start the search.</param>
        /// <param name="targetOffset">The target offset where the region shall be evaluated.</param>
        /// <param name="identifiers">The collected identifiers.</param>
        /// <param name="fields">The collected field identifiers.</param>
        /// <returns>
        /// The method returns the identified source code region.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If there is no region at <paramref name="targetOffset"/> the method returns
        /// <see cref="ShaderRegion.Default"/>. In this the caller needs to decide what region it
        /// actually is. (When <see cref="IdentifyRegion(ITextSource,int,int,NamedObjectCollection{NamedCompletionData},NamedObjectCollection{NamedCompletionData})"/>
        /// is called with a <paramref name="startOffset"/> of 0 then it is <see cref="ShaderRegion.Global"/>.)
        /// </para>
        /// <para>
        /// This method calls itself recursively. First identifies the outermost region, then it
        /// recursively refines the search and returns the innermost region.
        /// </para>
        /// </remarks>
        private ShaderRegion IdentifyRegion(ITextSource document, int startOffset, int targetOffset, NamedObjectCollection<NamedCompletionData> identifiers, NamedObjectCollection<NamedCompletionData> fields)
        {
            int offset = startOffset;
            bool collectIdentifiers = (identifiers != null);

            while (offset < document.TextLength && offset < targetOffset)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '/':
                        // Skip comments
                        if (offset + 1 < document.TextLength)
                        {
                            char nextChar = document.GetCharAt(offset + 1);
                            if (nextChar == '/')
                            {
                                // Line comment
                                offset = SkipLineComment(document, offset + 2);
                                if (targetOffset <= offset)
                                    return ShaderRegion.LineComment;
                            }
                            else if (nextChar == '*')
                            {
                                // Block comment
                                offset = SkipBlockComment(document, offset + 2);
                                if (targetOffset < offset)
                                    return ShaderRegion.BlockComment;
                            }
                            else
                            {
                                // No comment
                                ++offset;
                            }
                        }
                        else
                        {
                            // End of file -> Skip past end to terminate algorithm.
                            ++offset;
                        }
                        break;
                    case '"':
                        // Skip strings 
                        offset = SkipString(document, offset + 1);
                        if (targetOffset < offset)
                            return ShaderRegion.String;
                        break;
                    case '\'':
                        // Skip character literals
                        offset = SkipCharacterLiteral(document, offset + 1);
                        if (targetOffset < offset)
                            return ShaderRegion.CharacterLiteral;
                        break;
                    case '{':
                        // Identify the current block
                        int startOffsetOfBlock = offset;
                        ShaderRegion region = IdentifyBlockAt(document, offset);
                        offset = TextUtilities.FindClosingBracket(document, offset + 1, '{', '}');
                        if (offset == -1 || targetOffset < offset)
                        {
                            // Let's identify the region inside this block. (Recursion!)
                            ShaderRegion innerRegion = IdentifyRegion(document, startOffsetOfBlock + 1, targetOffset, identifiers, fields);
                            if (region == ShaderRegion.TechniqueOrPass10 && innerRegion == ShaderRegion.TechniqueOrPass)
                            {
                                // Return the more specific
                                return ShaderRegion.TechniqueOrPass10;
                            }
                            if (innerRegion == ShaderRegion.Default || innerRegion == ShaderRegion.Unknown)
                            {
                                // The inner region is unknown or same as outer region
                                return region;
                            }
                            // Return the more specific inner region.
                            return innerRegion;
                        }
                        ++offset;
                        break;
                    case '<':
                        // Check whether this is an annotation
                        if (IsStartOfAnnotation(document, offset))
                        {
                            int startOffsetOfAnnotation = offset;
                            offset = TextUtilities.FindClosingBracket(document, offset + 1, '<', '>');
                            if (offset == -1 || targetOffset <= offset)
                            {
                                // Let's identify the region inside the annotation. (Recursion!)
                                ShaderRegion innerRegion = IdentifyRegion(document, startOffsetOfAnnotation + 1, targetOffset, identifiers, fields);
                                if (innerRegion == ShaderRegion.Default || innerRegion == ShaderRegion.Unknown)
                                {
                                    // The inner region is unknown or same as outer region
                                    return ShaderRegion.Annotation;
                                }

                                // Return the more specific inner region.
                                return innerRegion;
                            }
                            ++offset;
                        }
                        else
                        {
                            ++offset;
                        }
                        break;
                    default:
                        if (Char.IsLetter(c) || c == '_')
                        {
                            if (collectIdentifiers)
                            {
                                string identifier = TextUtilities.GetIdentifierAt(document, offset);
                                if (!String.IsNullOrEmpty(identifier) && !_intelliSense.FullLookupTable.Contains(identifier))
                                {
                                    if (offset > 0 && document.GetCharAt(offset - 1) == '.')
                                    {
                                        if (!fields.Contains(identifier))
                                            fields.Add(new GuessCompletionData(identifier));
                                    }
                                    else
                                    {
                                        if (!identifiers.Contains(identifier))
                                            identifiers.Add(new GuessCompletionData(identifier));
                                    }
                                }
                            }
                            offset = SkipIdentifier(document, offset);
                        }
                        else if (Char.IsDigit(c))
                        {
                            offset = SkipNumber(document, offset);
                            if (targetOffset <= offset)
                            {
                                return ShaderRegion.Default;
                            }
                        }
                        else
                        {
                            ++offset;
                        }
                        break;
                }
            }
            return ShaderRegion.Default;
        }


        private static int SkipLineComment(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                if (c == '\r' || c == '\n')
                    return offset + 1;

                ++offset;
            }
            return offset;
        }


        private static int SkipBlockComment(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                if (c == '/' && document.GetCharAt(offset - 1) == '*')
                    return offset + 1;

                ++offset;
            }
            return offset;
        }


        private static int SkipCharacterLiteral(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '\'':
                        return offset + 1;
                    case '\\':
                        ++offset;  // Skip next character
                        break;
                }
                ++offset;
            }
            return offset;
        }


        private static int SkipString(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '\r':
                    case '\n':
                    case '"':
                        return offset + 1;
                    case '\\':
                        ++offset;  // Skip next character
                        break;
                }
                ++offset;
            }
            return offset;
        }


        private static int SkipIdentifier(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                if (TextUtilities.GetCharacterClass(c) != CharacterClass.IdentifierPart)
                    return offset;

                ++offset;
            }
            return offset;
        }


        private static int SkipNumber(ITextSource document, int offset)
        {
            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                if (!Char.IsLetterOrDigit(c))
                    return offset + 1;

                ++offset;
            }
            return offset;
        }


        /// <summary>
        /// Skips the white space backwards.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the first character to check.</param>
        /// <returns>The offset of the first non-whitespace character before <paramref name="offset"/>.</returns>
        public int SkipWhiteSpaceBackwards(ITextSource document, int offset)
        {
            while (offset >= 1 && Char.IsWhiteSpace(document.GetCharAt(offset)))
                --offset;

            return offset;
        }


        /// <summary>
        /// Determines whether offset is the start of an annotation.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the opening bracket '&lt;'.</param>
        /// <returns>
        /// <c>true</c> if an annotation start at the current offset; otherwise, <c>false</c>.
        /// </returns>
        private bool IsStartOfAnnotation(ITextSource document, int offset)
        {
            if (offset >= document.TextLength)
                return false;

            // The syntax for annotations looks like this:
            //   type identifier < annotation >;
            //   type identifier : SEMANTIC < annotation >;
            //   struct identifier < annotation > { ... }
            //   technique [identifier] < annotation > { ... }
            //   pass [identifier] < annotation > { ... }

            // Search backwards for identifier and keyword

            // Skip whitespaces
            --offset;
            offset = SkipWhiteSpaceBackwards(document, offset);

            string token = TextUtilities.GetIdentifierAt(document, offset);
            if (string.IsNullOrEmpty(token))
            {
                // This is something else
                return false;
            }

            if (IsType(token))
            {
                // This should be an annotation.
                return true;
            }

            // token is either a type, identifier or a semantic
            offset -= token.Length;
            offset = SkipWhiteSpaceBackwards(document, offset);

            if (document.GetCharAt(offset) == ':')
            {
                // token is a semantic. Skip the ':' and get identifier.
                --offset;
                offset = SkipWhiteSpaceBackwards(document, offset);
                token = TextUtilities.GetIdentifierAt(document, offset);

                offset -= token.Length;
                offset = SkipWhiteSpaceBackwards(document, offset);
            }

            // token is either an identifier or a type
            if (IsType(token))
            {
                // This should be an annotation.
                return true;
            }

            // token is an identifier.
            // Get the previous word.
            token = TextUtilities.GetIdentifierAt(document, offset);

            // Now, if token is a type we have an annotation here.
            return IsType(token);
        }


        /// <summary>
        /// Identifies the block (technique, pass, state block, struct, etc.) that starts at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset the opening brace '{'.</param>
        /// <returns>
        /// The type of the current block; <see cref="ShaderRegion.Default"/> is returned if
        /// this is a default block (shader code, method, ...).
        /// </returns>
        private ShaderRegion IdentifyBlockAt(ITextSource document, int offset)
        {
            if (offset >= document.TextLength)
                return ShaderRegion.Default;

            // Search backwards, skip whitespace.
            --offset;
            offset = SkipWhiteSpaceBackwards(document, offset);

            if (offset < 0)
                return ShaderRegion.Default;

            if (document.GetCharAt(offset) == '>')
            {
                // Skip annotation
                offset = TextUtilities.FindOpeningBracket(document, offset - 1, '<', '>') - 1;
                offset = SkipWhiteSpaceBackwards(document, offset);
            }

            if (offset < 0)
                return ShaderRegion.Default;

            if (document.GetCharAt(offset) == ')')
            {
                // This is most likely a function.
                return ShaderRegion.Code;
            }

            if (document.GetCharAt(offset) == ']')
            {
                // This could be a state group. 
                // Some state groups, such as SamplerState, can have indices.
                offset = TextUtilities.FindOpeningBracket(document, offset - 1, '[', ']') - 1;
                offset = SkipWhiteSpaceBackwards(document, offset);
            }

            string token = TextUtilities.GetIdentifierAt(document, offset);
            if (String.IsNullOrEmpty(token))
            {
                // Start of an unknown block
                return ShaderRegion.Unknown;
            }

            if (!_intelliSense.Keywords.Contains(token) && !IsType(token))
            {
                // This is not a type, it is either an identifier or a semantic.

                // Skip word
                offset -= token.Length;

                if (offset < 0)
                    return ShaderRegion.Unknown;

                // Skip whitespaces
                offset = SkipWhiteSpaceBackwards(document, offset);

                // Check previous word
                if (document.GetCharAt(offset) == ':')
                {
                    // token is semantic.
                    // The only block that can have a semantic is function:
                    //   "MyFunction() : OUTPUTSEMANTIC { ... }"
                    return ShaderRegion.Code;
                }

                // token is an identifier.
                // Get the word before the identifier.
                token = TextUtilities.GetIdentifierAt(document, offset);
                if (!IsType(token))
                    return ShaderRegion.Unknown;
            }

            // token now holds a type.
            // Check the type to identify the region.
            if (token == "struct"
                || token == "interface"
                || token == "cbuffer"
                || token == "tbuffer")
            {
                return ShaderRegion.StructureOrInterface;
            }
            if (token == "technique"
                || token == "Technique"
                || token == "technique10"
                || token == "Technique10"
                || token == "pass"
                || token == "pass")
            {
                return ShaderRegion.TechniqueOrPass;
            }
            if (token == "sampler_state")
            {
                return ShaderRegion.SamplerState;
            }
            if (token == "SamplerState"
                || token == "SamplerComparisonState"
                || token == "sampler"
                || token == "sampler1D"
                || token == "sampler2D"
                || token == "sampler3D"
                || token == "samplerCUBE")
            {
                return ShaderRegion.SamplerState10;
            }
            if (token == "StateBlock"
                || token == "stateblock_state")
            {
                return ShaderRegion.StateBlock;
            }
            if (token == "BlendState")
            {
                return ShaderRegion.BlendState10;
            }
            if (token == "DepthStencilState")
            {
                return ShaderRegion.DepthStencilState10;
            }
            if (token == "RasterizerState")
            {
                return ShaderRegion.RasterizerState10;
            }
            if (token == "asm")
            {
                return ShaderRegion.Assembler;
            }
            return ShaderRegion.Unknown;
        }


        public bool IsType(string symbol)
        {
            if (String.IsNullOrEmpty(symbol))
                return false;

            return _intelliSense.ScalarTypes.Contains(symbol)
                    || _intelliSense.Types.Contains(symbol)
                    || _intelliSense.SpecialTypes.Contains(symbol)
                    || _intelliSense.EffectTypes.Contains(symbol)
                    || symbol == "Technique"          // This is just a simple fix for older FX files: 
                    || symbol == "Technique10"        // Some older FX files use a capital first letter
                    || symbol == "Pass";              // instead of the lower-case keywords.
        }


        public bool IsStateGroup(ShaderRegion region)
        {
            switch (region)
            {
                case ShaderRegion.SamplerState:
                case ShaderRegion.StateBlock:
                case ShaderRegion.TechniqueOrPass:
                case ShaderRegion.BlendState10:
                case ShaderRegion.DepthStencilState10:
                case ShaderRegion.RasterizerState10:
                case ShaderRegion.SamplerState10:
                case ShaderRegion.TechniqueOrPass10:
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}
