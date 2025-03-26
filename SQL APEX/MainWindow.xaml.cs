using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace apexlegends
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=C:\Users\abel.alvarez\Desktop\bsLite.db";
        private string CharacterImage = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            
            LoadTeams();
            LoadCharacterAbilities();
            LoadCharacters();
            LoadRoles();
            LoadData();

            DBLiteCharacters.InitializingNewItem += DBLiteCharacters_InitializingNewItem;
            DBLiteAbilities.InitializingNewItem += DBLiteAbilities_InitializingNewItem;
            DBLiteRoles.InitializingNewItem += DBLiteRoles_InitializingNewItem;
            DBLiteTeam.InitializingNewItem += DBLiteTeam_InitializingNewItem;
            DBLiteCharactersAbilities.InitializingNewItem += DBLiteCharactersAbilities_InitializingNewItem;
            DBLiteTeamRoster.InitializingNewItem += DBLiteTeamRoster_InitializingNewItem;
        }

        private void LoadData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT *, case when CharacterImage is not null then 'Yes' else 'No(upload)' end as IsImageUploaded FROM characters", connection);
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtChar = new DataTable();
                    dataAdapter.Fill(dtChar);
                    DBLiteCharacters.ItemsSource = dtChar.DefaultView;

                    LoadAbilities();

                    command = new SQLiteCommand("SELECT * FROM roles", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtRole = new DataTable();
                    dataAdapter.Fill(dtRole);
                    DBLiteRoles.ItemsSource = dtRole.DefaultView;

                    command = new SQLiteCommand("SELECT * FROM team", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtTeam = new DataTable();
                    dataAdapter.Fill(dtTeam);
                    DBLiteTeam.ItemsSource = dtTeam.DefaultView;

                    command = new SQLiteCommand("SELECT * FROM charactersabilities", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtCharAbil = new DataTable();
                    dataAdapter.Fill(dtCharAbil);
                    DBLiteCharactersAbilities.ItemsSource = dtCharAbil.DefaultView;

                    command = new SQLiteCommand("SELECT * FROM teamcharacters", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtTeamChar = new DataTable();
                    dataAdapter.Fill(dtTeamChar);
                    DBLiteTeamCharacters.ItemsSource = dtTeamChar.DefaultView;

                    command = new SQLiteCommand("SELECT * FROM teamroster", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtTeamRoster = new DataTable();
                    dataAdapter.Fill(dtTeamRoster);
                    DBLiteTeamRoster.ItemsSource = dtTeamRoster.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to SQLite database: " + ex.Message);
                }
            }
        }

        private void LoadTeams()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT id, teamname FROM team", connection);
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtTeams = new DataTable();
                    dataAdapter.Fill(dtTeams);

                    ComboBoxTeam.ItemsSource = dtTeams.DefaultView;
                    ComboBoxTeam.SelectedValuePath = "id";
                    ComboBoxTeam.DisplayMemberPath = "teamname";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading teams: " + ex.Message);
                }
            }
        }

        private void ComboBoxTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTeam.SelectedItem != null)
            {
                int selectedTeam = int.Parse(ComboBoxTeam.SelectedValue.ToString());
                LoadTeamRoster(selectedTeam);
            }
        }

        private void LoadTeamRoster(int teamid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand($@"SELECT c.id AS CharacterId, c.name AS CharacterName, r.rolename as CharacterRole
                                                        FROM teamcharacters tc
                                                        JOIN characters c ON tc.idCharacters = c.id
                                                        JOIN team t ON tc.idTeam = t.id
                                                        left JOIN roles r on c.IdRole = r.Id
                                                        WHERE tc.IdTeam = {teamid}", connection);

                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtTeamRoster = new DataTable();
                    dataAdapter.Fill(dtTeamRoster);

                    DBLiteTeamRoster.ItemsSource = dtTeamRoster.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading team roster: " + ex.Message);
                }
            }
        }

        private void DBLiteTeamRoster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DBLiteTeamRoster.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)DBLiteTeamRoster.SelectedItem;
                int characterId = Convert.ToInt32(selectedRow["CharacterId"]);

                LoadCharacterAbilitiesForSelectedCharacter(characterId);
            }
        }

        private void LoadCharacterAbilitiesForSelectedCharacter(int characterId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand($@"
                                                        SELECT a.abilityname AS AbilityName,
                                                               a.abilitydescription AS AbilityDescription
                                                        FROM charactersabilities ca
                                                        JOIN abilities a ON ca.Ability_Id = a.id
                                                        WHERE ca.Character_Id = {characterId}", connection);

                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtCharacterAbilities = new DataTable();
                    dataAdapter.Fill(dtCharacterAbilities);

                    DBLiteOpenThen.ItemsSource = dtCharacterAbilities.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading character abilities: " + ex.Message);
                }
            }
        }

        private void LoadCharacterAbilities()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand($@"
                                                        SELECT ca.Character_Id, c.name AS CharacterName, ca.Ability_Id, a.abilityname AS AbilityName
                                                        FROM charactersabilities ca
                                                        JOIN characters c ON ca.Character_Id = c.id
                                                        JOIN abilities a ON ca.ability_Id = a.id", connection);

                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtCharAbil = new DataTable();
                    dataAdapter.Fill(dtCharAbil);

                    DBLiteCharactersAbilities.ItemsSource = dtCharAbil.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading character abilities: " + ex.Message);
                }
            }
        }

        private void LoadCharacters()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand($@"
                                                        SELECT c.id AS CharacterId, 
                                                               c.name AS name, 
                                                               c.description AS description, 
                                                               r.rolename AS IdRole,
                                                               c.CharacterImage
                                                        FROM characters c
                                                        LEFT JOIN roles r ON c.IdRole = r.id", connection);

                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtCharacters = new DataTable();
                    dataAdapter.Fill(dtCharacters);

                    DBLiteCharacters.ItemsSource = dtCharacters.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading characters: " + ex.Message);
                }
            }
        }

        private void LoadRoles()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT id, rolename FROM roles", connection);
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtRoles = new DataTable();
                    dataAdapter.Fill(dtRoles);

                    RolesComboBox.ItemsSource = dtRoles.DefaultView;
                    RolesComboBox.DisplayMemberPath = "rolename";
                    RolesComboBox.SelectedValuePath = "id";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading roles: " + ex.Message);
                }
            }
        }


        private void LoadAbilities()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand(@"
                SELECT a.id AS id, 
                       a.abilityname AS AbilityName,
                       a.abilitydescription AS AbilityDescription,
                       a.abilitytype AS AbilityType
                FROM abilities a", connection);

                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtAbilities = new DataTable();
                    dataAdapter.Fill(dtAbilities);

                    DBLiteAbilities.ItemsSource = dtAbilities.DefaultView;

                    SQLiteCommand typeCommand = new SQLiteCommand(@"
                SELECT DISTINCT a.abilitytype
                FROM abilities a", connection);

                    SQLiteDataAdapter typeAdapter = new SQLiteDataAdapter(typeCommand);
                    DataTable dtAbilityTypes = new DataTable();
                    typeAdapter.Fill(dtAbilityTypes);

                    ComboBoxAbilityType.ItemsSource = dtAbilityTypes.AsEnumerable()
                                                                   .Select(row => row.Field<string>("abilitytype"))
                                                                   .ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading abilities: " + ex.Message);
                }
            }
        }


        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            var selectedRow = DBLiteCharacters.SelectedItem as DataRowView;

            if (openFileDialog.ShowDialog() == true)
            {
                CharacterImage = openFileDialog.FileName;
                MessageBox.Show("Image selected: " + CharacterImage);

                string base64String = Convert.ToBase64String(ConvertImageToBytes(CharacterImage));

                if (selectedRow != null)
                {
                    selectedRow["CharacterImage"] = base64String;
                    UpdateCharacterImage(selectedRow);
                }
            }
            else if (selectedRow != null)
            {
                selectedRow["CharacterImage"] = DBNull.Value;
                imgCharacter.Source = null;
            }
        }

        private void DBLiteCharacters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            imgCharacter.Source = null;

            var selectedRow = DBLiteCharacters.SelectedItem as DataRowView;
           
            if (selectedRow != null)
            {
                UpdateCharacterImage(selectedRow);
            }
        }

        private void UpdateCharacterImage(DataRowView selectedRow)
        {
            string imageBytes = selectedRow["CharacterImage"] as string;

            if (!string.IsNullOrEmpty(imageBytes))
            {             
                byte[] byteArray = Convert.FromBase64String(imageBytes);
                imgCharacter.Source = LoadImageFromBytes(byteArray);
            }
        }

        private byte[] ConvertImageToBytes(string imagePath)
        {
            try
            {
                return File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading image: " + ex.Message);
                return null;
            }
        }

        private BitmapImage LoadImageFromBytes(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (tabCont.SelectedIndex == 0)
            {
                UpdateDataGrid("characters");
            }
            else if (tabCont.SelectedIndex == 1)
            {
                UpdateDataGrid("abilities");
            }
            else if (tabCont.SelectedIndex == 2)
            {
                UpdateDataGrid("roles");
            }
            else if (tabCont.SelectedIndex == 3)
            {
                UpdateDataGrid("team");
            }
            else if (tabCont.SelectedIndex == 4)
            {
                UpdateDataGrid("charactersabilities");
            }
            else if (tabCont.SelectedIndex == 5)
            {
                UpdateDataGrid("teamcharacters");
            }
            else if (tabCont.SelectedIndex == 6)
            {
                UpdateDataGrid("teamroster");
            }
            else
            {
                MessageBox.Show("Incorrect response");
            }
        }

        private void UpdateDataGrid(string tableName)
        {
            var dataTable = ((DataView)GetDataGridByTableName(tableName).ItemsSource).Table;
            List<KeyValuePair<string, string>> paramCollection = new List<KeyValuePair<string, string>>();
            bool AllGood = true;

            foreach (DataRow row in dataTable.Rows)
            {
                paramCollection.Clear();
                foreach (DataColumn col in dataTable.Columns)
                {
                    paramCollection.Add(new KeyValuePair<string, string>("@" + col.ColumnName, row[col.ColumnName].ToString()));
                }

                string Query = string.Empty;
                bool resultado;

                if (row.RowState == DataRowState.Modified)
                    Query = BuildUpdateQuery(tableName, row);
                else if (row.RowState == DataRowState.Added)
                    Query = BuildInsertQuery(tableName, row);

                if (Query == string.Empty) continue;

                resultado = ExecuteQueryWithTransaction(Query, paramCollection);

                if (!resultado)
                {
                    AllGood = false;
                }
            }

            if (AllGood)
                MessageBox.Show("Update successful!");
            else
                MessageBox.Show("Error: update didn't work.");
        }

        private DataGrid GetDataGridByTableName(string tableName)
        {
            if (tableName == "characters") return DBLiteCharacters;
            if (tableName == "abilities") return DBLiteAbilities;
            if (tableName == "roles") return DBLiteRoles;
            if (tableName == "team") return DBLiteTeam;
            if (tableName == "teamcharacters") return DBLiteTeamCharacters;
            if (tableName == "teamroster") return DBLiteTeamRoster;
            return DBLiteCharactersAbilities;
        }

        private string BuildUpdateQuery(string tableName, DataRow row)
        {
            string[] columnNames = GetTableColumnNames(tableName);
            string updateQuery = $"UPDATE {tableName} SET ";

            foreach (var columnName in columnNames)
            {
                if (columnName == "Id") continue;

                if (row[columnName] != DBNull.Value)
                {
                    string columnValue = row[columnName].ToString();
                    updateQuery += $"{columnName} = @{columnName}, ";
                }
            }

            if (updateQuery.EndsWith(", "))
            {
                updateQuery = updateQuery.Substring(0, updateQuery.Length - 2);
            }

            updateQuery += ", lastUpdateDate = '" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "', lastUpdateWho = '" + Environment.UserName + "'";

            updateQuery += " WHERE id = @id";

            return updateQuery;
        }

        private string BuildInsertQuery(string tableName, DataRow row)
        {
            string[] columnNames = GetTableColumnNames(tableName);
            string insertQuery = $"INSERT INTO {tableName} (";

            foreach (var columnName in columnNames)
            {
                if (columnName == "Id") continue;
                if (row[columnName] != DBNull.Value)
                {
                    insertQuery += $"{columnName}, ";
                }
            }

            if (insertQuery.EndsWith(", "))
            {
                insertQuery = insertQuery.Substring(0, insertQuery.Length - 2);
            }

            insertQuery += ", lastUpdateDate , lastUpdateWho) ";

            insertQuery += "VALUES (";

            foreach (var columnName in columnNames)
            {
                if (columnName == "Id") continue;
                if (row[columnName] != DBNull.Value)
                {
                    insertQuery += $"@{columnName}, ";
                }
            }

            if (insertQuery.EndsWith(", "))
            {
                insertQuery = insertQuery.Substring(0, insertQuery.Length - 2);
            }

            insertQuery += ", '" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "', '" + Environment.UserName + "');";

            return insertQuery;
        }

        private bool ExecuteQueryWithTransaction(string updateQuery, List<KeyValuePair<string, string>> parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SQLiteTransaction transaction = connection.BeginTransaction();

                    SQLiteCommand command = new SQLiteCommand(updateQuery, connection, transaction);

                    foreach (var parameter in parameters)
                    {
                        var param = new SQLiteParameter(parameter.Key, parameter.Value);
                        command.Parameters.Add(param);
                    }

                    command.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating database: " + ex.Message);
                    return false;
                }
            }
        }

        private string[] GetTableColumnNames(string tableName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand($"PRAGMA table_info({tableName});", connection);
                SQLiteDataReader reader = command.ExecuteReader();
                var columns = new List<string>();

                while (reader.Read())
                {
                    columns.Add(reader["name"].ToString());
                }

                return columns.ToArray();
            }
        }

        private int GetNextId(string tableName)
        {
            int nextId = 1;

            string query = $"SELECT MAX(id) FROM {tableName}";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, conn))
                {
                    object result = command.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        nextId = Convert.ToInt32(result) + 1;
                    }
                }
            }

            return nextId;
        }

        private int GetNextIdTeamRoster()
        {
            return GetNextId("teamroster");
        }

        private void DBLiteTeamRoster_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextIdTeamRoster();
        }

        private void DBLiteCharacters_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("characters");
        }

        private void DBLiteAbilities_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("abilities");
        }

        private void DBLiteRoles_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("roles");
        }

        private void DBLiteTeam_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("team");
        }

        private void DBLiteCharactersAbilities_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("charactersabilities");
        }

        private void DBLiteTeamCharacters_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextId("teamcharacters");
        }
    }
}

