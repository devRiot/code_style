using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using devRiot.Spec;
using devRiot.Scene;

namespace devRiot.Deco
{
    [ExecuteInEditMode]
    public class DecoAreaObject : MonoBehaviour
    {
        [SerializeField]
        private SpecDecoAreaInfo _specInfo;

        [SerializeField]
        private List<DecoObject> _decoObjects = new List<DecoObject>();

        [SerializeField]
        private List<DecoBonusObject> _bonusDecoList = new List<DecoBonusObject>();

        [SerializeField]
        private Transform _UIAnchor;

        [SerializeField]
        private bool _collectDecoObjects;

        private DecoEpisodeObject _episode = null;
        private bool _installed = false;
        private bool _bonusInstall = false;

        public List<DecoObject> DecoList {  get { return _decoObjects; } }
        public int Level { get { return _specInfo.level; } }
        public SpecDecoAreaInfo Spec { get { return _specInfo; } }
        public bool IsInstalled {  get { return _installed; } }
        public DecoEpisodeObject EpisodeObject { get { return _episode; } }

        public Vector3 UIAnchorPosition { 
            get
            {
                if(_UIAnchor != null)
                {
                    return _UIAnchor.position;
                }
                else
                {
                    DecoObject decoObject = _decoObjects[0];
                    return decoObject.transform.position;
                }
            } 
        }

        public virtual void Init(DecoEpisodeObject episodeObject)
        {
            _episode = episodeObject;

            SetSpec();

            _decoObjects.Clear();
            _bonusDecoList.Clear();

            DecoObject[] objects = GetComponentsInChildren<DecoObject>(true);
            for (int i = 0; i < objects.Length; i++)
            {
                DecoObject decoObject = objects[i];
                decoObject.Init(_specInfo);
                _decoObjects.Add(decoObject);

                //데코 하위에 있는 보너스오브젝트들을 등록한다.
                DecoBonusObject[] bonusDecosInDeco = decoObject.GetComponentsInChildren<DecoBonusObject>(true);
                for (int j = 0; j < bonusDecosInDeco.Length; j++)
                {
                    bonusDecosInDeco[j].Init(decoObject.transform);
                    _bonusDecoList.Add(bonusDecosInDeco[j]);
                }
            }
        }

        private void SetSpec()
        {
            _specInfo.name = gameObject.name;
            _specInfo.price = Random.Range(300, 1000);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(_collectDecoObjects)
            {
                _decoObjects.Clear();
                DecoObject[] objects = GetComponentsInChildren<DecoObject>(true);
                for(int i=0; i<objects.Length; i++)
                {
                    _decoObjects.Add(objects[i]);
                }
                _collectDecoObjects = false;
            }
        }
#endif

        public void StartLevel()
        {
            Debug.Log("StartLevel : " + gameObject.name);
        }

        public bool ContainDeco(string decoName)
        {
            foreach (DecoObject deco in _decoObjects)
            {
                if (deco.Name == decoName)
                {
                    return true;
                }
            }
            return false;
        }

        public void InstallBuildDeco(string decoName)
        {
            foreach(DecoObject deco in _decoObjects)
            {
                if(deco.Name == decoName)
                {
                    deco.Visible(true);
                }
            }

            _installed = true;
        }

        public void InstallBuildBonusDeco(DecoBonusConditionType conditionType)
        {
            StartCoroutine(CoProcessBuildBonusDeco(conditionType));
        }

        IEnumerator CoProcessBuildBonusDeco(DecoBonusConditionType conditionType)
        {
            foreach (DecoBonusObject bonusDeco in _bonusDecoList)
            {
                if (bonusDeco.ConditionType != conditionType)
                    continue;

                if (bonusDeco.IsParentActive == false)
                    continue;

                bonusDeco.gameObject.SetActive(true);

                //fx연출
                Vector3 spawnPos = bonusDeco.transform.position;
                ParticleManager.instance.SpawnParticleEffect2D("RespawnExplosion", spawnPos, 3f);
                yield return new WaitForSeconds(0.3f);
            }
            yield break;
        }
    }
}
