using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        private static void CanPasteMultiple(object sender, CanExecuteRoutedEventArgs args)
        {
            var textEditor = (TextEditor)sender;
            var textArea = textEditor.TextArea;

            if (textArea?.Document == null)
                return;

            args.CanExecute = !ClipboardRing.IsEmpty && textArea.ReadOnlySectionProvider.CanInsert(textArea.Caret.Offset);
            args.Handled = true;
        }


        private static void OnPasteMultiple(object sender, ExecutedRoutedEventArgs args)
        {
            var textEditor = (TextEditor)sender;
            var textArea = textEditor.TextArea;

            if (textArea?.Document == null)
                return;

            var completionWindow = new CompletionWindow(textArea);

            // ----- Create completion list.
            var completionList = completionWindow.CompletionList;
            var completionData = completionList.CompletionData;
            var stringBuilder = new StringBuilder();
            foreach (var text in ClipboardRing.GetEntries())
            {
                // Replace special characters for display.
                stringBuilder.Clear();
                stringBuilder.Append(text);
                stringBuilder.Replace("\n", "\\n");
                stringBuilder.Replace("\r", "\\r");
                stringBuilder.Replace("\t", "\\t");

                // Use TextBlock for TextTrimming.
                var textBlock = new TextBlock
                {
                    Text = stringBuilder.ToString(),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 135
                };
                completionData.Add(new CompletionData(text, text, null, textBlock));
            }

            // ----- Show completion window.
            completionList.SelectedItem = completionData[0];
            completionWindow.Show();

            // ----- Handle InsertionRequested event.
            completionList.InsertionRequested += (s, e) =>
                                                 {
                                                     var ta = completionWindow.TextArea;
                                                     var cl = completionWindow.CompletionList;
                                                     var text = cl.SelectedItem.Text;

                                                     // Copy text into clipboard.
                                                     var data = new DataObject();
                                                     if (EditingCommandHandler.ConfirmDataFormat(ta, data, DataFormats.UnicodeText))
                                                         data.SetText(text);

                                                     var copyingEventArgs = new DataObjectCopyingEventArgs(data, false);
                                                     ta.RaiseEvent(copyingEventArgs);
                                                     if (copyingEventArgs.CommandCancelled)
                                                         return;

                                                     try
                                                     {
                                                         Clipboard.SetDataObject(data, true);
                                                     }
                                                     catch (ExternalException)
                                                     {
                                                     }

                                                     ClipboardRing.Add(text);
                                                 };
        }
    }
}
