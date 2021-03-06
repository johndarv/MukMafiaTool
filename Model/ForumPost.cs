﻿namespace MukMafiaTool.Model
{
    using System;
    using System.Web;

    public class ForumPost
    {
        public string ForumPostNumber { get; set; }

        public string Poster { get; set; }

        public DateTime DateTime { get; set; }

        public HtmlString Content { get; set; }

        public int Day { get; set; }

        public int PageNumber { get; set; }

        public DateTime LastScanned { get; set; }

        public bool ManuallyEdited { get; set; }
    }
}