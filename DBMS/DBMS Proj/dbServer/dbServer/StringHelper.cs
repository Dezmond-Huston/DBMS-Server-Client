using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbServer
{


    public class StringHelper
    {
        public StringHelper() { }

        //spliting entries
        public static string[] getNameFields(string CMD)
        {
            string[] p1 = CMD.Split(new String[] { "select" }, StringSplitOptions.None);
            string[] p2 = p1[1].Split(new String[] { "from" }, StringSplitOptions.None);
            return p2[0].Split(',');
        }
        public static string getTableName(string CMD)
        {
            string[] p1 = CMD.Split(new String[] { "from" }, StringSplitOptions.None);
            string[] p2 = p1[1].Split(new String[] { "where" }, StringSplitOptions.None);
            return p2[0];
        }
        public static string getTableNameWithoutWhere(string CMD)
        {
            string[] p1 = CMD.Split(new String[] { "from" }, StringSplitOptions.None);
            return p1[1];
        }
        public static string getWhereCondition(string CMD)
        {
            string[] p1 = CMD.Split(new String[] { "where" }, StringSplitOptions.None);
            return p1[1];
        }

        //spliting the file into parts
        public static string splitTableIntoRows(string tableStr)
        {
            string rtnStr = tableStr.Replace(",", "\n");
            return rtnStr;
        }
        public static string[] getLabelFromTable(string tableStr)
        {
            tableStr = tableStr.Replace("\n", "");
            string[] p1 = tableStr.Split(new String[] { "," }, StringSplitOptions.None);
            string[] labels = p1[0].Split(new String[] { "\t" }, StringSplitOptions.None);//. /// puts space between labels
            return labels;
        }
        public static string splitTableIntoNewTable(string[] tableUserWanted, bool[] requestedFields)
        {
            string[] p1 = tableUserWanted[0].Split(new String[] { "," }, StringSplitOptions.None);
            string rtnStr = null;
            for (int i = 0; i < p1.Length - 1; i++)
            {
                rtnStr = rtnStr + selectSpecificElemetInRow(p1[i], requestedFields) + ",";
            }
            return rtnStr;
        }
        public static string selectSpecificElemetInRow(string row, bool[] specificElement)
        {
            string[] elements = row.Split(new String[] { "\t" }, StringSplitOptions.None);//. splits the elementss so that the columns can be grabbed individually
            for (int i = 0; i < specificElement.Length; i++)
            {
                if (specificElement[i] == false)
                    elements[i] = "";// puts a character before the successive columns
            }
            return string.Join("", elements);//. makes each individually printed column line up evenly with the side of screen
        }
    }
}


