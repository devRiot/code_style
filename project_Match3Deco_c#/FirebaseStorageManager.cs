using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine.Networking;
using System.IO;
using System;

namespace devRiot
{
    //파이어베이스 스토리지 접근을 위한 매니저 클래스
    //다운로드는 WebRequest를 사용한다.
    public class FirebaseStorageManager : MonoSingleton<FirebaseStorageManager>
    {
        private FirebaseStorage storage;
        private string _storageUrl = "";
        private bool _isDownload = false;
        
        public string StorageURL { get { return _storageUrl; } }
        public bool IsDownload {  get { return _isDownload; } }

        public override void Init()
        {
            base.Init();

            _storageUrl = "gs://project-match3deco.appspot.com/data/";
            _isDownload = false;

            storage = FirebaseStorage.DefaultInstance;
        }

        public void DownloadFile(string filename)
        {
            _isDownload = true;
            // Firebase Storage에서 다운로드할 파일 경로 설정
            string storagePath = StorageURL + filename;

            // 다운로드할 파일의 Reference를 가져옵니다.
            StorageReference storageRef = storage.GetReferenceFromUrl(storagePath);

            // 파일 다운로드
            storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    string downloadUrl = task.Result.ToString();
                    StartCoroutine(DownloadJsonData(downloadUrl, filename));
                }
                else
                {
                    Debug.LogError("Failed to get download URL: " + task.Exception);
                }
            });
        }

        private IEnumerator DownloadJsonData(string downloadUrl, string fileName)
        {
            UnityWebRequest www = UnityWebRequest.Get(downloadUrl);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download JSON data: " + www.error);
            }
            else
            {
                //저장할 폴더경로 만들기
                string folderPath = Path.Combine(Application.persistentDataPath, "data");

                // 폴더가 존재하지 않으면 생성합니다.
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                //다운로드 데이터 저장하기
                string filePath = Path.Combine(folderPath, fileName);
                string jsonData = www.downloadHandler.text;

                try
                {
                    File.WriteAllText(filePath, jsonData);
                    Debug.Log("JSON data saved to: " + filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to save JSON data: " + e.Message);
                }

                _isDownload = false;
            }
        }
    }
}