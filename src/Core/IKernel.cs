using System;
using System.Collections.Generic;

namespace MicroIOC.Core
{
    /// <summary>
    /// Contract for managing the internals of the container.
    /// </summary>
    public interface IKernel : IDisposable
    {
        HashSet<Node> Nodes { get;  }
        void CreateNode(Node node);
        object Resolve(Type type);
        object Resolve(string key);
    	object[] ResolveAll(Type type);
    }
}