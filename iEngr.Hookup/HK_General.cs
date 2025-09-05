using iEngr.Hookup.Models;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace iEngr.Hookup
{

    public static partial class HK_General
    {
        public static int ProjLanguage = 4; // 4: 中文； 2为英文
        public static bool IsAutoComosUpdate = true;
        public static string UserName = "Anonymous";
        public static readonly string[] portDef = { "EQ1", "DF1", "AS1", "NEQ" };
        public static readonly string[] portNA = { "NA", "IS" };


        public static string ErrMsgOmMatData {  get; set; }

    }
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(
            this Dictionary<TKey, TValue> original)
        {
            return new Dictionary<TKey, TValue>(original);
        }

        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(
            this Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            return original.ToDictionary(
                pair => pair.Key,
                pair => (TValue)pair.Value.Clone()
            );
        }
    }

}
