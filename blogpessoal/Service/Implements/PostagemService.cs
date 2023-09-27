using blogpessoal.Data;
using blogpessoal.Model;
using Microsoft.EntityFrameworkCore;

namespace blogpessoal.Service.Implements
{
    public class PostagemService : IPostagemService
    {

        private readonly AppDbContext _context;

        public PostagemService(AppDbContext context)
        {
            _context = context;
        }  

        public async Task<IEnumerable<Postagem>> GetAll()
        {
            return await _context.Postagens
                .Include(p => p.Tema)
                .ToListAsync();
        }

        public async Task<Postagem> GetById(long id)
        {
            try
            {
                var PostagemUpdate = await _context.Postagens
                    .Include(p => p.Tema)
                    .FirstAsync(i => i.id == id);

                return PostagemUpdate;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Postagem>> GetByTitulo(string titulo)
        {
            var Postagens = await _context.Postagens
                .Include(p => p.Tema)
                .Where(p =>p.titulo.Contains(titulo)) .ToListAsync();
            return Postagens;
        }

        public async Task<Postagem?> Create(Postagem postagem)
        {
            if(postagem.Tema is not null)
            {
                var BuscaTema = await _context.Temas.FindAsync(postagem.Tema.id);

                if (BuscaTema is null)
                    return null;
            }
            postagem.Tema = postagem.Tema is not null ? _context.Temas.FirstOrDefault(t => t.id ==postagem.Tema.id) : null;

            await _context.Postagens.AddAsync(postagem);
            await _context.SaveChangesAsync();

            return postagem;
        }

        public async Task<Postagem?> Update(Postagem postagem)
        {
            var PostagemUpdate = await _context.Postagens.FindAsync(postagem.id);

            if(PostagemUpdate is null)
                return null;

            postagem.Tema = postagem.Tema is not null ? _context.Temas.FirstOrDefault(t => t.id == postagem.Tema.id) : null;

            _context.Entry(PostagemUpdate).State = EntityState.Detached;
            _context.Entry(postagem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return postagem;
        }

        public async Task Delete(Postagem postagem)
        {
            _context.Remove(postagem);
            await _context.SaveChangesAsync();
        }

    }
}
