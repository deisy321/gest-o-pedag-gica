using gestaopedagogica.Data;
using gestaopedagogica.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestaopedagogica.Services
{
    public class ComentarioService
    {
        private readonly ApplicationDbContext _context;

        public ComentarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Adicionar comentário do professor
        public async Task<Comentario?> AdicionarComentarioAsync(int trabalhoId, string autorId, string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
                return null;

            var comentario = new Comentario
            {
                TrabalhoId = trabalhoId,
                AutorId = autorId,
                Conteudo = conteudo.Trim(),
                Tipo = Comentario.TipoComentario.Observacao, // Apenas observação do professor
                DataCriacao = DateTime.UtcNow,
                ComentarioPaiId = null // Não usa hierarquia
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            return comentario;
        }

        // Listar comentários de um trabalho (para aluno ver)
        public async Task<List<Comentario>> ObterComentariosAsync(int trabalhoId)
        {
            return await _context.Comentarios
                .Where(c => c.TrabalhoId == trabalhoId)
                .Include(c => c.Autor)
                .OrderByDescending(c => c.DataCriacao)
                .AsNoTracking()
                .ToListAsync();
        }

        // Atualizar comentário do professor
        public async Task<bool> AtualizarComentarioAsync(int comentarioId, string novoConteudo)
        {
            var comentario = await _context.Comentarios.FindAsync(comentarioId);
            if (comentario == null) return false;

            comentario.Conteudo = novoConteudo.Trim();
            comentario.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Deletar comentário
        public async Task<bool> DeletarComentarioAsync(int comentarioId)
        {
            var comentario = await _context.Comentarios.FindAsync(comentarioId);
            if (comentario == null) return false;

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            return true;
        }

        // Contar comentários de um trabalho
        public async Task<int> ContarComentariosAsync(int trabalhoId)
        {
            return await _context.Comentarios.CountAsync(c => c.TrabalhoId == trabalhoId);
        }
    }
}