namespace MukMafiaTool.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Vote
    {
        /// <summary>
        /// Gets or sets a value indicating whether this vote is an unvote or not.
        /// </summary>
        public bool IsUnvote { get; set; }

        /// <summary>
        /// Gets or sets the name of the player who cast the vote.
        /// </summary>
        public string Voter { get; set; }

        /// <summary>
        /// Gets or sets the name of the recipient of the vote.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the vote was cast.
        /// </summary>
        [Display(Name = "Date and Time of Post")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the forum post number in which the vote was found.
        /// </summary>
        public string ForumPostNumber { get; set; }

        /// <summary>
        /// Gets or sets the index of the vote within the post content. If it is the first vote in the post, it will be 0, if it is the second, 1, etc.
        /// </summary>
        public int PostContentIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this post has been edited manually after it was created in the database.
        /// </summary>
        public bool ManuallyEdited { get; set; }

        /// <summary>
        /// Gets or sets the day (in game day terms) on which the vote was cast.
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vote is valid.
        /// </summary>
        public bool Invalid { get; set; }
    }
}