using System.Collections.Generic;

namespace Wyam.Abstractions
{
    public interface IPipeline
    {
        string Name { get; }
        int Count { get; }
    }
}
