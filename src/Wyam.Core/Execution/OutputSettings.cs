using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    internal class OutputSettings : IOutputSettings
    {
        private string _linkRoot = "/";

        public OutputSettings()
        {
            HideLinkIndexPages = true;
            HideLinkWebExtensions = true;
        }

        public string LinkRoot
        {
            get { return _linkRoot; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(value));
                }
                _linkRoot = value;
            }
        }

        public bool HideLinkIndexPages { get; set; }

        public bool HideLinkWebExtensions { get; set; }
    }
}
