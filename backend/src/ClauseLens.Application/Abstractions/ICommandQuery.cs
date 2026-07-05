using MediatR;

namespace ClauseLens.Application.Abstractions;

/// <summary>
/// Marker for write-side operations. Implemented by every command record.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface ICommand : IRequest { }

/// <summary>
/// Marker for read-side operations. Implemented by every query record.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse> { }
