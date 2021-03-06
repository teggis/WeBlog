﻿using System;
using Sitecore.Buckets.Extensions;
using Sitecore.Buckets.Util;
using Sitecore.Data;
using Sitecore.StringExtensions;

namespace Sitecore.Modules.WeBlog.Buckets
{
    public class DynamicBucketFolderPathSelector : IDynamicBucketFolderPath
    {
        [Obsolete]
        public string GetFolderPath(ID newItemId, ID parentItemId, DateTime creationDateOfNewItem)
        {
            return GetFolderPath(Sitecore.Context.ContentDatabase, string.Empty, null, newItemId, parentItemId,
                          creationDateOfNewItem);
        }

        public string GetFolderPath(Database database, string name, ID templateId, ID newItemId, ID parentItemId,
                                    DateTime creationDateOfNewItem)
        {
            if (database == null)
                return string.Empty;

            var parentItem = database.GetItem(parentItemId);
            if (parentItem != null)
            {
                // Ensure we're at the root of the bucket
                parentItem = parentItem.GetParentBucketItemOrRootOrSelf();
                // The following is a Sitecore config path query, not a Sitecore content tree path
                var xpath = "/sitecore/WeBlog/DynamicBucketFolderPathSelector/*[includeTemplates/*[text() = '{0}'] or paths/*[text() = '{1}']]/handler"
                    .FormatWith(
                        parentItem.TemplateID, parentItem.Paths.FullPath.ToLower());
                var configNode = Sitecore.Configuration.Factory.GetConfigNode(xpath);

                if (configNode != null)
                {
                    var handler = Sitecore.Configuration.Factory.CreateObject<IDynamicBucketFolderPath>(configNode);
                    if (handler != null)
                    {
#if SC70
// We only target specific Sitecore versions when there's an API contention
                        return (handler as IDynamicBucketFolderPath).GetFolderPath(newItemId, parentItemId,
                                                                                   creationDateOfNewItem);
#else
                        return (handler as IDynamicBucketFolderPath).GetFolderPath(database, name, templateId, newItemId,
                                                                                   parentItemId, creationDateOfNewItem);
#endif
                    }
                }
            }
#if SC70
            return new GuidFolderPath().GetFolderPath(newItemId, parentItemId, creationDateOfNewItem);
#else
            return new BucketFolderPathResolver().GetFolderPath(database, name, templateId, newItemId, parentItemId, creationDateOfNewItem);
#endif
        }
    }
}