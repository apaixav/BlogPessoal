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
    public class UserControllerTest : IClassFixture<WebAppFactory>
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
        public async Task DeveCriarNovoUsuario()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                {"nome" , "João"},
                {"usuario", "joao@email.com"},
                {"senha", "12345678" },
                {"foto", "-"}
            };
            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/Json");
            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);
            resposta.EnsureSuccessStatusCode();
            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        [Fact, Order(2)]
        public async Task NaoDeveCriarUsuarioDuplicado()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                {"nome","Ingrid" },
                {"usuario", "ingridemail.com" },
                {"senha", "12345678" },
                {"foto", "-"}
            };
            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/Json");
            await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);
            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Order(3)]
        public async Task DeveAtualizarUsuario()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                { "nome", "Paulo Antunes" },
                { "usuario", "paulo@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };
            var postJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicaoPost = new StringContent(postJson, Encoding.UTF8, "application/json");
            var respostaPost = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicaoPost);
            var corpoRespostaPost = await respostaPost.Content.ReadFromJsonAsync<User>();
            if (corpoRespostaPost is not null)
            {
                Id = corpoRespostaPost.Id.ToString();
            }
            var atualizarUsuario = new Dictionary<string, string>
            {
                {"id", Id },
                {"nome" , "José"},
                {"usuario", "jose@email.com"},
                {"senha", "12345678" },
                {"foto", "-"}
            };
            var updateJson = JsonConvert.SerializeObject(atualizarUsuario);
            var corpoRequisicaoUpdate = new StringContent(updateJson, Encoding.UTF8, "application/Json");
            _client.SetFakeBearerToken((object)token);
            var respostaPut = await _client.PutAsync("/usuarios/atualizar", corpoRequisicaoUpdate);
            respostaPut.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact, Order(4)]
        public async Task DeveListarTodosUsuarios()
        {
            _client.SetFakeBearerToken((object)token);
            var resposta = await _client.GetAsync("/usuarios/all");
            resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, Order(5)]
        public async Task DeveListarUmUsuario()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                { "nome", "Ana Paula" },
                { "usuario", "anapaula@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };
            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicaoPost = new StringContent(usuarioJson, Encoding.UTF8, "application/json");
            var respostaPost = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicaoPost);
            respostaPost.EnsureSuccessStatusCode();
            _client.SetFakeBearerToken((object)token);
            var corpoRespostaPost = await respostaPost.Content.ReadFromJsonAsync<User>();
            var idUsuarioCriado = corpoRespostaPost.Id.ToString();
            var respostaGet = await _client.GetAsync($"/usuarios/{idUsuarioCriado}");
            respostaGet.StatusCode.Should().Be(HttpStatusCode.OK);

        }
        [Fact, Order(6)]
        public async Task DeveAutenticarUmUsuario()
        {
            var novoUsuario = new Dictionary<string, string>
            {
                { "nome", "Paulo" },
                { "usuario", "paulo@email.com.br" },
                { "senha", "12345678" },
                { "foto", "-" }
            };
            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);
            var corpoRequisicaoPost = new StringContent(usuarioJson, Encoding.UTF8, "application/json");
            var respostaPost = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicaoPost);
            respostaPost.EnsureSuccessStatusCode();

            var usuario = new Dictionary<string, string>
            {
                {"usuario", "paulo@email.com.br"},
                {"senha", "12345678" }
            };

            var autenticaJson = JsonConvert.SerializeObject(usuario);
            var corpoRequisicao = new StringContent(autenticaJson, Encoding.UTF8, "application/Json");

            var resposta = await _client.PostAsync("/usuarios/logar", corpoRequisicao);
            resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        }
    }
}
