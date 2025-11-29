using MarketPlace.DTO;
using MarketPlace.Models;

namespace MarketPlace.Mappers;

public static class OrderMappers
{
    public static OrderResponse ToOrderResponse(this Order order)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        return new OrderResponse
        {
            Id = order.Id,
            Status = order.Status,
            OrderedAt = order.OrderedAt,
            StatusChangedAt = order.StatusChangedAt,
            User = order.User.ToUserDto(),
            Job = order.Job.ToJobDto()
        };
    }

    public static UserDto ToUserDto(this User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        return new UserDto
        {
            Id = user.Id,
            Surname = user.Surname,
            Name = user.Name,
            Patronymic = user.Patronymic,
            Email = user.Email,
            AvatarPath = user.AvatarPath,
            City = user.CityNavigation.ToCityDto()
        };
    }

    public static CityDto ToCityDto(this City city)
    {
        if (city is null)
            throw new ArgumentNullException(nameof(city));

        return new CityDto
        {
            Id = city.Id,
            Name = city.Name,
            Region = city.Region
        };
    }

    public static JobDto ToJobDto(this Job job)
    {
        if (job is null)
            throw new ArgumentNullException(nameof(job));

        return new JobDto
        {
            Id = job.Id,
            Name = job.Name,
            Description = job.Description,
            Price = job.Price,
            CoverUrl = job.CoverUrl,
            CreatedAt = job.CreatedAt,
            CategoryId = job.CategoryId,
            User = job.Master.ToUserDto(),
            Category = job.Category.ToCategoryDto(),
            Tags = job.Tags.Select(t => t.ToTagDto()).ToList(),
            OrdersCount = job.Orders?.LongCount() ?? 0
        };
    }

    public static CategoryDto ToCategoryDto(this Category category)
    {
        if (category is null) throw new ArgumentNullException(nameof(category));

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public static TagDto ToTagDto(this Tag tag)
    {
        if (tag is null) throw new ArgumentNullException(nameof(tag));

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}