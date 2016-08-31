using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Blog
{
    public static class BlogPipelines
    {
        public const string RawPosts = nameof(RawPosts);
        public const string Posts = nameof(Posts);
        public const string Tags = nameof(Tags);
        public const string Pages = nameof(Pages);
        public const string RenderPages = nameof(RenderPages);
        public const string Resources = nameof(Resources);
        public const string Feed = nameof(Feed);
    }
}
