// <copyright file="EntryInfo.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace BookSynopsisTemplate
{
    // Directives
    using System;

    /// <summary>
    /// Entry info.
    /// </summary>
    public class EntryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BookSynopsisTemplate.EntryInfo"/> class.
        /// </summary>
        /// <param name="character">The character.</param>
        public EntryInfo(string character)
        {
            // Set character
            this.Character = character;
        }

        /// <summary>
        /// Gets or sets the character.
        /// </summary>
        /// <value>The character.</value>
        public string Character { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the image path.
        /// </summary>
        /// <value>The image path.</value>
        public string ImagePath { get; set; }

        /// <summary>
        /// Gets or sets the synopsys.
        /// </summary>
        /// <value>The synopsys.</value>
        public string Synopsys { get; set; }

        /// <summary>
        /// Gets or sets the div html.
        /// </summary>
        /// <value>The div html.</value>
        public string DivHtml { get; set; }
    }
}
