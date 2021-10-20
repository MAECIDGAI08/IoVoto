using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Utility
{
    public class Utils
    {
        public static Boolean ValidateField(string arg, string validation)
        {//valida la coerenza dei dati in input lato server
            //arg: array con i name dei campi da validare
            //validation: array con il tipo di validazione che i campi in arg devono subire
            Boolean validationResult = false;
            switch (validation)
            {
                case "none": validationResult = true; break;
                case "required": if (!String.IsNullOrEmpty(arg)) { validationResult = true; } break;
                case "integer": if (String.IsNullOrEmpty(arg) || (!String.IsNullOrEmpty(arg) && (arg.ToString().All(Char.IsDigit) || arg.ToString().Substring(0, 1) == "-" && arg.ToString().Substring(1).All(Char.IsDigit)))) { validationResult = true; } break;
                case "req_integer": if (!String.IsNullOrEmpty(arg) && (arg.ToString().All(Char.IsDigit) || arg.ToString().Substring(0, 1) == "-" && arg.ToString().Substring(1).All(Char.IsDigit))) { validationResult = true; } break;
                case "req_check": if (!String.IsNullOrEmpty(arg) && arg.ToString().All(Char.IsDigit) && Int32.Parse(arg) == 1) { validationResult = true; } break;
            }
            return validationResult;
        }

        public static void LogTrace(string ip, string args)
        {
            string path = Path.GetFullPath(Directory.GetCurrentDirectory() + "/Logs/ExtraLog_" + DateTime.Now.ToString("yyyyMM") + ".txt");
            try
            {
                FileStream fileStream = null;
                fileStream = File.Open(path, File.Exists(path) ? FileMode.Append : FileMode.OpenOrCreate);
                using (StreamWriter fs = new StreamWriter(fileStream))
                {
                    fs.WriteLine(DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss") + " - " + ip + " - " + args);
                };
                fileStream.Close();
            }
            catch (IOException)
            { }
        }
        /*
        public static Boolean validationLoop(HttpContext context, string[] fields, string[] types, IFormCollection form)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (!form.ContainsKey(fields[i]))
                {
                    LogTrace(context, fields[i] + " not set");
                    return false;
                }
                else
                {
                    if (!ValidateField(form[fields[i]], types[i]))
                    {
                        LogTrace(context, fields[i] + " no " + types[i] + " validation");
                        return false;
                    }
                }
            }
            return true;
        }*/
        
    }

}
