using Coworking.Domain.Entities;
using Coworking.Infrastructure;
using Coworking.Infrastructure.Queries.Reservations;
using Coworking.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Application.Handlers.Reservations;

public class GetAllReservationsHandler : IRequestHandler<GetAllReservationsQuery, List<Domain.Entities.Reservations>>
{
    private readonly IReservationsRepository _service;
    
    public GetAllReservationsHandler(IReservationsRepository service)
    {
        _service = service;
    }
    
    public async Task<List<Domain.Entities.Reservations>> Handle(GetAllReservationsQuery request, CancellationToken cancellationToken)
    {
        // Si es admin, retornamos todas
        if (request.Role == "Admin")
        {
            return await _service.GetAllAsync();
        }
        // Si es user, retornamos solo sus reservas
        return await _service.GetByUserIdAsync(request.UserId);

    }
}