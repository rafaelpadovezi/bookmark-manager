﻿using BookmarkManager.Utils;
using System;

namespace BookmarkManager.Domain.Models
{
    public class Bookmark : Entity
    {
        public Bookmark() { }

        public Bookmark(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            DisplayName = Url;
        }

        public string DisplayName { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }

        internal void Update(string title, string description, string imageUrl)
        {
            Title = title ?? Url;
            DisplayName = Title;
            Description = description;
            ImageUrl = imageUrl;
        }
    }
}
