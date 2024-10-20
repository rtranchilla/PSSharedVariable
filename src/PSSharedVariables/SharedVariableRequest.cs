using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSSharedVariables
{
    public interface ISharedVariableRequest : IBaseRequest { }
    public sealed class SharedVariableGetRequest : ISharedVariableRequest, IRequest<object>
    {
        public SharedVariableGetRequest() { }
        public SharedVariableGetRequest(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
        public string Name { get; }
    }
    public sealed class SharedVariableExistsRequest : ISharedVariableRequest, IRequest<bool>
    {
        public SharedVariableExistsRequest(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
        public string Name { get; }
    }
}