using blogpessoal.Model;
using blogpessoal.Security;
using blogpessoal.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blogpessoal.Controllers
{
    [Route("~/usuarios")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
       
            private readonly IUserService _usuarioService;
            private readonly IValidator<User> _usuarioValidator;
            private readonly IAuthService _authService;


        public UserController(
                IUserService usuarioService, 
                IValidator<User> usuarioValidator,
                IAuthService authService)
        {
                _usuarioService = usuarioService;
                _usuarioValidator = usuarioValidator;
                _authService = authService;

        }
        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _usuarioService.GetAll());

        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(long id)
        {
            var Resposta = await _usuarioService.GetById(id);

            if(Resposta is null)
                return NotFound();

            return Ok(Resposta);
        }
        [AllowAnonymous]
        [HttpPost("cadastrar")]
        public async Task<ActionResult> Create([FromBody]User usuario)
        {
            var validarUser = await _usuarioValidator.ValidateAsync(usuario);

            if (!validarUser.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, validarUser);

            var Resposta = await _usuarioService.Create(usuario);

            if (Resposta is null)
                return BadRequest("Usuário já está Cadastrado!");

            return CreatedAtAction(nameof(GetById), new {Id = usuario.Id}, usuario);
        }
        [Authorize]
        [HttpPut("atualizar")]
        public async Task<ActionResult> Update([FromBody] User usuario)
        {
            if (usuario.Id == 0)
                return BadRequest("Id da User é invalido");

            var validarUser = await _usuarioValidator.ValidateAsync(usuario);

            if (!validarUser.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, validarUser);
            }

            var UserUpdate = await _usuarioService.GetByUsuario(usuario.Usuario);

            if (UserUpdate is not null && UserUpdate.Id != usuario.Id)
                return BadRequest("O Usuário (e-mail) já está em uso pro outro Usuário!");

            var Resposta = await _usuarioService.Update(usuario);

            if (Resposta == null)
                return NotFound("User não Encontrada");

            return Ok(Resposta);
        }
            [AllowAnonymous]
            [HttpPost("logar")]
          public async Task<ActionResult> Autenticar([FromBody] UserLogin userLogin)
          {
                var Resposta = await _authService.Autenticar(userLogin);

                if (Resposta is null)
                    return Unauthorized("Usuário e/ou Senha inválidos");

                return Ok(Resposta);
          }
        

        
    }
}
