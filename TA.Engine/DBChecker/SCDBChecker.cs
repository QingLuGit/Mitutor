using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA.Engine.Data;
using DBHandler.DBAccess;

namespace TA.Engine.SCChecker
{
    class SCDBChecker
    {
        public List<TestItem> GenerateTestItems(int count)
        {
            string tableName = "scorescripts";

            string sqlquery = "select top " + count.ToString() + " * from dbo." + tableName + " order by newid()";

            string[] targetColumns = { "script" };

            SQLServerAccessor dbAccessor = new SQLServerAccessor();

            List<Dictionary<string, string>> output = dbAccessor.TableSearch(targetColumns, sqlquery);

            List<TestItem> list = new List<TestItem>();
            foreach (Dictionary<string, string> rec in output)
            {
                TestItem item = new TestItem();
                item.question = rec["script"];
                list.Add(item);
            }

            return list;
        }
    }
}
