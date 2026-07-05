using ClauseLens.Application.Abstractions;
using MediatR;

namespace ClauseLens.Application.Contracts.Queries;

public sealed record GetContractQuery(Guid ContractId) : IQuery<Result<ContractDetailDto>>;

public sealed record ContractDetailDto(
    Guid Id, string FileName, long FileSize, string FileFormat, string Status,
    DateTime CreatedAt, IReadOnlyList<ClauseDto> Clauses
);

public sealed record ClauseDto(Guid Id, int Index, string Heading, string Text, string Status, string? SystemNote);

public sealed class GetContractHandler : IRequestHandler<GetContractQuery, Result<ContractDetailDto>>
{
    private readonly IContractRepository _contracts;
    public GetContractHandler(IContractRepository contracts) => _contracts = contracts;

    public async Task<Result<ContractDetailDto>> Handle(GetContractQuery q, CancellationToken ct)
    {
        var c = await _contracts.FindByIdAsync(q.ContractId, ct);
        if (c is null) return Result<ContractDetailDto>.Failure("Contract not found.", "NOT_FOUND");
        return Result<ContractDetailDto>.Success(new ContractDetailDto(
            c.Id, c.FileName, c.FileSize, c.FileFormat, c.Status.ToString(), c.CreatedAt,
            c.Clauses.Select(cl => new ClauseDto(cl.Id, cl.Index, cl.Heading, cl.Text, cl.Status.ToString(), cl.SystemNote)).ToList()
        ));
    }
}
