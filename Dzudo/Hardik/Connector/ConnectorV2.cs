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

        public List<Match> GetAllMatches()
        {
            var matches = new List<Match>();

            using (var cmd = new NpgsqlCommand(
                "SELECT m.*, " +
                "u1.\"Name\" as p1_name, u1.secname as p1_secname, u1.datesorevnovaniy as p1_datesorev, u1.club as p1_club, u1.adres as p1_adres, u1.ves as p1_ves, u1.gender as p1_gender, " +
                "u2.\"Name\" as p2_name, u2.secname as p2_secname, u2.datesorevnovaniy as p2_datesorev, u2.club as p2_club, u2.adres as p2_adres, u2.ves as p2_ves, u2.gender as p2_gender, " +
                "uw.\"Name\" as w_name, uw.secname as w_secname, uw.datesorevnovaniy as w_datesorev, uw.club as w_club, uw.adres as w_adres, uw.ves as w_ves, uw.gender as w_gender, " +
                "ul.\"Name\" as l_name, ul.secname as l_secname, ul.datesorevnovaniy as l_datesorev, ul.club as l_club, ul.adres as l_adres, ul.ves as l_ves, ul.gender as l_gender " +
                "FROM \"Sec\".matches m " +
                "LEFT JOIN \"Sec\".ukhasniki u1 ON m.participant1_name = u1.\"Name\" " +
                "LEFT JOIN \"Sec\".ukhasniki u2 ON m.participant2_name = u2.\"Name\" " +
                "LEFT JOIN \"Sec\".ukhasniki uw ON m.winner_name = uw.\"Name\" " +
                "LEFT JOIN \"Sec\".ukhasniki ul ON m.loser_name = ul.\"Name\"",
                _connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var match = new Match
                    {
                        GroupId = reader.GetInt32(reader.GetOrdinal("groupid")),
                        tatamiid = reader.GetInt32(reader.GetOrdinal("tatamiid")),
                        participant1_name = reader.GetString(reader.GetOrdinal("participant1_name")),
                        participant2_name = reader.GetString(reader.GetOrdinal("participant2_name")),
                        winner_name = reader.IsDBNull(reader.GetOrdinal("winner_name")) ? null : reader.GetString(reader.GetOrdinal("winner_name")),
                        loser_name = reader.IsDBNull(reader.GetOrdinal("loser_name")) ? null : reader.GetString(reader.GetOrdinal("loser_name"))
                    };

                    // Заполняем данные участников
                    match.Participant1 = new UkhasnikiDao
                    {
                        Name = reader.GetString(reader.GetOrdinal("p1_name")),
                        SecName = reader.IsDBNull(reader.GetOrdinal("p1_secname")) ? null : reader.GetString(reader.GetOrdinal("p1_secname")),
                        DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("p1_datesorev")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("p1_datesorev"))),
                        Club = reader.IsDBNull(reader.GetOrdinal("p1_club")) ? null : reader.GetString(reader.GetOrdinal("p1_club")),
                        Adres = reader.IsDBNull(reader.GetOrdinal("p1_adres")) ? null : reader.GetString(reader.GetOrdinal("p1_adres")),
                        Ves = reader.IsDBNull(reader.GetOrdinal("p1_ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("p1_ves")),
                        Gender = reader.IsDBNull(reader.GetOrdinal("p1_gender")) ? null : reader.GetString(reader.GetOrdinal("p1_gender"))
                    };

                    match.Participant2 = new UkhasnikiDao
                    {
                        Name = reader.GetString(reader.GetOrdinal("p2_name")),
                        SecName = reader.IsDBNull(reader.GetOrdinal("p2_secname")) ? null : reader.GetString(reader.GetOrdinal("p2_secname")),
                        DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("p2_datesorev")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("p2_datesorev"))),
                        Club = reader.IsDBNull(reader.GetOrdinal("p2_club")) ? null : reader.GetString(reader.GetOrdinal("p2_club")),
                        Adres = reader.IsDBNull(reader.GetOrdinal("p2_adres")) ? null : reader.GetString(reader.GetOrdinal("p2_adres")),
                        Ves = reader.IsDBNull(reader.GetOrdinal("p2_ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("p2_ves")),
                        Gender = reader.IsDBNull(reader.GetOrdinal("p2_gender")) ? null : reader.GetString(reader.GetOrdinal("p2_gender"))
                    };

                    if (match.winner_name != null)
                    {
                        match.Winner = new UkhasnikiDao
                        {
                            Name = reader.GetString(reader.GetOrdinal("w_name")),
                            SecName = reader.IsDBNull(reader.GetOrdinal("w_secname")) ? null : reader.GetString(reader.GetOrdinal("w_secname")),
                            DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("w_datesorev")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("w_datesorev"))),
                            Club = reader.IsDBNull(reader.GetOrdinal("w_club")) ? null : reader.GetString(reader.GetOrdinal("w_club")),
                            Adres = reader.IsDBNull(reader.GetOrdinal("w_adres")) ? null : reader.GetString(reader.GetOrdinal("w_adres")),
                            Ves = reader.IsDBNull(reader.GetOrdinal("w_ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("w_ves")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("w_gender")) ? null : reader.GetString(reader.GetOrdinal("w_gender"))
                        };
                    }

                    if (match.loser_name != null)
                    {
                        match.Loser = new UkhasnikiDao
                        {
                            Name = reader.GetString(reader.GetOrdinal("l_name")),
                            SecName = reader.IsDBNull(reader.GetOrdinal("l_secname")) ? null : reader.GetString(reader.GetOrdinal("l_secname")),
                            DateSorevnovaniy = reader.IsDBNull(reader.GetOrdinal("l_datesorev")) ? default : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("l_datesorev"))),
                            Club = reader.IsDBNull(reader.GetOrdinal("l_club")) ? null : reader.GetString(reader.GetOrdinal("l_club")),
                            Adres = reader.IsDBNull(reader.GetOrdinal("l_adres")) ? null : reader.GetString(reader.GetOrdinal("l_adres")),
                            Ves = reader.IsDBNull(reader.GetOrdinal("l_ves")) ? 0 : reader.GetDecimal(reader.GetOrdinal("l_ves")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("l_gender")) ? null : reader.GetString(reader.GetOrdinal("l_gender"))
                        };
                    }

                    matches.Add(match);
                }
            }

            return matches;
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