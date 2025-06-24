using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.Hardik.Connector.Date;
using Npgsql;

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

        public List<UkhasnikiDao> GetAllUkhasnikis() // получение участников
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
                        Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? null : reader.GetString(reader.GetOrdinal("gender")),
                        Ves = reader.IsDBNull(reader.GetOrdinal("ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ves")),
                        IsDisqualified = reader.GetBoolean(reader.GetOrdinal("is_disqualified"))
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
                        Number = reader.GetInt32(reader.GetOrdinal("number")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
                    });
                }
            }
            return tatamis;
        }

        public void SaveMatchResults(Match match)
        {
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"Sec\".matches (groupid, tatamiid, participant1_name, participant2_name, winner_name, loser_name) " +
                "VALUES (@groupId, @tatamiId, @part1Name, @part2Name, @winnerName, @loserName)", _connection))
            {
                cmd.Parameters.AddWithValue("@groupId", match.GroupId);
                cmd.Parameters.AddWithValue("@tatamiId", match.tatamiid);
                cmd.Parameters.AddWithValue("@part1Name", match.Participant1.Name);
                cmd.Parameters.AddWithValue("@part2Name", match.Participant2.Name);
                cmd.Parameters.AddWithValue("@winnerName", match.Winner.Name);
                cmd.Parameters.AddWithValue("@loserName", match.Participant1.Name == match.Winner.Name ? match.Participant2.Name : match.Participant1.Name);

                cmd.ExecuteNonQuery();
            }
        }

        public List<GroupDao_2> GetGroupsForTatami(int tatamiId)
        {
            var groups = new List<GroupDao_2>();

            // Получаем информацию о группах из таблицы groups
            var groupInfos = new List<(int Id, string AgeCategory, string WeightCategory, char Gender)>();
            using (var cmd = new NpgsqlCommand(
                "SELECT id, age_category, weight_category, gender FROM \"Sec\".groups WHERE tatami_id = @tatamiId", _connection))
            {
                cmd.Parameters.AddWithValue("@tatamiId", tatamiId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        groupInfos.Add((
                            reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("age_category")),
                            reader.GetString(reader.GetOrdinal("weight_category")),
                            reader.GetChar(reader.GetOrdinal("gender"))
                        ));
                    }
                }
            }

            // Для каждой группы получаем участников
            foreach (var groupInfo in groupInfos)
            {
                var participants = new List<UkhasnikiDao>();
                using (var cmd = new NpgsqlCommand(
                    "SELECT u.* FROM \"Sec\".ukhasniki u " +
                    "JOIN \"Sec\".tatami_participants tp ON u.\"Name\" = tp.participant_name " +
                    "WHERE tp.tatami_id = @tatamiId AND EXISTS (" +
                    "  SELECT 1 FROM \"Sec\".groups g " +
                    "  WHERE g.id = @groupId AND " +
                    "  u.ves::text LIKE g.weight_category AND " +
                    "  g.gender = u.gender)", _connection))
                {
                    cmd.Parameters.AddWithValue("@tatamiId", tatamiId);
                    cmd.Parameters.AddWithValue("@groupId", groupInfo.Id);
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
                                Ves = reader.IsDBNull(reader.GetOrdinal("ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ves")),
                                Gender = reader.IsDBNull(reader.GetOrdinal("gender")) ? null : reader.GetString(reader.GetOrdinal("gender"))
                            });
                        }
                    }
                }

                var group = new GroupDao_2(groupInfo.AgeCategory, groupInfo.WeightCategory, groupInfo.Gender)
                {
                    Id = groupInfo.Id,
                    TatamiId = tatamiId,
                    Participants = participants,
                    IsCompleted = false // Можно добавить запрос для получения этого значения из БД
                };
                groups.Add(group);
            }

            return groups;
        }

        public void Updateukhasniki(UkhasnikiDao ukhasniki)
        {
            using (var cmd = new NpgsqlCommand(
                "UPDATE \"Sec\".ukhasniki SET secname = @SecName, club = @Club, adres = @Adres, ves = @Ves, gender = @Gender, is_disqualified = @IsDisqualified " +
                "WHERE \"Name\" = @Name",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", ukhasniki.Name);
                cmd.Parameters.AddWithValue("@SecName", ukhasniki.SecName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Club", ukhasniki.Club ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", ukhasniki.Adres ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Ves", ukhasniki.Ves);
                cmd.Parameters.AddWithValue("@Gender", ukhasniki.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsDisqualified", ukhasniki.IsDisqualified);
                cmd.ExecuteNonQuery();
            }
        }

        public async Task AddUkhasnikiAsync(UkhasnikiDao ukhasniki)
        {
            using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"Sec\".ukhasniki (\"Name\", secname, datesorevnovaniy, club, adres, ves, gender, is_disqualified) " +
                "VALUES (@Name, @SecName, @DateSorevnovaniy, @Club, @Adres, @Ves, @Gender, @IsDisqualified)",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", ukhasniki.Name);
                cmd.Parameters.AddWithValue("@SecName", ukhasniki.SecName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DateSorevnovaniy", ukhasniki.DateSorevnovaniy.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@Club", ukhasniki.Club ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Adres", ukhasniki.Adres ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Ves", ukhasniki.Ves);
                cmd.Parameters.AddWithValue("@Gender", ukhasniki.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsDisqualified", ukhasniki.IsDisqualified);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void DeleteUkhasniki(string name)
        {
            // Сначала удаляем связанные записи в tatami_participants
            using (var cmd = new NpgsqlCommand(
                "DELETE FROM \"Sec\".tatami_participants WHERE participant_name = @Name",
                _connection))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.ExecuteNonQuery();
            }

            // Затем удаляем участника
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
                "SELECT DISTINCT weight_category FROM \"Sec\".groups", _connection))
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
                "SELECT DISTINCT age_category FROM \"Sec\".groups", _connection))
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