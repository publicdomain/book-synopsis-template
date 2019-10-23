// <copyright file="ViewHtmlForm.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace BookSynopsisTemplate
{
    // Directives
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// View html form.
    /// </summary>
    public partial class ViewHtmlForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BookSynopsisTemplate.ViewHtmlForm"/> class.
        /// </summary>
        /// <param name="htmlString">Html string.</param>
        public ViewHtmlForm(string htmlString)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();

            // Set HTML into text box
            this.htmlTextBox.Text = htmlString;
        }
    }
}
