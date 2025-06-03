using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.Hardik.Connector.Date;
using Npgsql;
using Group = Dzudo.Hardik.Connector.Date.GroupDao_2;

namespace ukhasnikis_BD_Sec.Hardik.Connect
{
    public class DatabaseConnection : IDisposable
    {
        private readonly NpgsqlConnection _connection;

        public DatabaseConnection() // подключение к БД
        {
            string connectionString = "Server=45.67.56.214;Port=5421;Database=user16;User Id=user16;Password=dZ28IVE5;";
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public NpgsqlConnection GetConnection() => _connection;

        public List<UkhasnikiDao> GetAllUkhasnikis() // работа с участниками
        {
            var ukhasnikis = new List<UkhasnikiDao>();
            using (var cmd = new NpgsqlCommand("SELECT * FROM \"Sec\".ukhasniki", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    ukhasnikis.Add(new UkhasnikiDao
                    {
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        SecName = reader.IsDBNull(reader.GetOrdinal("secname")) ? null : reader.GetString(reader.GetOrdinal("secname")),
                        DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("datesorevnovaniy")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("datesorevnovaniy"))),
                        Club = reader.IsDBNull(reader.GetOrdinal("club")) ? null : reader.GetString(reader.GetOrdinal("club")),
                        Adres = reader.IsDBNull(reader.GetOrdinal("adres")) ? null : reader.GetString(reader.GetOrdinal("adres")),
                        Ves = reader.IsDBNull(reader.GetOrdinal("ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ves"))
                    });
                }
            }
            return ukhasnikis;
        }

        public List<OrganizatorDao> GetAllOrganizators()
        {
            var organizators = new List<OrganizatorDao>();
            using (var cmd = new NpgsqlCommand("SELECT * FROM \"Sec\".organizator", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    organizators.Add(new OrganizatorDao
                    {
                        login = reader.GetString(reader.GetOrdinal("login")),
                        pass = reader.GetString(reader.GetOrdinal("pass")),
                        pozition = reader.GetString(reader.GetOrdinal("pozition"))
                    });
                }
            }
            return organizators;
        }

        public List<Tatami> GetAllTatamis()
        {
            var tatamis = new List<Tatami>();
            using (var cmd = new NpgsqlCommand("SELECT * FROM \"Sec\".tatami", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tatamis.Add(new Tatami
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        numdber = reader.GetInt32(reader.GetOrdinal("number")),
                        Activ = reader.GetBoolean(reader.GetOrdinal("is_active"))
                    });
                }
            }
            return tatamis;
        }

        public void SaveMatchResults(Match match)
        {
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"Sec\".matches (group_id, tatami_id, participant1_name, participant2_name, winner_name) " +
                "VALUES (@groupId, @tatamiId, @part1Name, @part2Name, @winnerName)", _connection))
            {
                cmd.Parameters.AddWithValue("@groupId", match.GroupId);
                cmd.Parameters.AddWithValue("@tatamiId", match.tatamiid);
                cmd.Parameters.AddWithValue("@part1Name", match.Participant1.Name);
                cmd.Parameters.AddWithValue("@part2Name", match.Participant2.Name);
                cmd.Parameters.AddWithValue("@winnerName", match.Winner.Name);

                cmd.ExecuteNonQuery();
            }
        }

        public List<GroupDao_2> GetGroupsForTatami(int tatamiId)
        {
            var groups = new List<GroupDao_2>();
            var participants = new List<UkhasnikiDao>();

            using (var cmd = new NpgsqlCommand(
                "SELECT u.* FROM \"Sec\".ukhasniki u " +
                "JOIN \"Sec\".tatami_participants tp ON u.\"Name\" = tp.participant_name " +
                "WHERE tp.tatami_id = @tatamiId", _connection))
            {
                cmd.Parameters.AddWithValue("@tatamiId", tatamiId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        participants.Add(new UkhasnikiDao
                        {
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            SecName = reader.IsDBNull(reader.GetOrdinal("secname")) ? null : reader.GetString(reader.GetOrdinal("secname")),
                            DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("datesorevnovaniy")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("datesorevnovaniy"))),
                            Club = reader.IsDBNull(reader.GetOrdinal("club")) ? null : reader.GetString(reader.GetOrdinal("club")),
                            Adres = reader.IsDBNull(reader.GetOrdinal("adres")) ? null : reader.GetString(reader.GetOrdinal("adres")),
                            Ves = reader.IsDBNull(reader.GetOrdinal("ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ves"))
                        });
                    }
                }
            }

            var weightCategories = participants.Select(p => p.Ves).Distinct().ToList();

            foreach (var weight in weightCategories)
            {
                var groupParticipants = participants.Where(p => p.Ves == weight).ToList();
                var group = new GroupDao_2("Adult", $"{weight} kg", 'M')
                {
                    TatamiId = tatamiId,
                    Participants = groupParticipants
                };
                groups.Add(group);
            }

            return groups;
        }

        public void Updateukhasniki(UkhasnikiDao ukhasniki)
        {
            using (var cmd = new NpgsqlCommand(
                "UPDATE \"Sec\".ukhasniki SET secname = @SecName, club = @Club, adres = @Adres, ves = @Ves " +
                "WHERE \"Name\" = @Name",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", ukhasniki.Name);
                cmd.Parameters.AddWithValue("@SecName", ukhasniki.SecName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Club", ukhasniki.Club ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", ukhasniki.Adres ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Ves", ukhasniki.Ves);
                cmd.ExecuteNonQuery();
            }
        }

        public async Task AddUkhasnikiAsync(UkhasnikiDao ukhasniki)
        {
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"Sec\".ukhasniki (\"Name\", secname, datesorevnovaniy, club, adres, ves) " +
                "VALUES (@Name, @SecName, @DateSorevnovaniy, @Club, @Adres, @Ves)",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", ukhasniki.Name);
                cmd.Parameters.AddWithValue("@SecName", ukhasniki.SecName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DateSorevnovaniy", ukhasniki.DateSorevnovaniy.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@Club", ukhasniki.Club ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", ukhasniki.Adres ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Ves", ukhasniki.Ves);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void DeleteUkhasniki(string name)
        {
            using (var cmd = new NpgsqlCommand(
                "DELETE FROM \"Sec\".ukhasniki WHERE \"Name\" = @Name",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> GetWeightCategories()
        {
            var categories = new List<string>();
            using (var cmd = new NpgsqlCommand(
                "SELECT DISTINCT weight_category FROM \"Sec\".categories", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            return categories;
        }

        public List<string> GetAgeCategories()
        {
            var categories = new List<string>();
            using (var cmd = new NpgsqlCommand(
                "SELECT DISTINCT age_category FROM \"Sec\".categories", _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            return categories;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}