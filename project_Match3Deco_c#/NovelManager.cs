using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace devRiot.Novel
{
    //인게임 텍스트연출을 위한 노벨매니저
    //스크립트 리스트와 스크립트 큐로 일단 나눠져 있다.
    public class NovelManager : Singleton<NovelManager>
    {
        private List<NovelScriptInfo> _scriptList;
        private Queue<NovelScriptInfo> _processQueue;

        public int ScriptCount { get { return _scriptList.Count; } }
        public int ScriptQueue { get { return _processQueue.Count; } }

        private bool _wait = false;
        public bool Wait { 
            get { return _wait; } 
            set { _wait = value; }
        }

        public void Init()
        {
            _scriptList = new List<NovelScriptInfo>();
            _processQueue = new Queue<NovelScriptInfo>();
        }

        public void AddScript(NovelScriptInfo script)
        {
#if DISABLE_NOVEL
            Debug.Log("skip novel script : " + script.command.ToString());
#else
            _scriptList.Add(script);
#endif
        }

        public void ProcessScript()
        {
            if (_scriptList.Count == 0)
                return;
            foreach(NovelScriptInfo script in _scriptList)
            {
                _processQueue.Enqueue(script);
            }
            _scriptList.Clear();
        }

        public NovelScriptInfo NextScript()
        {
            if (_processQueue.Count > 0)
            {
                NovelScriptInfo next = _processQueue.Peek();
                if (next.isProcess == false)
                {
                    next.isProcess = true;
                    return next;
                }
            }
            return null;
        }

        public void RemoveScript(NovelScriptInfo script)
        {
            if(_processQueue.Peek().Equals(script))
            {
                _processQueue.Dequeue();
            }
        }

        public IEnumerator CoProcessWaitInteraction()
        {
            _wait = true;
            Debug.Log("CoProcessWaitInteraction Start");
            while(_wait)
            {
                yield return 0;
            }

            Debug.Log("CoProcessWaitInteraction End");
            yield break;
        }

        //큐의 모든 스크립트가 진행될때까지 대기한다.
        public IEnumerator CoProcessEndScriptCheck()
        {
            while (_processQueue.Count > 0)
            {
                yield return 0;
            }
            yield break;
        }
    }
}