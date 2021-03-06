﻿using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Links;
using Sitecore.Modules.WeBlog.Data.Items;
using Sitecore.Modules.WeBlog.Managers;
using Sitecore.Modules.WeBlog.Model;
using Sitecore.Web;

namespace Sitecore.Modules.WeBlog.Components
{
    public class TagCloudCore : ITagCloudCore
    {
        public string SortingOptions { get; set; }
        private int _max;
        private Tag[] _tags;

        public BlogHomeItem CurrentBlog { get; set; }

        public Tag[] Tags
        {
            get
            {
                if (_tags == null)
                {
                    LoadTags();
                }
                return _tags;
            }
            set { _tags = value; }
        }

        private string[] m_SortingNames = null;

        public string[] GetSortNames(string sortingOptions)
        {
                if (m_SortingNames != null)
                {
                    return m_SortingNames;
                }
                if (string.IsNullOrEmpty(sortingOptions))
                {
                    return new string[0];
                }
                var database = CurrentBlog.Database;
                m_SortingNames = sortingOptions.Split('|')
                    .Select(id => database.GetItem(id))
                    .Where(item => item != null)
                    .Select(item => item.Name.ToLowerInvariant())
                    .ToArray();
                return m_SortingNames;
        }


        public TagCloudCore(IBlogManager blogManagerInstance)
        {            
            CurrentBlog = blogManagerInstance.GetCurrentBlog();
        }

        /// <summary>
        /// Load the tags for the current blog into this control
        /// </summary>
        protected virtual void LoadTags()
        {
            Tags = ManagerFactory.TagManagerInstance.GetTagsForBlog(CurrentBlog);
            if (Tags.Any())
            {
                _max = (from tag in Tags select tag.Count).Max();
            }
        }

        /// <summary>
        /// Calculate the weight class to use for the current tag
        /// </summary>
        /// <param name="tagWeight">The weight to get the weight class for</param>
        /// <returns>The weight class for the given weight</returns>
        public virtual string GetTagWeightClass(int tagWeight)
        {
            var weightPercent = ((double)tagWeight / _max) * 100;

            // Give weigth to tags
            return GetWeight(weightPercent).ToString();
        }

        /// <summary>
        /// Get the URL for a tag
        /// </summary>
        /// <param name="tag">The tag to get the URL for</param>
        /// <returns>The tag URL</returns>
        public virtual string GetTagUrl(string tag)
        {
            // Get url of parent 
            var browseUrl = WebUtil.RemoveQueryString(LinkManager.GetItemUrl(CurrentBlog.InnerItem));

            return browseUrl + "?tag=" + HttpUtility.UrlEncode(tag);
        }

        /// <summary>
        /// Calculate the weight percentile for this tag
        /// </summary>
        /// <param name="weightPercent">The weight as a percentage</param>
        /// <returns>The weight percentile</returns>
        protected int GetWeight(double weightPercent)
        {
            var weight = weightPercent >= 99 ? 1 : (weightPercent >= 70 ? 2 : (weightPercent >= 40 ? 3 : (weightPercent >= 20 ? 4 : (weightPercent >= 3 ? 5 : 0))));
            return weight;
        }
    }
}