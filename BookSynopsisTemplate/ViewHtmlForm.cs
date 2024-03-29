﻿// <copyright file="ViewHtmlForm.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace BookSynopsisTemplate
{
    // Directives
    using System;
    using System.Drawing;
    using System.IO;
    using System.Text;
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

            // Hidden cancel button for closing when hitting escape
            var cancelButton = new Button
            {
                Size = new Size(0, 0),
                TabStop = false
            };
            this.Controls.Add(cancelButton);
            this.CancelButton = cancelButton;

            // Set HTML into text box
            this.htmlTextBox.Text = htmlString;
            this.htmlTextBox.SelectionStart = 0;
        }

        /// <summary>
        /// Handles the save tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open save dialog
            if (this.saveHtmlFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Use file stream
                using (FileStream fileStream = (FileStream)this.saveHtmlFileDialog.OpenFile())
                {
                    // Use stream writer
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        // Write to disk
                        streamWriter.Write(this.htmlTextBox.Text);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the exit tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Close dialog
            this.Close();
        }

        /// <summary>
        /// Handles the copy tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnCopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Copy text to clipboard
            Clipboard.SetText(this.htmlTextBox.Text);
        }

        /// <summary>
        /// Handles the select all tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSelectAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Select all on text box
            this.htmlTextBox.SelectAll();
        }
    }
}
