using blogpessoal.Model;
using FluentValidation;

namespace blogpessoal.Validator
{
    public class PostagemValidator : AbstractValidator<Postagem>
    {
        public PostagemValidator() {

            RuleFor(p => p.titulo)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(100);

            RuleFor(p => p.texto)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(1000);
        }
    }
}
