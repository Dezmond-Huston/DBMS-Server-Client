using System;
using System.IO;
using System.Collections.Generic;

namespace dbServer
{
    public class DBMS
    {
        public static string[] userWantedTable;
        public static string[] fieldsUserWants;
        public static string tableUserWants, conditionUserWants;

       

        public DBMS() { }

        public static void doSQLCommand(string SQLCommand)
        {
            string[] parts = SQLCommand.Split('|');
            int threadID = int.Parse(parts[0]);
            string dataRetrievedFromDBMS = parts[1];

            //Remove any double spaces
            dataRetrievedFromDBMS = dataRetrievedFromDBMS.Replace("  ", " ");
            //Remove all endlines and nextline code
            dataRetrievedFromDBMS = dataRetrievedFromDBMS.Replace("\n", " ");
            dataRetrievedFromDBMS = dataRetrievedFromDBMS.Replace("\r", " ");

            grabAllWhatUserWants(dataRetrievedFromDBMS);

            //Pulling Table and Check if it exist
            pullTable(threadID);
        }


        private static void grabAllWhatUserWants(string dataRetrievedFromDBMS)
        {
            string[] fieldsUserWants_ = StringHelper.getNameFields(dataRetrievedFromDBMS.ToLower());
            for (int i = 0; i < fieldsUserWants_.Length; i++)
            {
                fieldsUserWants_[i] = fieldsUserWants_[i].Replace(" ", "");
            }
            fieldsUserWants = fieldsUserWants_;

            if (dataRetrievedFromDBMS.ToLower().Contains("where"))
                isWhere(dataRetrievedFromDBMS);
            else
                noWhere(dataRetrievedFromDBMS);
        }
        private static void noWhere(string dataRetrievedFromDBMS)
        {
            string tableUserWants_ = StringHelper.getTableNameWithoutWhere(dataRetrievedFromDBMS.ToLower());
            tableUserWants_ = tableUserWants_.Replace(" ", "");
            tableUserWants = tableUserWants_;
        }
        private static void isWhere(string dataRetrievedFromDBMS)
        {
            string tableUserWants_ = StringHelper.getTableName(dataRetrievedFromDBMS.ToLower());
            tableUserWants_ = tableUserWants_.Replace(" ", "");
            tableUserWants = tableUserWants_;

            string conditionUserWants_ = StringHelper.getWhereCondition(dataRetrievedFromDBMS.ToLower());
            conditionUserWants_ = conditionUserWants_.Replace(" ", "");
            conditionUserWants = conditionUserWants_;
        }
        private static void pullTable(int threadID)
        {
            bool tableExist = FileManager.tableExist(tableUserWants);
            if (tableExist)
            {
                userWantedTable = FileManager.getTable(tableUserWants);

                string tableIntoStr = printAsOneString(userWantedTable);

                if (contains(fieldsUserWants, "*"))
                {
                    string rowAfterSplit = StringHelper.splitTableIntoRows(tableIntoStr);
                    Server.loadData(threadID, rowAfterSplit);
                }
                else
                {
                    string requestedTable = newRequestedTable();

                    string rowSplit1 = StringHelper.splitTableIntoRows(requestedTable);

                    Server.loadData(threadID, rowSplit1);
                }
            }
            if (!tableExist)
            {
                
                Server.loadData(threadID, " Table does not exist!\n For help enter 'help...'");
            }
        }


        private static bool contains(string[] commandArray, string symbol)
        {
            foreach (string str in commandArray)
            {
                if (str.Contains(symbol))
                {
                    return true;
                }
            }
            return false;
        }
        private static string newRequestedTable()
        {
            string tableIntoStr = printAsOneString(userWantedTable);

            string[] allLabels = StringHelper.getLabelFromTable(tableIntoStr);

            bool[] labelIndex = new bool[allLabels.Length];
            resetLabels(labelIndex);
            for (int i = 0; i < allLabels.Length; i++)
            {
                for (int j = 0; j < fieldsUserWants.Length; j++)
                {
                    if (allLabels[i] == fieldsUserWants[j])
                    {
                        labelIndex[i] = true;
                    }
                }
            }

            string requestedTable = StringHelper.splitTableIntoNewTable(userWantedTable, labelIndex);
            return requestedTable;
        }
        private static void resetLabels(bool[] labelArray)
        {
            for (int i = 0; i < labelArray.Length; i++)
            {
                labelArray[i] = false;
            }
        }
        private static void print(bool[] Array)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Console.WriteLine("-->" + Array[i].ToString());
            }
        }
        private static void print(string[] Array)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Console.WriteLine("-->" + Array[i]);
            }
        }
        private static void print(int[] Array)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                Console.WriteLine("-->" + Array[i]);
            }
        }
        private static void print(string stg)
        {
            Console.WriteLine(stg);
        }
        private static string printAsOneString(string[] stringArray)
        {
            string p = null;
            for (int i = 0; i < stringArray.Length; i++)
            {
                p = p + "\n" + stringArray[i];
            }
            return p;
        }
    }
}