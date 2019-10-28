// <copyright file="MainForm.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace BookSynopsisTemplate
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <summary>
    /// Main form class.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The template string.
        /// </summary>
        private string templateString;

        /// <summary>
        /// The entry html.
        /// </summary>
        private string entryHtml;

        /// <summary>
        /// The entry info dictionary.
        /// </summary>
        private Dictionary<string, EntryInfo> entryInfoDictionary = new Dictionary<string, EntryInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BookSynopsisTemplate.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the main form load event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMainFormLoad(object sender, EventArgs e)
        {
            // Require template file
            if (!File.Exists("template.txt"))
            {
                // Advise user
                MessageBox.Show($"Template file missing!{Environment.NewLine}{Environment.NewLine}Please add \"template.txt\" to app folder.", "Template required", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                // Exit program
                this.Close();
            }
            else
            {
                // Set initial template string
                this.templateString = File.ReadAllText("template.txt");

                // Set regex
                var regex = new Regex(@"<!-- entry-begin -->(.+)<!-- entry-end -->", RegexOptions.Singleline);

                // Grab entry HTML
                this.entryHtml = regex.Match(this.templateString).Groups[1].Value;

                // Set processed template string
                this.templateString = regex.Replace(this.templateString, "[ENTRIES-HTML]");
            }
        }

        /// <summary>
        /// Handles the add button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAddButtonClick(object sender, EventArgs e)
        {
            /* Check fields */

            // Set missing fields string
            string missingFieldString = $"{(this.characterTextBox.TextLength == 0 ? $"- Character.{Environment.NewLine}" : string.Empty)}" +
                $"{(this.bookIdTextBox.TextLength == 0 ? $"- Book ID.{Environment.NewLine}" : string.Empty)}" +
                $"{(this.bookTitleTextBox.TextLength == 0 ? $"- Book fitle.{Environment.NewLine}" : string.Empty)}" +
                $"{(this.bookOpenFileDialog.FileName.Length == 0 ? $"- Book file.{Environment.NewLine}" : string.Empty)}" +
                $"{(this.imageOpenFileDialog.FileName.Length == 0 ? $"- Book image.{Environment.NewLine}" : string.Empty)}" +
                $"{(this.synopsisTextBox.TextLength == 0 ? $"- Synopsis.{Environment.NewLine}" : string.Empty)}";

            // Check missing fields string length
            if (missingFieldString.Length > 0)
            {
                // Advise user
                MessageBox.Show($"Please add the following:{Environment.NewLine}{Environment.NewLine}{missingFieldString}", "Missing fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Halt flow
                return;
            }

            /* Process new entry */

            // Set book ID
            string bookID = this.bookIdTextBox.Text;

            // Set new entry
            var entryInfo = new EntryInfo(this.characterTextBox.Text)
            {
                ID = bookID,
                Title = this.bookTitleTextBox.Text,
                FilePath = this.bookOpenFileDialog.FileName,
                ImagePath = this.imageOpenFileDialog.FileName,
                Synopsys = this.synopsisTextBox.Text,
                HTML = this.entryHtml
            };

            // Check it's not present in dictionary
            if (!this.entryInfoDictionary.ContainsKey(bookID))
            {
                // Add new entry
                this.entryInfoDictionary.Add(bookID, entryInfo);

                // Add to combo box
                this.removeEntryComboBox.Items.Add(bookID);
            }
            else
            {
                // Update entry (a.k.a. edit)
                this.entryInfoDictionary[bookID] = entryInfo;
            }

            /* Post-addition routine */

            // Replace character, once
            if (this.characterTextBox.Enabled)
            {
                // Change in template string
                this.templateString = this.templateString.Replace("[CHARACTER-NAME]", this.characterTextBox.Text);

                // Lock character text box
                this.characterTextBox.Enabled = false;
            }

            // Make other replacements
            this.entryInfoDictionary[bookID].HTML = this.entryInfoDictionary[bookID].HTML.Replace("[BOOK-ID]", bookID);
            this.entryInfoDictionary[bookID].HTML = this.entryInfoDictionary[bookID].HTML.Replace("[BOOK-TITLE]", this.bookTitleTextBox.Text);
            this.entryInfoDictionary[bookID].HTML = this.entryInfoDictionary[bookID].HTML.Replace("[FILE-PATH]", this.relativePathsToolStripMenuItem.Checked ? this.GetRelativeUriFromApp(this.bookOpenFileDialog.FileName) : new Uri(this.bookOpenFileDialog.FileName).AbsoluteUri);
            this.entryInfoDictionary[bookID].HTML = this.entryInfoDictionary[bookID].HTML.Replace("[IMAGE-PATH]", this.relativePathsToolStripMenuItem.Checked ? this.GetRelativeUriFromApp(this.imageOpenFileDialog.FileName) : new Uri(this.imageOpenFileDialog.FileName).AbsoluteUri);
            this.entryInfoDictionary[bookID].HTML = this.entryInfoDictionary[bookID].HTML.Replace("[BOOK-SYNOPSIS]", this.synopsisTextBox.Text);

            // Check if must clear 
            if (this.clearOnAddToolStripMenuItem.Checked)
            {
                // Clear pertinent fields
                this.ClearFields();
            }

            // Copy to clipboard
            if (this.copyOnAddToolStripMenuItem.Checked)
            {
                // Set generated HTML into clipboard
                Clipboard.SetText(this.GetHtml());
            }

            // Update entry count
            this.UpdateStatus();
        }

        /// <summary>
        /// Updates the status.
        /// </summary>
        private void UpdateStatus()
        {
            // Display entry count
            this.mainToolStripStatusLabel.Text = $"Entry count: {this.removeEntryComboBox.Items.Count}";
        }

        /// <summary>
        /// Clears the fields.
        /// </summary>
        private void ClearFields()
        {
            // Text boxes
            this.bookIdTextBox.Clear();
            this.bookTitleTextBox.Clear();
            this.synopsisTextBox.Clear();

            // Open file dialogs
            this.bookOpenFileDialog.FileName = string.Empty;
            this.imageOpenFileDialog.FileName = string.Empty;
        }

        /// <summary>
        /// Gets the relative URI from app.
        /// </summary>
        /// <returns>The relative URI from app.</returns>
        /// <param name="fullPath">Full path.</param>
        private string GetRelativeUriFromApp(string fullPath)
        {
            // Return relative Uri
            return new Uri(Application.ExecutablePath).MakeRelativeUri(new Uri(fullPath)).ToString();
        }

        /// <summary>
        /// Gets the html.
        /// </summary>
        /// <returns>The html.</returns>
        private string GetHtml()
        {
            // Set base HTML
            var html = this.templateString;

            // Declare entries HTML variable
            var entriesHtml = string.Empty;

            // Populate entries
            for (int i = 0; i < this.removeEntryComboBox.Items.Count; i++)
            {
                // Append current one
                entriesHtml += $"{this.entryInfoDictionary[this.removeEntryComboBox.Items[i].ToString()].HTML}{Environment.NewLine}";
            }

            // Set entries into HTML
            html = html.Replace("[ENTRIES-HTML]", entriesHtml);

            // Return generated HTML
            return html;
        }

        /// <summary>
        /// Handles the view html button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnViewHtmlButtonClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the remove entry combo box selected index changed event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRemoveEntryComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the browse for file button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnBrowseForFileButtonClick(object sender, EventArgs e)
        {
            // Browse for book file
            this.bookOpenFileDialog.ShowDialog();
        }

        /// <summary>
        /// Handles the browse for image button click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnBrowseForImageButtonClick(object sender, EventArgs e)
        {
            // Browse for image file
            this.imageOpenFileDialog.ShowDialog();
        }

        /// <summary>
        /// Handles the new tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnNewToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the exit tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the options tool strip menu item drop down item clicked event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOptionsToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Set item
            var clickedToolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;

            // Toggle checked state
            clickedToolStripMenuItem.Checked = !clickedToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Handles the headquarters patreon.com tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnHeadquartersPatreoncomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the source code github.com tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the original thread donationcoder.com tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }

        /// <summary>
        /// Handles the about tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code.
        }
    }
}