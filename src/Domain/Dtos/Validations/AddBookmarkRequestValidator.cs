using FluentValidation;
using System;

namespace BookmarkManager.Domain.Dtos.Validations
{
    public class AddBookmarkRequestValidator : AbstractValidator<AddBookmarkRequest>
    {
        public AddBookmarkRequestValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty()
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _));
        }
    }
}
