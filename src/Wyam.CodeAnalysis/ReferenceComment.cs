namespace Wyam.CodeAnalysis
{
    public class ReferenceComment
    {
        private readonly string _link;

        public ReferenceComment(string name, string link, string html)
        {
            Name = name;
            _link = link;
            Html = html;
        }

        public string Name { get; }
        public string Link => _link ?? Name;
        public string Html { get; }
    }
}