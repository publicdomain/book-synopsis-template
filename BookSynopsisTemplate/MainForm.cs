﻿// <copyright file="MainForm.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace BookSynopsisTemplate
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using PublicDomain;

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
        /// The assembly version.
        /// </summary>
        private Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// The semantic version.
        /// </summary>
        private string semanticVersion = string.Empty;

        /// <summary>
        /// The associated icon.
        /// </summary>
        private Icon associatedIcon = null;

        /// <summary>
        /// The friendly name of the program.
        /// </summary>
        private string friendlyName = "Book Synopsis Template";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BookSynopsisTemplate.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();

            // Set semantic version
            this.semanticVersion = this.assemblyVersion.Major + "." + this.assemblyVersion.Minor + "." + this.assemblyVersion.Build;
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
            this.mainToolStripStatusLabel.Text = this.entryInfoDictionary.Count > 0 ? $"Entry count: {this.removeEntryComboBox.Items.Count}" : "Populate fields then add new entry";
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
            // Set new view HTML form
            var viewHtmlForm = new ViewHtmlForm(this.GetHtml());

            // Show it as dialog
            viewHtmlForm.ShowDialog();
        }

        /// <summary>
        /// Handles the remove entry combo box selected index changed event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRemoveEntryComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Check for null to avoid "Object reference not set"
            if (this.removeEntryComboBox.SelectedItem == null)
            {
                // Halt flow
                return;
            }

            // Delete from dictionary
            this.entryInfoDictionary.Remove(this.removeEntryComboBox.GetItemText(this.removeEntryComboBox.SelectedItem));

            // Delete from combo box
            this.removeEntryComboBox.Items.Remove(this.removeEntryComboBox.SelectedItem);

            // Copy on delete
            if (this.copyOnDeleteToolStripMenuItem.Checked)
            {
                // Set updated HTML into clipboard
                Clipboard.SetText(this.GetHtml());
            }

            // Reset text
            this.ResetRemoveEntryComboBoxText();

            // Reflect new entry count
            this.UpdateStatus();
        }

        /// <summary>
        /// Resets the remove entry combo box text.
        /// </summary>
        private void ResetRemoveEntryComboBoxText()
        {
            // Reset text
            this.removeEntryComboBox.Text = "Select entry to delete...";
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
            // Reset character text box
            this.characterTextBox.Clear();
            this.characterTextBox.Enabled = true;

            // Reset other fields
            this.ClearFields();

            // Reset entry info dictionary
            this.entryInfoDictionary.Clear();

            // Reset remove entry combo box
            this.removeEntryComboBox.Items.Clear();
            this.ResetRemoveEntryComboBoxText();

            // Display initial status
            this.UpdateStatus();
        }

        /// <summary>
        /// Handles the exit tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Exit program
            this.Close();
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
            // Open Patreon headquarters
            Process.Start("https://www.patreon.com/publicdomain");
        }

        /// <summary>
        /// Handles the source code github.com tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open GitHub
            Process.Start("https://github.com/publicdomain");
        }

        /// <summary>
        /// Handles the original thread donationcoder.com tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open original thread @ DonationCoder
            Process.Start("https://www.donationcoder.com/forum/index.php?topic=48893.0");
        }

        /// <summary>
        /// Handles the about tool strip menu item click event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Set license text
            var licenseText = $"CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication{Environment.NewLine}" +
                $"https://creativecommons.org/publicdomain/zero/1.0/legalcode{Environment.NewLine}{Environment.NewLine}" +
                $"Libraries and icons have separate licenses.{Environment.NewLine}{Environment.NewLine}" +
                $"Book read icon by IO-Images - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/vectors/book-read-books-learn-text-icon-1157658/{Environment.NewLine}{Environment.NewLine}" +
                $"Patreon icon used according to published brand guidelines{Environment.NewLine}" +
                $"https://www.patreon.com/brand{Environment.NewLine}{Environment.NewLine}" +
                $"GitHub mark icon used according to published logos and usage guidelines{Environment.NewLine}" +
                $"https://github.com/logos{Environment.NewLine}{Environment.NewLine}" +
                $"DonationCoder icon used with permission{Environment.NewLine}" +
                $"https://www.donationcoder.com/forum/index.php?topic=48718{Environment.NewLine}{Environment.NewLine}" +
                $"PublicDomain icon is based on the following source images:{Environment.NewLine}{Environment.NewLine}" +
                $"Bitcoin by GDJ - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/vectors/bitcoin-digital-currency-4130319/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter P by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/p-glamour-gold-lights-2790632/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter D by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/d-glamour-gold-lights-2790573/{Environment.NewLine}{Environment.NewLine}";

            // Set about form
            var aboutForm = new AboutForm(
                $"About {this.friendlyName}",
                $"{this.friendlyName} {this.semanticVersion}",
                $"Made for: fredemeister{Environment.NewLine}DonationCoder.com{Environment.NewLine}Week #44 @ October 2019",
                licenseText,
                this.Icon.ToBitmap());

            // Check for an associated icon
            if (this.associatedIcon == null)
            {
                // Set associated icon from exe file, once
                this.associatedIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            }

            // Set about form icon
            aboutForm.Icon = this.associatedIcon;

            // Match topmost
            aboutForm.TopMost = this.TopMost;

            // Show about form
            aboutForm.ShowDialog();
        }
    }
}