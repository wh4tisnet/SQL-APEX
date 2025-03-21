﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace apexlegends
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=C:\Users\ifoa\Downloads\sql-main\sql-main\bsLite.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            DBLiteCharacters.InitializingNewItem += DBLiteCharacters_InitializingNewItem;
            DBLiteAbilities.InitializingNewItem += DBLiteAbilities_InitializingNewItem;
            DBLiteRoles.InitializingNewItem += DBLiteRoles_InitializingNewItem;
            DBLiteTeam.InitializingNewItem += DBLiteTeam_InitializingNewItem;
            DBLiteCharactersAbilities.InitializingNewItem += DBLiteCharactersAbilities_InitializingNewItem;
        }

        private void LoadData()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    SQLiteCommand command = new SQLiteCommand("SELECT * FROM characters", connection);
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtChar = new DataTable();
                    dataAdapter.Fill(dtChar);
                    DBLiteCharacters.ItemsSource = dtChar.DefaultView;

                    command = new SQLiteCommand("SELECT * FROM abilities", connection);
                    dataAdapter = new SQLiteDataAdapter(command);
                    DataTable dtAbil = new DataTable();
                    dataAdapter.Fill(dtAbil);
                    DBLiteAbilities.ItemsSource = dtAbil.DefaultView;

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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error connecting to SQLite database: " + ex.Message);
                }
            }
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

            insertQuery += ", lastUpdateDate , lastUpdateWho ";

            insertQuery += ") VALUES (";

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

            insertQuery += ", '" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "', '" + Environment.UserName + "'";

            insertQuery += ")";

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

        private int GetNextIdTeam()
        {
            return GetNextId("team");
        }

        private int GetNextIdCharactersAbilities()
        {
            return GetNextId("charactersabilities");
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
            newItem.Row["Id"] = GetNextIdTeam();
        }

        private void DBLiteCharactersAbilities_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataRowView newItem = (DataRowView)e.NewItem;
            newItem.Row["Id"] = GetNextIdCharactersAbilities();
        }
    }
}
