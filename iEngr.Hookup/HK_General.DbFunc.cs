using iEngr.Hookup.Models;
using iEngr.Hookup.Services;
using iEngr.Hookup.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace iEngr.Hookup
{

    public static partial class HK_General
    {
        internal static OdbcConnection GetConnection()
        {
            try
            {
                // 定义 DSN 名称
                string dsnName = "ComosExt";
                // 创建 OdbcConnection 对象并传入 DSN 连接字符串
                // OdbcConnection connection = new OdbcConnection($"DSN={dsnName};UID=COMOSSH;Pwd=comos#321");
                string connectionString = "Driver={SQL Server};Server=172.18.11.47;Database=ComosExt;Uid=COMOSSH;Pwd=comos#321;";
                OdbcConnection connection = new OdbcConnection(connectionString);
                // 打开数据库连接
                connection.Open();
                // 返回连接对象
                return connection;
            }
            catch (Exception ex)
            {
                // 处理异常
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }
        }
        private static int GetNewID(string tableName)
        {
            string query = $"SELECT  MAX(ID) FROM {tableName}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteScalar() + 1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.GetNewID(string tableName), Error: {ex.Message}");
                }
            }
            return 1;
        }
        internal static bool IsIDExisting(string tableName, int id)
        {
            string query = $"SELECT CASE WHEN EXISTS(SELECT ID FROM {tableName} WHERE ID = {id})" +
                           $" THEN  1 ELSE 0 END AS result";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return ((int)command.ExecuteScalar()==1);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.IsIDExisting(string tableName, int id), Error: {ex.Message}");
                    return false;
                }
            }
        }
        internal static int UpdateLibData(string tableName, int id, string fieldName, object value)
        {
            int count = 0;
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    string query = value == null?
                                   $"UPDATE {tableName} SET " +
                                   $"{fieldName} = Null " +
                                   $"WHERE ID = {id}":
                                   $"UPDATE {tableName} SET " +
                                   $"{fieldName} = '{value}' " +
                                   $"WHERE ID = {id}";                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.UpdateLibData(string tableName, int id, string fieldName, object value)), Error: {ex.Message}");
                }
            }
            return count;
        }
        internal static int DeleteByID(string tableName, int id)
        {
            string query = $"DELETE FROM {tableName} WHERE ID = {id}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.DeleteByID(string tableName, int id), Error: {ex.Message}");
                    return 0;
                }
            }
        }


        #region MatGenLib
        internal static ObservableCollection<MatListItem> UpdateQueryResult(string conditions = null, bool isForced=false)
        {
            ObservableCollection<MatListItem> result = new ObservableCollection<MatListItem>();
            if (!isForced && conditions == null) return result;
            string query = $"select " +
                $"mgl.ID as ID, " +
                $"mgl.CatID as CatID, " +
                $"mgl.NameID as NameID, " +
                $"mn.SpecCn as NameCn, " +
                $"mn.SpecEn as NameEn, " +
                $"mn.TypeP2 as AlterCode, " +
                $"mn.Qty as Qty, " +
                $"mn.Unit as Unit, " +
                $"mn.SupDisc as SupplyDiscipline, " +
                $"mn.SupResp as SupplyResponsible, " +
                $"mn.ErecDisc as ErectionDiscipline, " +
                $"mn.ErecResp as ErectionResponsible, " +
                $"mgl.TypeP1 as TypeP1, " +
                $"mgl.TypeP2 as TypeP2, " +
                $"mgl.SizeP1 as SizeP1, " +
                $"mgl.SizeP2 as SizeP2, " +
                $"mgl.PClass as PClass, " +
                $"mgl.MatMatID as MatMatID, " +
                $"mgl.MoreSpecCn as MoreSpecCn, " +
                $"mgl.MoreSpecEn as MoreSpecEn, " +
                $"mgl.RemarksCn as RemarksCn, " +
                $"mgl.RemarksEn as RemarksEn, " +
                $"mgl.TechSpecMain as TechSpecMain, " +
                $"mgl.TechSpecAux as TechSpecAux, " +
                $"mgl.Status as Status, " +
                $"mgl.MatMatID as MatMatID, " +
                $"mm.SpecCn as SpecMatMatCn, " +
                $"mm.SpecEn as SpecMatMatEn, " +
                $"pn.SpecCn as SpecPN, " +
                $"mgl.Comments as Comments " +
                $"from HK_MatGenLib mgl " +
                $"inner join HK_LibMatName mn on mgl.NameID = mn.ID " +
                $"left join HK_LibMatMat mm on mgl.MatMatID = mm.ID " +
                $"left join HK_LibPN pn on mgl.PClass = pn.ID " +
                $"{conditions} " +
                $"order by mn.SortNum, mm.SortNum, mgl.TypeP1, mgl.SizeP1, mgl.TypeP2, mgl.SizeP2, mgl.TechSpecMain, mgl.TechSpecAux";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MatListItem item = new MatListItem
                            {
                                MatLibItem = new HKMatGenLib()
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    CatID = Convert.ToString(reader["CatID"]),
                                    NameID = Convert.ToString(reader["NameID"]),
                                    TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                    TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                    TypeP1 = Convert.ToString(reader["TypeP1"]),
                                    TypeP2 = Convert.ToString(reader["TypeP2"]),
                                    SizeP1 = Convert.ToString(reader["SizeP1"]),
                                    SizeP2 = Convert.ToString(reader["SizeP2"]),
                                    PClass = Convert.ToString(reader["PClass"]),
                                    MatMatID = Convert.ToString(reader["MatMatID"]),
                                    MoreSpecCn = Convert.ToString(reader["MoreSpecCn"]),
                                    MoreSpecEn = Convert.ToString(reader["MoreSpecEn"]),
                                    RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                    RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                    Status = Convert.ToByte(reader["Status"]),
                                    Comments = Convert.ToString(reader["Comments"]),
                                },
                                AlterCode = Convert.ToString(reader["AlterCode"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                SpecMoreCn = Convert.ToString(reader["MoreSpecCn"]),
                                SpecMoreEn = Convert.ToString(reader["MoreSpecEn"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                MatMatCode = Convert.ToString(reader["MatMatID"]),
                                MatMatCn = Convert.ToString(reader["SpecMatMatCn"]),
                                MatMatEn = Convert.ToString(reader["SpecMatMatEn"]),
                                SpecPClass = Convert.ToString(reader["SpecPN"]),
                                Qty = Convert.ToString(reader["Qty"]),
                                Unit = Convert.ToString(reader["Unit"]),
                                SupplyDiscipline= Convert.ToString(reader["SupplyDiscipline"]),
                                SupplyResponsible= Convert.ToString(reader["SupplyResponsible"]),
                                ErectionDiscipline= Convert.ToString(reader["ErectionDiscipline"]),
                                ErectionResponsible= Convert.ToString(reader["ErectionResponsible"]),
                                ID = Convert.ToInt32(reader["ID"]).ToString("D4"),
                            };
                            item.SpecMainCn = getSpecMainAux(item.MatLibItem.TechSpecMain, 4);
                            item.SpecAuxCn = getSpecMainAux(item.MatLibItem.TechSpecAux, 4);
                            item.SpecPortCn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode,4);
                            item.SpecMainEn = getSpecMainAux(item.MatLibItem.TechSpecMain, 2);
                            item.SpecAuxEn = getSpecMainAux(item.MatLibItem.TechSpecAux, 2);
                            item.SpecPortEn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode,2);
                            result.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.UpdateQueryResult, Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
                return result;
            }
        }
        internal static MatListItem UpdateQueryResult(int ID)
        {
            MatListItem item = new MatListItem();
            // 构建 SQL 查询语句
            string query = $"select " +
                $"mgl.ID as ID, " +
                $"mgl.CatID as CatID, " +
                $"mgl.NameID as NameID, " +
                $"mn.SpecCn as NameCn, " +
                $"mn.SpecEn as NameEn, " +
                $"mn.TypeP2 as AlterCode, " +
                $"mn.Qty as Qty, " +
                $"mn.Unit as Unit, " +
                $"mn.SupDisc as SupplyDiscipline, " +
                $"mn.SupResp as SupplyResponsible, " +
                $"mn.ErecDisc as ErectionDiscipline, " +
                $"mn.ErecResp as ErectionResponsible, " +
                $"mgl.TypeP1 as TypeP1, " +
                $"mgl.TypeP2 as TypeP2, " +
                $"mgl.SizeP1 as SizeP1, " +
                $"mgl.SizeP2 as SizeP2, " +
                $"mgl.PClass as PClass, " +
                $"mgl.MoreSpecCn as MoreSpecCn, " +
                $"mgl.MoreSpecEn as MoreSpecEn, " +
                $"mgl.RemarksCn as RemarksCn, " +
                $"mgl.RemarksEn as RemarksEn, " +
                $"mgl.TechSpecMain as TechSpecMain, " +
                $"mgl.TechSpecAux as TechSpecAux, " +
                $"mgl.Status as Status, " +
                $"mgl.MatMatID as MatMatID, " +
                $"mm.SpecCn as SpecMatMatCn, " +
                $"mm.SpecEn as SpecMatMatEn, " +
                $"pn.SpecCn as SpecPN, " +
                $"mgl.Comments as Comments " +
                $"from HK_MatGenLib mgl " +
                $"inner join HK_LibMatName mn on mgl.NameID = mn.ID " +
                $"left join HK_LibMatMat mm on mgl.MatMatID = mm.ID " +
                $"left join HK_LibPN pn on mgl.PClass = pn.ID " +
                $"WHERE mgl.ID = {ID}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            item = new MatListItem
                            {
                                MatLibItem = new HKMatGenLib()
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    CatID = Convert.ToString(reader["CatID"]),
                                    NameID = Convert.ToString(reader["NameID"]),
                                    TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                    TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                    TypeP1 = Convert.ToString(reader["TypeP1"]),
                                    TypeP2 = Convert.ToString(reader["TypeP2"]),
                                    SizeP1 = Convert.ToString(reader["SizeP1"]),
                                    SizeP2 = Convert.ToString(reader["SizeP2"]),
                                    PClass = Convert.ToString(reader["PClass"]),
                                    MatMatID = Convert.ToString(reader["MatMatID"]),
                                    MoreSpecCn = Convert.ToString(reader["MoreSpecCn"]),
                                    MoreSpecEn = Convert.ToString(reader["MoreSpecEn"]),
                                    RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                    RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                    Status = Convert.ToByte(reader["Status"]),
                                    Comments = Convert.ToString(reader["Comments"]),
                                },
                                AlterCode = Convert.ToString(reader["AlterCode"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                SpecMoreCn = Convert.ToString(reader["MoreSpecCn"]),
                                SpecMoreEn = Convert.ToString(reader["MoreSpecEn"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                MatMatCode = Convert.ToString(reader["MatMatID"]),
                                MatMatCn = Convert.ToString(reader["SpecMatMatCn"]),
                                MatMatEn = Convert.ToString(reader["SpecMatMatEn"]),
                                SpecPClass = Convert.ToString(reader["SpecPN"]),
                                Qty = Convert.ToString(reader["Qty"]),
                                Unit = Convert.ToString(reader["Unit"]),
                                SupplyDiscipline = Convert.ToString(reader["SupplyDiscipline"]),
                                SupplyResponsible = Convert.ToString(reader["SupplyResponsible"]),
                                ErectionDiscipline = Convert.ToString(reader["ErectionDiscipline"]),
                                ErectionResponsible = Convert.ToString(reader["ErectionResponsible"]),
                                ID = Convert.ToInt32(reader["ID"]).ToString("D4"),
                            };
                            item.SpecMainCn = getSpecMainAux(item.MatLibItem.TechSpecMain, 4);
                            item.SpecAuxCn = getSpecMainAux(item.MatLibItem.TechSpecAux, 4);
                            item.SpecPortCn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode, 4);
                            item.SpecMainEn = getSpecMainAux(item.MatLibItem.TechSpecMain, 2);
                            item.SpecAuxEn = getSpecMainAux(item.MatLibItem.TechSpecAux, 2);
                            item.SpecPortEn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode, 2);
                        }
                        return item;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.UpdateQueryResult(ID), Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
                return null;
            }
        }
        public static int DataUpdate(int ID, string matData)
        {
            if (matData == null || ID == 0) return 0;
            var arrMatData = matData.Split(',').ToArray<string>();
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 0:CatID,1:NameID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatMatID, 14,Status
                    string query = $"UPDATE HK_MatGenLib " +
                                   $"SET CatID='{arrMatData[1].Substring(0, 2)}'," +
                                   $"NameID='{arrMatData[1]}'," +
                                   $"TechSpecMain='{arrMatData[2].Replace('|', ',')}'," +
                                   $"TechSpecAux='{arrMatData[3].Replace('|', ',')}'," +
                                   $"TypeP1='{arrMatData[4]}'," +
                                   $"SizeP1='{arrMatData[5]}'," +
                                   $"TypeP2='{arrMatData[6]}'," +
                                   $"SizeP2='{arrMatData[7]}'," +
                                   $"MoreSpecCn=N'{arrMatData[8]}'," +
                                   $"MoreSpecEn=N'{arrMatData[9]}'," +
                                   $"RemarksCn=N'{arrMatData[10]}'," +
                                   $"RemarksEn=N'{arrMatData[11]}'," +
                                   $"PClass='{(arrMatData[2] + "|" + arrMatData[3]).Split('|').FirstOrDefault(x => x.StartsWith("PN"))?.Split(':')[1]}'," +
                                   $"MatMatID='{arrMatData[13]}'," +
                                   $"Status = 1, " +
                                   $"LastBy='{UserName}'," +
                                   $"LastOn = '{DateTime.Now.ToString()}' " +
                                   $"WHERE ID = {ID}";
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.DataUpdate, Error: {ex.Message}");
                    return 0;
                }
            }
        }
        internal static int DataDelMark(int ID)
        {
            // 构建 SQL 查询语句
            string query = $"UPDATE HK_MatGenLib " +
                           $"SET Status = Status - 1, LastBy = '{UserName}', LastOn = '{DateTime.Now.ToString()}' " +
                           $"WHERE ID = {ID}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.DataDelMark, Error: {ex.Message}");
                }
            }
            return 0;
        }
        internal static int DataDel(int ID)
        {
            // 构建 SQL 查询语句
            string query = $"DELETE FROM HK_MatGenLib WHERE ID = {ID}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.DataDel, Error: {ex.Message}");
                }
            }
            return 0;
        }
        internal static int CountExistingData(string conditions = null)
        {
            string query = $"SELECT  COUNT(*) FROM HK_MatGenLib mgl {conditions}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.CountExistingData, Error: {ex.Message}");
                    return -1;
                }
            }
        }
        internal static int NewDataAdd(string matData, out int newID)
        {
            newID = GetNewID();
            if (matData == null) return 0;
            var arrMatData = matData.Split(',').ToArray<string>();
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 0:CatID,1:NameID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatMatID, 14,Status
                    string query = $"INSERT INTO HK_MatGenLib (ID, CatID, NameID, TechSpecMain, TechSpecAux, " +
                                            $"TypeP1, SizeP1, TypeP2, SizeP2, " +
                                            $"MoreSpecCn, MoreSpecEn, RemarksCn, RemarksEn, " +
                                            $"PClass, MatMatID, Status, LastBy, LastOn) VALUES (" +
                                            $"{newID}," +
                                            $"'{arrMatData[1].Substring(0, 2)}'," +
                                            $"'{arrMatData[1]}'," +
                                            $"'{arrMatData[2].Replace('|', ',')}'," +
                                            $"'{arrMatData[3].Replace('|', ',')}'," +
                                            $"'{arrMatData[4]}'," +
                                            $"'{arrMatData[5]}'," +
                                            $"'{arrMatData[6]}'," +
                                            $"'{arrMatData[7]}'," +
                                            $"N'{arrMatData[8]}'," +
                                            $"N'{arrMatData[9]}'," +
                                            $"N'{arrMatData[10]}'," +
                                            $"N'{arrMatData[11]}'," +
                                            $"'{(arrMatData[2] + "|" + arrMatData[3]).Split('|').FirstOrDefault(x => x.StartsWith("PN"))?.Split(':')[1]}'," +
                                            $"'{arrMatData[13]}'," +
                                            $"1, " +
                                            $"'{UserName}'," +
                                            $"'{DateTime.Now.ToString()}'" +
                                            $")";
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.NewDataAdd, Error: {ex.Message}");
                    //MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                    return 0;
                }
            }
        }
        private static int GetNewID()
        {
            string query = $"SELECT  MAX(ID) FROM HK_MatGenLib";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteScalar() + 1;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.GetNewID, Error: {ex.Message}");
                }
            }
            return 0;
        }
        private static string getSpecMainAux(string input, int language = 4)
        {
            string result = string.Empty;
            string results = string.Empty;
            if (string.IsNullOrEmpty(input)) return string.Empty;
            List<string> segments = input.Split(',').ToList();
            try
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    result = string.Empty;
                    string name = segments[i].Trim().Split(':')?[0];
                    string value = segments[i].Trim().Split(':')?[1];
                    if (string.IsNullOrEmpty(value))
                        continue;
                    else
                    {
                        if (dicSpecDic.ContainsKey(name))
                        {
                            string prefix = dicSpecDic[name].PrefixCn;
                            string suffix = dicSpecDic[name].SuffixCn;
                            if (language == 2)
                            {
                                prefix = dicSpecDic[name].PrefixEn;
                                suffix = dicSpecDic[name].SuffixEn;
                            }
                            if (dicSpecDic[name].Link.StartsWith("LibPipeOD"))
                                result = $"{prefix}{getPipeData(value, dicSpecDic[name].Link.Split(',')[1])}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibPN"))
                                result = $"{prefix}{((language == 2) ? dicPN[value].SpecEn : dicPN[value].SpecCn)}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGenOption"))
                                result = $"{prefix}{((language == 2) ? dicGenOption[value].SpecEn : dicGenOption[value].SpecCn)}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGland"))
                                result = $"{prefix}{((language == 2) ? dicGland[value].SpecEn : dicGland[value].SpecCn)}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibThread"))
                                result = $"{prefix}{((language == 2) ? dicThread[value].SpecEn : dicThread[value].SpecCn)}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibTubeOD"))
                                result = $"{prefix}{((language == 2) ? dicTubeOD[value].SpecEn : dicTubeOD[value].SpecCn)}{suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibSteel"))
                                result = $"{prefix}{getSteelData(value, dicSpecDic[name].Link.Split(',')[1],language)}{suffix}";
                            else
                                result = $"{prefix}{value}{suffix}";
                        }
                    }
                    results = string.IsNullOrEmpty(results) ? result : (string.IsNullOrEmpty(result) ? results : results + ", " + result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"___HK_General.getSpecMainAux, Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
            return results;
        }
        private static string getSpecPort(string typeP1, string sizeP1, string typeP2, string sizeP2, string alterCode = "", int language = 4)
        {
            string result1 = string.Empty; //Port1
            string result2 = string.Empty; //Port2
            try
            {
                if (!string.IsNullOrEmpty(typeP1))
                {
                    if (string.IsNullOrEmpty(sizeP1))
                        result1 = (language == 2)?dicPortType[typeP1].NameEn: dicPortType[typeP1].NameCn;
                    else if (dicPortType.ContainsKey(typeP1))
                    {
                        string prefix = dicPortType[typeP1].PrefixCn;
                        string suffix = dicPortType[typeP1].SuffixCn;
                        if (language == 2)
                        {
                            prefix = dicPortType[typeP1].PrefixEn;
                            suffix = dicPortType[typeP1].SuffixEn;
                        }
                        if (dicPortType[typeP1].Link.StartsWith("LibPipeOD"))
                            result1 = $"{prefix}{getPipeData(sizeP1, dicPortType[typeP1].Link.Split(',')[1])}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibPN"))
                            result1 = $"{prefix}{((language == 2) ? dicPN[sizeP1].SpecEn : dicPN[sizeP1].SpecCn)}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGenOption"))
                            result1 = $"{prefix}{((language == 2) ? dicGenOption[sizeP1].SpecEn : dicGenOption[sizeP1].SpecCn)}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGland"))
                            result1 = $"{prefix}{((language == 2) ? dicGland[sizeP1].SpecEn : dicGland[sizeP1].SpecCn)}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibThread"))
                            result1 = $"{prefix}{((language == 2) ? dicThread[sizeP1].SpecEn : dicThread[sizeP1].SpecCn)}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibTubeOD"))
                            result1 = $"{prefix}{((language == 2) ? dicTubeOD[sizeP1].SpecEn : dicTubeOD[sizeP1].SpecCn)}{suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibSteel"))
                            result1 = $"{prefix}{getSteelData(sizeP1, dicPortType[typeP1].Link.Split(',')[1],language)}{suffix}";
                        else
                            result1 = $"{prefix}{((language == 2) ? dicPN[sizeP1].SpecEn : dicPN[sizeP1].SpecCn)}{suffix}";
                    }
                }
                if (!string.IsNullOrEmpty(typeP2))
                {
                    if (string.IsNullOrEmpty(sizeP2))
                        result2 = (language == 2) ? dicPortType[typeP2].NameEn : dicPortType[typeP2].NameCn;
                    else if (dicPortType.ContainsKey(typeP2))
                    {
                        string prefix = dicPortType[typeP2].PrefixCn;
                        string suffix = dicPortType[typeP2].SuffixCn;
                        if (language == 2)
                        {
                            prefix = dicPortType[typeP2].PrefixEn;
                            suffix = dicPortType[typeP2].SuffixEn;
                        }
                        if (dicPortType[typeP2].Link.StartsWith("LibPipeOD"))
                            result2 = $"{prefix}{getPipeData(sizeP2, dicPortType[typeP2].Link.Split(',')[1])}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibPN"))
                            result2 = $"{prefix}{((language == 2) ? dicPN[sizeP2].SpecEn : dicPN[sizeP2].SpecCn)}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGenOption"))
                            result2 = $"{prefix}{((language == 2) ? dicGenOption[sizeP2].SpecEn : dicGenOption[sizeP2].SpecCn)}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGland"))
                            result2 = $"{prefix}{((language == 2) ? dicGland[sizeP2].SpecEn : dicGland[sizeP2].SpecCn)}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibThread"))
                            result2 = $"{prefix}{((language == 2) ? dicThread[sizeP2].SpecEn : dicThread[sizeP2].SpecCn)}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibTubeOD"))
                            result2 = $"{prefix}{((language == 2) ? dicTubeOD[sizeP2].SpecEn : dicTubeOD[sizeP2].SpecCn)}{suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibSteel"))
                            result2 = $"{prefix}{getSteelData(sizeP2, dicPortType[typeP2].Link.Split(',')[1], language)}{suffix}";
                        else
                            result2 = $"{prefix}{((language == 2) ? dicPN[sizeP2].SpecEn : dicPN[sizeP2].SpecCn)}{suffix}";
                    }
                }
                if (alterCode == "AS1" || alterCode == "DF1" && result1 == result2)
                    result2 = string.Empty;
                return string.IsNullOrEmpty(result2) ? result1 : (string.IsNullOrEmpty(result1) ? result2 : result1 + " - " + result2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"___HK_General.getSpecPort, Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
        }
        private static string getPipeData(string input, string key = "")
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key == "DN")
                return dicPipeOD[input]?.DN;
            else if (key == "NPS")
                return dicPipeOD[input]?.NPS;
            else if (key == "HGIa")
                return dicPipeOD[input]?.HGIa?.ToString();
            else if (key == "HGIb")
                return dicPipeOD[input]?.HGIb?.ToString();
            else if (key == "HGII")
                return dicPipeOD[input]?.HGII?.ToString();
            else if (key == "GBI")
                return dicPipeOD[input]?.GBI?.ToString();
            else if (key == "GBII")
                return dicPipeOD[input]?.GBII?.ToString();
            else if (key == "ISO")
                return dicPipeOD[input]?.ISO?.ToString();
            else if (key == "ASME")
                return dicPipeOD[input]?.ASME?.ToString();
            else if (key == "SWDiaGB")
                return dicPipeOD[input]?.SWDiaGB?.ToString();
            else
                return dicPipeOD[input]?.DN;
        }
        private static string getSteelData(string input, string key = "", int language = 4)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key == "CSSpec")
                return (language == 2) ? dicSteel[input]?.CSSpecEn : dicSteel[input]?.CSSpecCn;
            else if (key == "IBSpec")
                return (language == 2) ? dicSteel[input]?.CSSpecEn : dicSteel[input]?.CSSpecCn;
            else if (key == "CSSpecCN")
                return dicSteel[input]?.CSSpecCn;
            else if (key == "CSSpecEn")
                return dicSteel[input]?.CSSpecEn;
            else if (key == "IBSpecCn")
                return dicSteel[input]?.IBSpecCn;
            else if (key == "IBSpecEn")
                return dicSteel[input]?.IBSpecEn;
            else if (key == "CSb")
                return dicSteel[input]?.CSb?.ToString();
            else if (key == "CSd")
                return dicSteel[input]?.CSd?.ToString();
            else if (key == "IBb")
                return dicSteel[input]?.IBb?.ToString();
            else if (key == "IBd")
                return dicSteel[input]?.IBd?.ToString();
            else
                return string.Empty;
        }
        #endregion

        #region TreeNode
        internal static int NewNodeAdd(HkTreeItem item, int count =0)
        {
            if (item == null || item.Parent == null) return 0;
            string tableName = "HK_TreeNode";
            int newID = GetNewID(tableName);
            item.ID = newID.ToString();
            using (OdbcConnection conn = GetConnection())
            {
                string diagramID = int.TryParse(item.DiagID, out int diagID) ? diagID.ToString() : "Null";
                try
                {
                    string query = $"INSERT INTO {tableName} (ID, NodeName, NodeValue, Name, " +
                                            $"DiagID, PicturePath, Properties, IsExpanded, " +
                                            $"ParentID, Status, IndexOf, LastBy, LastOn) VALUES (" +
                                            $"{newID}," +
                                            $"'{item.NodeName}'," +
                                            $"'{item.NodeValue}'," +
                                            $"'{item.Name}'," +
                                            $"{diagramID}," +
                                            $"'{item.PicturePath}'," +
                                            $"'{item.PropertiesString}'," +
                                            $"'{item.IsExpanded}'," +
                                            $"'{item.Parent.ID}'," +
                                            $"1," +
                                            $"{item.Parent.Children.IndexOf(item)}," +
                                            $"'{UserName}'," +
                                            $"'{DateTime.Now.ToString()}'" +
                                            $")";
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        count += command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.NewNodeAdd(HkTreeItem item, int count =0), Error: {ex.Message}");
                    //MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            foreach (var subItem in item.Children)
            {
                UpdateNode(subItem,count);
            }
            return count;
        }
        internal static int UpdateNode(HkTreeItem item, int count=0, bool IsRecursive = false)
        {
            if (item == null) return 0;
            if (!int.TryParse(item.ID, out int id))
            {
                return NewNodeAdd(item,count);
            }
            string tableName = "HK_TreeNode";
            if (!IsIDExisting(tableName, id))
            {
                item.ID = null;
                return NewNodeAdd(item, count);
            }
            using (OdbcConnection conn = GetConnection())
            {
                //string diagramID = int.TryParse(item.DiagID, out int diagID) ? diagID.ToString() : "Null";
                string parentID = int.TryParse(item.Parent?.ID, out int parent_id) ? parent_id.ToString() : "Null";
                string diagID = item.DiagID == null ? "Null": "'"+item.DiagID+"'";
                try
                {
                    string query = $"UPDATE {tableName} SET " +
                                               $"NodeName = '{item.NodeName}'," +
                                               $"NodeValue = '{item.NodeValue}'," +
                                               $"Name = '{item.Name}'," +
                                               $"DiagID = {diagID}," +
                                               $"PicturePath = '{item.PicturePath}'," +
                                               $"Properties = '{item.PropertiesString}'," +
                                               $"IsExpanded = '{item.IsExpanded}'," +
                                               $"ParentID = {parentID}," +
                                               $"Status = 1," +
                                               $"IndexOf = {(item.Parent == null? 0: item.Parent.Children.IndexOf(item))}," +
                                               $"LastBy='{UserName}'," +
                                               $"LastOn = '{DateTime.Now.ToString()}' " +
                                               $"WHERE ID = {id}";                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        count += command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.UpdateNode(HkTreeItem item, int count=0), Error: {ex.Message}");
                    //MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            if (IsRecursive)
            {
                foreach (var subItem in item.Children)
                {
                    UpdateNode(subItem, count, true);
                }
            }
            return count;
        }
        internal static int UpdateIndexOf(HkTreeItem item)
        {
            if ((item == null || item.Parent == null)) return 0;
            int count = 0;
            string tableName = "HK_TreeNode";
            foreach (var subItem in item.Parent.Children)
            {
                if (int.TryParse(subItem.ID, out int id))
                {
                    using (OdbcConnection conn = GetConnection())
                    {
                        try
                        {
                            string query = $"UPDATE {tableName} SET " +
                                           $"IndexOf = {subItem.Parent.Children.IndexOf(subItem)} " +
                                           $"WHERE ID = {id}";                    // 创建并配置 OdbcCommand 对象
                            using (OdbcCommand command = new OdbcCommand(query, conn))
                            {
                                // 执行查询，获取记录数
                                count += command.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            // 处理异常
                            Debug.WriteLine($"___HK_General.UpdateIndexOf(HkTreeItem item), Error: {ex.Message}");
                        }
                    }
                }
            }
            return count;
        }
        internal static int NodeDelete(string tableName, int id)
        {
            string query = $"DELETE FROM {tableName} WHERE ID = {id}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.NodeDelete(int id), Error: {ex.Message}");
                    return 0;
                }
            }
        }
        internal static int NodeDelete(HkTreeItem item)
        {
            if (item == null) return 0;
                foreach (var childItem in item.Children)
                {
                    childItem.Parent = item.Parent;
                    UpdateNode(childItem);
                }
            // 构建 SQL 查询语句
            if (int.TryParse(item.ID, out int id))
            {
                return NodeDelete("HK_TreeNode",id);
            }
            return 0;
        }
        internal static int NodeDelete(HkTreeItem item, ref int count)
        {
            if (item == null) return count;
            foreach (var childItem in item.Children)
            {
                NodeDelete(childItem, ref count);
            }
            // 构建 SQL 查询语句
            if (int.TryParse(item.ID, out int id))
            {
                count += NodeDelete("HK_TreeNode", id);
            }
            return count;

        }
        internal static List<HkTreeItem> GetAllTreeNodeItems()
        {
            List<HkTreeItem> result = new List<HkTreeItem>();
            string query = $"select * " +
                $"from HK_TreeNode " +
                $"where Status >=0 " +
                $"order by IndexOf";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HkTreeItem item = new HkTreeItem
                            {
                                ID = Convert.ToString(reader["ID"]),
                                NodeName = Convert.ToString(reader["NodeName"]),
                                NodeValue = Convert.ToString(reader["NodeValue"]),
                                Name = Convert.ToString(reader["Name"]),
                                DiagID = Convert.IsDBNull(reader["DiagID"])? null: Convert.ToString(reader["DiagID"]),
                                PicturePath = Convert.ToString(reader["PicturePath"]),
                                PropertiesString = Convert.ToString(reader["Properties"]),
                                IsExpanded = Convert.ToBoolean(reader["IsExpanded"]),
                                ParentID = Convert.ToString(reader["ParentID"]),
                                Status = Convert.ToByte(reader["Status"]),
                                LastOn = Convert.ToDateTime(reader["LastOn"]),
                                LastBy = Convert.ToString(reader["PicturePath"]),
                            };
                            result.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General. GetAllTreeNodeItems(), Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
                return result;
            }
        }
        internal static bool IsIDAssigned(int id)
        {
            string query = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM HK_TreeNode WHERE ',' + DiagID + ',' LIKE '%,{id},%')" +
                           $" THEN  1 ELSE 0 END AS result";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return ((int)command.ExecuteScalar() == 1);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.IsIDAssigned(int id), Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Hk_Diagram
        internal static int NewDiagAdd(HkTreeItem item)
        {
            if (item == null || item.Parent == null) return 0;
            string tableName = "HK_Diagram";
            int newID = GetNewID(tableName);
            using (OdbcConnection conn = GetConnection())
            {
                ObservableCollection<LabelDisplay> propLabelItems = GetPropLabelItems(item);
                string desc = string.Join(",", propLabelItems.Select(x => x.DisplayName + ":" + x.DisplayValue1));
                string name = item.DisPlayName;
                try
                {


                    string query = $"INSERT INTO {tableName} (ID, NameCn, NameEn, DescCn, " +
                                            $"DescEn, PicturePath, RemarksCn, RemarksEn, " +
                                            $"Status, LastBy, LastOn) VALUES (" +
                                            $"{newID}," +
                                            $"'{name}'," +
                                            $"'{name}'," +
                                            $"'{desc}'," +
                                            $"'{desc}'," +
                                            $"'{item.ActivePicturePath}'," +
                                            $"Null," +
                                            $"Null," +
                                            $"1," +
                                            $"'{UserName}'," +
                                            $"'{DateTime.Now.ToString()}'" +
                                            $")";
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.NewDiagAdd(HkTreeItem item), Error: {ex.Message}");
                    return 0;
                    //MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            return newID;
        }
        internal static ObservableCollection<DiagramItem> GetDiagramItems()
        {
            ObservableCollection<DiagramItem> diagramItems = new ObservableCollection<DiagramItem>();
            string query = $"select diag.* , " +
                $"(SELECT COUNT(*) FROM HK_DiagBom bom WHERE bom.DiagID = diag.ID) as BomCount " +
                $"from HK_Diagram diag " +
                $"where Status >=0";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DiagramItem item = new DiagramItem
                            {
                                IsLibItem = true,
                                ID = Convert.ToInt32(reader["ID"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                DescCn = Convert.ToString(reader["DescCn"]),
                                DescEn = Convert.ToString(reader["DescEn"]),
                                PicturePath = Convert.ToString(reader["PicturePath"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                Status = Convert.ToByte(reader["Status"]),
                                LastOn = Convert.ToDateTime(reader["LastOn"]),
                                LastBy = Convert.ToString(reader["PicturePath"]),
                                BomQty = Convert.ToString(reader["BomCount"]),
                            };
                            diagramItems.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.GetDiagramItems(string idsString), Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            return diagramItems;
        }
        internal static DiagramItem GetDiagramItem(string id)
        {
            DiagramItem item = null;
            if (int.TryParse(id, out int intID))
            {
                string query = $"select diag.* " +
                   $"from HK_Diagram diag " +
                   $"where Id = {intID}";
                using (OdbcConnection conn = GetConnection())
                {
                    try
                    {
                        using (OdbcCommand command = new OdbcCommand(query, conn))
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                item = new DiagramItem
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    NameCn = Convert.ToString(reader["NameCn"]),
                                    NameEn = Convert.ToString(reader["NameEn"]),
                                    DescCn = Convert.ToString(reader["DescCn"]),
                                    DescEn = Convert.ToString(reader["DescEn"]),
                                    PicturePath = Convert.ToString(reader["PicturePath"]),
                                    RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                    RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                    Status = Convert.ToByte(reader["Status"]),
                                    LastOn = Convert.ToDateTime(reader["LastOn"]),
                                    LastBy = Convert.ToString(reader["PicturePath"]),
                                };
                                return item;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"___HK_General.GetDiagramItems(string id), Error: {ex.Message}");
                        // 可以选择返回空列表或者其他适当的处理
                    }
                }
            }
            return item;
        }
        internal static ObservableCollection<DiagramItem> GetDiagramItems(string idsString, bool isOwned, bool isInherit = false)
        {
            ObservableCollection<DiagramItem> diagramItems = new ObservableCollection<DiagramItem>();
            if (string.IsNullOrEmpty(idsString)) return diagramItems;
            string query = $"select diag.* " +
               $"from HK_Diagram diag " +
               $"inner join STRING_SPLIT('{idsString}', ',') s  ON diag.ID = TRY_CAST(s.value AS INT) " +
               $"where Status >=0";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DiagramItem item = new DiagramItem
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                DescCn = Convert.ToString(reader["DescCn"]),
                                DescEn = Convert.ToString(reader["DescEn"]),
                                PicturePath = Convert.ToString(reader["PicturePath"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                Status = Convert.ToByte(reader["Status"]),
                                LastOn = Convert.ToDateTime(reader["LastOn"]),
                                LastBy = Convert.ToString(reader["PicturePath"]),
                                IsOwned=isOwned,
                                IsInherit = isInherit,
                            };
                            diagramItems.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.GetDiagramItems(string idsString), Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            return diagramItems;
        }
        internal static int UpdateDiagram(int id, string fieldName, object value)
        {
            int count = 0;
            string tableName = "HK_Diagram";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    string query = $"UPDATE {tableName} SET " +
                                   $"{fieldName} = '{value}' " +
                                   $"WHERE ID = {id}";                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.UpdateDiagram(int id, string fieldName, object value), Error: {ex.Message}");
                }
            }
            return count;
        }
        #endregion

        #region HK_DiagBom
        internal static ObservableCollection<BomItem> GetDiagBomItems(string id)
        {
            ObservableCollection<BomItem> diagBomItems = new ObservableCollection<BomItem>();
            if (int.TryParse(id, out int diagID))
            {
                string query = $"select bom.*, mn.SpecCn as NameCn, mn.SpecEn as NameEn, " +
                               $"mgl.TechSpecMain as TechSpecMain, mgl.TechSpecAux as TechSpecAux, " +
                               $"mgl.TypeP1 as TypeP1, mgl.TypeP2 as TypeP2, " +
                               $"mgl.SizeP1 as SizeP1, mgl.SizeP2 as SizeP2, " +
                               $"mgl.MatMatID as MatMatID, mgl.CatID as CatID, mgl.NameID as NameID, " +
                               $"mgl.MoreSpecCn as LibMoreSpecCn, mgl.MoreSpecEn as LibMoreSpecEn, " +
                               $"mgl.RemarksCn as LibRemarksCn , mgl.Remarksen as LibRemarksEn, " +
                               $"mgl.PClass as PClass, mgl.Status as Status," +
                               $"mgl.Comments as Comments, pn.SpecCn as SpecPN " +
                               $"from HK_DiagBom bom " +
                               $"inner join HK_MatGenLib mgl on bom.MatLibID = mgl.ID " +
                               $"left join HK_LibMatName mn on mn.ID = mgl.NameID " +
                               $"left join HK_LibPN pn on mgl.PClass = pn.ID " +
                               $"where DiagID = {diagID} " +
                               $"order by No";
                using (OdbcConnection conn = GetConnection())
                {
                    try
                    {
                        using (OdbcCommand command = new OdbcCommand(query, conn))
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BomItem item = new BomItem
                                {
                                    MatLibItem = new HKMatGenLib()
                                    {
                                        ID = Convert.ToInt32(reader["MatLibID"]),
                                        CatID = Convert.ToString(reader["CatID"]),
                                        NameID = Convert.ToString(reader["NameID"]),
                                        TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                        TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                        TypeP1 = Convert.ToString(reader["TypeP1"]),
                                        TypeP2 = Convert.ToString(reader["TypeP2"]),
                                        SizeP1 = Convert.ToString(reader["SizeP1"]),
                                        SizeP2 = Convert.ToString(reader["SizeP2"]),
                                        PClass = Convert.ToString(reader["PClass"]),
                                        MatMatID = Convert.ToString(reader["MatMatID"]),
                                        MoreSpecCn = Convert.ToString(reader["LibMoreSpecCn"]),
                                        MoreSpecEn = Convert.ToString(reader["LibMoreSpecEn"]),
                                        RemarksCn = Convert.ToString(reader["LibRemarksCn"]),
                                        RemarksEn = Convert.ToString(reader["LibRemarksEn"]),
                                        Status = Convert.ToByte(reader["Status"]),
                                        Comments = Convert.ToString(reader["Comments"]),
                                    },
                                    BomID = Convert.ToInt32(reader["ID"]),
                                    No = Convert.ToString(reader["No"]),
                                    NameCn = Convert.ToString(reader["NameCn"]),
                                    NameEn = Convert.ToString(reader["NameEn"]),
                                    RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                    RemarksEn = Convert.ToString(reader["RemarksEn"]),
                                    SpecMoreCn = Convert.ToString(reader["MoreSpecCn"]),
                                    SpecMoreEn = Convert.ToString(reader["MoreSpecEn"]),
                                    MatMatCode = Convert.ToString(reader["MatMatID"]),
                                    SpecPClass = Convert.ToString(reader["SpecPN"]),
                                    Qty = Convert.ToString(reader["Qty"]),
                                    Unit = Convert.ToString(reader["Unit"]),
                                    SupplyDiscipline = Convert.ToString(reader["SupDisc"]),
                                    SupplyResponsible = Convert.ToString(reader["SupResp"]),
                                    ErectionDiscipline = Convert.ToString(reader["ErecDisc"]),
                                    ErectionResponsible = Convert.ToString(reader["ErecResp"]),
                                    ID = Convert.ToInt32(reader["MatLibID"]).ToString("D4"),
                                    IsLibItem = true,
                                };
                                item.SpecMainCn = getSpecMainAux(item.MatLibItem.TechSpecMain, 4);
                                item.SpecAuxCn = getSpecMainAux(item.MatLibItem.TechSpecAux, 4);
                                item.SpecPortCn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode, 4);
                                item.SpecMainEn = getSpecMainAux(item.MatLibItem.TechSpecMain, 2);
                                item.SpecAuxEn = getSpecMainAux(item.MatLibItem.TechSpecAux, 2);
                                item.SpecPortEn = getSpecPort(item.MatLibItem.TypeP1, item.MatLibItem.SizeP1, item.MatLibItem.TypeP2, item.MatLibItem.SizeP2, item.AlterCode, 2);
                                item.SpecAllCn = (item as MatListItem).SpecAllCn;
                                item.SpecAllEn = (item as MatListItem).SpecAllCn;
                                diagBomItems.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"___HK_General. GetDiagBomItems(string id), Error: {ex.Message}");
                    }
                }
            }
            return diagBomItems;
        }
        internal static int NewDiagBomAdd(int diagId, string itemNO, MatListItem item)
        {
            if (item == null) return 0;
            string tableName = "HK_DiagBom";
            int newID = GetNewID(tableName);
            if (!(int.TryParse(itemNO, out int itemNo))) return 0; //int.TryParse(diagID, out int diagId) && 
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    string query = $"INSERT INTO {tableName} (ID, DiagID, No, MatLibID, " +
                                            $"Qty, Unit, SupDisc, SupResp, ErecDisc, ErecResp, " +
                                            $"MoreSpecCn, MoreSpecEn, RemarksCn, RemarksEn, " +
                                            $"Status, LastBy, LastOn) VALUES (" +
                                            $"{newID}," +
                                            $"{diagId}," +
                                            $"{itemNo}," +
                                            $"{item.ID}," +
                                            $"'{item.Qty}'," +
                                            $"'{item.Unit}'," +
                                            $"'{item.SupplyDiscipline}'," +
                                            $"'{item.SupplyResponsible}'," +
                                            $"'{item.ErectionDiscipline}'," +
                                            $"'{item.ErectionResponsible}'," +
                                            $"'{item.SpecMoreCn}'," +
                                            $"'{item.SpecMoreEn}'," +
                                            $"'{item.RemarksCn}'," +
                                            $"'{item.RemarksEn}'," +
                                            $"1," +
                                            $"'{UserName}'," +
                                            $"'{DateTime.Now.ToString()}'" +
                                            $")";
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Debug.WriteLine($"___HK_General.NewDiagBomAdd(string diagID, int itemNo, MatListItem item), Error: {ex.Message}");
                    return 0;
                    //MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
            }
            return newID;
        }
        internal static int GetDiagBomCount(int diagId)
        {
            string query = $"Select Count(*) From HK_DiagBom where DiagID = {diagId}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.GetDiagBomCount(string id), Error: {ex.Message}");
                    return -1;
                }
            }
        }
        internal static int DiagBomDelete(int id)
        {
            string query = $"DELETE FROM HK_DiagBom WHERE ID = {id}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 创建并配置 OdbcCommand 对象
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    {
                        // 执行查询，获取记录数
                        return (int)command.ExecuteNonQuery(); ;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"___HK_General.DiagBomDelete(int id), Error: {ex.Message}");
                    return 0;
                }
            }
        }
        #endregion
    }


}

