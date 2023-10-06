using blogpessoal.Model;
using BlogPessoalTeste.Factory;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions.Ordering;

namespace BlogPessoalTeste.Controller
{
    internal class UserControllerTest : IClassFixture<WebAppFactory>
    {

        protected readonly WebAppFactory _factory;
        protected HttpClient _client;

        private readonly dynamic token;

        private string Id { get; set; } = string.Empty;

        public UserControllerTest(WebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            token = GetToken();

        }
        private static dynamic GetToken()
        {
            dynamic data = new ExpandoObject();
            data.sub = "root@root.com";
            return data;
        }

        [Fact, Order(1)]
        public async Task DeveCriarUmUsuario()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                {"nome","Ingrid" },
                {"usuario", "ingrid@email.com" },
                {"senha", "123456789" },
                {"foto","" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuario/cadastrar", corpoRequisicao);

            resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        [Fact, Order(2)]
        public async Task DeveDarErroEmail()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                {"nome","Ingrid" },
                {"usuario", "ingridemail.com" },
                {"senha", "123456789" },
                {"foto","" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuario/cadastrar", corpoRequisicao);

            //resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Order(3)]
        public async Task NaoDeveCriarUsuarioDuplicado()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                {"nome","Karina" },
                {"usuario", "Karinaemail.com" },
                {"senha", "123456789" },
                {"foto","" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/usuario/cadastrar", corpoRequisicao);

            var resposta = await _client.PostAsync("/usuario/cadastrar", corpoRequisicao);
            //resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Order(4)]
        public async Task DeveListarTodosUsuarios()
        {
            _client.SetFakeBearerToken((object)token);

            var resposta = await _client.GetAsync("/usuario/all");

            resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        }
        [Fact, Order(5)]
        public async Task DeveAtualizarUmUsuario()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                {"nome","João" },
                {"usuario", "João@email.com" },
                {"senha", "123456789" },
                {"foto","" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuario/cadastrar", corpoRequisicao);

            var corpoRespostaPost = await resposta.Content.ReadFromJsonAsync<User>();

            if(corpoRespostaPost != null)
            {
                Id = corpoRespostaPost.Id.ToString();
            }
            var atualizarUsuario = new Dictionary<string, string>()
            {
                {"id",Id },
                {"nome","João" },
                {"usuario", "João@email.com" },
                {"senha", "123456789" },
                {"foto","" }
            };

            var usuarioJsonAtualizar = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicaoAtualizar = new StringContent(usuarioJson, Encoding.UTF8, "application/json");
           
            _client.SetFakeBearerToken((object)token);

            var respostaPut = await _client.PutAsync("/usuarios/atualizar",corpoRequisicaoAtualizar); 

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);

        }
    }
}
