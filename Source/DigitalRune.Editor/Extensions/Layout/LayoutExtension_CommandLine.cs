// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.CommandLine;


namespace DigitalRune.Editor.Layout
{
    partial class LayoutExtension
    {
        private SwitchValueArgument<string> _layoutArgument;


        private void AddCommandLineArguments()
        {
            // Add command-line argument "--layout <layout_name>"
            _layoutArgument = new SwitchValueArgument<string>(
                "layout", 
                new ValueArgument<string>("name", "The name of the layout."),
                "Specifies the layout to open on startup.")
            {
                IsOptional = true,
                Category = "UI",
            };
            Editor.CommandLineParser.Arguments.Add(_layoutArgument);
        }


        private void RemoveCommandLineArguments()
        {
            Editor.CommandLineParser.Arguments.Remove(_layoutArgument);
        }


        private string GetLayoutNameFromCommandLine()
        {
            var parseResult = Editor.CommandLineResult;
            var layout = parseResult.ParsedArguments[_layoutArgument] as ArgumentResult<string>;
            return layout?.Values[0];
        }
    }
}
