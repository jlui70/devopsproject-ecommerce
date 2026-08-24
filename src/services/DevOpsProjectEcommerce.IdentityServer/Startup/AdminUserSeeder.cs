using System.Security.Cryptography;
using System.Text;
using DevOpsProjectEcommerce.IdentityServer.Domain.Repositories.Contexts;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DevOpsProjectEcommerce.IdentityServer.Startup
{
    /// <summary>
    /// ADR-0025, Decisao 2 — a configuracao e' a origem da verdade da senha do admin.
    ///
    /// Antes desta rotina, a senha vinha de duas fontes que divergiam em silencio:
    ///
    ///   * o banco recebia o hash FIXO gravado pela migration 20260724000000_FixAdminUser.
    ///     O `HasData` de IdentityServerContext.OnModelCreating calcula o hash a partir da
    ///     configuracao, mas isso so' vale em design time — o que roda contra o banco e' a
    ///     migration, com o valor embutido nela;
    ///   * `Main` e `Order` usavam a configuracao (`Identity:Admin:User:Password`) como
    ///     credencial de servico para servico, via AuthorizationHeaderValueGetter do Refit.
    ///
    /// Com isso, a senha semeada pelo operador no Estagio 0.8 do RUNBOOK era ignorada pelo
    /// banco e usada pelos servicos. Tudo subia saudavel e a falha aparecia so' no checkout,
    /// quando o `Main` tentava obter token no `Order` (401). Achado ao vivo em 2026-08-24.
    ///
    /// Havia ainda um problema de conformidade: o hash fixo e' gravado nos DOIS bancos, o
    /// que impedia staging de ter credencial propria, como exigem os ADR-0011/0012.
    ///
    /// Esta rotina roda a cada inicializacao, depois das migrations, e converge o usuario
    /// admin para o que estiver na configuracao. E' idempotente: rodar de novo com a mesma
    /// configuracao nao muda nada.
    /// </summary>
    public static class AdminUserSeeder
    {
        public static void SyncAdminUser(IdentityServerContext context, IConfiguration configuration)
        {
            var email = configuration.GetValue<string>("Identity:Admin:User");
            var password = configuration.GetValue<string>("Identity:Admin:User:Password");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Log.Warning(
                    "Identity:Admin:User e/ou Identity:Admin:User:Password ausentes — " +
                    "usuario admin NAO sincronizado. O login usara o valor gravado pela migration.");
                return;
            }

            // Mesma expressao usada por UserService.CheckPasswordAsync e pelo HasData do
            // contexto. A conversao UTF8 sobre bytes de SHA256 e' lossy, mas deterministica:
            // gravacao e verificacao aplicam exatamente a mesma transformacao, entao a
            // comparacao continua valida. Nao alterar aqui sem alterar la'.
            var hashedPassword = Encoding.UTF8.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

            // ON CONFLICT cobre o caso de a linha ter sido removida; no fluxo normal a
            // migration ja criou o Id=1 e o caminho executado e' o UPDATE. Os valores vao
            // como parametros (ExecuteSqlInterpolated), nao concatenados no SQL.
            var affected = context.Database.ExecuteSqlInterpolated(
                $@"INSERT INTO ""User"" (""Id"", ""Email"", ""Password"")
                   VALUES (1, {email}, {hashedPassword})
                   ON CONFLICT (""Id"") DO UPDATE
                       SET ""Email"" = EXCLUDED.""Email"",
                           ""Password"" = EXCLUDED.""Password""");

            Log.Information(
                "Usuario admin sincronizado a partir da configuracao (email {Email}, {Rows} linha afetada).",
                email, affected);
        }
    }
}
