using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iEngr.Hookup
{

    public partial class HK_General
    {
        public HK_General()
        {
            dicSubCatIni();
            dicPortTypeIni();
            dicSpecDicIni();
            dicPipeODIni();
            dicPNIni();
            dicSteelIni();
            dicThreadIni();
            dicTubeODIni();
            dicGlandIni();
            dicGenOptionIni();
            dicNoLinkSpecIni();
        }
        public static int intLan = 0; // 0: 中文； 其它为英文
        public readonly string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        public readonly string[] portNA = { "NA", "IS"};
        internal OdbcConnection GetConnection()
        {
            try
            {
                // 定义 DSN 名称
                string dsnName = "ComosExt";
                // 创建 OdbcConnection 对象并传入 DSN 连接字符串
                OdbcConnection connection = new OdbcConnection($"DSN={dsnName};UID=COMOSSH;Pwd=comos#321");
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
        internal ObservableCollection<HKMatGenLib> UpdateQueryResult(string conditions = null)
        {
            ObservableCollection<HKMatGenLib> result = new ObservableCollection<HKMatGenLib>();
            if (conditions == null) return result;
            string query = $"select " +
                $"mgl.ID as ID, " +
                $"mgl.CatID as CatID, " +
                $"mgl.SubCatID as SubCatID, " +
                $"sc.SpecCn as NameCn, " +
                $"sc.SpecEn as NameEn, " +
                $"sc.TypeP2 as AlterCode, " +
                $"mgl.TypeP1 as TypeP1, " +
                $"mgl.TypeP2 as TypeP2, " +
                $"mgl.SizeP1 as SizeP1, " +
                $"mgl.SizeP2 as SizeP2, " +
                $"mgl.PClass as PClass, " +
                $"mgl.MatSpec as MatSpec, " +
                $"mgl.MoreSpecCn as MoreSpecCn, " +
                $"mgl.MoreSpecEn as MoreSpecEn, " +
                $"mgl.RemarksCn as RemarksCn, " +
                $"mgl.RemarksEn as RemarksEn, " +
                $"mgl.TechSpecMain as TechSpecMain, " +
                $"mgl.TechSpecAux as TechSpecAux " +
                $"from HK_MatGenLib mgl " +
                $"inner join HK_MatSubCat sc on mgl.SubCatID = sc.ID " +
                $"{conditions}";
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand(query, conn))
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HKMatGenLib item = new HKMatGenLib
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                CatID = Convert.ToString(reader["CatID"]),
                                SubCatID = Convert.ToString(reader["SubCatID"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                AlterCode = Convert.ToString(reader["AlterCode"]),
                                TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                TypeP1 = Convert.ToString(reader["TypeP1"]),
                                TypeP2 = Convert.ToString(reader["TypeP2"]),
                                SizeP1 = Convert.ToString(reader["SizeP1"]),
                                SizeP2 = Convert.ToString(reader["SizeP2"]),
                                PClass = Convert.ToString(reader["PClass"]),
                                MatMatID = Convert.ToString(reader["MatSpec"]),
                                MoreSpecCn = Convert.ToString(reader["MoreSpecCn"]),
                                MoreSpecEn = Convert.ToString(reader["MoreSpecEn"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                            };
                            item.SpecCombMain = getSpecMainAux(item.TechSpecMain);
                            item.SpecCombAux = getSpecMainAux(item.TechSpecAux);
                            item.SpecCombPort = getSpecPort(item.TypeP1, item.SizeP1, item.TypeP2, item.SizeP2, item.AlterCode);
                            item.SpecPClass = dicPN.TryGetValue(item.PClass, out HKLibPN pclass) ? pclass.Spec : string.Empty;
                            //item.SpecMat = dicMatMat[item.MatSpec].Spec;
                            result.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"HK_General.UpdateQueryResult{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
                    return result;
            }
        }
        internal HKMatGenLib UpdateQueryResult(int ID)
        {
            HKMatGenLib item = new HKMatGenLib();
            // 构建 SQL 查询语句
            string query = $"select " +
                $"mgl.ID as ID, " +
                $"mgl.CatID as CatID, " +
                $"mgl.SubCatID as SubCatID, " +
                $"sc.SpecCn as NameCn, " +
                $"sc.SpecEn as NameEn, " +
                $"sc.TypeP2 as AlterCode, " +
                $"mgl.TypeP1 as TypeP1, " +
                $"mgl.TypeP2 as TypeP2, " +
                $"mgl.SizeP1 as SizeP1, " +
                $"mgl.SizeP2 as SizeP2, " +
                $"mgl.PClass as PClass, " +
                $"mgl.MatSpec as MatSpec, " +
                $"mgl.MoreSpecCn as MoreSpecCn, " +
                $"mgl.MoreSpecEn as MoreSpecEn, " +
                $"mgl.RemarksCn as RemarksCn, " +
                $"mgl.RemarksEn as RemarksEn, " +
                $"mgl.TechSpecMain as TechSpecMain, " +
                $"mgl.TechSpecAux as TechSpecAux " +
                $"from HK_MatGenLib mgl " +
                $"inner join HK_MatSubCat sc on mgl.SubCatID = sc.ID " +
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
                            item = new HKMatGenLib
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                CatID = Convert.ToString(reader["CatID"]),
                                SubCatID = Convert.ToString(reader["SubCatID"]),
                                NameCn = Convert.ToString(reader["NameCn"]),
                                NameEn = Convert.ToString(reader["NameEn"]),
                                AlterCode = Convert.ToString(reader["AlterCode"]),
                                TechSpecMain = Convert.ToString(reader["TechSpecMain"]),
                                TechSpecAux = Convert.ToString(reader["TechSpecAux"]),
                                TypeP1 = Convert.ToString(reader["TypeP1"]),
                                TypeP2 = Convert.ToString(reader["TypeP2"]),
                                SizeP1 = Convert.ToString(reader["SizeP1"]),
                                SizeP2 = Convert.ToString(reader["SizeP2"]),
                                PClass = Convert.ToString(reader["PClass"]),
                                MatMatID = Convert.ToString(reader["MatSpec"]),
                                MoreSpecCn = Convert.ToString(reader["MoreSpecCn"]),
                                MoreSpecEn = Convert.ToString(reader["MoreSpecEn"]),
                                RemarksCn = Convert.ToString(reader["RemarksCn"]),
                                RemarksEn = Convert.ToString(reader["RemarksEn"]),
                            };
                            item.SpecCombMain = getSpecMainAux(item.TechSpecMain);
                            item.SpecCombAux = getSpecMainAux(item.TechSpecAux);
                            item.SpecCombPort = getSpecPort(item.TypeP1, item.SizeP1, item.TypeP2, item.SizeP2, item.AlterCode);
                            item.SpecPClass = dicPN.TryGetValue(item.PClass, out HKLibPN pclass) ? pclass.Spec : string.Empty;
                            //item.SpecMat = dicMatMat[item.MatSpec].Spec;
                        }
                        return item;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    MessageBox.Show($"HK_General.UpdateQueryResult{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                }
                return null;
            }
        }
        public int DataUpdate(int ID, string matData)
        {
            if (matData == null || ID ==0) return 0;
            var arrMatData = matData.Split(',').ToArray<string>();
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status
                    string query = $"UPDATE HK_MatGenLib " +
                                   $"SET CatID='{arrMatData[1].Substring(0, 2)}'," +
                                   $"SubCatID='{arrMatData[1]}'," +
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
                                   $"MatSpec='{arrMatData[13]}'," +
                                   $"Status=1 " +
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
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    return 0;
                }
            }
        }
        internal int DataDelMark(int ID)
        {
            // 构建 SQL 查询语句
            string query = $"UPDATE HK_MatGenLib " +
                           $"SET Status = Status - 1" +
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
                    // 处理异常
                    MessageBox.Show($"HK_General.DataDelMark{Environment.NewLine}Error: {ex.Message}");
                }
            }
            return 0;
        }
        internal int DataDel(int ID)
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
                    // 处理异常
                    MessageBox.Show($"HK_General.DataDel{Environment.NewLine}Error: {ex.Message}");
                }
            }
            return 0;
        }
        internal int CountExistingData(string conditions = null)
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
                    // 处理异常
                    MessageBox.Show($"{nameof(HK_General)}.{nameof(CountExistingData)}{Environment.NewLine}Error: {ex.Message}");
                    return -1;
                }
            }
        }
        internal int NewDataAdd(string matData, out int newID)
        {
            newID = GetNewID();
            if (matData == null) return 0;
            var arrMatData = matData.Split(',').ToArray<string>();
            using (OdbcConnection conn = GetConnection())
            {
                try
                {
                    // 0:CatID, 1:SubCatID, 2:TechSpecMain, 3:TechSpecAux, 4:TypeP1, 5:SizeP1, 6:TypeP2, 7:SizeP2, 8:MoreSpecCn, 9:MoreSpecEn, 10:RemarksCn, 11: RemarksEn, 12, PClass, 13:MatSpec, 14,Status
                    string query = $"INSERT INTO HK_MatGenLib (ID, CatID, SubCatID, TechSpecMain, TechSpecAux, " +
                                            $"TypeP1, SizeP1, TypeP2, SizeP2, " +
                                            $"MoreSpecCn, MoreSpecEn, RemarksCn, RemarksEn, " +
                                            $"PClass, MatSpec, Status) VALUES (" +
                                            $"{newID}," +
                                            $"'{arrMatData[1].Substring(0,2)}'," +
                                            $"'{arrMatData[1]}'," +
                                            $"'{arrMatData[2].Replace('|',',')}'," +
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
                                            $"1" +
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
                    MessageBox.Show($"数据未记录！{Environment.NewLine}HK_General.NewDataAdd{Environment.NewLine}Error: {ex.Message}");
                    // 可以选择返回空列表或者其他适当的处理
                    return 0;
                }
            }
        }
        private int GetNewID()
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
                    // 处理异常
                    MessageBox.Show($"HK_General.GetNewID{Environment.NewLine}Error: {ex.Message}");
                }
            }
            return 0;
        }

        private string getConditionExp(List<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;

            return string.Join(" AND ", input.Where(s => !string.IsNullOrEmpty(s)));
        }
        private string getSpecExp(string field, List<string> input)
        {
            if (input == null || !input.Any())
                return string.Empty;

            var conditions = input
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => $"{field} LIKE '%{s}%'");

            return string.Join(" AND ", conditions);
        }
        private string getSpecMainAux(string input)
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
                            if (dicSpecDic[name].Link.StartsWith("LibPipeOD"))
                                result = $"{dicSpecDic[name].Prefix}{getPipeData(value, dicSpecDic[name].Link.Split(',')[1])}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibPN"))
                                result = $"{dicSpecDic[name].Prefix}{dicPN[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGenOption"))
                                result = $"{dicSpecDic[name].Prefix}{dicGenOption[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibGland"))
                                result = $"{dicSpecDic[name].Prefix}{dicGland[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibThread"))
                                result = $"{dicSpecDic[name].Prefix}{dicThread[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibTubeOD"))
                                result = $"{dicSpecDic[name].Prefix}{dicTubeOD[value].Spec}{dicSpecDic[name].Suffix}";
                            else if (dicSpecDic[name].Link.StartsWith("LibSteel"))
                                result = $"{dicSpecDic[name].Prefix}{getSteelData(value, dicSpecDic[name].Link.Split(',')[1])}{dicSpecDic[name].Suffix}";
                            else
                                result = $"{dicSpecDic[name].Prefix}{value}{dicSpecDic[name].Suffix}";
                        }
                    }
                    results = string.IsNullOrEmpty(results) ? result : (string.IsNullOrEmpty(result) ? results : results + ", " + result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
            return results;
        }
        private string getSpecPort(string typeP1, string sizeP1, string typeP2, string sizeP2, string alterCode = "")
        {
            string result1 = string.Empty;
            string result2 = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(typeP1))
                {
                    if (string.IsNullOrEmpty(sizeP1))
                        result1 = dicPortType[typeP1].Name;
                    else if (dicPortType.ContainsKey(typeP1))
                    {
                        if (dicPortType[typeP1].Link.StartsWith("LibPipeOD"))
                            result1 = $"{dicPortType[typeP1].Prefix}{getPipeData(sizeP1, dicPortType[typeP1].Link.Split(',')[1])}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibPN"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicPN[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGenOption"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicGenOption[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibGland"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicGland[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibThread"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicThread[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibTubeOD"))
                            result1 = $"{dicPortType[typeP1].Prefix}{dicTubeOD[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                        else if (dicPortType[typeP1].Link.StartsWith("LibSteel"))
                            result1 = $"{dicPortType[typeP1].Prefix}{getSteelData(sizeP1, dicPortType[typeP1].Link.Split(',')[1])}{dicPortType[typeP1].Suffix}";
                        else
                            result1 = $"{dicPortType[typeP1].Prefix}{dicPN[sizeP1].Spec}{dicPortType[typeP1].Suffix}";
                    }
                }
                if (!string.IsNullOrEmpty(typeP2))
                {
                    if (string.IsNullOrEmpty(sizeP2))
                        result2 = dicPortType[typeP2].Name;
                    else if (dicPortType.ContainsKey(typeP2))
                    {
                        if (dicPortType[typeP2].Link.StartsWith("LibPipeOD"))
                            result2 = $"{dicPortType[typeP2].Prefix}{getPipeData(sizeP2, dicPortType[typeP2].Link.Split(',')[1])}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibPN"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicPN[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGenOption"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicGenOption[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibGland"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicGland[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibThread"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicThread[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibTubeOD"))
                            result2 = $"{dicPortType[typeP2].Prefix}{dicTubeOD[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                        else if (dicPortType[typeP2].Link.StartsWith("LibSteel"))
                            result2 = $"{dicPortType[typeP2].Prefix}{getSteelData(sizeP2, dicPortType[typeP2].Link.Split(',')[1])}{dicPortType[typeP2].Suffix}";
                        else
                            result2 = $"{dicPortType[typeP2].Prefix}{dicPN[sizeP2].Spec}{dicPortType[typeP2].Suffix}";
                    }
                }
                if (alterCode == "AS1" || alterCode == "DF1" && result1 == result2)
                    result2 = string.Empty;
                return string.IsNullOrEmpty(result2) ? result1 : (string.IsNullOrEmpty(result1) ? result2 : result1 + " / " + result2);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // 可以选择返回空列表或者其他适当的处理
                return string.Empty;
            }
        }
        private string getPipeData(string input, string key = "")
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
        private string getSteelData(string input, string key = "")
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (key == "CSSpec")
                return dicSteel[input]?.CSSpec;
            else if (key == "IBSpec")
                return dicSteel[input]?.IBSpec;
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
    }
}
