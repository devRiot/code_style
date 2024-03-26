using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Corgi
{
    public static class CorgiFile
    {
        public static string GetPartialKey()
        {
            return "58A368FD970F";
        }

		//파일 존재여부 체크
		public static bool CanRead(string fileName)
		{
			string path = PathForDocumentsFile(fileName);
			return (File.Exists(path)) ? true : false;
		}

        //파일 쓰기
        public static void WriteStringToFile( string str, string fileName )
        {
            string path = PathForDocumentsFile(fileName);
            FileStream file = new FileStream ( path, FileMode.Create, FileAccess.Write );
 
            StreamWriter sw = new StreamWriter( file );
            sw.WriteLine( str );
 
            sw.Close();
            file.Close();
        }
        
 
        //파일 읽기
        public static string ReadStringFromFile( string fileName )//, int lineIndex )
        {
            string path = PathForDocumentsFile( fileName );
 
            if( File.Exists( path ))
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader st = new StreamReader(file);

                string str = null;
                str = st.ReadLine();

                st.Close();
                file.Close();
 
                return str;
            }
            else
            {
                return null;
            }        
        }

		public static string ReadStringToEndFromFile( string fileName )//, int lineIndex )
		{
			string path = PathForDocumentsFile( fileName );
			
			if( File.Exists( path ))
			{
				FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
				StreamReader st = new StreamReader(file);
				
				string str = null;
				str = st.ReadToEnd();
				
				st.Close();
				file.Close();
				
				return str;
			}
			else
			{
				return null;
			}        
		}

		public static bool DeleteFile(string fileName)
		{
			string path = PathForDocumentsFile(fileName);
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
					return true;
				}
				catch { }
			}
			
			return false;
		}

		public static bool CopyFile(string fileNameSrc, string fileNameDest, bool overWrite = false)
		{
			string pathSrc = PathForDocumentsFile(fileNameSrc);
			string pathDest = PathForDocumentsFile(fileNameDest);
			if (File.Exists(pathSrc))
			{
				try
				{
                    File.Copy(pathSrc, pathDest, overWrite);
					return true;
				}
				catch { }
			}
			
			return false;
		}

        public static List<FileInfo> GetFileList(string path)
        {
            try
            {
                string DirectoryPath = PathForDocuments(path);

                List<FileInfo> Files = new List<FileInfo>();
                
                DirectoryInfo Directory = new DirectoryInfo(DirectoryPath);
                FileInfo[] SubFiles = Directory.GetFiles();
                foreach (FileInfo SubFile in SubFiles)
                {
                    Files.Add(SubFile);
                }
                
                return Files;
            }
            catch (Exception a)
            {
                throw a;
            }
        }
 
        //파일 경로 알아오기
        public static string PathForDocumentsFile( string fileName )
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5);
                path = path.Substring(0, path.LastIndexOf('/'));     
                path = Path.Combine(path, "Documents");

                string dir = Path.GetDirectoryName(Path.Combine(path, fileName));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Path.Combine(path, fileName);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                string path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));

                string dir = Path.GetDirectoryName(Path.Combine(path, fileName));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Path.Combine(path, fileName);
            }
            else
            {
                // windows
                string path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));

                string dir = Path.GetDirectoryName(Path.Combine(path, fileName));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                    
                return Path.Combine(path, fileName);
            }
        }

        public static string PathForDocuments(string pathToFind)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5);
                path = path.Substring(0, path.LastIndexOf('/'));
                path = Path.Combine(path, "Documents");

                string dir = Path.GetDirectoryName(Path.Combine(path, pathToFind));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                string path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));

                string dir = Path.GetDirectoryName(Path.Combine(path, pathToFind));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
            else
            {
                // windows
                string path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));

                string dir = Path.GetDirectoryName(Path.Combine(path, pathToFind));

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
        }

        public static void Serialize<T>(string fileName, T gameObject)
        {
            string path = PathForDocumentsFile(fileName);

            BinaryFormatter binFmt = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                try
                {
                    binFmt.Serialize(fs, gameObject);
                }
                catch(SerializationException e)
                {
                    Debug.LogError("failed to serialize. : " + e.Message);
                }
				fs.Close();
            }
        }

        public static T DeSerialize<T>(string fileName)
        {
            string path = PathForDocumentsFile(fileName);

            T retObject = default(T);

            BinaryFormatter binFmt = new BinaryFormatter(); ;
            using (FileStream rdr = new FileStream(path, FileMode.Open))
            {
                try
                {
                    retObject = (T)binFmt.Deserialize(rdr);
                }
                catch(SerializationException e)
                {
                    Debug.LogError("failed to serialize : " + e.Message);
                }
				rdr.Close();
                return retObject;
            }
        }

    }
}
