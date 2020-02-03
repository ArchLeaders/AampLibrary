using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AampLibraryCSharp
{
    public class Hashes
    {
        //Based from https://github.com/jam1garner/aamp2xml

        public static Dictionary<uint, string> hashName = new Dictionary<uint, string>();

        private static void CheckHash(string hashStr)
        {
            uint hash = Crc32.Compute(hashStr);
            if (!hashName.ContainsKey(hash))
                hashName.Add(hash, hashStr);
        }

        private static void GenerateHashes()
        {
            foreach (string hashStr in Properties.Resources.aamp_hashed_names.Split('\n'))
            {
                SetNumberedString(hashStr, 6);

                CheckHash(hashStr);
            }
            foreach (string hashStr in Properties.Resources.aamp_hashed_names_numbered.Split('\n'))
            {
                string[] strArray = Hashes.GeneratedNumberHashes(hashStr);
                if (strArray.Length > 0) {
                    foreach (string strHash in strArray)
                        CheckHash(strHash);
                }
                else {
                    CheckHash(hashStr);
                }
            }
            
            SetNumberedString("PointLightRig", 50);
            SetNumberedString("SpotLightRig", 50);
            SetNumberedString("AI_", 1000);
            SetNumberedString("Action_", 1000);
            SetNumberedString("HemisphereLight", 30);
            SetNumberedString("Fog", 30);
            SetNumberedString("DirectionalLight", 30);
            SetNumberedString("BloomObj", 30);
            SetNumberedString("OfxLargeLensFlareRig", 30);
            SetNumberedString("name", 50);
            SetNumberedString("intensity", 50);
            SetNumberedString("connection_curve_", 100);
            SetNumberedString("bone_", 100);
            SetNumberedString("output_single_", 100);
            SetNumberedString("support_bone_", 100);
        }

        private static void SetNumberedString(string hashStr, int Amount)
        {
            for (int i = 0; i < Amount; i++)
            {
                CheckHash($"{hashStr}{i}");
            }
        }

        public static string[] GeneratedNumberHashes(string hash)
        {
            if (!hash.Contains("%"))
                return new string[0];
            string[] strArray = new string[6];
            for (int index = 0; index < 6; ++index)
                strArray[index] = hash.Replace("%d", index.ToString()) ?? "";
            return strArray;
        }

        public static uint SetName(string Name) {
            return Crc32.Compute(Name);
        }

        public static string GetName(uint hash)
        {
            if (hashName.Count == 0)
                GenerateHashes();
            string name = null;
            hashName.TryGetValue(hash, out name);

            if (name == null)
                return hash.ToString();

            return name;
        }
    }
}
