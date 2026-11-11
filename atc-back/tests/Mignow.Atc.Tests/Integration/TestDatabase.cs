using Npgsql;

namespace Mignow.Atc.Tests.Integration;

/// <summary>
/// Banco de teste no Postgres local (<c>MIGNOW_ATC_TEST</c>; sem Docker nesta máquina, por isso não usa Testcontainers).
/// O schema <c>public</c> é recriado uma vez por execução da suíte; a migração recria tudo. Sobreponível por ATC_TEST_DB.
/// </summary>
public static class TestDatabase
{
    public static readonly string ConnectionString = Environment.GetEnvironmentVariable("ATC_TEST_DB")
        ?? "Host=localhost;Port=5432;Database=MIGNOW_ATC_TEST;Username=postgres";

    public const string SecretKey = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8="; // só testes

    private static readonly Lock Gate = new();
    private static bool _reset;

    public static void ResetOnce()
    {
        lock (Gate)
        {
            if (_reset)
                return;
            EnsureDatabase();
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;", conn);
            cmd.ExecuteNonQuery();
            _reset = true;
        }
    }

    private static void EnsureDatabase()
    {
        var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
        var name = builder.Database!;
        builder.Database = "postgres";
        using var conn = new NpgsqlConnection(builder.ConnectionString);
        conn.Open();
        using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @n", conn);
        exists.Parameters.AddWithValue("n", name);
        if (exists.ExecuteScalar() is null)
        {
            using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", conn);
            create.ExecuteNonQuery();
        }
    }
}
