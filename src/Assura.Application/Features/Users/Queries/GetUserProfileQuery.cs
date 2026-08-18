using Assura.Application.Common.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;



namespace Assura.Application.Features.Users.Queries;



public record GetUserProfileQuery(int UserId) : IRequest<UserProfileDto?>;



public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>

{

    private readonly IApplicationDbContext _context;



    public GetUserProfileQueryHandler(IApplicationDbContext context)

    {

        _context = context;

    }



    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)

    {

        Console.WriteLine($"[DEBUG] GetUserProfileQueryHandler: Handling request for UserId: {request.UserId}");

        

        var user = await _context.Users

            .AsNoTracking()

            .Include(u => u.Division)

            .Where(u => u.Id == request.UserId)

            .Select(u => new UserProfileDto

            {

                Id = u.Id,

                Username = u.Username,

                FirstName = u.FirstName,

                LastName = u.LastName,

                Email = u.Email,

                Role = u.Role.HasValue ? u.Role.Value.ToString() : "User",

                DivisionId = u.DivisionId,

                DivisionName = u.Division != null ? u.Division.Name : null,

                PhoneNumber = u.PhoneNumber

            })

            .FirstOrDefaultAsync(cancellationToken);



        Console.WriteLine($"[DEBUG] GetUserProfileQueryHandler: Result found: {(user != null ? "Yes" : "No")}");

        return user;

    }

}

