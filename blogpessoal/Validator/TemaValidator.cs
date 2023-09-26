using blogpessoal.Model;
using FluentValidation;

namespace blogpessoal.Validator
{
    public class TemaValidator : AbstractValidator<Tema>
    {
        public TemaValidator() {

            RuleFor(p => p.descricao)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(1000);

        }
    }
}
