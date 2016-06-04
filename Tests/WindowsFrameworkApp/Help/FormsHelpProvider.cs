using DigitalRune.Windows.Framework;


namespace WindowsFrameworkApp
{
    /// <summary>
    /// Uses <see cref="Help"/> to display Help information.
    /// </summary>
    public class FormsHelpProvider : IHelpProvider
    {
        /// <overloads>
        /// <summary>
        /// Displays the contents of the Help file found at the specified URL.
        /// </summary>
        /// </overloads>
        /// <summary>
        /// Displays the contents of the Help file found at the specified URL.
        /// </summary>
        /// <param name="url">The path and name of the Help file.</param>
        /// <remarks>
        /// The <paramref name="url"/> parameter can be of the form C:\path\sample.chm or 
        /// /folder/file.htm.
        /// </remarks>
        public void ShowHelp(string url)
        {
            System.Windows.Forms.Help.ShowHelp(null, url);
        }


        /// <summary>
        /// Displays the contents of the Help file found at the specified URL for a specific keyword.
        /// </summary>
        /// <param name="url">The path and name of the Help file.</param>
        /// <param name="keyword">The keyword to display Help for.</param>
        /// <remarks>
        /// The <paramref name="url"/> parameter can be of the form C:\path\sample.chm or 
        /// /folder/file.htm. If you provide the keyword <see langword="null"/>, the table of contents 
        /// for the Help file will be displayed. 
        /// </remarks>
        public void ShowHelp(string url, string keyword)
        {
            System.Windows.Forms.Help.ShowHelp(null, url, keyword);
        }
    }
}
