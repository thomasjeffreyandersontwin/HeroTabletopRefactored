using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;
using Module.Shared;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace HeroVirtualTableTop.Common
{
    public class CommonLibrary
    {
        #region Resource Dictionary and Style related
        public static System.Windows.Style GetCustomStyle(string styleName)
        {
            System.Windows.ResourceDictionary resource = new System.Windows.ResourceDictionary
            {
                Source = new Uri(Constants.RESOURCE_DICTIONARY_PATH, UriKind.RelativeOrAbsolute)
            };
            return (System.Windows.Style)resource[styleName];
        }

        public static System.Windows.Style GetCustomWindowStyle()
        {
            return GetCustomStyle(Constants.CUSTOM_MODELESS_TRANSPARENT_WINDOW_STYLENAME);
        }

        #endregion

        #region JSON Serialize/Deserialize
        public static T GetDeserializedJSONFromFile<T>(string fileName)
        {
            T obj = default(T);
            if (!File.Exists(fileName))
            {
                CreateFile(fileName);
                return obj;
            }
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = new StreamReader(fileName))
            using (JsonReader reader = new JsonTextReader(sr))
            {

                serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                serializer.Formatting = Formatting.Indented;
                serializer.TypeNameHandling = TypeNameHandling.Objects;

                obj = serializer.Deserialize<T>(reader);
            }
            return obj;
        }

        public static void SerializeObjectAsJSONToFile<T>(string fileName, T obj)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(fileName))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {

                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    serializer.Formatting = Formatting.Indented;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;
                    serializer.Serialize(writer, obj);
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region File I/O
        public static void CreateFile(string fileName)
        {
            FileStream fs = File.Create(fileName);
            fs.Dispose();
        }

        #endregion

        #region General Control

        public static Visual GetAncestorByType(DependencyObject element, Type type)
        {
            while (element != null && !(element.GetType() == type))
                element = VisualTreeHelper.GetParent(element);

            return element as Visual;
        }

        public static Visual GetTemplateAncestorByType(DependencyObject element, Type type)
        {
            while (element != null && !(element.GetType() == type))
                element = (element as FrameworkElement).TemplatedParent;

            return element as Visual;
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (int i = 0;
                i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        public static string GetContainerWindowName(object element)
        {
            Window win = null;
            string winName = "";

            if (element is Window)
            {
                win = element as Window;
                winName = win.Name;
            }
            else
            {
                DependencyObject dObj = element as DependencyObject;
                while (win == null)
                {
                    FrameworkElement elem = dObj as FrameworkElement;
                    dObj = elem.Parent;
                    if (dObj is Window)
                    {
                        win = dObj as Window;
                        winName = win.Name;
                        break;
                    }
                }
            }

            return winName;
        }

        // Helper to search up the VisualTree
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #endregion

        #region Collision Info
        //X:[126.30] Y:[-0.50] Z:[-60.09] D:[0.00]
        public static Vector3 GetCollisionVector(string collisionInfo)
        {
            float X = 0f, Y = 0f, Z = 0f;
            try
            {
                int indexXStart = collisionInfo.IndexOf("[");
                int indexXEnd = collisionInfo.IndexOf("]");
                string xStr = collisionInfo.Substring(indexXStart + 1, indexXEnd - indexXStart - 1);
                X = float.Parse(xStr);

                int indexYStart = collisionInfo.IndexOf("[", indexXEnd);
                int indexYEnd = collisionInfo.IndexOf("]", indexYStart);
                string yStr = collisionInfo.Substring(indexYStart + 1, indexYEnd - indexYStart - 1);
                Y = float.Parse(yStr);

                int indexZStart = collisionInfo.IndexOf("[", indexYEnd);
                int indexZEnd = collisionInfo.IndexOf("]", indexZStart);
                string zStr = collisionInfo.Substring(indexZStart + 1, indexZEnd - indexZStart - 1);
                Z = float.Parse(zStr);
            }
            catch (Exception ex)
            {
            }

            return new Vector3(X, Y, Z);
        }

        #endregion

        # region Vector Maths

        public static Vector3 GetRoundedVector(Vector3 vector, int decimalPlaces)
        {
            float x = (float)Math.Round(vector.X, decimalPlaces);
            float y = (float)Math.Round(vector.Y, decimalPlaces);
            float z = (float)Math.Round(vector.Z, decimalPlaces);

            return new Vector3(x, y, z);
        }

        public static double GetRadianAngle(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        #endregion

        #region UI Settings

        //public static void SaveUISettings(string settingsName, object value)
        //{
        //    if (GlobalVariables_UISettings.ContainsKey(settingsName))
        //        GlobalVariables_UISettings[settingsName] = value;
        //    else
        //        GlobalVariables_UISettings.Add(settingsName, value);
        //}

        //public static object GetUISettings(string settingsName)
        //{
        //    if (GlobalVariables_UISettings.ContainsKey(settingsName))
        //        return GlobalVariables_UISettings[settingsName];
        //    else
        //        return null;
        //}

        #endregion

        #region Misc

        public static bool IsNumeric(object value)
        {
            int k;
            if (value != null && Int32.TryParse(value.ToString(), out k))
                return true;
            return false;
        }

        public static int CompareStrings(string s1, string s2)
        {
            //string pattern = "([A-Za-z\\s]*)([0-9]*)";
            string pattern = @"^(.*?)(\d+)(\D*)$";
            string h1 = Regex.Match(s1, pattern).Groups[1].Value;
            string h2 = Regex.Match(s2, pattern).Groups[1].Value;
            if (h1 != h2)
                return h1.CompareTo(h2);
            string t1 = Regex.Match(s1, pattern).Groups[2].Value;
            string t2 = Regex.Match(s2, pattern).Groups[2].Value;
            if (IsNumeric(t1) && IsNumeric(t2))
                return int.Parse(t1).CompareTo(int.Parse(t2));
            else if (!string.IsNullOrEmpty(t1) && !string.IsNullOrEmpty(t2))
                return t1.CompareTo(t2);
            else
                return s1.CompareTo(s2);
        }

        #endregion
    }
    public class StringValueComparer : IComparer<string>
    {
        public int Compare(string s1, string s2)
        {
            return CommonLibrary.CompareStrings(s1, s2);
        }
    }
}
